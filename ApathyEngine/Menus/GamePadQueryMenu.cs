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
    /// Provides a default implementation of a menu that can be used to ask the player if they want to use
    /// a game pad that was just plugged in.
    /// </summary>
    /// <remarks>This menu does not do any updating regarding gamepad connection/disconnection, only asking the player.</remarks>
    public class GamePadQueryMenu : Menu
    {
        public GamePadQueryMenu()
        { }

        public override void Draw(GameTime gameTime)
        {
            if(GameManager.PreviousState == GameState.Running)
                GameManager.DrawLevel(gameTime);

            RenderingDevice.SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.LinearClamp, null, null);

            RenderingDevice.SpriteBatch.Draw(loader.halfBlack, new Rectangle(0, 0, (int)RenderingDevice.Width, (int)RenderingDevice.Height), Color.White);
            string text = "An Xbox controller has been plugged in. Press\nStart to continue the game using the\ncontroller, or disconnect it to close\nthis prompt.";
            RenderingDevice.SpriteBatch.DrawString(loader.BiggerFont, text, new Vector2(RenderingDevice.Width, RenderingDevice.Height) * 0.5f, Color.White, 0, loader.BiggerFont.MeasureString(text) * 0.5f, RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);

            RenderingDevice.SpriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            if(!InputManager.CurrentPad.IsConnected)
            {
                GameManager.State = GameManager.PreviousState;
                MediaSystem.PlayAll();
            }
            if(InputManager.CurrentPad.WasButtonJustPressed(Buttons.Start) || InputManager.CurrentPad.WasButtonJustPressed(Buttons.A))
            {
                InputManager.SetInputDevice(new HumanGamepad(PlayerIndex.One));
                MediaSystem.PlayAll();
            }
        }
    }
}
