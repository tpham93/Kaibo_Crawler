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
        Matrix projection;
        Matrix view;

        public Matrix viewProjection;

        Vector3 position;

        float yaw;
        float pitch;

        float rotationSpeed;
        float moveSpeed;

        float oldMouseX;
        float oldMouseY;

        public Camera(Vector3 position, GraphicsDevice graphics)
        {
            this.position = position;

            this.rotationSpeed = 0.5f;
            this.moveSpeed = 0.5f;

            view = Matrix.LookAtRH(
                this.position,   // Position
                new Vector3(0.0f, 20.0f, 0.0f),     // At (point which is centered in the middle of the screen).
                Vector3.Up);     // Up


            projection = Matrix.PerspectiveFovRH(
                0.6f,                               // Field of view
                (float)graphics.BackBuffer.Width / graphics.BackBuffer.Height,  // Aspect ratio
                0.5f,                               // Near clipping plane
                200.0f);                            // Far clipping plane

        }

        public void update()
        {

            if (Input.isPressed(Keys.W))
                move(Vector3.UnitZ * -moveSpeed);

            else if (Input.isPressed(Keys.S))
                move(Vector3.UnitZ * moveSpeed);

            if (Input.isPressed(Keys.A))
                move(Vector3.UnitX * -moveSpeed);

            else if (Input.isPressed(Keys.D))
                move(Vector3.UnitX * moveSpeed);


            pitch = MathUtil .Clamp(pitch, -1.5f, 1.5f);

            Vector2 mousePos = Input.getMousePos();

            float dx = mousePos.X - oldMouseX;
            yaw -= rotationSpeed * dx;

            float dy = mousePos.Y - oldMouseY;
            pitch -= rotationSpeed * dy;

            resetMouse();
            updateMatrices();
        }

        public void updateMatrices() 
        {
            Matrix rotation = Matrix.RotationX(pitch) * Matrix.RotationY(yaw);
            Vector3 transformedRef = Helpers.Transform(new Vector3(0, 0, -1),ref rotation);
            Vector3 lookAt = position + transformedRef;

            view = Matrix.LookAtRH(position, lookAt, Vector3.Up);

            viewProjection = view * projection;
        }

        public void resetMouse()
        {
            Input.mouse.SetPosition(new Vector2(0.5f));

            oldMouseX = 0.5f;
            oldMouseY = 0.5f;
        }

        public void move(Vector3 dir)
        {
            Matrix rotationY = Matrix.RotationY(yaw);
            dir = Helpers.Transform(dir,ref rotationY);

            position += dir;
        }

    }
}
