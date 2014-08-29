using ApathyEngine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Input
{
    /// <summary>
    /// An interface-like abstract class for an input device that looks like a mouse.
    /// </summary>
    public abstract class AbstractMouse
    {
        /// <summary>
        /// Gets a MouseState representing the current state of the IMouse.
        /// </summary>
        public abstract MouseState RawState { get; }

        /// <summary>
        /// Specifies the state of the mouse's left mouse button.
        /// </summary>
        public abstract ButtonState LeftButton { get; }

        /// <summary>
        /// Specifies the state of the mouse's middle mouse button.
        /// </summary>
        public abstract ButtonState MiddleButton { get; }

        /// <summary>
        /// Specifies the state of the mouses's right mouse button.
        /// </summary>
        public abstract ButtonState RightButton { get; }

        /// <summary>
        /// Gets the cumulative value of the scroll wheel since the game has started.
        /// </summary>
        public abstract int ScrollWheelValue { get; }

        /// <summary>
        /// Specifies the horizontal position of the cursor.
        /// </summary>
        public abstract int X { get; }
        
        /// <summary>
        /// Specifies the vertical position of the cursor.
        /// </summary>
        public abstract int Y { get; }

        /// <summary>
        /// A vector containing the X position of the mouse in the X coordinate, and the Y position in the Y coordinate.
        /// </summary>
        public virtual Vector2 MousePosition { get { return new Vector2(X, Y); } }

        /// <summary>
        /// A vector containing the distance and direction the mouse moved between last frame and this frame.
        /// </summary>
        public abstract Vector2 MousePositionDelta { get; }

        /// <summary>
        /// Gets the value the scroll wheel has changed this frame.
        /// </summary>
        public abstract int ScrollWheelDelta { get; }

        /// <summary>
        /// Checks to see if the specified mouse button was just clicked.
        /// </summary>
        /// <param name="mouseButton">LMB = 1, RMB = 2, MMB = 3. Defaults to LMB.</param>
        /// <returns>True if the specified mouse button was just clicked.</returns>
        public abstract bool WasMouseJustClicked(int mouseButton = 1);

        /// <summary>
        /// Checks to see if the specified mouse button was just clicked.
        /// </summary>
        /// <param name="mouseButton">LMB = 1, RMB = 2, MMB = 3. Defaults to LMB.</param>
        /// <returns>True if the specified mouse button was just released.</returns>
        public abstract bool WasMouseJustReleased(int mouseButton = 1);

        /// <summary>
        /// Checks to see if the specified mouse button is being held.
        /// </summary>
        /// <param name="mouseButton">LMB = 1, RMB = 2, MMB = 3. Defaults to LMB.</param>
        /// <returns>True if the specified mouse button was held last frame and is still held this frame.</returns>
        public abstract bool IsMouseBeingHeld(int mouseButton = 1);

        /// <summary>
        /// Determines if a given mouse button has been held for the given number of frames.
        /// </summary>
        /// <param name="mouseButton">LMB = 1, RMB = 2, MMB = 3. Defaults to LMB.</param>
        /// <param name="consecutiveFrames">The number of frames to check. A value of 2 defaults to <pre>IsMouseBeingHeld(mouseButton)</pre>,
        /// values below 2 are invalid.</param>
        /// <returns>True if the key has been held for the given number of frames.</returns>
        public abstract bool IsMouseBeingHeld(int mouseButton, int consecutiveFrames);

        /// <summary>
        /// Checks to see if the mouse is within a rectangular set of coordinates.
        /// </summary>
        /// <param name="upperLeft">Upper left coordinate of the rectangular area.</param>
        /// <param name="lowerRight">Lower right coordinate of the rectangular area.</param>
        /// <returns>True if the mouse is within the given coordinates.</returns>
        public virtual bool IsWithinCoordinates(Vector2 upperLeft, Vector2 lowerRight)
        {
            return ((X > upperLeft.X && X < lowerRight.X) &&
                    (Y > upperLeft.Y && Y < lowerRight.Y));
        }

        /// <summary>
        /// Checks to see if the mouse is on top of a <pre>Sprite</pre>. Does not account for transparency.
        /// </summary>
        /// <param name="sprite"><pre>Sprite</pre> to check.</param>
        /// <returns>True if the mouse is between the <pre>Sprite</pre>'s upper left and lower right corners.</returns>
        public virtual bool IsWithinCoordinates(Sprite sprite)
        {
            return IsWithinCoordinates(sprite.UpperLeft, sprite.LowerRight);
        }

        /// <summary>
        /// Checks to see if the mouse is within or exactly on a circle defined by a center and radius.
        /// </summary>
        /// <param name="center">Vector describing the center of the circle.</param>
        /// <param name="radius">Radius of the circle, in pixels.</param>
        /// <returns>True if the mouse is within the circle.</returns>
        public virtual bool IsWithinCoordinates(Vector2 center, int radius)
        {
            return Math.Pow((X - center.X), 2) + Math.Pow((Y - center.Y), 2) <= Math.Pow(radius, 2);
        }

        /// <summary>
        /// Updates the input device.
        /// </summary>
        /// <param name="gameTime">GameTime this frame.</param>
        public abstract void Update(GameTime gameTime);
    }
}
