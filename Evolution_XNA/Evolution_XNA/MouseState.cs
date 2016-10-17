using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Evolution
{
    class Mouse
    {
        public static MouseState GetState()
        {
            var state = Microsoft.Xna.Framework.Input.Mouse.GetState();
            return new MouseState
            {
                LeftButton = state.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed ? ButtonState.Pressed : ButtonState.Released,
                X = state.X,
                Y = state.Y
            };
        }
    }
    class MouseState
    {
        public Point Position { get { return new Point(X, Y); } }
        public int X;
        public int Y;
        public ButtonState LeftButton;
    }
    enum ButtonState
    {
        Pressed,
        Released
    }
}
