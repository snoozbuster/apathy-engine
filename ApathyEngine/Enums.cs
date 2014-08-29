using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine
{
    /// <summary>
    /// Keeps track of the game state.
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// Indicates the game is actively running.
        /// </summary>
        Running,
        /// <summary>
        /// Indicates the game is showing the Game Over screen.
        /// </summary>
        GameOver,
        /// <summary>
        /// Indicates the game is showing the Main Menu.
        /// </summary>
        MainMenu,
        /// <summary>
        /// Indicates the game is within a level and paused.
        /// </summary>
        Paused,
        /// <summary>
        /// Indicates the game is showing the gamepad disconnected screen.
        /// </summary>
        Paused_DC,
        /// <summary>
        /// Indicates the game is showing the options menu.
        /// </summary>
        Menuing_Opt,
        /// <summary>
        /// Indicates the game is showing the level select menu.
        /// </summary>
        Menuing_Lev,
        /// <summary>
        /// Indicates the game is showing the exit confirmation menu.
        /// </summary>
        Exiting,
        /// <summary>
        /// Indicates the game is showing the results screen after a level is completed.
        /// </summary>
        Results,
        /// <summary>
        /// Indicates the game is showing the credits.
        /// </summary>
        Ending,
        /// <summary>
        /// Indicates the game is showing the Objectives menu.
        /// </summary>
        Menuing_Obj,
        /// <summary>
        /// Indicates the game is showing the High Scores menu.
        /// </summary>
        Menuing_HiS,
        /// <summary>
        /// Indicates the game is asking if the user would like to continue with a gamepad.
        /// </summary>
        Paused_PadQuery,
        /// <summary>
        /// Indicates the Media Player is open.
        /// </summary>
        Paused_SelectingMedia
    }
}
