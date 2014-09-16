using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ApathyEngine.Input
{
    public static class InputManager
    {

        /// <summary>
        /// The pad that pressed start. This pad has messages sent to it. On a computer, this is PlayerIndex.One by default.
        /// This is only changed at the Press Start screen, so if temporarily changing input devices you can reset to normal by
        /// storing ControlScheme and/or checking this value.
        /// </summary>
        public static PlayerIndex? MessagePad { get; private set; }
        /// <summary>
        /// A object that can be queried for gamepad input.
        /// </summary>
        public static IGamepad CurrentPad { get; private set; }

        /// <summary>
        /// A bool specifying if a gamepad was ever connected.
        /// </summary>
        public static bool WasConnected { get; private set; }

        /// <summary>
        /// Current keyboard state.
        /// </summary>
        public static IKeyboard KeyboardState { get; private set; }

#if WINDOWS
        /// <summary>
        /// Current mouse state.
        /// </summary>
        public static AbstractMouse MouseState { get; private set; }
#endif

        /// <summary>
        /// Represents the currently in use control scheme.
        /// </summary>
        public static ControlScheme ControlScheme { get; private set; }

        /// <summary>
        /// Defines the set of Windows-esque options.
        /// </summary>
        public static WindowsOptions WindowsOptions { get; private set; }

        /// <summary>
        /// Defines the set of Xbox options.
        /// </summary>
        public static XboxOptions XboxOptions { get; private set; }

        private static GamePadState playerOneState;
        private static GamePadState playerOneLastFrame;

        private static GamePadState playerTwoState;
        private static GamePadState playerTwoLastFrame;

        private static GamePadState playerThreeState;
        private static GamePadState playerThreeLastFrame;

        private static GamePadState playerFourState;
        private static GamePadState playerFourLastFrame;

        private static bool xboxPluggedInWhenKeyboard;

        static InputManager()
        {
            ControlScheme = ControlScheme.None;
            KeyboardState = new HumanKeyboard();
            MouseState = new NullMouse();
            CurrentPad = new NullGamepad();
        }

        public static void Update(GameTime gameTime, bool isSelectingSave)
        {
            KeyboardState.Update(gameTime);
#if WINDOWS
            MouseState.Update(gameTime);
#endif

            #region Gamepad Updates
            if(MessagePad == null)
            {
                // If primary index not selected, update all.
                playerOneLastFrame = playerOneState;
                playerOneState = GamePad.GetState(PlayerIndex.One);

                playerTwoLastFrame = playerTwoState;
                playerTwoState = GamePad.GetState(PlayerIndex.Two);

                playerThreeLastFrame = playerThreeState;
                playerThreeState = GamePad.GetState(PlayerIndex.Three);

                playerFourLastFrame = playerFourState;
                playerFourState = GamePad.GetState(PlayerIndex.Four);
            }
            else
            {
                // Otherwise, just update the one that is primary.
                CurrentPad.Update(gameTime);
                    
                if(!CurrentPad.IsConnected)
                    xboxPluggedInWhenKeyboard = false;
#if XBOX360
                if(!CurrentPad.IsConnected)
                    game.ChangeState(GameState.Paused_DC);
#else
                if(!CurrentPad.IsConnected && WasConnected && game.State != GameState.Exiting &&
                    !isSelectingSave && game.State != GameState.PausedGamepadDC)
                {
                    if(game.State == GameState.MediaPlayerMenu || game.State == GameState.Exiting)
                        game.ChangeState(game.PreviousState);
                    game.ChangeState(GameState.PausedGamepadDC);
                    ControlScheme = ControlScheme.None;
                    MediaSystem.PauseAuxilary();
                }
                else if(!xboxPluggedInWhenKeyboard && !WasConnected && CurrentPad.IsConnected &&
                    game.State != GameState.Exiting && !isSelectingSave &&
                    game.State != GameState.PadQueryMenu)
                {
                    if(game.State == GameState.MediaPlayerMenu || game.State == GameState.Exiting)
                        game.ChangeState(game.PreviousState);
                    game.ChangeState(GameState.PadQueryMenu);
                    ControlScheme = ControlScheme.None;
                    MediaSystem.PauseAuxilary();
                }
#endif
            }
            #endregion
        }

        /// <summary>
        /// Updates things for the main menu.
        /// </summary>
        public static void PlayerSelect(Game game)
        {
            if(WasButtonJustPressed(PlayerIndex.One, Buttons.Start) ||
                WasButtonJustPressed(PlayerIndex.One, Buttons.A))
                MessagePad = PlayerIndex.One;
            else if(WasButtonJustPressed(PlayerIndex.Two, Buttons.Start) ||
                WasButtonJustPressed(PlayerIndex.Two, Buttons.A))
                MessagePad = PlayerIndex.Two;
            else if(WasButtonJustPressed(PlayerIndex.Three, Buttons.Start) ||
                WasButtonJustPressed(PlayerIndex.Three, Buttons.A))
                MessagePad = PlayerIndex.Three;
            else if(WasButtonJustPressed(PlayerIndex.Four, Buttons.Start) ||
                WasButtonJustPressed(PlayerIndex.Four, Buttons.A))
                MessagePad = PlayerIndex.Four;

            if(MessagePad != null) // gamepad was detected, initialize CurrentPad
                SetInputDevice(new HumanGamepad(MessagePad.Value));
            else if(KeyboardState.WasKeyJustPressed(Keys.Enter) || (game.IsActive && Mouse.GetState().LeftButton == ButtonState.Pressed))
            {
                MessagePad = PlayerIndex.One;
                SetInputDevice(new HumanKeyboard(), new HumanMouse(game));
                xboxPluggedInWhenKeyboard = playerOneState.IsConnected;
#if XBOX360
                SimpleMessageBox.ShowMessageBox("Notice", "Keyboard is enabled.", new string[] { "Okay" }, 0, MessageBoxIcon.Alert);
#endif
            }
        }

        /// <summary>
        /// Checks to see if a button was just pressed given a player index and button.
        /// </summary>
        /// <param name="index">Index to check.</param>
        /// <param name="buttons">Button to check.</param>
        /// <returns>True if the button was pressed this frame and released last frame by the given player index.</returns>
        public static bool WasButtonJustPressed(PlayerIndex index, Buttons buttons)
        {
            switch(index)
            {
                case PlayerIndex.One: return (playerOneState.IsButtonDown(buttons) && playerOneLastFrame.IsButtonUp(buttons));
                case PlayerIndex.Two: return (playerTwoState.IsButtonDown(buttons) && playerTwoLastFrame.IsButtonUp(buttons));
                case PlayerIndex.Three: return (playerThreeState.IsButtonDown(buttons) && playerThreeLastFrame.IsButtonUp(buttons));
                case PlayerIndex.Four: return (playerFourState.IsButtonDown(buttons) && playerFourLastFrame.IsButtonUp(buttons));
                default: throw new ArgumentException("A player index outside the normal range was specified.");
            }
        }

        /// <summary>
        /// Checks to see if the input for a machine was just pressed this frame given a machine number.
        /// </summary>
        /// <param name="machineNumber">Machine to check.</param>
        /// <returns>True if the input associated with that machine input was just pressed this frame.</returns>
        public static bool CheckMachineJustPressed(int machineNumber)
        {
            string keyIdentifier = machineNumber.ToString().Last().ToString();

            Keys key, altKey;
            Enum.TryParse("D" + keyIdentifier, out key);
            Enum.TryParse("NumPad" + keyIdentifier, out altKey);

            if(ControlScheme == ControlScheme.Keyboard)
            {
                Keys boundKey = (Keys)typeof(WindowsOptions).GetField("Machine" + keyIdentifier + "Key").GetValue(InputManager.WindowsOptions);
                if(boundKey == key)
                    return KeyboardState.WasKeyJustPressed(altKey) || KeyboardState.WasKeyJustPressed(boundKey);
                else return KeyboardState.WasKeyJustPressed(boundKey);
            }
            else
            {
                Buttons boundButton = (Buttons)typeof(XboxOptions).GetField("Machine" + keyIdentifier + "Key").GetValue(InputManager.XboxOptions);
                return CurrentPad.WasButtonJustPressed(boundButton);
            }
        }

        /// <summary>
        /// Checks to see if the input for a machine is being held down this frame given a machine number.
        /// </summary>
        /// <param name="machineNumber">Machine to check.</param>
        /// <returns>True if the input associated with that machine input is pressed this frame.</returns>
        public static bool CheckMachineIsHeld(int machineNumber)
        {
            string keyIdentifier = machineNumber.ToString().Last().ToString();

            Keys key, altKey;
            Enum.TryParse("D" + keyIdentifier, out key);
            Enum.TryParse("NumPad" + keyIdentifier, out altKey);

            if(ControlScheme == ControlScheme.Keyboard)
            {
                Keys boundKey = (Keys)typeof(WindowsOptions).GetField("Machine" + keyIdentifier + "Key").GetValue(InputManager.WindowsOptions);
                if(boundKey == key)
                    return KeyboardState.IsKeyDown(altKey) || KeyboardState.IsKeyDown(boundKey);
                else return KeyboardState.IsKeyDown(boundKey);
            }
            else
            {
                Buttons boundButton = (Buttons)typeof(XboxOptions).GetField("Machine" + keyIdentifier + "Key").GetValue(InputManager.XboxOptions);
                return CurrentPad.IsButtonDown(boundButton);
            }
        }

        /// <summary>
        /// Resets all current input devices to initial states, allowing input from all human sources. Gamepad input is assumed to come from <pre>PlayerIndex.One</pre>. 
        /// Implicitly shifts <pre>ControlScheme</pre> to <pre>ControlScheme.None</pre>.
        /// </summary>
        /// <param name="game">Required to pass to the <pre>HumanMouse</pre> constructor.</param>
        public static void RemoveInputDevices(Game game)
        {
            MessagePad = null;
            ControlScheme = ControlScheme.None;
            MouseState = new HumanMouse(game);
            KeyboardState = new HumanKeyboard();
            CurrentPad = new HumanGamepad(PlayerIndex.One);
        }

        /// <summary>
        /// Instructs the input system to use the specified input devices. Implicitly shifts <pre>ControlScheme</pre>
        /// to <pre>ControlScheme.Keyboard</pre>.
        /// </summary>
        /// <param name="keyboard">Keyboard analogue to use.</param>
        /// <param name="mouse">Mouse analogue to use.</param>
        public static void SetInputDevice(IKeyboard keyboard, AbstractMouse mouse)
        {
            MouseState = mouse;
            KeyboardState = keyboard;
            CurrentPad = new NullGamepad();
            ControlScheme = ControlScheme.Keyboard;
            WasConnected = false;
        }

        /// <summary>
        /// Instructs the input system to use the specified input devices. Implicitly shifts <pre>ControlScheme</pre>
        /// to <pre>ControlScheme.XboxController</pre>.
        /// </summary>
        /// <param name="gamepad">Gamepad analogue to use.</param>
        public static void SetInputDevice(IGamepad gamepad)
        {
            MouseState = new NullMouse();
            KeyboardState = new NullKeyboard();
            ControlScheme = ControlScheme.XboxController;
            CurrentPad = gamepad;
            WasConnected = true;
        }

        public static void SetOptions(WindowsOptions winop, XboxOptions xop)
        {
            WindowsOptions = winop;
            XboxOptions = xop;
        }
    }
}
