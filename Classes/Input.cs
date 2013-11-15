using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaibo_Crawler
{
    public static class Input
    {
        public static MouseManager mouse;
        public static KeyboardManager keyboard;

        static KeyboardState currKeyboard;
        static KeyboardState prevKeyboard;

        static MouseState currMouse;
        static MouseState prevMouse;

        public static void init(Game game)
        {
            mouse = new MouseManager(game);
            keyboard = new KeyboardManager(game);
        }

        public static void update()
        {
            prevKeyboard = currKeyboard;
            currKeyboard = keyboard.GetState();

            prevMouse = currMouse;
            currMouse = mouse.GetState();
        }

        public static bool isClicked(Keys k)
        {
            return currKeyboard.IsKeyDown(k) && prevKeyboard.IsKeyUp(k);
        }

        public static bool isPressed(Keys k)
        {
            return currKeyboard.IsKeyDown(k);
        }

        public static bool isReleased(Keys k)
        {
            return currKeyboard.IsKeyUp(k) && prevKeyboard.IsKeyDown(k);
        }

        public static bool leftClicked()
        {
            return currMouse.Left == ButtonState.Pressed && prevMouse.Left == ButtonState.Released;
        }

        public static bool leftPressed()
        {
            return currMouse.Left == ButtonState.Pressed;
        }

        public static bool leftReleased()
        {
            return currMouse.Left == ButtonState.Released && prevMouse.Left == ButtonState.Pressed;
        }

        public static bool rightClicked()
        {
            return currMouse.Right == ButtonState.Pressed && prevMouse.Right == ButtonState.Released;
        }

        public static bool rightPressed()
        {
            return currMouse.Right == ButtonState.Pressed;
        }

        public static bool rightReleased()
        {
            return currMouse.Right == ButtonState.Released && prevMouse.Right == ButtonState.Pressed;
        }

        public static Vector2 getMousePos()
        {
            return new Vector2(currMouse.X, currMouse.Y);
        }

    }
}
