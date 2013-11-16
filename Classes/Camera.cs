using SharpDX;
using SharpDX.Toolkit.Graphics;
using SharpDX.Toolkit.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaibo_Crawler
{
    class Camera
    {
        private Matrix projection;
        private Matrix view;
        private Matrix viewProjection;

        private Vector3 direction;
        private float yaw;
        private float pitch;

        private float rotationSpeed;

        private float oldMouseX;
        private float oldMouseY;


        public Matrix ViewProjection
        {
            get { return viewProjection; }
        }
        public Vector3 Direction
        {
            get { return direction; }
        }
        public float Yaw
        {
            get { return yaw; }
        }

        public Camera(Vector3 position, GraphicsDevice graphics)
        {

            this.rotationSpeed = 0.5f;

            view = Matrix.LookAtRH(
                position,                                                       // Position
                new Vector3(0.0f, 20.0f, 0.0f),                                 // At (point which is centered in the middle of the screen).
                Vector3.Up);                                                    // Up


            projection = Matrix.PerspectiveFovRH(
                0.6f,                                                           // Field of view
                (float)graphics.BackBuffer.Width / graphics.BackBuffer.Height,  // Aspect ratio
                0.5f,                                                           // Near clipping plane
                200.0f);                                                        // Far clipping plane

        }

        public void update(Vector3 position)
        {

            pitch = MathUtil .Clamp(pitch, -1.5f, 1.5f);

            Vector2 mousePos = Input.getMousePos();

            float dx = mousePos.X - oldMouseX;
            yaw -= rotationSpeed * dx;

            float dy = mousePos.Y - oldMouseY;
            pitch -= rotationSpeed * dy;

            resetMouse();
            updateMatrices(position);
        }

        private void updateMatrices(Vector3 position) 
        {
            Matrix rotation = Matrix.RotationX(pitch) * Matrix.RotationY(yaw);

            direction = Helpers.Transform(-Vector3.UnitZ,ref rotation);

            Vector3 lookAt = position + direction;

            view = Matrix.LookAtRH(position , lookAt, Vector3.Up);

            viewProjection = view * projection;
        }

        private void resetMouse()
        {
            Input.mouse.SetPosition(new Vector2(0.5f));

            oldMouseX = 0.5f;
            oldMouseY = 0.5f;
        }

    }
}
