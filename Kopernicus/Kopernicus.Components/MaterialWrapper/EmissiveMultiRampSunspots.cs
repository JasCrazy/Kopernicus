// Material wrapper generated by shader translator tool
using System;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    namespace MaterialWrapper
    {
        public class EmissiveMultiRampSunspots : Material
        {
            // Internal property ID tracking object
            protected class Properties
            {
                // Return the shader for this wrapper
                private const string shaderName = "Emissive Multi Ramp Sunspots";
                public static Shader shader
                {
                    get { return Shader.Find (shaderName); }
                }

                // Ramp Map (RGBA), default = "white" { }
                public const string rampMapKey = "_RampMap";
                public int rampMapID { get; private set; }

                // Noise Map (RGBA), default = "white" { }
                public const string noiseMapKey = "_NoiseMap";
                public int noiseMapID { get; private set; }

                // Emission Color 0, default = (1,1,1,1)
                public const string emitColor0Key = "_EmitColor0";
                public int emitColor0ID { get; private set; }

                // Emission Color 1, default = (1,1,1,1)
                public const string emitColor1Key = "_EmitColor1";
                public int emitColor1ID { get; private set; }

                // Sunspot Map (R), default = "white" { }
                public const string sunspotTexKey = "_SunspotTex";
                public int sunspotTexID { get; private set; }

                // Sunspot Power, default = 1
                public const string sunspotPowerKey = "_SunspotPower";
                public int sunspotPowerID { get; private set; }

                // Sunspot Color, default = (0,0,0,0)
                public const string sunspotColorKey = "_SunspotColor";
                public int sunspotColorID { get; private set; }

                // Rimlight Color, default = (1,1,1,1)
                public const string rimColorKey = "_RimColor";
                public int rimColorID { get; private set; }

                // Rimlight Power, default = 0.2
                public const string rimPowerKey = "_RimPower";
                public int rimPowerID { get; private set; }

                // Rimlight Blend, default = 0.2
                public const string rimBlendKey = "_RimBlend";
                public int rimBlendID { get; private set; }

                // Singleton instance
                private static Properties singleton = null;
                public static Properties Instance
                {
                    get
                    {
                        // Construct the singleton if it does not exist
                        if(singleton == null)
                            singleton = new Properties();
            
                        return singleton;
                    }
                }

                private Properties()
                {
                    rampMapID = Shader.PropertyToID(rampMapKey);
                    noiseMapID = Shader.PropertyToID(noiseMapKey);
                    emitColor0ID = Shader.PropertyToID(emitColor0Key);
                    emitColor1ID = Shader.PropertyToID(emitColor1Key);
                    sunspotTexID = Shader.PropertyToID(sunspotTexKey);
                    sunspotPowerID = Shader.PropertyToID(sunspotPowerKey);
                    sunspotColorID = Shader.PropertyToID(sunspotColorKey);
                    rimColorID = Shader.PropertyToID(rimColorKey);
                    rimPowerID = Shader.PropertyToID(rimPowerKey);
                    rimBlendID = Shader.PropertyToID(rimBlendKey);
                }
            }

            // Is some random material this material 
            public static bool UsesSameShader(Material m)
            {
                return m.shader.name == Properties.shader.name;
            }

            // Ramp Map (RGBA), default = "white" { }
            public Texture2D rampMap
            {
                get { return GetTexture (Properties.Instance.rampMapID) as Texture2D; }
                set { SetTexture (Properties.Instance.rampMapID, value); }
            }

            public Vector2 rampMapScale
            {
                get { return GetTextureScale (Properties.rampMapKey); }
                set { SetTextureScale (Properties.rampMapKey, value); }
            }

            public Vector2 rampMapOffset
            {
                get { return GetTextureOffset (Properties.rampMapKey); }
                set { SetTextureOffset (Properties.rampMapKey, value); }
            }

            // Noise Map (RGBA), default = "white" { }
            public Texture2D noiseMap
            {
                get { return GetTexture (Properties.Instance.noiseMapID) as Texture2D; }
                set { SetTexture (Properties.Instance.noiseMapID, value); }
            }

            public Vector2 noiseMapScale
            {
                get { return GetTextureScale (Properties.noiseMapKey); }
                set { SetTextureScale (Properties.noiseMapKey, value); }
            }

            public Vector2 noiseMapOffset
            {
                get { return GetTextureOffset (Properties.noiseMapKey); }
                set { SetTextureOffset (Properties.noiseMapKey, value); }
            }

            // Emission Color 0, default = (1,1,1,1)
            public Color emitColor0
            {
                get { return GetColor (Properties.Instance.emitColor0ID); }
                set { SetColor (Properties.Instance.emitColor0ID, value); }
            }

            // Emission Color 1, default = (1,1,1,1)
            public Color emitColor1
            {
                get { return GetColor (Properties.Instance.emitColor1ID); }
                set { SetColor (Properties.Instance.emitColor1ID, value); }
            }

            // Sunspot Map (R), default = "white" { }
            public Texture2D sunspotTex
            {
                get { return GetTexture (Properties.Instance.sunspotTexID) as Texture2D; }
                set { SetTexture (Properties.Instance.sunspotTexID, value); }
            }

            public Vector2 sunspotTexScale
            {
                get { return GetTextureScale (Properties.sunspotTexKey); }
                set { SetTextureScale (Properties.sunspotTexKey, value); }
            }

            public Vector2 sunspotTexOffset
            {
                get { return GetTextureOffset (Properties.sunspotTexKey); }
                set { SetTextureOffset (Properties.sunspotTexKey, value); }
            }

            // Sunspot Power, default = 1
            public float sunspotPower
            {
                get { return GetFloat (Properties.Instance.sunspotPowerID); }
                set { SetFloat (Properties.Instance.sunspotPowerID, value); }
            }

            // Sunspot Color, default = (0,0,0,0)
            public Color sunspotColor
            {
                get { return GetColor (Properties.Instance.sunspotColorID); }
                set { SetColor (Properties.Instance.sunspotColorID, value); }
            }

            // Rimlight Color, default = (1,1,1,1)
            public Color rimColor
            {
                get { return GetColor (Properties.Instance.rimColorID); }
                set { SetColor (Properties.Instance.rimColorID, value); }
            }

            // Rimlight Power, default = 0.2
            public float rimPower
            {
                get { return GetFloat (Properties.Instance.rimPowerID); }
                set { SetFloat (Properties.Instance.rimPowerID, value); }
            }

            // Rimlight Blend, default = 0.2
            public float rimBlend
            {
                get { return GetFloat (Properties.Instance.rimBlendID); }
                set { SetFloat (Properties.Instance.rimBlendID, value); }
            }

            public EmissiveMultiRampSunspots() : base(Properties.shader)
            {
            }

            public EmissiveMultiRampSunspots(string contents) : base(contents)
            {
                base.shader = Properties.shader;
            }

            public EmissiveMultiRampSunspots(Material material) : base(material)
            {
                // Throw exception if this material was not the proper material
                if (material.shader.name != Properties.shader.name)
                    throw new InvalidOperationException("Type Mismatch: Emissive Multi Ramp Sunspots shader required");
            }

        }
    }
}