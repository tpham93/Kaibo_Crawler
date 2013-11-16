using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using System;

namespace Kaibo_Crawler
{
    class Helpers
    {
        /// <summary>
        /// Transformation of a Vector3 without projection.
        /// </summary>
        /// <param name="_v">The vector to transform</param>
        /// <param name="_m">The transformation matrix (rotation, scalation, translation)</param>
        /// <returns>The first 3 components of (_v, 1) * _m</returns>
        public static Vector3 Transform(Vector3 _v, ref Matrix _m)
        {
            Vector4 r;
            Vector3.Transform(ref _v, ref _m, out r);
            return new Vector3(r.X, r.Y, r.Z);
        }


        /// <summary>
        /// Computes the transposed inverse matrix without translation part.
        /// This matrix is usefull for normal-space transformations
        /// </summary>
        /// <param name="_m"></param>
        /// <returns></returns>
        public static Matrix CreateInverseTranspose(ref Matrix _m)
        {
            Matrix transformInvTr = _m;
            transformInvTr.TranslationVector = new Vector3(0.0f, 0.0f, 0.0f);
            transformInvTr.Invert();
            transformInvTr.Transpose();
            return transformInvTr;
        }


        public static void drawModel(Model model, GraphicsDevice graphicsDevice, SharpDX.Toolkit.Graphics.Effect effect, Matrix scale, Matrix rotation, Matrix translation, Matrix viewProjection, GameTime gameTime)
        {
            drawModel(model, graphicsDevice, effect, scale * rotation * translation, viewProjection, gameTime);
        }

        public static void drawModel(Model model, GraphicsDevice graphicsDevice, SharpDX.Toolkit.Graphics.Effect effect, Matrix transformation, Matrix viewProjection, GameTime gameTime)
        {
            // Fill the one constant buffer. Hint: it is not necessary to set
            // things which did not change each frame. But in our case everything
            // is changing
            var transformCB = effect.ConstantBuffers["Transforms"];
            transformCB.Parameters["worldViewProj"].SetValue(transformation * viewProjection);
            transformCB.Parameters["world"].SetValue(transformation);
            Matrix worldInvTr = Helpers.CreateInverseTranspose(ref transformation);
            transformCB.Parameters["worldInvTranspose"].SetValue(worldInvTr);

            // Slow rotating light
            double angle = -gameTime.TotalGameTime.TotalMilliseconds / 3000.0;
            transformCB.Parameters["lightPos"].SetValue(new Vector3((float)Math.Sin(angle) * 50.0f, 30.0f, (float)Math.Cos(angle) * 50.0f));

            // Draw model
            effect.CurrentTechnique.Passes[0].Apply();
            model.Draw(graphicsDevice, effect);
        }
    }
}
