using System;
using System.Text;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using Assimp;
using System;
using SharpDX.DirectInput;
using SharpDX.Toolkit.Input;

namespace Kaibo_Crawler
{
    // Use these namespaces here to override SharpDX.Direct3D11


    /// <summary>
    /// Simple Kaibo_Crawler game using SharpDX.Toolkit.
    /// </summary>
    public class Kaibo_Crawler : Game
    {

        private GraphicsDeviceManager m_graphicsDeviceManager;
        private SharpDX.Toolkit.Graphics.Effect m_simpleEffect;

        RasterizerState m_backfaceCullingState;
        DepthStencilState m_depthStencilStateState;
        DepthStencilState m_noDepthStencil;
        SamplerState m_linearSamplerState;
        BlendState m_blendStateOpaque;
        BlendState m_alphaBlendState;

        Player player;

        Map map;

        SpriteBatch spritebatch;

        Texture2D gameOver;

        Texture2D compass;
        Texture2D compassNeedle;

        int width = 800;
        int height = 600;
        /// <summary>
        /// Initializes a new instance of the <see cref="Kaibo_Crawler" /> class.
        /// </summary>
        public Kaibo_Crawler()
        {
            // Creates a graphics manager. This is mandatory.
            m_graphicsDeviceManager = new GraphicsDeviceManager(this);
            m_graphicsDeviceManager.PreferredDepthStencilFormat = DepthFormat.Depth32;
            m_graphicsDeviceManager.SetPreferredGraphicsProfile(SharpDX.Direct3D.FeatureLevel.Level_10_1);

            // Setup the relative directory to the executable directory
            // for loading contents with the ContentManager
            Content.RootDirectory = "content";
            //m_graphicsDeviceManager.IsFullScreen = true;

            m_graphicsDeviceManager.PreferredBackBufferWidth = width;
            m_graphicsDeviceManager.PreferredBackBufferHeight = height;

        }

        protected override void Initialize()
        {
            base.Initialize();

            

            Window.Title = "Kaibo Crawler";

            // Setup states
            var rasterizerStateDesc = SharpDX.Direct3D11.RasterizerStateDescription.Default();
            m_backfaceCullingState = RasterizerState.New(GraphicsDevice, "CullModeBack", rasterizerStateDesc);

            var depthStencilStateDesc = SharpDX.Direct3D11.DepthStencilStateDescription.Default();
            m_depthStencilStateState = DepthStencilState.New(GraphicsDevice, "NormalZBufferUse", depthStencilStateDesc);

            depthStencilStateDesc = SharpDX.Direct3D11.DepthStencilStateDescription.Default();
            depthStencilStateDesc.IsDepthEnabled = false;
           

            m_noDepthStencil = DepthStencilState.New(GraphicsDevice, "NoZBufferUse", depthStencilStateDesc);


            var samplerStateDesc = SharpDX.Direct3D11.SamplerStateDescription.Default();
            samplerStateDesc.AddressV = SharpDX.Direct3D11.TextureAddressMode.Wrap;
            samplerStateDesc.AddressU = SharpDX.Direct3D11.TextureAddressMode.Wrap;
            samplerStateDesc.Filter = SharpDX.Direct3D11.Filter.MinMagMipPoint;
            m_linearSamplerState = SamplerState.New(GraphicsDevice, "LinearSampler", samplerStateDesc);


            var blendStateDesc = SharpDX.Direct3D11.BlendStateDescription.Default();
            m_blendStateOpaque = BlendState.New(GraphicsDevice, "Opaque", blendStateDesc);


            var blendStateDesc1 = SharpDX.Direct3D11.BlendStateDescription.Default();
            blendStateDesc1.IndependentBlendEnable = false;
            blendStateDesc1.AlphaToCoverageEnable = false;


            m_alphaBlendState = BlendState.New(GraphicsDevice,  SharpDX.Direct3D11.BlendOption.SourceAlpha,         //sourceBlend
                                                                SharpDX.Direct3D11.BlendOption.InverseSourceAlpha,         //destinationBlend
                                                                SharpDX.Direct3D11.BlendOperation.Add,              //blendoperation
                                                                SharpDX.Direct3D11.BlendOption.SourceAlpha,    //source alphaBlend
                                                                SharpDX.Direct3D11.BlendOption.InverseSourceAlpha,         //destination alpha blend
                                                                SharpDX.Direct3D11.BlendOperation.Add,              //alphablend operation
                                                                SharpDX.Direct3D11.ColorWriteMaskFlags.All,       //rendertarget mask
                                                                -1);                                                //mask
            

           

                
            Input.init(this);
        }

        protected override void LoadContent()
        {
            // Importer for many models
            var importer = new AssimpImporter();

            // Load a specific model
            //string fileName = System.IO.Path.GetFullPath(Content.RootDirectory + "/tower.3ds");
            //Scene scene = importer.ImportFile(fileName, PostProcessSteps.MakeLeftHanded);
            //m_model = new Model(scene, GraphicsDevice, Content);

            // Load shader 
            EffectCompilerFlags compilerFlags = EffectCompilerFlags.None;
            EffectCompiler compiler = new EffectCompiler();
#if DEBUG
            compilerFlags |= EffectCompilerFlags.Debug;
#endif
            var simpleShaderCompileResult = compiler.CompileFromFile(Content.RootDirectory + "/pointlight.fx", compilerFlags);
            if (simpleShaderCompileResult.HasErrors)
            {
                System.Console.WriteLine(simpleShaderCompileResult.Logger.Messages);
                System.Diagnostics.Debugger.Break();
            }
            m_simpleEffect = new SharpDX.Toolkit.Graphics.Effect(GraphicsDevice, simpleShaderCompileResult.EffectData);
            m_simpleEffect.Parameters["diffuseSampler"].SetResource(m_linearSamplerState);
            base.LoadContent();

            map = new Map(@"Content\map.PNG", new Size2(20, 20));
            map.LoadContent(GraphicsDevice, Content);

            player = new Player(new Vector3(0.0f, 10.0f, 0.0f), GraphicsDevice);
            player.Map = map;

            spritebatch = new SpriteBatch(GraphicsDevice, 2048);
   
          

            compass = Content.Load<Texture2D>("compass.png");
            
            compassNeedle = Content.Load<Texture2D>("needle.png");

            gameOver = Content.Load<Texture2D>("gameover.png");

        }

        protected override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                Input.update();

                player.update(gameTime);

                if (Input.isClicked(Keys.Escape))
                    Exit();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clears the screen
            GraphicsDevice.Clear(Color.Black);



            GraphicsDevice.SetRasterizerState(m_backfaceCullingState);
            GraphicsDevice.SetDepthStencilState(m_depthStencilStateState);
            GraphicsDevice.SetBlendState(m_blendStateOpaque);


            // Defines the transformation for the next model to be drawn
            //Matrix transformation = Matrix.RotationY((float)gameTime.TotalGameTime.TotalMilliseconds / 1000.0f);
            // Draws the model
            //Helpers.drawModel(m_model, GraphicsDevice, m_simpleEffect, transformation, player.Cam.ViewProjection, gameTime);
            map.Draw(player, GraphicsDevice, m_simpleEffect, gameTime);

            GraphicsDevice.SetBlendState(m_alphaBlendState);
            spritebatch.Begin(SpriteSortMode.Deferred, m_alphaBlendState, m_linearSamplerState, m_noDepthStencil, m_backfaceCullingState, null, Matrix.Identity);

                if (player.Won)
                    spritebatch.Draw(gameOver, Vector2.Zero, Color.White);

                if (player.IsMapOpen)
                {
                    spritebatch.Draw(map.Minimap, new Rectangle(0, 0, 800, 600), new Rectangle(0, 0, map.Minimap.Width, map.Minimap.Height), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spritebatch.Draw(compass, new Vector2(width - compassNeedle.Width / 2, compassNeedle.Height / 2), new Rectangle(0, 0, 128, 128), Color.White, 0, new Vector2(64, 64), 1.0f, SpriteEffects.None, 0);
                    spritebatch.Draw(compassNeedle, new Vector2(width - compassNeedle.Width / 2, compassNeedle.Height / 2), new Rectangle(0, 0, 128, 128), Color.White, player.Cam.Yaw, new Vector2(64,64), 1.0f, SpriteEffects.None, 0);

                  
                }

            spritebatch.End();

          

            base.Draw(gameTime);
        }
    }
}
