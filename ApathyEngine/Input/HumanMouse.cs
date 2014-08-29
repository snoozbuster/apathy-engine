using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ApathyEngine.Input
{
    public class HumanMouse : AbstractMouse
    {
        private MouseState state;
        private MouseState stateLastFrame;

        private Dictionary<int, int> framesHeldFor = new Dictionary<int, int>(3);

        private Game game;

        /// <summary>
        /// Creates a new object to keep track of input from the real mouse.
        /// </summary>
        /// <param name="game">Game object. Used to keep track of IsActive.</param>
        public HumanMouse(Game game)
        {
            this.game = game;
            for(int i = 1; i <= 3; i++)
                framesHeldFor.Add(i, 0);
        }

        public override MouseState RawState { get { return state; } }

        public override ButtonState LeftButton { get { return state.LeftButton; } }

        public override ButtonState MiddleButton { get { return state.MiddleButton; } }

        public override ButtonState RightButton { get { return state.RightButton; } }

        public override int ScrollWheelValue { get { return state.ScrollWheelValue; } }

        public override int X { get { return state.X; } }

        public override int Y { get { return state.Y; } }

        public override Vector2 MousePositionDelta { get { return MousePosition - new Vector2(stateLastFrame.X, stateLastFrame.Y); } }

        public override int ScrollWheelDelta { get { return state.ScrollWheelValue - stateLastFrame.ScrollWheelValue; } }

        public override bool WasMouseJustClicked(int mouseButton = 1)
        {
            if(!game.IsActive)
                return false;

            switch(mouseButton)
            {
                case 1: return stateLastFrame.LeftButton == ButtonState.Released && LeftButton == ButtonState.Pressed;
                case 2: return stateLastFrame.RightButton == ButtonState.Released && RightButton == ButtonState.Pressed;
                case 3: return stateLastFrame.MiddleButton == ButtonState.Released && MiddleButton == ButtonState.Pressed;
                default: throw new ArgumentException("No such mouse button.");
            }
        }

        public override bool WasMouseJustReleased(int mouseButton = 1)
        {
            if(!game.IsActive)
                return false;

            switch(mouseButton)
            {
                case 1: return LeftButton == ButtonState.Released && stateLastFrame.LeftButton == ButtonState.Pressed;
                case 2: return RightButton == ButtonState.Released && stateLastFrame.RightButton == ButtonState.Pressed;
                case 3: return MiddleButton == ButtonState.Released && stateLastFrame.MiddleButton == ButtonState.Pressed;
                default: throw new ArgumentException("No such mouse button.");
            }
        }

        public override bool IsMouseBeingHeld(int mouseButton = 1)
        {
            if(!game.IsActive)
                return false;

            switch(mouseButton)
            {
                case 1: return LeftButton == ButtonState.Pressed && stateLastFrame.LeftButton == ButtonState.Pressed;
                case 2: return RightButton == ButtonState.Pressed && stateLastFrame.RightButton == ButtonState.Pressed;
                case 3: return MiddleButton == ButtonState.Pressed && stateLastFrame.MiddleButton == ButtonState.Pressed;
                default: throw new ArgumentException("No such mouse button.");
            }
        }

        public override bool IsMouseBeingHeld(int mouseButton, int consecutiveFrames)
        {
            if(consecutiveFrames < 2)
                throw new ArgumentException("consecutiveFrames must be >= 2");

            return framesHeldFor[mouseButton] >= consecutiveFrames;
        }

        public override void Update(GameTime gameTime)
        {
            stateLastFrame = state;
            state = Mouse.GetState();

            if(LeftButton == ButtonState.Pressed)
                framesHeldFor[1]++;
            else
                framesHeldFor[1] = 0;

            if(RightButton == ButtonState.Pressed)
                framesHeldFor[2]++;
            else
                framesHeldFor[2] = 0;

            if(MiddleButton == ButtonState.Pressed)
                framesHeldFor[3]++;
            else
                framesHeldFor[3] = 0;
        }
    }
}
