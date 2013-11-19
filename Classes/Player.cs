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
        private Camera cam;
        private Map map;
        private Vector3 position;
        private float moveSpeed = 0.5f;
        private bool key;

        public Camera Cam
        {
            get { return cam; }
        }
        public Map Map
        {
            get { return map; }
            set {
                map = value;
                position = value.StartPosition;
            }
        }
        public Vector3 Position
        {
            get { return position; }
        }
        public Vector3 Direction
        {
            get { return cam.Direction; }
        }

        public Player(Vector3 position, GraphicsDevice graphics)
        {
            cam = new Camera(position, graphics);
        }

        public void update()
        {
            Vector3 moveVector = new Vector3();

            if (Input.isPressed(Keys.W))
                moveVector += (Vector3.UnitZ * -moveSpeed);

            if (Input.isPressed(Keys.S))
                moveVector += (Vector3.UnitZ * moveSpeed);

            if (Input.isPressed(Keys.A))
                moveVector += (Vector3.UnitX * -moveSpeed);

            if (Input.isPressed(Keys.D))
                moveVector += (Vector3.UnitX * moveSpeed);

            move(moveVector);

            cam.update(position);
        }

        private void move(Vector3 direction)
        {
            Matrix rotationY = Matrix.RotationY(cam.Yaw);
            Vector3 movement =Helpers.Transform(direction, ref rotationY);

            if (!map.intersects(position + Vector3.UnitX * movement.X, new Vector2(5, 5)))
            {
                position += Vector3.UnitX * movement.X;
            }
            if (!map.intersects(position + Vector3.UnitZ * movement.Z, new Vector2(5, 5)))
            {
                position += Vector3.UnitZ * movement.Z;
            }

        }

        public void addKey()
        {
            key = true;
        }
        public bool hasKey()
        {
            return key;
        }
    }
}
