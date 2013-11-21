using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using SharpDX.Toolkit.Input;

namespace Kaibo_Crawler
{
    class Player
    {

     

        private Camera cam;
        private Map map;
        private Vector3 position;
        private float moveSpeed = 0.5f;
        private bool key;
        private bool isMoving;
        private TimeSpan movingTime;
        private float height;
        private bool won;

        public Camera Cam
        {
            get { return cam; }
        }
        public Map Map
        {
            get { return map; }
            set
            {
                map = value;
                position = value.StartPosition;
                Random r = new Random();
                cam.Yaw = (float)(r.NextDouble() * 2 * Math.PI);
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
        public bool IsMoving
        {
            get { return isMoving; }
        }
        public TimeSpan MovingTime
        {
            get { return movingTime; }
        }
        public float Height
        {
            get { return height; }
            set { height = value; }
        }


        public Player(Vector3 position, GraphicsDevice graphics)
        {
            cam = new Camera(position, graphics);
            height = 0;
            won = false;
        }

        public void update(GameTime gameTime)
        {
            Vector3 moveVector = new Vector3();

            float currentSpeed = (Input.isPressed(Keys.Shift))?moveSpeed * 2:moveSpeed;

            if (Input.isPressed(Keys.W))
                moveVector += (Vector3.UnitZ * -currentSpeed);

            if (Input.isPressed(Keys.S))
                moveVector += (Vector3.UnitZ * currentSpeed);

            if (Input.isPressed(Keys.A))
                moveVector += (Vector3.UnitX * -currentSpeed);

            if (Input.isPressed(Keys.D))
                moveVector += (Vector3.UnitX * currentSpeed);

            if (moveVector != Vector3.Zero)
            {
                isMoving = true;
                movingTime += gameTime.ElapsedGameTime;
                move(moveVector);
            }
            else
            {
                isMoving = false;
                movingTime = new TimeSpan();

                if (Input.leftClicked())
                {
                    map.trigger(this, false);
                }
            }

            cam.update(position);
        }

        private void move(Vector3 direction)
        {
            Matrix rotationY = Matrix.RotationY(cam.Yaw);
            Vector3 movement = Helpers.Transform(direction, ref rotationY);

            if (!map.intersects(position + Vector3.UnitX * movement.X, new Vector2(2, 2)))
            {
                position += Vector3.UnitX * movement.X;
            }
            if (!map.intersects(position + Vector3.UnitZ * movement.Z, new Vector2(2, 2)))
            {
                position += Vector3.UnitZ * movement.Z;
            }

            map.trigger(this, true);

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
