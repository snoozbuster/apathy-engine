using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ApathyEngine.Graphics;

namespace ApathyEngine.Input
{
    /// <summary>
    /// A class that represents a keyboard that never reads input.
    /// </summary>
    public class NullKeyboard : IKeyboard
    {
        public KeyboardState RawState { get { return new KeyboardState(); } }

        public Keys[] GetPressedKeys() { return new Keys[] { }; }

        public bool IsKeyDown(Keys key) { return false; }

        public bool IsKeyUp(Keys key) { return false; }

        public bool WasKeyJustReleased(Keys key) { return false; }

        public bool WasKeyJustPressed(Keys key) { return false; }

        public bool IsKeyBeingHeld(Keys key) { return false; }

        public bool IsKeyBeingHeld(Keys key, int consecutiveFrames) { return false; }

        public void Update(GameTime gameTime) { }
    }

    /// <summary>
    /// A class that represents a gamepad that never reads input.
    /// </summary>
    public class NullGamepad : IGamepad
    {
        public GamePadState RawState { get { return new GamePadState(); } }

        public bool IsConnected { get { return false; } }

        public Buttons[] GetPressedButtons() { return new Buttons[] { }; }

        public bool IsButtonDown(Buttons button) { return false; }

        public bool IsButtonUp(Buttons button) { return false; }

        public GamePadButtons Buttons { get { return new GamePadButtons(); } }

        public GamePadThumbSticks ThumbSticks { get { return new GamePadThumbSticks(); } }

        public GamePadDPad DPad { get { return new GamePadDPad(); } }

        public GamePadTriggers Triggers { get { return new GamePadTriggers(); } }

        public bool WasButtonJustReleased(Buttons button) { return false; }

        public bool WasButtonJustPressed(Buttons button) { return false; }

        public bool IsButtonBeingHeld(Buttons button) { return false; }

        public bool IsButtonBeingHeld(Buttons button, int consecutiveFrames) { return false; }

        public void Update(GameTime gameTime) { }
    }

    /// <summary>
    /// Represents a mouse that never reads input and never moves from (0, 0).
    /// </summary>
    public class NullMouse : AbstractMouse
    {
        public override MouseState RawState { get { return new MouseState(); } }

        public override ButtonState LeftButton { get { return ButtonState.Released; } }

        public override ButtonState MiddleButton { get { return ButtonState.Released; } }

        public override ButtonState RightButton { get { return ButtonState.Released; } }

        public override int ScrollWheelValue { get { return 0; } }

        public override int X { get { return 0; } }

        public override int Y { get { return 0; } }

        public override Vector2 MousePositionDelta { get { return Vector2.Zero; } }

        public override int ScrollWheelDelta { get { return 0; } }

        public override bool WasMouseJustClicked(int mouseButton = 1) { return false; }

        public override bool WasMouseJustReleased(int mouseButton = 1) { return false; }

        public override bool IsMouseBeingHeld(int mouseButton = 1) { return false; }

        public override bool IsMouseBeingHeld(int mouseButton, int consecutiveFrames) { return false; }

        public override void Update(GameTime gameTime) { }

        public override bool IsWithinCoordinates(Sprite sprite) { return false; }

        public override bool IsWithinCoordinates(Vector2 center, int radius) { return false; }

        public override bool IsWithinCoordinates(Vector2 upperLeft, Vector2 lowerRight) { return false; }

        public override Vector2 MousePosition { get { return Vector2.Zero; } }
    }
}
