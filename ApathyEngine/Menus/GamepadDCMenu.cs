using ApathyEngine.Input;
using ApathyEngine.Media;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Menus
{
    /// <summary>
    /// Provides an implementation of a menu that can be used to ask the player to plug a disconnected gamepad
    /// back in, or allow them to continue without it using the keyboard.
    /// </summary>
    public class GamepadDCMenu : Menu
    {
        /// <summary>
        /// Creates a menu that will appear when the gamepad is disconnected.
        /// </summary>
        /// <param name="game">Owning game.</param>
        public GamepadDCMenu(BaseGame game)
            :base(game)
        { }

        public override void Draw(GameTime gameTime)
        {
#if XBOX
                SimpleMessageBox.ShowMessageBox("Game Pad Disconnected!", "The primary game pad has become disconnected. Please reconnect it to continue.",
                    new string[] { "Okay" }, 0, MessageBoxIcon.Error);
#else
            if(game.PreviousState == GameState.Running)
                game.DrawRunning(gameTime);

            RenderingDevice.SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.LinearClamp, null, null);

            RenderingDevice.SpriteBatch.Draw(loader.halfBlack, new Rectangle(0, 0, (int)RenderingDevice.Width, (int)RenderingDevice.Height), Color.White);
            RenderingDevice.SpriteBatch.DrawString(loader.BiggerFont, "The gamepad has become disconnected. Please\nreconnect it, or press the Spacebar to\ncontinue using the keyboard.", new Vector2(RenderingDevice.Width, RenderingDevice.Height) * 0.5f, Color.White, 0, loader.BiggerFont.MeasureString("The gamepad has become disconnected. Please\nreconnect it, or press the Spacebar to\ncontinue using the keyboard.") * 0.5f, RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);

            RenderingDevice.SpriteBatch.End();
#endif
        }

        public override void Update(GameTime gameTime)
        {
            if(InputManager.CurrentPad.IsConnected)
            {
                game.ChangeState(game.PreviousState);
                MediaSystem.PlayAll();
            }
#if WINDOWS
            if(InputManager.KeyboardState.WasKeyJustPressed(Keys.Space))
            {
                InputManager.SetInputDevice(new HumanKeyboard(), new HumanMouse(Program.Game));
                MediaSystem.PlayAll();
            }
#endif
        }
    }
}
