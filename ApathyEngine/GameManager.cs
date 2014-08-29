using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ApathyEngine
{
    public static class GameManager
    {
        public static Space Space { get; private set; }

        #region Game State
        /// <summary>
        /// To keep track of the game state. 
        /// </summary>
        public static GameState State
        {
            get { return state; }
            set
            {
                if(value != state) // if we're setting the state to something it already is, don't update previous state.
                    PreviousState = state;
                state = value;
            }
        }

        private static GameState state = GameState.MainMenu;

        public static GameState PreviousState { get; private set; }

        private static GameState enteredFrom;
        /// <summary>
        /// Specify this when a level is loaded.
        /// Valid values are:
        /// Running - Goes to the next level when this one is over.
        /// Level Select - Returns to the level select menu when this one is over.
        /// </summary>
        public static GameState LevelEnteredFrom
        {
            get { return enteredFrom; }
            set
            {
                if(value != GameState.Menuing_Lev && value != GameState.Running && value != GameState.MainMenu)
                    throw new InvalidOperationException("LevelEnteredFrom was set to an invalid state.");
                enteredFrom = value;
            }
        }
        #endregion

        public static IInputManager Manager { get; private set; }
        public static Game Game { get; private set; }

        public static void FirstStageInitialization(Game g)
        {
            Game = g;
            Space = new Space();
            Space.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, 0, -9.81f);
        }

        public static void Initialize(SpriteFont f, IInputManager man)
        {
            Manager = man;
            PreviousState = GameState.MainMenu;
            State = GameState.MainMenu;
            font = f;
        }

        private static SpriteFont font;
    }
}
