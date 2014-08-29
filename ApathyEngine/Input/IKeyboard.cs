using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Input
{
    /// <summary>
    /// An interface for an input device that looks like a keyboard.
    /// </summary>
    public interface IKeyboard
    {
        /// <summary>
        /// A property that returns a KeyboardState representing the state of the input device.
        /// </summary>
        KeyboardState RawState { get; }

        /// <summary>
        /// Returns an array of pressed <pre>Keys</pre>.
        /// </summary>
        /// <returns></returns>
        Keys[] GetPressedKeys();

        /// <summary>
        /// Determines if a given key is pressed.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is pressed, otherwise false.</returns>
        bool IsKeyDown(Keys key);

        /// <summary>
        /// Determines if a given key is released.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is not pressed, otherwise false.</returns>
        bool IsKeyUp(Keys key);

        /// <summary>
        /// Determines if a given key was just released this frame.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the given key was pressed last frame and released this frame.</returns>
        bool WasKeyJustReleased(Keys key);

        /// <summary>
        /// Determines if a given key was just pressed this frame.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the given key was released last frame and pressed this frame.</returns>
        bool WasKeyJustPressed(Keys key);

        /// <summary>
        /// Determines if a given key was pressed last frame and is still pressed this frame.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the given key was pressed last frame and pressed this frame.</returns>
        bool IsKeyBeingHeld(Keys key);

        /// <summary>
        /// Determines if a given key has been held for the given number of frames.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <param name="consecutiveFrames">The number of frames to check. A value of 2 defaults to <pre>IsKeyBeingHeld(key)</pre>,
        /// values below 2 are invalid.</param>
        /// <returns>True if the key has been held for the given number of frames.</returns>
        bool IsKeyBeingHeld(Keys key, int consecutiveFrames);

        /// <summary>
        /// Updates the IKeyboard.
        /// </summary>
        /// <param name="gameTime">GameTime this frame.</param>
        void Update(GameTime gameTime);
    }
}
