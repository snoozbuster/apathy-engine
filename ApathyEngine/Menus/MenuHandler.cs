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
    public class MenuHandler
    {
        private static bool instantiated = false;

        /// <summary>
        /// Keeps track of whether the keyboard or mouse was last used as input; if
        /// the keyboard is used the mouse will be disabled until the mouse state changes.
        /// </summary>
        /// <remarks>This isn't really the best place to put this; it might move to a protected static
        /// property on MenuControl if I can swing that. </remarks>
        public bool MouseTempDisabled { get; set; }

        protected MouseState mouseLastFrame;

        protected Dictionary<int, Menu> menuMap = new Dictionary<int, Menu>();
        protected Dictionary<int, bool> drawPreviousMap = new Dictionary<int, bool>();

        protected BaseGame game;

        /// <summary>
        /// Creates a new MenuHandler. 
        /// </summary>
        /// <param name="game">Owning game.</param>
        public MenuHandler(BaseGame game)
        {
            if(instantiated)
                throw new InvalidOperationException("MenuHandler is a singleton and can only be instantiated once.");
            instantiated = true;

            this.game = game;
        }

        /// <summary>
        /// Registers a menu for use with the handler.
        /// </summary>
        /// <param name="state">State to map to the menu. Uses an integer internally, so out of range <pre>GameState</pre>s are valid.</param>
        /// <param name="menu">Menu to map to the given <paramref name="state"/>.</param>
        /// <param name="drawPreviousState">Indicates if the previous state's menu (if any) should be drawn under the current state's.</param>
        public void Register(GameState state, Menu menu, bool drawPreviousState = false)
        {
            if(menuMap.ContainsKey((int)state))
                throw new ArgumentException("State has already been registered to another menu.", "state");

            menuMap.Add((int)state, menu);
            drawPreviousMap.Add((int)state, drawPreviousState);
        }

        /// <summary>
        /// Draws the menu associated with the current game state, if any.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw(GameTime gameTime)
        {
            int state = (int)game.State;
            int prevState = (int)game.PreviousState;
            if(drawPreviousMap[state] && menuMap.ContainsKey(prevState))
                menuMap[prevState].Draw(gameTime);
            if(menuMap.ContainsKey(state))
                menuMap[state].Draw(gameTime);
        }

        /// <summary>
        /// Updates the menu associated with the current state, if any.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            checkForMouseMove();

            int state = (int)game.State;
            if(menuMap.ContainsKey(state))
                menuMap[state].Update(gameTime);

            if((InputManager.CurrentPad.WasButtonJustPressed(game.Manager.CurrentSaveXboxOptions.MusicKey) || InputManager.KeyboardState.WasKeyJustPressed(Program.Game.Manager.CurrentSaveWindowsOptions.MusicKey)) &&
               (game.State != GameState.PausedGamepadDC && game.State != GameState.PadQueryMenu && game.State != GameState.Exiting &&
               game.State != GameState.Ending && game.State != GameState.MediaPlayerMenu && !IsSelectingSave))
            {
                game.ChangeState(GameState.MediaPlayerMenu);
                MediaSystem.PauseAuxilary();
            }

            mouseLastFrame = InputManager.MouseState.RawState;
        }

        /// <summary>
        /// If the mouse has moved, it becomes enabled.
        /// </summary>
        protected void checkForMouseMove()
        {
            if(InputManager.MouseState.RawState != mouseLastFrame)
                MouseTempDisabled = false;
        }
    }
}
