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
    class Player
    {
        public Camera cam;
        public Vector3 position;

        float moveSpeed = 0.5f;

        public Player(Vector3 position, GraphicsDevice graphics)
        {
            cam = new Camera(position, graphics);
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

            cam.update(position);
        }

        private void move(Vector3 direction) 
        {
            Matrix rotationY = Matrix.RotationY(cam.yaw);
            position += Helpers.Transform(direction, ref rotationY);
        }

        public Vector3 getLookDirection() 
        {
            return cam.direction;
        }
    }
}
