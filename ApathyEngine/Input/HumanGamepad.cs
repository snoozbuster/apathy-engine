using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ApathyEngine.Input
{
    /// <summary>
    /// A class representing gamepad input from a human.
    /// </summary>
    public class HumanGamepad : IGamepad
    {
        private GamePadState stateLastFrame;
        private GamePadState state;
        private readonly PlayerIndex index;

        private Dictionary<Buttons, int> framesHeldFor = new Dictionary<Buttons, int>();

        /// <summary>
        /// Creates a new <pre>HumanGamepad</pre> that looks for input from a given <pre>PlayerIndex</pre>.
        /// </summary>
        /// <param name="index"><pre>PlayerIndex</pre> of the desired gamepad.</param>
        public HumanGamepad(PlayerIndex index)
        {
            this.index = index;
        }

        public GamePadState RawState { get { return state; } }

        public bool IsConnected { get { return state.IsConnected; } }

        public GamePadButtons Buttons { get { return state.Buttons; } }

        public GamePadTriggers Triggers { get { return state.Triggers; } }

        public GamePadDPad DPad { get { return state.DPad; } }

        public GamePadThumbSticks ThumbSticks { get { return state.ThumbSticks; } }

        public Buttons[] GetPressedButtons()
        {
            return state.GetPressedButtons();
        }

        public bool IsButtonDown(Buttons button)
        {
            return state.IsButtonDown(button);
        }

        public bool IsButtonUp(Buttons button)
        {
            return state.IsButtonUp(button);
        }

        public bool WasButtonJustReleased(Buttons button)
        {
            return stateLastFrame.IsButtonDown(button) && IsButtonUp(button);
        }

        public bool WasButtonJustPressed(Buttons button)
        {
            return IsButtonDown(button) && stateLastFrame.IsButtonUp(button);
        }

        public bool IsButtonBeingHeld(Buttons button)
        {
            return IsButtonDown(button) && stateLastFrame.IsButtonDown(button);
        }

        public bool IsButtonBeingHeld(Buttons button, int consecutiveFrames)
        {
            if(consecutiveFrames < 2)
                throw new ArgumentException("consecutiveFrames must be >= 2");

            if(framesHeldFor.ContainsKey(button))
                return framesHeldFor[button] >= consecutiveFrames;

            return false; // key not being held
        }

        public void Update(GameTime gameTime)
        {
            stateLastFrame = state;
            state = GamePad.GetState(index);

            Buttons[] pressedButtons = GetPressedButtons();

            // add new keys, update held keys
            foreach(Buttons button in pressedButtons)
                if(!framesHeldFor.ContainsKey(button))
                    framesHeldFor.Add(button, 1);
                else
                    framesHeldFor[button]++;

            // remove released keys
            foreach(Buttons button in stateLastFrame.GetPressedButtons().Except(pressedButtons))
                framesHeldFor.Remove(button);
        }
    }
}
