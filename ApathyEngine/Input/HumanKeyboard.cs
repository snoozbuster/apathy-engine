using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Input
{
    /// <summary>
    /// A class representing keyboard input from a human.
    /// </summary>
    public class HumanKeyboard : IKeyboard
    {
        private KeyboardState keyboardLastFrame;
        private KeyboardState keyboardThisFrame;

        private Dictionary<Keys, int> framesHeldFor = new Dictionary<Keys, int>();

        public KeyboardState RawState { get { return keyboardThisFrame; } }

        public Keys[] GetPressedKeys()
        {
            return keyboardThisFrame.GetPressedKeys();
        }

        public bool IsKeyDown(Keys key)
        {
            return keyboardThisFrame.IsKeyDown(key);
        }

        public bool IsKeyUp(Keys key)
        {
            return keyboardThisFrame.IsKeyUp(key);
        }

        public bool WasKeyJustReleased(Keys key)
        {
            return keyboardLastFrame.IsKeyDown(key) && IsKeyUp(key);
        }

        public bool WasKeyJustPressed(Keys key)
        {
            return keyboardLastFrame.IsKeyDown(key) && IsKeyUp(key);
        }

        public bool IsKeyBeingHeld(Keys key)
        {
            return keyboardLastFrame.IsKeyDown(key) && IsKeyDown(key);
        }

        public bool IsKeyBeingHeld(Keys key, int consecutiveFrames)
        {
            if(consecutiveFrames < 2)
                throw new ArgumentException("consecutiveFrames must be >= 2");

            if(framesHeldFor.ContainsKey(key))
                return framesHeldFor[key] >= consecutiveFrames;

            return false; // key not being held
        }

        public void Update(GameTime gameTime)
        {
            keyboardLastFrame = keyboardThisFrame;
            keyboardThisFrame = Keyboard.GetState();

            Keys[] pressedKeys = GetPressedKeys();

            // add new keys, update held keys
            foreach(Keys key in pressedKeys)
                if(!framesHeldFor.ContainsKey(key))
                    framesHeldFor.Add(key, 1);
                else
                    framesHeldFor[key]++;

            // remove released keys
            foreach(Keys key in keyboardLastFrame.GetPressedKeys().Except(pressedKeys))
                framesHeldFor.Remove(key);
        }
    }
}
