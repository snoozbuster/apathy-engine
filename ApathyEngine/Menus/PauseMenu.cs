using ApathyEngine.Graphics;
using ApathyEngine.Input;
using ApathyEngine.Media;
using ApathyEngine.Menus.Controls;
using ApathyEngine.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Menus
{
    /// <summary>
    /// Provides a default implementation of a pause menu that can optionally include a restart button.
    /// </summary>
    /// <remarks>This implementation assumes a horizontal button layout.</remarks>
    private class PauseMenu : Menu
    {
        private bool confirming = false;
        private bool confirmRestart = false;
        private ConfirmationMenu menu;
        private ConfirmationMenu restartMenu;

        /// <summary>
        /// Creates a pause menu with no restart option.
        /// </summary>
        /// <param name="game">Owning game.</param>
        /// <param name="resumeSprite">Sprite to use for the resume button.</param>
        /// <param name="mainMenuSprite">Sprite to use for the main menu button.</param>
        /// <param name="quitSprite">Sprite to use for the quit button.</param>
        /// <param name="mainMenuConfirmStrings">These strings will be passed into the constructor of the <pre>ConfirmationMenu</pre> to provide additional information about returning to the main menu.</param>
        public PauseMenu(BaseGame game, Sprite resumeSprite, Sprite mainMenuSprite, Sprite quitSprite, params string[] mainMenuConfirmStrings)
            :base(game)
        {
            MenuControl resume, mainMenu, quit;

            resume = new MenuButton(resumeSprite, delegate { MediaSystem.PlaySoundEffect(SFXOptions.Pause); game.ChangeState(GameState.Running); });
            mainMenu = new MenuButton(mainMenuSprite, delegate { menu.Reset(); confirming = true; });
            quit = new MenuButton(quitSprite, delegate { game.ChangeState(GameState.Exiting); });

            resume.SetDirectionals(null, mainMenu, null, null);
            mainMenu.SetDirectionals(resume, quit, null, null);
            quit.SetDirectionals(mainMenu, null, null, null);

            controlArray.AddRange(new MenuControl[] { resume, mainMenu, quit });
            selectedControl = resume;
            selectedControl.IsSelected = null;

            menu = new ConfirmationMenu(game, delegate { GameManager.CurrentLevel.RemoveFromGame(GameManager.Space); MediaSystem.PlayTrack(SongOptions.Menu); if(GameManager.LevelEnteredFrom == GameState.LevelSelectMenu) game.ChangeState(GameState.LevelSelectMenu); else game.ChangeState(GameState.MainMenu); },
                new[] { "Are you sure you want to return to the main menu?" }.Concat(mainMenuConfirmStrings).ToArray());
        }

        /// <summary>
        /// Creates a pause menu with a restart option.
        /// </summary>
        /// <param name="game">Owning game.</param>
        /// <param name="restartSprite">Sprite to use for the restart button.</param>
        /// <param name="resumeSprite">Sprite to use for the resume button.</param>
        /// <param name="mainMenuSprite">Sprite to use for the main menu button.</param>
        /// <param name="quitSprite">Sprite to use for the quit button.</param>
        /// <param name="mainMenuConfirmationStrings">Confirmation strings that will be passed into the ConfirmationMenu used for the Main Menu button.</param>
        /// <param name="restartConfirmationString">This string will be passed into the ConfirmationMenu used when the restart button is activated.</param>
        public PauseMenu(BaseGame game, Sprite resumeSprite, Sprite restartSprite, Sprite mainMenuSprite, Sprite quitSprite, string restartConfirmationString, params string[] mainMenuConfirmationStrings)
            : this(game, resumeSprite, mainMenuSprite, quitSprite, mainMenuConfirmationStrings)
        {
            MenuControl restart = new MenuButton(restartSprite, delegate { restartMenu.Reset(); confirmRestart = true; });

            controlArray[0].SetDirectionals(null, restart, null, null);
            restart.SetDirectionals(controlArray[0], controlArray[1], null, null);
            controlArray[1].SetDirectionals(restart, controlArray[2], null, null);

            controlArray.Insert(1, restart);

            restartMenu = new ConfirmationMenu(game, delegate { GameManager.CurrentLevel.ResetLevel(); game.ChangeState(GameState.Running); MenuHandler.mainMenu.ResetTimer(); },
                restartConfirmationString);
        }

        public override void Draw(GameTime gameTime)
        {
            game.DrawRunning(gameTime);

            RenderingDevice.SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.LinearClamp, null, null);

            RenderingDevice.SpriteBatch.Draw(loader.halfBlack, new Rectangle(0, 0, (int)RenderingDevice.Width, (int)RenderingDevice.Height), Color.White);
            RenderingDevice.SpriteBatch.Draw(loader.mainMenuLogo, new Vector2(RenderingDevice.Width * 0.97f - loader.mainMenuLogo.Width / 2, RenderingDevice.Height * 0.03f - loader.mainMenuLogo.Height / 2), null, Color.White, 0.0f, new Vector2(loader.mainMenuLogo.Width / 2, loader.mainMenuLogo.Height / 2), 0.3f * RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
            RenderingDevice.SpriteBatch.DrawString(loader.BiggerFont, "Paused", new Vector2(RenderingDevice.Width * 0.5f, RenderingDevice.Height * 0.3f), Color.OrangeRed, 0, loader.BiggerFont.MeasureString("Paused") * 0.5f, RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);

            Vector2 stringLength = loader.BiggerFont.MeasureString("Press       to resume");
            Vector2 screenSpot = new Vector2(RenderingDevice.Width * 0.5f, RenderingDevice.Height * 0.5f);
            RenderingDevice.SpriteBatch.DrawString(loader.BiggerFont, "Press       to resume", screenSpot, Color.White, 0, stringLength * 0.5f, RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
            if(InputManager.ControlScheme == ControlScheme.Keyboard)
                SymbolWriter.WriteKeyboardIcon(Program.Game.Manager.CurrentSaveWindowsOptions.PauseKey, screenSpot, new Vector2((stringLength.X * 0.5f + SymbolWriter.IconCenter.X * 0.5f - loader.BiggerFont.MeasureString("Press ").X), SymbolWriter.IconCenter.Y), false);
            else
                SymbolWriter.WriteXboxIcon(Program.Game.Manager.CurrentSaveXboxOptions.PauseKey, screenSpot, new Vector2((stringLength.X * 0.5f + SymbolWriter.IconCenter.X * 0.5f - loader.BiggerFont.MeasureString("Press ").X), SymbolWriter.IconCenter.Y), false);

            base.Draw(gameTime);

            if(confirming)
                menu.Draw(gameTime);
            else if(confirmRestart)
                restartMenu.Draw(gameTime);

            RenderingDevice.SpriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            if(confirming)
            {
                menu.Update(gameTime);
                if(menu.Finished)
                {
                    confirming = false;
                    menu.Reset();
                }
                return;
            }
            else if(confirmRestart)
            {
                restartMenu.Update(gameTime);
                if(restartMenu.Finished)
                {
                    confirmRestart = false;
                    restartMenu.Reset();
                }
                return;
            }

            if(InputManager.KeyboardState.WasKeyJustPressed(game.Manager.CurrentSaveWindowsOptions.PauseKey) ||
                InputManager.CurrentPad.WasButtonJustPressed(game.Manager.CurrentSaveXboxOptions.PauseKey))
            {
                game.ChangeState(GameState.Running);
                MediaSystem.PlaySoundEffect(SFXOptions.Pause);
                selectedControl.IsSelected = false;
                controlArray[0].IsSelected = null;
                return;
            }
            base.Update(gameTime);
        }
    }
}
