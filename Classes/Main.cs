using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using Assimp;
using System;
using SharpDX.DirectInput;

namespace DrawModel
{
    class DrawModel : Game
    {
        private GraphicsDeviceManager m_graphicsDeviceManager;
        private Model m_model;
        private SharpDX.Toolkit.Graphics.Effect m_simpleEffect;
        private Matrix m_viewProjection;

        KeyboardState test;

        RasterizerState m_backfaceCullingState;
        DepthStencilState m_depthStencilStateState;
        SamplerState m_linearSamplerState;
        BlendState m_blendStateOpaque;

        /// <summary>
        /// Entry point create and starts the game
        /// </summary>
        static void Main()
        {
            var program = new DrawModel();
            program.Run();
        }

        DrawModel()
        {
            // Creates a graphics manager. This is mandatory.
            m_graphicsDeviceManager = new GraphicsDeviceManager(this);
            m_graphicsDeviceManager.PreferredDepthStencilFormat = DepthFormat.Depth32; 
            m_graphicsDeviceManager.SetPreferredGraphicsProfile(SharpDX.Direct3D.FeatureLevel.Level_10_1);

            // Setup the relative directory to the executable directory
            // for loading contents with the ContentManager
            Content.RootDirectory = "content";
        }

        protected override void Initialize()
        {
            base.Initialize();

            Window.Title = "Draw Model Demo";

            // Setup Camera
            Matrix view = Matrix.LookAtRH(
                new Vector3(0.0f, 40.0f, -90.0f),   // Position
                new Vector3(0.0f, 20.0f, 0.0f),     // At (point which is centered in the middle of the screen).
                new Vector3(0.0f, 1.0f, 0.0f));     // Up
            Matrix projection = Matrix.PerspectiveFovRH(
                0.6f,                               // Field of view
                (float)GraphicsDevice.BackBuffer.Width / GraphicsDevice.BackBuffer.Height,  // Aspect ratio
                0.5f,                               // Near clipping plane
                200.0f);                            // Far clipping plane
            m_viewProjection = view * projection;

            // Setup states
            var rasterizerStateDesc = SharpDX.Direct3D11.RasterizerStateDescription.Default();
            m_backfaceCullingState = RasterizerState.New(GraphicsDevice, "CullModeBack", rasterizerStateDesc);

            var depthStencilStateDesc = SharpDX.Direct3D11.DepthStencilStateDescription.Default();
            m_depthStencilStateState = DepthStencilState.New(GraphicsDevice, "NormalZBufferUse", depthStencilStateDesc);

            var samplerStateDesc = SharpDX.Direct3D11.SamplerStateDescription.Default();
            samplerStateDesc.AddressV = SharpDX.Direct3D11.TextureAddressMode.Wrap;
            samplerStateDesc.AddressU = SharpDX.Direct3D11.TextureAddressMode.Wrap;
            m_linearSamplerState = SamplerState.New(GraphicsDevice, "LinearSampler", samplerStateDesc);

            var blendStateDesc = SharpDX.Direct3D11.BlendStateDescription.Default();
            m_blendStateOpaque = BlendState.New(GraphicsDevice, "Opaque", blendStateDesc);
        }

        protected override void LoadContent()
        {
            // Importer for many models
            var importer = new AssimpImporter();

            // Load a specific model
            string fileName = System.IO.Path.GetFullPath(Content.RootDirectory + "/tower.3ds");
            Scene scene = importer.ImportFile(fileName, PostProcessSteps.MakeLeftHanded);
            m_model = new Model(scene, GraphicsDevice, Content);

            // Load shader
            EffectCompilerFlags compilerFlags = EffectCompilerFlags.None;
            EffectCompiler compiler = new EffectCompiler();
#if DEBUG
            compilerFlags |= EffectCompilerFlags.Debug;
#endif
            var simpleShaderCompileResult = compiler.CompileFromFile(Content.RootDirectory + "/simple.fx", compilerFlags);
            if (simpleShaderCompileResult.HasErrors)
            {
                System.Console.WriteLine(simpleShaderCompileResult.Logger.Messages);
                System.Diagnostics.Debugger.Break();
            }
            m_simpleEffect = new SharpDX.Toolkit.Graphics.Effect(GraphicsDevice, simpleShaderCompileResult.EffectData);
            m_simpleEffect.Parameters["diffuseSampler"].SetResource(m_linearSamplerState);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clears the screen
            GraphicsDevice.Clear(Color.CornflowerBlue);

           

            GraphicsDevice.SetRasterizerState(m_backfaceCullingState);
            GraphicsDevice.SetDepthStencilState(m_depthStencilStateState);
            GraphicsDevice.SetBlendState(m_blendStateOpaque);

            //UpdateMatrices(gameTime);
            
           
            // Defines the transformation for the next model to be drawn
            Matrix transformation = Matrix.RotationY((float)gameTime.TotalGameTime.TotalMilliseconds / 1000.0f);
            // Draws the model
            drawModel(m_model, GraphicsDevice, m_simpleEffect, transformation, gameTime);

            base.Draw(gameTime);
        }

        private void drawModel(Model model, GraphicsDevice graphicsDevice, Effect effect, Matrix scale, Matrix rotation, Matrix translation, GameTime gameTime)
        {
            drawModel(model, graphicsDevice, effect, scale * rotation * translation, gameTime);
        }

        private void drawModel(Model model, GraphicsDevice graphicsDevice, Effect effect, Matrix transformation, GameTime gameTime)
        {
            // Fill the one constant buffer. Hint: it is not necessary to set
            // things which did not change each frame. But in our case everything
            // is changing
            var transformCB = m_simpleEffect.ConstantBuffers["Transforms"];
            transformCB.Parameters["worldViewProj"].SetValue(transformation * m_viewProjection);
            transformCB.Parameters["world"].SetValue(transformation);
            Matrix worldInvTr = Helpers.CreateInverseTranspose(ref transformation);
            transformCB.Parameters["worldInvTranspose"].SetValue(worldInvTr);

            // Slow rotating light
            double angle = -gameTime.TotalGameTime.TotalMilliseconds / 3000.0;
            transformCB.Parameters["lightPos"].SetValue(new Vector3((float)Math.Sin(angle) * 50.0f, 30.0f, (float)Math.Cos(angle) * 50.0f));

            // Draw model
            m_simpleEffect.CurrentTechnique.Passes[0].Apply();
            m_model.Draw(GraphicsDevice, m_simpleEffect);
        }
        /*
        private void UpdateMatrices(GameTime gameTime)
        {
            // New world matrix
            Matrix world = Matrix.RotationY((float)gameTime.TotalGameTime.TotalMilliseconds / 1000.0f);
            world *= Matrix.Scaling(12, 12, 12);

            var transformCB = m_simpleEffect.ConstantBuffers["Transforms"];
            transformCB.Parameters["worldViewProj"].SetValue(world * m_viewProjection);
            transformCB.Parameters["world"].SetValue(world);
            Matrix worldInvTr = Helpers.CreateInverseTranspose(ref world);
            transformCB.Parameters["worldInvTranspose"].SetValue(worldInvTr);

            // Slow rotating light
            double angle = -gameTime.TotalGameTime.TotalMilliseconds / 3000.0;
            transformCB.Parameters["lightPos"].SetValue(new Vector3((float)Math.Sin(angle) * 50.0f, 30.0f, (float)Math.Cos(angle) * 50.0f));
        }*/
    }
}
