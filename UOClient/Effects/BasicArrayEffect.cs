using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace UOClient.Effects
{
    public class BasicArrayEffect
    {
        private readonly Effect effect;

        private readonly EffectParameter texture0;
        private readonly EffectParameter texture1;
        private readonly EffectParameter alphaMaskTexture;

        private readonly EffectParameter texture0Stretch;
        private readonly EffectParameter texture1Stretch;
        private readonly EffectParameter alphaMaskStretch;

        private readonly EffectParameter textureIndex;
        private readonly EffectParameter diffuseColorParam;
        private readonly EffectParameter emissiveColorParam;
        private readonly EffectParameter specularColorParam;
        private readonly EffectParameter specularPowerParam;
        private readonly EffectParameter eyePositionParam;
        private readonly EffectParameter fogColorParam;
        private readonly EffectParameter fogVectorParam;
        private readonly EffectParameter worldParam;
        private readonly EffectParameter worldInverseTransposeParam;
        private readonly EffectParameter worldViewProjParam;

        private int _shaderIndex = -1;

        private bool lightingEnabled;
        private bool preferPerPixelLighting;
        private bool oneLight;
        private bool fogEnabled;
        private bool textureEnabled;
        private bool vertexColorEnabled;

        private Matrix world = Matrix.Identity;
        private Matrix view = Matrix.Identity;
        private Matrix projection = Matrix.Identity;

        private Matrix worldView;

        private Vector3 diffuseColor = Vector3.One;
        private Vector3 emissiveColor = Vector3.Zero;
        private Vector3 ambientLightColor = Vector3.Zero;

        private float alpha = 1;
        private float fogStart = 0;
        private float fogEnd = 1;

        private EffectDirtyFlags dirtyFlags = EffectDirtyFlags.All;

        /// <summary>
        /// Gets or sets the world matrix.
        /// </summary>
        public Matrix World
        {
            get => world;
            set
            {
                world = value;
                dirtyFlags |= EffectDirtyFlags.World | EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the view matrix.
        /// </summary>
        public Matrix View
        {
            get => view;
            set
            {
                view = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.EyePosition | EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        public Matrix Projection
        {
            get => projection;
            set
            {
                projection = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj;
            }
        }


        /// <summary>
        /// Gets or sets the material diffuse color (range 0 to 1).
        /// </summary>
        public Vector3 DiffuseColor
        {
            get => diffuseColor;
            set
            {
                diffuseColor = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }


        /// <summary>
        /// Gets or sets the material emissive color (range 0 to 1).
        /// </summary>
        public Vector3 EmissiveColor
        {
            get => emissiveColor;
            set
            {
                emissiveColor = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }

        /// <summary>
        /// Gets or sets the material specular color (range 0 to 1).
        /// </summary>
        public Vector3 SpecularColor
        {
            get => specularColorParam.GetValueVector3();
            set => specularColorParam.SetValue(value);
        }

        /// <summary>
        /// Gets or sets the material specular power.
        /// </summary>
        public float SpecularPower
        {
            get => specularPowerParam.GetValueSingle();
            set => specularPowerParam.SetValue(value);
        }

        /// <summary>
        /// Gets or sets the material alpha.
        /// </summary>
        public float Alpha
        {
            get => alpha;
            set
            {
                alpha = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }

        /// <inheritdoc/>
        public bool LightingEnabled
        {
            get => lightingEnabled;
            set
            {
                if (lightingEnabled != value)
                {
                    lightingEnabled = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex | EffectDirtyFlags.MaterialColor;
                }
            }
        }

        /// <summary>
        /// Gets or sets the per-pixel lighting prefer flag.
        /// </summary>
        public bool PreferPerPixelLighting
        {
            get => preferPerPixelLighting;
            set
            {
                if (preferPerPixelLighting != value)
                {
                    preferPerPixelLighting = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
            }
        }

        public Vector3 AmbientLightColor
        {
            get => ambientLightColor;
            set
            {
                ambientLightColor = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }

        public DirectionalLight DirectionalLight0 { get; }
        public DirectionalLight DirectionalLight1 { get; }
        public DirectionalLight DirectionalLight2 { get; }

        public bool FogEnabled
        {
            get => fogEnabled;
            set
            {
                if (fogEnabled != value)
                {
                    fogEnabled = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex | EffectDirtyFlags.FogEnable;
                }
            }
        }

        public float FogStart
        {
            get => fogStart;
            set
            {
                fogStart = value;
                dirtyFlags |= EffectDirtyFlags.Fog;
            }
        }

        public float FogEnd
        {
            get => fogEnd;
            set
            {
                fogEnd = value;
                dirtyFlags |= EffectDirtyFlags.Fog;
            }
        }

        public Vector3 FogColor
        {
            get => fogColorParam.GetValueVector3();
            set => fogColorParam.SetValue(value);
        }


        /// <summary>
        /// Gets or sets whether texturing is enabled.
        /// </summary>
        public bool TextureEnabled
        {
            get => textureEnabled;
            set
            {
                if (textureEnabled != value)
                {
                    textureEnabled = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether vertex color is enabled.
        /// </summary>
        public bool VertexColorEnabled
        {
            get => vertexColorEnabled;
            set
            {
                if (vertexColorEnabled != value)
                {
                    vertexColorEnabled = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
            }
        }

        public EffectTechnique CurrentTechnique
        {
            get => effect.CurrentTechnique;
            set => effect.CurrentTechnique = value;
        }

        public Texture2D Texture0
        {
            get => texture0.GetValueTexture2D();
            set => texture0.SetValue(value);
        }

        public int TextureIndex
        {
            get => textureIndex.GetValueInt32();
            set => textureIndex.SetValue(value);
        }

        public int TextureStretch
        {
            get => texture0Stretch.GetValueInt32();
            set => texture0Stretch.SetValue(value);
        }

        /// <summary>
        /// Creates a new BasicEffect with default parameter settings.
        /// </summary>
        public BasicArrayEffect(ContentManager contentManager)
        {
            effect = contentManager.Load<Effect>("shaders/basic-array");

            texture0 = effect.Parameters["Texture"];
            diffuseColorParam = effect.Parameters["DiffuseColor"];
            emissiveColorParam = effect.Parameters["EmissiveColor"];
            specularColorParam = effect.Parameters["SpecularColor"];
            specularPowerParam = effect.Parameters["SpecularPower"];
            eyePositionParam = effect.Parameters["EyePosition"];
            fogColorParam = effect.Parameters["FogColor"];
            fogVectorParam = effect.Parameters["FogVector"];
            worldParam = effect.Parameters["World"];
            worldInverseTransposeParam = effect.Parameters["WorldInverseTranspose"];
            worldViewProjParam = effect.Parameters["WorldViewProj"];
            texture0Stretch = effect.Parameters["TextureStretch"];
            textureIndex = effect.Parameters["TextureIndex"];

            DirectionalLight0 = new DirectionalLight(effect.Parameters["DirLight0Direction"],
                                          effect.Parameters["DirLight0DiffuseColor"],
                                          effect.Parameters["DirLight0SpecularColor"],
                                          null);

            DirectionalLight1 = new DirectionalLight(effect.Parameters["DirLight1Direction"],
                                          effect.Parameters["DirLight1DiffuseColor"],
                                          effect.Parameters["DirLight1SpecularColor"],
                                          null);

            DirectionalLight2 = new DirectionalLight(effect.Parameters["DirLight2Direction"],
                                          effect.Parameters["DirLight2DiffuseColor"],
                                          effect.Parameters["DirLight2SpecularColor"],
                                          null);

            DirectionalLight0.Enabled = true;
            SpecularColor = Vector3.One;
            SpecularPower = 16;
        }

        /// <inheritdoc/>
        public void EnableDefaultLighting()
        {
            LightingEnabled = true;

            AmbientLightColor = EffectHelpers.EnableDefaultLighting(DirectionalLight0, DirectionalLight1, DirectionalLight2);
        }

        /// <summary>
        /// Lazily computes derived parameter values immediately before applying the effect.
        /// </summary>
        public void PreDraw()
        {
            // Recompute the world+view+projection matrix or fog vector?
            dirtyFlags = EffectHelpers.SetWorldViewProjAndFog(dirtyFlags, ref world, ref view, ref projection, ref worldView, fogEnabled, fogStart, fogEnd, worldViewProjParam, fogVectorParam);

            // Recompute the diffuse/emissive/alpha material color parameters?
            if ((dirtyFlags & EffectDirtyFlags.MaterialColor) != 0)
            {
                EffectHelpers.SetMaterialColor(lightingEnabled, alpha, ref diffuseColor, ref emissiveColor, ref ambientLightColor, diffuseColorParam, emissiveColorParam);

                dirtyFlags &= ~EffectDirtyFlags.MaterialColor;
            }

            if (lightingEnabled)
            {
                // Recompute the world inverse transpose and eye position?
                dirtyFlags = EffectHelpers.SetLightingMatrices(dirtyFlags, ref world, ref view, worldParam, worldInverseTransposeParam, eyePositionParam);


                // Check if we can use the only-bother-with-the-first-light shader optimization.
                bool newOneLight = !DirectionalLight1.Enabled && !DirectionalLight2.Enabled;

                if (oneLight != newOneLight)
                {
                    oneLight = newOneLight;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
            }

            // Recompute the shader index?
            if ((dirtyFlags & EffectDirtyFlags.ShaderIndex) != 0)
            {
                int shaderIndex = 0;

                if (!fogEnabled)
                    shaderIndex += 1;

                if (vertexColorEnabled)
                    shaderIndex += 2;

                if (textureEnabled)
                    shaderIndex += 4;

                if (lightingEnabled)
                {
                    if (preferPerPixelLighting)
                        shaderIndex += 24;
                    else if (oneLight)
                        shaderIndex += 16;
                    else
                        shaderIndex += 8;
                }

                dirtyFlags &= ~EffectDirtyFlags.ShaderIndex;

                if (_shaderIndex != shaderIndex)
                {
                    _shaderIndex = shaderIndex;
                    effect.CurrentTechnique = effect.Techniques[_shaderIndex];
                }
            }
        }
    }
}