/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace
 * ------------------------------------------------------------- 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 * MA 02110-1301  USA
 * 
 * This library is intended to be used as a plugin for Kerbal Space Program
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        /**
         * Class which manages loading from config nodes via reflection and 
         * attribution
         **/
        public class Parser
        {
            // Create an object form a configuration node (Generic)
            public static T CreateObjectFromConfigNode <T> (ConfigNode node, bool getChilds = true) where T : class, new()
            {
                T o = new T ();
                LoadObjectFromConfigurationNode (o, node, getChilds);
                return o;
            }

            // Create an object form a configuration node (Runtime type identification)
            public static object CreateObjectFromConfigNode (Type type, ConfigNode node, bool getChilds = true)
            {
                object o = Activator.CreateInstance (type);
                LoadObjectFromConfigurationNode (o, node, getChilds);
                return o;
            }

            // Create an object form a configuration node (Runtime type identification) with constructor parameters
            public static object CreateObjectFromConfigNode (Type type, ConfigNode node, object[] arguments, bool getChilds = true)
            {
                object o = Activator.CreateInstance (type, arguments);
                LoadObjectFromConfigurationNode (o, node, getChilds);
                return o;
            }
			
            /**
             * Load data for an object's ParserTarget fields and properties from a configuration node
             * 
             * @param o Object for which to load data.  Needs to be instatiated object
             * @param node Configuration node from which to load data
             **/
            public static void LoadObjectFromConfigurationNode (object o, ConfigNode node, bool getChilds = true)
            {
                // Get the object as a parser event subscriber (will be null if 'o' does not conform)
                IParserEventSubscriber subscriber = o as IParserEventSubscriber;

                // Generate two lists -> those tagged preapply and those not
                List<KeyValuePair<bool,MemberInfo> > preapplyMembers = new List<KeyValuePair<bool,MemberInfo> > ();
                List<KeyValuePair<bool,MemberInfo> > postapplyMembers = new List<KeyValuePair<bool,MemberInfo> > ();

                // Discover members tagged with parser attributes
                foreach (MemberInfo member in o.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) 
                {
                    // Is this member a parser target?
                    ParserTarget[] attributes = member.GetCustomAttributes ((typeof(ParserTarget)), true) as ParserTarget[];
                    if (attributes.Length > 0) 
                    {
                        // If this member is a collection
                        bool isCollection = attributes[0].GetType().Equals(typeof(ParserTargetCollection));
                        KeyValuePair<bool, MemberInfo> entry = new KeyValuePair<bool, MemberInfo>(isCollection, member);

                        // If this member has the preapply attribute, we need to process it
                        if (member.GetCustomAttributes ((typeof(PreApply)), true).Length > 0)
                            preapplyMembers.Add (entry);
                        else
                            postapplyMembers.Add (entry);
                    }
                }

                // Process the preapply members
                foreach (KeyValuePair<bool, MemberInfo> member in preapplyMembers)
                {
                    if(member.Key)
                    {
                        LoadCollectionMemberFromConfigurationNode(member.Value, o, node);
                    }
                    else
                    {
                        LoadObjectMemberFromConfigurationNode (member.Value, o, node);
                    }
                }

                // Call Apply
                if (subscriber != null) 
                    subscriber.Apply (node); 
                
                // Process the postapply members
                foreach (KeyValuePair<bool, MemberInfo> member in postapplyMembers)
                {
                    if(member.Key)
                    {
                        LoadCollectionMemberFromConfigurationNode(member.Value, o, node);
                    }
                    else
                    {
                        LoadObjectMemberFromConfigurationNode (member.Value, o, node);
                    }
                }

                // Call PostApply
                if (subscriber != null)
                    subscriber.PostApply (node);
            }

            /** 
             * Load collection for ParserTargetCollection
             * 
             * @param member Member to load data for
             * @param o Instance of the object which owns member
             * @param node Configuration node from which to load data
             **/
            public static void LoadCollectionMemberFromConfigurationNode (MemberInfo member, object o, ConfigNode node, bool getChilds = true)
            {
                // Get the target attribute
                ParserTargetCollection target = (member.GetCustomAttributes ((typeof(ParserTargetCollection)), true) as ParserTargetCollection[]) [0];

                // Figure out if this field exists and if we care
                bool isNode = node.HasNode (target.fieldName);
                bool isValue = node.HasValue (target.fieldName);

                // Obtain the type the member is (can only be field or property)
                Type targetType = null;
                object targetValue = null;
                if (member.MemberType == MemberTypes.Field) 
                {
                    targetType = (member as FieldInfo).FieldType;
                    if (getChilds)
                        targetValue = (member as FieldInfo).GetValue(o);
                    else
                        targetValue = null;
                } 
                else 
                {
                    targetType = (member as PropertyInfo).PropertyType;
                    try
                    {
                        if ((member as PropertyInfo).CanRead && getChilds)
                            targetValue = (member as PropertyInfo).GetValue(o, null);
                        else
                            targetValue = null;
                    }
                    catch (Exception e)
                    {
                        // Ignore runtime getters
                        if (!e.Source.Contains(".get_"))
                            Debug.LogException(e);
                    }
                }

                // If there was no data found for this node
                if (!isNode && !isValue) 
                {
                    if (!target.optional && !(target.allowMerge && targetValue != null)) 
                    {
                        // Error - non optional field is missing
                        throw new ParserTargetMissingException ("Missing non-optional field: " + o.GetType () + "." + target.fieldName);
                    }

                    // Nothing to do, so return
                    return;
                }

                // If we are dealing with a generic collection
                if (targetType.IsGenericType) 
                {
                    // If the target is a generic dictionary
                    if ((typeof(IDictionary)).IsAssignableFrom (targetType)) 
                    {
                        throw new Exception ("Generic dictionaries are unsupported at this time");
                    } 

                    // If the target is a generic collection
                    else if ((typeof(IList)).IsAssignableFrom (targetType))
                    {
                        // We need a node for this decoding
                        if(!isNode)
                        {
                            throw new Exception("Loading a generic list requires sources to be nodes");
                        }

                        // Get the target value as a collection
                        IList collection = targetValue as IList;
                        
                        // Get the internal type of this collection
                        Type genericType = targetType.GetGenericArguments () [0];

                        // Create a new collection if merge is disallowed or if the collection is null
                        if (collection == null || !target.allowMerge) 
                        {
                            collection = Activator.CreateInstance (targetType) as IList;
                            targetValue = collection;
                        }

                        // Iterate over all of the nodes in this node
                        foreach (ConfigNode subnode in node.GetNode(target.fieldName).nodes) 
                        {
                            // Check for the name significance
                            if (target.nameSignificance == NameSignificance.None) 
                            {
                                // Just processes the contents of the node
                                collection.Add (CreateObjectFromConfigNode (genericType, subnode, target.getChild));
                            }

                            // Otherwise throw an exception because we don't support named ones yet
                            else if (target.nameSignificance == NameSignificance.Type) 
                            {
                                // Generate the type from the name
                                Type elementType = Injector.ModTypes.FirstOrDefault(t => t.Name == subnode.name);

                                // Add the object to the collection
                                collection.Add (CreateObjectFromConfigNode (elementType, subnode, target.getChild));
                            }
                        }
                    }
                }

                // If we are dealing with a non generic collection
                else 
                {
                    // Check for invalid scenarios
                    if (target.nameSignificance == NameSignificance.None) 
                    {
                        throw new Exception ("Can not infer type from non generic target; can not infer type from zero name significance");
                    }
                }
                
                // If the member type is a field, set the value
                if(member.MemberType == MemberTypes.Field)
                {
                    (member as FieldInfo).SetValue(o, targetValue);
                }
                
                // If the member wasn't a field, it must be a property.  If the property is writable, set it.
                else if((member as PropertyInfo).CanWrite)
                {
                    (member as PropertyInfo).SetValue(o, targetValue, null);
                }
            }

            /**
             * Load data for ParserTarget field or property from a configuration node
             * 
             * @param member Member to load data for
             * @param o Instance of the object which owns member
             * @param node Configuration node from which to load data
             **/
            public static void LoadObjectMemberFromConfigurationNode (MemberInfo member, object o, ConfigNode node, bool getChilds = true)
            {
                // Get the parser target, only one is allowed so it will be first
                ParserTarget target = (member.GetCustomAttributes ((typeof(ParserTarget)), true) as ParserTarget[]) [0];
                
                // Figure out if this field exists and if we care
                bool isNode = node.HasNode (target.fieldName);
                bool isValue = node.HasValue (target.fieldName);
                
                // Obtain the type the member is (can only be field or property)
                Type targetType = null;
                object targetValue = null;
                if (member.MemberType == MemberTypes.Field)
                {
                    targetType = (member as FieldInfo).FieldType;
                    if (getChilds)
                        targetValue = (member as FieldInfo).GetValue(o);
                    else
                        targetValue = null;
                }
                else
                {
                    targetType = (member as PropertyInfo).PropertyType;
                    try
                    {
                        if ((member as PropertyInfo).CanRead && getChilds)
                            targetValue = (member as PropertyInfo).GetValue(o, null);
                        else
                            targetValue = null;
                    }
                    catch (Exception e)
                    {
                        // Ignore runtime getters
                        if (!e.StackTrace.Contains(".get_"))
                            Debug.LogException(e);
                    }
                }

                Logger.Active.Log("Parsing Target " + target.fieldName + " in (" + o.GetType() + ") as (" + targetType + ")");

                // If there was no data found for this node
                if (!isNode && !isValue) 
                {
                    if (!target.optional && !(target.allowMerge && targetValue != null)) 
                    {
                        // Error - non optional field is missing
                        throw new ParserTargetMissingException ("Missing non-optional field: " + o.GetType () + "." + target.fieldName);
                    }
                    
                    // Nothing to do, so DONT return!
                    return;
                }

                // Does this node have a required config source type (and if so, check if valid)
                RequireConfigType[] attributes = member.GetCustomAttributes (typeof(RequireConfigType), true) as RequireConfigType[];
                if (attributes.Length > 0) 
                {
                    if((attributes[0].type == ConfigType.Node && !isNode) || (attributes[0].type == ConfigType.Value && !isValue))
                    {
                        throw new ParserTargetTypeMismatchException (target.fieldName + " requires config value of " + attributes[0].type);
                    }
                }
                
                // If this object is a value (attempt no merge here)
                if(isValue)
                {
                    // If the target is a string, it works natively
                    if(targetType.Equals(typeof (string)))
                    {
                        targetValue = node.GetValue(target.fieldName);
                    }

                    // Figure out if this object is a parsable type
                    else if((typeof (IParsable)).IsAssignableFrom(targetType))
                    {
                        // Create a new object
                        IParsable targetParsable = (IParsable) Activator.CreateInstance(targetType);
                        targetParsable.SetFromString(node.GetValue(target.fieldName));
                        targetValue = targetParsable;
                    }

                    // Throw exception or print error
                    else
                    {
                        Logger.Active.Log("[Kopernicus]: Configuration.Parser: ParserTarget \"" + target.fieldName + "\" is a non parsable type: " + targetType);
                        return;
                    }
                }
                
                // If this object is a node (potentially merge)
                else if(isNode)
                {
                    // If the target type is a ConfigNode, this works natively
                    if (targetType.Equals(typeof(ConfigNode)))
                    {
                        targetValue = node.GetNode(target.fieldName);
                    }

                    // We need to get an instance of the object we are trying to populate
                    // If we are not allowed to merge, or the object does not exist, make a new instance
                    else if(targetValue == null || !target.allowMerge)
                    {
                        targetValue = CreateObjectFromConfigNode(targetType, node.GetNode(target.fieldName), target.getChild);
                    }
                    
                    // Otherwise we can merge this value
                    else
                    {
                        LoadObjectFromConfigurationNode(targetValue, node.GetNode(target.fieldName), target.getChild);
                    }
                }
                
                // If the member type is a field, set the value
                if(member.MemberType == MemberTypes.Field)
                {
                    (member as FieldInfo).SetValue(o, targetValue);
                }

                // If the member wasn't a field, it must be a property.  If the property is writable, set it.
                else if((member as PropertyInfo).CanWrite)
                {
                    (member as PropertyInfo).SetValue(o, targetValue, null);
                }
            }

            // Loads ParserTargets from other assemblies in GameData/
            public static void LoadExternalParserTargetsRecursive(ConfigNode node)
            {
                LoadExternalParserTargets(node);
                foreach (var childNode in node.GetNodes())
                {
                    LoadExternalParserTargetsRecursive(childNode);
                }
            }

            // Loads ParserTargets from other assemblies in GameData/
            public static void LoadExternalParserTargets(ConfigNode node)
            {
                // Look for types in other assemblies with the ExternalParserTarget attribute and the parentNodeName equal to this node's name
                List<Assembly> assemblies = new List<Assembly>();
                foreach (Type t in Injector.ModTypes)
                    assemblies.AddUnique(t.Assembly);
                foreach (Assembly assembly in assemblies)
                {
                    try
                    {
                        // Only get types implementing IParserEventSubscriber, and extending ExternalParserTargetLoader
                        foreach (Type type in assembly.GetTypes().Where(t => t.GetInterface("IParserEventSubscriber") != null).Where(t => t.BaseType == typeof(ExternalParserTargetLoader)))
                        {
                            ExternalParserTarget[] attributes = type.GetCustomAttributes(typeof(ExternalParserTarget), false) as ExternalParserTarget[];
                            if (attributes.Length != 0)
                            {
                                ExternalParserTarget external = attributes[0];
                                if (node.name != external.parentNodeName)
                                    continue;
                                string nodeName = external.configNodeName;
                                if (nodeName == null)
                                    nodeName = type.Name;
                                if (node.HasNode(nodeName))
                                {
                                    try
                                    {
                                        Logger.Active.Log("Parsing ExternalTarget " + nodeName + " in node " + external.parentNodeName + " from Assembly " + assembly.FullName);
                                        ConfigNode nodeToLoad = node.GetNode(nodeName);
                                        ExternalParserTargetLoader obj = Activator.CreateInstance(type) as ExternalParserTargetLoader;
                                        obj.generatedBody = BaseLoader.generatedBody;
                                        LoadObjectFromConfigurationNode(obj, nodeToLoad);
                                    }
                                    catch (MissingMethodException missingMethod)
                                    {
                                        Logger.Active.Log("Failed to load ExternalParserTarget " + nodeName + " because it does not have a parameterless constructor");
                                        Logger.Active.LogException(missingMethod);
                                    }
                                    catch (Exception exception)
                                    {
                                        Logger.Active.Log("Failed to load ExternalParserTarget " + nodeName + " from node " + external.parentNodeName);
                                        Logger.Active.LogException(exception);
                                    }
                                }
                            }
                        }
                    }
                    catch (ReflectionTypeLoadException e)
                    {

                    }
                }

            }

            // End class
        }
    }
}

