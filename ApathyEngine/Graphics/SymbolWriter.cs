using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ApathyEngine.Graphics
{
    /// <summary>
    /// A class that can write Xbox and keyboard icons from a texture map of them.
    /// </summary>
    public static class SymbolWriter
    {
        /// <summary>
        /// Gets the local center of the icons.
        /// </summary>
        public static readonly Vector2 IconCenter;

        private static Texture2D keyboardTex;
        private static Texture2D xboxTex;
        private static readonly Rectangle iconSize;
        private static SpriteBatch spriteBatch;

        private static int alpha = 255;

        private enum XboxButtons
        {
            A = 0,
            B,
            X,
            Y,
            LThumb,
            RThumb,
            LeftTrigger,
            RightTrigger,
            LeftBumper,
            RightBumper,
            DpadUp,
            DpadDown,
            DpadLeft,
            DpadRight,
            DpadNeutral,
            Back,
            Start,
            LeftClick,
            RightClick,
            None
        }

        private enum KeyboardButtons1
        {
            Hyphen = 0,
            Tilde,
            Plus,
            Equals,
            Zero,
            One,
            Two,
            Three,
            Four,
            Five,
            Six,
            Seven,
            Eight,
            Nine,
            A,
            B,
            C,
            D,
            E,
            F,
            G,
            H,
            KeyNotSupported
        }

        private enum KeyboardButtons2
        {
            I = 0,
            J,
            K,
            L,
            M,
            N,
            O,
            P,
            Q,
            R,
            S,
            T,
            U,
            V,
            W,
            X,
            Y,
            Z,
            Asterisk,
            Apostrophe,
            Semicolon,
            Period,
            KeyNotSupported
        }
        private enum KeyboardButtons3
        {
            Comma = 0,
            ForwardSlash,
            BackwardSlash,
            QuestionMark,
            LeftArrow,
            RightArrow,
            UpArrow,
            DownArrow,
            LeftBracket,
            RightBracket,
            Space,
            PageUp,
            PageDown,
            Insert,
            Home,
            Esc,
            Enter,
            End,
            Delete,
            Backspace,
            Tab,
            F1,
            KeyNotSupported
        }

        private enum KeyboardButtons4
        {
            F2 = 0,
            F3,
            F4,
            F5,
            F6,
            F7,
            F8,
            F9,
            F10,
            F11,
            F12,
            LeftShift,
            RightShift,
            LeftControl,
            RightControl,
            LeftAlt,
            RightAlt,
            Numpad0,
            Numpad1,
            Numpad2,
            Numpad3,
            KeyNotSupported
        }

        private enum KeyboardButtons5
        {
            Numpad4 = 0,
            Numpad5,
            Numpad6,
            Numpad7,
            Numpad8,
            Numpad9,
            KeyNotSupported,
            None
        }

        /// <summary>
        /// Writes a keyboard symbol. Call SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.LinearClamp, null, null) first and be sure to call 
        /// SpriteBatch.End() after! Throws a KeyNotFoundException you're expected to catch if the 
        /// key pressed isn't supported.
        /// </summary>
        /// <param name="key">The key to write.</param>
        /// <param name="pos">The position to place it. This is relative to the center.</param>
        /// <param name="scaleDown">This is for when smaller text is needed. It scales down to
        /// a 40x40 icon.</param>
        // This is one of the worstly-coded things I've ever written, just because I'm so
        // freaking lazy. It's a good thing I'm not writing an engine.
        public static void WriteKeyboardIcon(Keys key, Vector2 pos, bool scaleDown)
        {
            WriteKeyboardIcon(key, pos, IconCenter, scaleDown);
        }

        public static void WriteKeyboardIcon(Keys key, Vector2 pos, int a)
        {
            alpha = a;
            WriteKeyboardIcon(key, pos, true);
            alpha = 255;
        }

        /// <summary>
        /// Writes a keyboard symbol. Call SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.LinearClamp, null, null) first and be sure to call 
        /// SpriteBatch.End() after! Throws a KeyNotFoundException you're expected to catch if the 
        /// key pressed isn't supported.
        /// </summary>
        /// <param name="key">The key to write.</param>
        /// <param name="pos">The position to place it. This is relative to the center.</param>
        /// <param name="scaleDown">This is for when smaller text is needed. It scales down to
        /// a 40x40 icon.</param>
        /// <param name="origin">The relative origin to use.</param>
        // This is one of the worstly-coded things I've ever written, just because I'm so
        // freaking lazy. It's a good thing I'm not writing an engine.
        public static void WriteKeyboardIcon(Keys key, Vector2 pos, Vector2 origin, bool scaleDown)
        {
            KeyboardButtons1 but = ConvertToKeyboard(key);
            KeyboardButtons2 but2 = ConvertToKeyboard(ref key);
            KeyboardButtons3 but3 = ConvertToKeyboard3(key);
            KeyboardButtons4 but4 = ConvertToKeyboard4(key);
            KeyboardButtons5 but5 = ConvertToKeyboard5(key);
            Color col = new Color(255, 255, 255, alpha) * (alpha / 255f);
            if(but != KeyboardButtons1.KeyNotSupported)
            {
                var currentButton = but;
                Rectangle temp = new Rectangle(iconSize.Width * (int)currentButton, 0, iconSize.Width, iconSize.Height);
                if(!scaleDown)
                    spriteBatch.Draw(keyboardTex, pos, temp, col, 0.0f, origin, 0.83f * RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
                else if(scaleDown)
                    spriteBatch.Draw(keyboardTex, pos, temp, col, 0.0f, origin, 0.444f * RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
            }
            else if(but2 != KeyboardButtons2.KeyNotSupported)
            {
                var currentButton = but2;
                Rectangle temp = new Rectangle(iconSize.Width * (int)currentButton, iconSize.Height, iconSize.Width, iconSize.Height);
                if(!scaleDown)
                    spriteBatch.Draw(keyboardTex, pos, temp, col, 0.0f, origin, 0.83f * RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
                else if(scaleDown)
                    spriteBatch.Draw(keyboardTex, pos, temp, col, 0.0f, origin, 0.444f * RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
            }
            else if(but3 != KeyboardButtons3.KeyNotSupported)
            {
                var currentButton = but3;
                Rectangle temp = new Rectangle(iconSize.Width * (int)currentButton, iconSize.Height * 2, iconSize.Width, iconSize.Height);
                if(!scaleDown)
                    spriteBatch.Draw(keyboardTex, pos, temp, col, 0.0f, origin, 0.83f * RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
                else if(scaleDown)
                    spriteBatch.Draw(keyboardTex, pos, temp, col, 0.0f, origin, 0.444f * RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
            }
            else if(but4 != KeyboardButtons4.KeyNotSupported)
            {
                var currentButton = but4;
                Rectangle temp = new Rectangle(iconSize.Width * (int)currentButton, iconSize.Height * 3, iconSize.Width, iconSize.Height);
                if(!scaleDown)
                    spriteBatch.Draw(keyboardTex, pos, temp, col, 0.0f, origin, 0.83f * RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
                else if(scaleDown)
                    spriteBatch.Draw(keyboardTex, pos, temp, col, 0.0f, origin, 0.444f * RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
            }
            else if(but5 != KeyboardButtons5.KeyNotSupported)
            {
                var currentButton = but5;
                Rectangle temp = new Rectangle(iconSize.Width * (int)currentButton, iconSize.Height * 4, iconSize.Width, iconSize.Height);
                if(!scaleDown)
                    spriteBatch.Draw(keyboardTex, pos, temp, col, 0.0f, origin, 0.83f * RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
                else if(scaleDown)
                    spriteBatch.Draw(keyboardTex, pos, temp, col, 0.0f, origin, 0.444f * RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
            }
            else if(but == KeyboardButtons1.KeyNotSupported && but2 == KeyboardButtons2.KeyNotSupported && but3 == KeyboardButtons3.KeyNotSupported
                && but4 == KeyboardButtons4.KeyNotSupported && but5 == KeyboardButtons5.KeyNotSupported)
                throw new KeyNotFoundException("This key is not supported as valid input for this game.");
        }

        #region Keyboard Conversion
        /// <summary>
        /// This function takes a Microsoft.Xna.Framework.InputManager.Keys and converts it to a
        /// KeyboardButton used by the SymbolWriter.
        /// </summary>
        /// <param name="key">The key to convert.</param>
        /// <returns>Returns the KeyboardButton equivelant to the given Keys. Note that an 
        /// invalid value will throw an error and needs to be handled.</returns>
        private static KeyboardButtons1 ConvertToKeyboard(Keys key)
        {
            switch(key)
            {
                case Keys.OemMinus:
                case Keys.Subtract: return KeyboardButtons1.Hyphen;
                case Keys.OemTilde: return KeyboardButtons1.Tilde;
                case Keys.Add: return KeyboardButtons1.Plus;
                case Keys.OemPlus: return KeyboardButtons1.Equals;
                case Keys.D0: return KeyboardButtons1.Zero;
                case Keys.D1: return KeyboardButtons1.One;
                case Keys.D2: return KeyboardButtons1.Two;
                case Keys.D3: return KeyboardButtons1.Three;
                case Keys.D4: return KeyboardButtons1.Four;
                case Keys.D5: return KeyboardButtons1.Five;
                case Keys.D6: return KeyboardButtons1.Six;
                case Keys.D7: return KeyboardButtons1.Seven;
                case Keys.D8: return KeyboardButtons1.Eight;
                case Keys.D9: return KeyboardButtons1.Nine;
                case Keys.A: return KeyboardButtons1.A;
                case Keys.B: return KeyboardButtons1.B;
                case Keys.C: return KeyboardButtons1.C;
                case Keys.D: return KeyboardButtons1.D;
                case Keys.E: return KeyboardButtons1.E;
                case Keys.F: return KeyboardButtons1.F;
                case Keys.G: return KeyboardButtons1.G;
                case Keys.H: return KeyboardButtons1.H;
                default: return KeyboardButtons1.KeyNotSupported;
            }
        }

        /// <summary>
        /// This function takes a Microsoft.Xna.Framework.InputManager.Keys and converts it to a
        /// KeyboardButton used by the SymbolWriter.
        /// </summary>
        /// <param name="key">The key to convert.</param>
        /// <returns>Returns the KeyboardButton equivelant to the given Keys. Note that an 
        /// invalid value will throw an error that should be handled.</returns>
        private static KeyboardButtons2 ConvertToKeyboard(ref Keys key)
        {
            switch(key)
            {
                case Keys.I: return KeyboardButtons2.I;
                case Keys.J: return KeyboardButtons2.J;
                case Keys.K: return KeyboardButtons2.K;
                case Keys.L: return KeyboardButtons2.L;
                case Keys.M: return KeyboardButtons2.M;
                case Keys.N: return KeyboardButtons2.N;
                case Keys.O: return KeyboardButtons2.O;
                case Keys.P: return KeyboardButtons2.P;
                case Keys.Q: return KeyboardButtons2.Q;
                case Keys.R: return KeyboardButtons2.R;
                case Keys.S: return KeyboardButtons2.S;
                case Keys.T: return KeyboardButtons2.T;
                case Keys.U: return KeyboardButtons2.U;
                case Keys.V: return KeyboardButtons2.V;
                case Keys.W: return KeyboardButtons2.W;
                case Keys.X: return KeyboardButtons2.X;
                case Keys.Y: return KeyboardButtons2.Y;
                case Keys.Z: return KeyboardButtons2.Z;
                case Keys.Multiply: return KeyboardButtons2.Asterisk;
                case Keys.OemQuotes: return KeyboardButtons2.Apostrophe;
                case Keys.OemSemicolon: return KeyboardButtons2.Semicolon;
                case Keys.OemPeriod: return KeyboardButtons2.Period;
                default: return KeyboardButtons2.KeyNotSupported;
            }
        }

        /// <summary>
        /// This function takes a Microsoft.Xna.Framework.InputManager.Keys and converts it to a
        /// KeyboardButton used by the SymbolWriter.
        /// </summary>
        /// <param name="key">The key to convert.</param>
        /// <returns>Returns the KeyboardButton equivelant to the given Keys. Note that an 
        /// invalid value will throw an error that should be handled.</returns>
        private static KeyboardButtons3 ConvertToKeyboard3(Keys key)
        {
            switch(key)
            {
                case Keys.OemComma: return KeyboardButtons3.Comma;
                case Keys.OemQuestion: return KeyboardButtons3.ForwardSlash;
                case Keys.OemPipe: return KeyboardButtons3.BackwardSlash;
                case Keys.Left: return KeyboardButtons3.LeftArrow;
                case Keys.Right: return KeyboardButtons3.RightArrow;
                case Keys.Up: return KeyboardButtons3.UpArrow;
                case Keys.Down: return KeyboardButtons3.DownArrow;
                case Keys.OemOpenBrackets: return KeyboardButtons3.LeftBracket;
                case Keys.OemCloseBrackets: return KeyboardButtons3.RightBracket;
                case Keys.Space: return KeyboardButtons3.Space;
                case Keys.PageUp: return KeyboardButtons3.PageUp;
                case Keys.PageDown: return KeyboardButtons3.PageDown;
                case Keys.Insert: return KeyboardButtons3.Insert;
                case Keys.Home: return KeyboardButtons3.Home;
                case Keys.Escape: return KeyboardButtons3.Esc;
                case Keys.Enter: return KeyboardButtons3.Enter;
                case Keys.End: return KeyboardButtons3.End;
                case Keys.Delete: return KeyboardButtons3.Delete;
                case Keys.Back: return KeyboardButtons3.Backspace;
                case Keys.Tab: return KeyboardButtons3.Tab;
                case Keys.F1: return KeyboardButtons3.F1;
                default: return KeyboardButtons3.KeyNotSupported;
            }
        }

        /// <summary>
        /// This function takes a Microsoft.Xna.Framework.InputManager.Keys and converts it to a
        /// KeyboardButton used by the SymbolWriter.
        /// </summary>
        /// <param name="key">The key to convert.</param>
        /// <returns>Returns the KeyboardButton equivelant to the given Keys. Note that an 
        /// invalid value will throw an error that should be handled.</returns>
        private static KeyboardButtons4 ConvertToKeyboard4(Keys key)
        {
            switch(key)
            {
                case Keys.F2: return KeyboardButtons4.F2;
                case Keys.F3: return KeyboardButtons4.F3;
                case Keys.F4: return KeyboardButtons4.F4;
                case Keys.F5: return KeyboardButtons4.F5;
                case Keys.F6: return KeyboardButtons4.F6;
                case Keys.F7: return KeyboardButtons4.F7;
                case Keys.F8: return KeyboardButtons4.F8;
                case Keys.F9: return KeyboardButtons4.F9;
                case Keys.F10: return KeyboardButtons4.F10;
                case Keys.F11: return KeyboardButtons4.F11;
                case Keys.F12: return KeyboardButtons4.F12;
                case Keys.LeftShift: return KeyboardButtons4.LeftShift;
                case Keys.RightShift: return KeyboardButtons4.RightShift;
                case Keys.LeftControl: return KeyboardButtons4.LeftControl;
                case Keys.RightControl: return KeyboardButtons4.RightControl;
                case Keys.LeftAlt: return KeyboardButtons4.LeftAlt;
                case Keys.RightAlt: return KeyboardButtons4.RightAlt;
                case Keys.NumPad0: return KeyboardButtons4.Numpad0;
                case Keys.NumPad1: return KeyboardButtons4.Numpad1;
                case Keys.NumPad2: return KeyboardButtons4.Numpad2;
                case Keys.NumPad3: return KeyboardButtons4.Numpad3;
                default: return KeyboardButtons4.KeyNotSupported;
            }
        }

        /// <summary>
        /// This function takes a Microsoft.Xna.Framework.InputManager.Keys and converts it to a
        /// KeyboardButton used by the SymbolWriter.
        /// </summary>
        /// <param name="key">The key to convert.</param>
        /// <returns>Returns the KeyboardButton equivelant to the given Keys. Note that an 
        /// invalid value will throw an error that should be handled.</returns>
        private static KeyboardButtons5 ConvertToKeyboard5(Keys key)
        {
            switch(key)
            {
                case Keys.NumPad4: return KeyboardButtons5.Numpad4;
                case Keys.NumPad5: return KeyboardButtons5.Numpad5;
                case Keys.NumPad6: return KeyboardButtons5.Numpad6;
                case Keys.NumPad7: return KeyboardButtons5.Numpad7;
                case Keys.NumPad8: return KeyboardButtons5.Numpad8;
                case Keys.NumPad9: return KeyboardButtons5.Numpad9;
                default: return KeyboardButtons5.KeyNotSupported;
            }

        }
        #endregion

        /// <summary>
        /// Writes an Xbox symbol. Call SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.LinearClamp, null, null) first and be sure to call 
        /// SpriteBatch.End() after!
        /// </summary>
        /// <param name="button">The button to write.</param>
        /// <param name="pos">The position to place it. This is relative to the upper left.</param>
        /// <param name="scaleDown">This is for when smaller text is needed. It scales down to
        /// 40x40 icon.</param>
        public static void WriteXboxIcon(Buttons button, Vector2 pos, bool scaleDown)
        {
            WriteXboxIcon(button, pos, IconCenter, scaleDown);
        }

        public static void WriteXboxIcon(Buttons button, Vector2 pos, int a)
        {
            alpha = a;
            WriteXboxIcon(button, pos, true);
            alpha = 255;
        }

        /// <summary>
        /// Writes an Xbox symbol. Call SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.LinearClamp, null, null) first and be sure to call 
        /// SpriteBatch.End() after!
        /// </summary>
        /// <param name="button">The button to write.</param>
        /// <param name="pos">The position to place it. This is relative to the upper left.</param>
        /// <param name="scaleDown">This is for when smaller text is needed. It scales down to
        /// 40x40 icon.</param>
        /// <param name="origin">The relative origin to use.</param>
        public static void WriteXboxIcon(Buttons button, Vector2 pos, Vector2 origin, bool scaleDown)
        {
            XboxButtons but = ConvertToXbox(button);
            Rectangle temp = new Rectangle(iconSize.Width * (int)but, 0, iconSize.Width, iconSize.Height);
            Color col = new Color(255, 255, 255, alpha) * (alpha / 255f);
            if(!scaleDown)
                spriteBatch.Draw(xboxTex, pos, temp, col, 0.0f, origin, 0.83f * RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
            if(scaleDown)
                spriteBatch.Draw(xboxTex, pos, temp, col, 0.0f, origin, 0.444f * RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
        }

        /// <summary>
        /// This function takes a Microsoft.Xna.Framework.InputManager.Buttons and converts it to a
        /// XboxButton used by the SymbolWriter.
        /// </summary>
        /// <param name="but">The button to convert. Use a casted negative value for Dpad with no input.</param>
        /// <returns>Returns the XboxButton equivelant to the given Buttons button. Note that an 
        /// invalid value returns a neutral D-pad, and that any specific Thumbstick direction 
        /// returns a thumbstick in the default position.</returns>
        private static XboxButtons ConvertToXbox(Buttons but)
        {
            switch(but)
            {
                case Buttons.A: return XboxButtons.A;
                case Buttons.B: return XboxButtons.B;
                case Buttons.Back: return XboxButtons.Back;
                case Buttons.DPadDown: return XboxButtons.DpadDown;
                case Buttons.DPadLeft: return XboxButtons.DpadLeft;
                case Buttons.DPadRight: return XboxButtons.DpadRight;
                case Buttons.DPadUp: return XboxButtons.DpadUp;
                case Buttons.LeftShoulder: return XboxButtons.LeftBumper;
                case Buttons.LeftStick: return XboxButtons.LeftClick;
                case Buttons.LeftThumbstickDown:
                case Buttons.LeftThumbstickLeft:
                case Buttons.LeftThumbstickRight:
                case Buttons.LeftThumbstickUp: return XboxButtons.LThumb;
                case Buttons.LeftTrigger: return XboxButtons.LeftTrigger;
                case Buttons.RightShoulder: return XboxButtons.RightBumper;
                case Buttons.RightStick: return XboxButtons.RightClick;
                case Buttons.RightThumbstickDown:
                case Buttons.RightThumbstickLeft:
                case Buttons.RightThumbstickRight:
                case Buttons.RightThumbstickUp: return XboxButtons.RThumb;
                case Buttons.RightTrigger: return XboxButtons.RightTrigger;
                case Buttons.Start: return XboxButtons.Start;
                case Buttons.X: return XboxButtons.X;
                case Buttons.Y: return XboxButtons.Y;
                default: return XboxButtons.DpadNeutral;
            }
        }

        static SymbolWriter()
        {
            iconSize = new Rectangle(0, 0, 90, 90);
            IconCenter = new Vector2(iconSize.Width * 0.5f, iconSize.Height * 0.5f);
            keyboardTex = Resources.GetResourceFromManager<Texture2D>("Font/KeyboardFinal_reach");
            xboxTex = Resources.GetResourceFromManager<Texture2D>("Font/xboxbuttons_final");
            RenderingDevice.GDM.DeviceCreated += onGDMCreation;
            spriteBatch = new SpriteBatch(RenderingDevice.GraphicsDevice);
        }

        private static void onGDMCreation(object sender, EventArgs e)
        {
            keyboardTex = Resources.GetResourceFromManager<Texture2D>("Font/KeyboardFinal_reach");
            xboxTex = Resources.GetResourceFromManager<Texture2D>("Font/xboxbuttons_final");
        }
    }
}
