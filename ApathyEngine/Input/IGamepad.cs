using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Input
{
    /// <summary>
    /// An interface for an input device that looks like a Gamepad.
    /// </summary>
    public interface IGamepad
    {
        /// <summary>
        /// A property that returns a GamePadState representing the state of the input device.
        /// </summary>
        GamePadState RawState { get; }

        /// <summary>
        /// Indicates if the gamepad is currently connected or not.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Returns an array of pressed <pre>Buttons</pre>.
        /// </summary>
        /// <returns></returns>
        Buttons[] GetPressedButtons();

        /// <summary>
        /// Determines if a given button is pressed.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button is pressed, otherwise false.</returns>
        bool IsButtonDown(Buttons button);

        /// <summary>
        /// Determines if a given button is released.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button is not pressed, otherwise false.</returns>
        bool IsButtonUp(Buttons button);

        /// <summary>
        /// Returns a struct of the button states of the gamepad.
        /// </summary>
        GamePadButtons Buttons { get; }

        /// <summary>
        /// Returns a struct of the thumbstick states of the gamepad.
        /// </summary>
        GamePadThumbSticks ThumbSticks { get; }

        /// <summary>
        /// Returns a struct of the Dpad states of the gamepad.
        /// </summary>
        GamePadDPad DPad { get; }

        /// <summary>
        /// Returns a struct of the trigger states of the gamepad.
        /// </summary>
        GamePadTriggers Triggers { get; }

        /// <summary>
        /// Determines if a given button was just released this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the given button was pressed last frame and released this frame.</returns>
        bool WasButtonJustReleased(Buttons button);

        /// <summary>
        /// Determines if a given button was just pressed this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the given button was released last frame and pressed this frame.</returns>
        bool WasButtonJustPressed(Buttons button);

        /// <summary>
        /// Determines if a given button was pressed last frame and is still pressed this frame.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the given button was pressed last frame and pressed this frame.</returns>
        bool IsButtonBeingHeld(Buttons button);

        /// <summary>
        /// Determines if a given button has been held for the given number of frames.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <param name="consecutiveFrames">The number of frames to check. A value of 2 defaults to <pre>IsbuttonBeingHeld(button)</pre>,
        /// values below 2 are invalid.</param>
        /// <returns>True if the button has been held for the given number of frames.</returns>
        bool IsButtonBeingHeld(Buttons button, int consecutiveFrames);

        /// <summary>
        /// Updates the IGamepad.
        /// </summary>
        /// <param name="gameTime">GameTime this frame.</param>
        void Update(GameTime gameTime);
    }
}
