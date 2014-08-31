using ApathyEngine.Menus.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Menus
{
    /// <summary>
    /// Provides a default implementation of a menu that can be used to confirm if the player
    /// wants to exit the game.
    /// </summary>
    /// <remarks>Ideally, this class (and the Exiting game state) should be replaced by the <pre>ConfirmationMenu</pre>
    /// class; but since there's a few different places you can exit the game from, here it remains for now.</remarks>
    public class ExitMenu : Menu
    {
        private readonly string exitString = "Are you sure you want to exit?";
        private Vector2 exitStringCenter;
        private readonly SpriteFont font;

        public ExitMenu()
        {
            MenuButton yes, no;
            yes = new MenuButton(loader.yesButton, delegate { Program.Game.Exit(); });
            no = new MenuButton(loader.noButton, delegate { GameManager.State = GameManager.PreviousState; });
            yes.SetDirectionals(null, no, null, null);
            no.SetDirectionals(yes, null, null, null);
            selectedControl = yes;
            yes.IsSelected = null;

            controlArray.AddRange(new MenuControl[] { yes, no });

            font = loader.BiggerFont;
            exitStringCenter = font.MeasureString(exitString) * 0.5f;
        }

        public override void Draw(GameTime gameTime)
        {
            RenderingDevice.SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.LinearClamp, null, null);

            RenderingDevice.SpriteBatch.Draw(loader.halfBlack, new Rectangle(0, 0, (int)RenderingDevice.Width, (int)RenderingDevice.Height), Color.White);
            RenderingDevice.SpriteBatch.Draw(loader.mainMenuLogo, new Vector2(RenderingDevice.Width * 0.97f - loader.mainMenuLogo.Width / 2, RenderingDevice.Height * 0.03f - loader.mainMenuLogo.Height / 2), null, Color.White, 0.0f, new Vector2(loader.mainMenuLogo.Width / 2, loader.mainMenuLogo.Height / 2), 0.3f * RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
            RenderingDevice.SpriteBatch.DrawString(font, exitString, new Vector2(RenderingDevice.Width * 0.5f, RenderingDevice.Height * 0.3f), Color.White, 0.0f, exitStringCenter, RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
            base.Draw(gameTime);

            RenderingDevice.SpriteBatch.End();
        }
    }
}
