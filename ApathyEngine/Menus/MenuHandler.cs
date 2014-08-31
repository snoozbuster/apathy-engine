using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.IO;
using ApathyEngine.Input;
using ApathyEngine.Media;
using ApathyEngine.Graphics;
using ApathyEngine.Utilities;

namespace ApathyEngine.Menus
{
    public static class MenuHandler
    {
        /// <summary>
        /// This is for if you're navigating with the mouse and ditch it for the keyboard. 
        /// Upon pressing a keyboard button, when appropriate, this will toggle and the
        /// game will ignore mouse input until the mouse state changes.
        /// </summary>
        public static bool MouseTempDisabled { get; set; }

        private static PauseMenu pauseMenu;
        private static MainMenu mainMenu;
        private static GamepadDCMenu disconnectMenu;
        private static ExitMenu exitMenu;
        private static GamePadQueryMenu queryMenu;
        private static MediaPlayerMenu mediaMenu;
        private static CreditsMenu endingMenu;

        private static MouseState mouseLastFrame;

        private static Dictionary<int, Menu> menuMap = new Dictionary<int, Menu>();
        private static Dictionary<int, bool> drawPreviousMap = new Dictionary<int, bool>();

        /// <summary>
        /// Registers a menu for use with the handler.
        /// </summary>
        /// <param name="state">State to map to the menu. Uses an integer internally, so out of range <pre>GameState</pre>s are accepted.</param>
        /// <param name="menu">Menu to map to the given <paramref name="state"/>.</param>
        /// <param name="drawPreviousState">Indicates if the previous state's menu (if any) should be drawn under the current state's.</param>
        public static void Register(GameState state, Menu menu, bool drawPreviousState = false)
        {
            if(menuMap.ContainsKey((int)state))
                throw new ArgumentException("State already exists in the map.", "state");

            menuMap.Add((int)state, menu);
            drawPreviousMap.Add((int)state, drawPreviousState);
        }

        /// <summary>
        /// Draws the menu associated with the current game state, if any.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public static void Draw(GameTime gameTime)
        {
            int state = (int)GameManager.State;
            int prevState = (int)GameManager.PreviousState;
            if(drawPreviousMap[state] && menuMap.ContainsKey(prevState))
                menuMap[prevState].Draw(gameTime);
            if(menuMap.ContainsKey(state))
                menuMap[state].Draw(gameTime);
        }

        /// <summary>
        /// Updates the menu associated with the current state, if any.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public static void Update(GameTime gameTime)
        {
            CheckForMouseMove();

            int state = (int)GameManager.State;
            if(menuMap.ContainsKey(state))
                menuMap[state].Update(gameTime);

            if((InputManager.CurrentPad.WasButtonJustPressed(Program.Game.Manager.CurrentSaveXboxOptions.MusicKey) || InputManager.KeyboardState.WasKeyJustPressed(Program.Game.Manager.CurrentSaveWindowsOptions.MusicKey)) &&
               (GameManager.State != GameState.Paused_DC && GameManager.State != GameState.Paused_PadQuery && GameManager.State != GameState.Exiting &&
               GameManager.State != GameState.Ending && GameManager.State != GameState.Paused_SelectingMedia && !MenuHandler.IsSelectingSave))
            {
                GameManager.State = GameState.Paused_SelectingMedia;
                MediaSystem.PauseAuxilary();
            }

            mouseLastFrame = InputManager.MouseState.RawState;
        }

        #region Helper methods
        /// <summary>
        /// If the mouse has moved, it becomes enabled.
        /// </summary>
        public static void CheckForMouseMove()
        {
            if(InputManager.MouseState.RawState != mouseLastFrame)
                MenuHandler.MouseTempDisabled = false;
        }

        /// <summary>
        /// Checks to see if the mouse was within a certain set of coordinates last frame.
        /// </summary>
        /// <param name="upperLeft">Upper left corner of defining rectangle.</param>
        /// <param name="lowerRight">Lower right corner of defining rectangle.</param>
        /// <returns>True if the mouse was within the rectangle.</returns>
        private static bool wasMouseWithinCoordinates(Vector2 upperLeft, Vector2 lowerRight)
        {
            return ((mouseLastFrame.X > upperLeft.X && mouseLastFrame.X < lowerRight.X) &&
                    (mouseLastFrame.Y > upperLeft.Y && mouseLastFrame.Y < lowerRight.Y));
        }
        #endregion
    }
}
