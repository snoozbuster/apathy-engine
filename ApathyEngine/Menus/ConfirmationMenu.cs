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
    /// Provides a default implementation of a menu that can be used to query a binary choice from the player.
    /// </summary>
    public class ConfirmationMenu : Menu
    {
        /// <summary>
        /// Gets if the user has finished selecting a value (either yes or no).
        /// </summary>
        public bool Finished { get; protected set; }

        protected readonly Vector2[] confirmStringMeasurements;
        protected SpriteFont font { get { return loader.BiggerFont; } }
        protected readonly string[] confirmationStrings;

        /// <summary>
        /// Creates a confirmation menu.
        /// </summary>
        /// <param name="game">Owning game.</param>
        /// <param name="confirmationStrings">The strings to prompt the user with. Each string will be displayed centered on a new line.</param>
        /// <param name="onYes">The delegate to perform if the user selects yes.</param>
        public ConfirmationMenu(BaseGame game, Action onYes, params string[] confirmationStrings)
            :base(game)
        {
            if(confirmationStrings.Length == 0)
                throw new ArgumentException("confirmationStrings must contain at least one string.", "confirmationStrings");

            MenuButton yes, no;
            yes = new MenuButton(loader.yesButton, onYes + delegate { Finished = true; });
            no = new MenuButton(loader.noButton, delegate { Finished = true; });
            yes.SetDirectionals(null, no, null, null);
            no.SetDirectionals(yes, null, null, null);
            selectedControl = no;
            no.IsSelected = null;

            controlArray.AddRange(new MenuControl[] { yes, no });

            this.confirmationStrings = confirmationStrings;
            this.confirmStringMeasurements = new Vector2[confirmationStrings.Length];
            for(int i = 0; i < confirmationStrings.Length; i++)
                confirmStringMeasurements[i] = font.MeasureString(confirmationStrings[i]);
        }

        public override void Draw(GameTime gameTime)
        {
            RenderingDevice.SpriteBatch.Draw(loader.halfBlack, new Rectangle(0, 0, (int)RenderingDevice.Width, (int)RenderingDevice.Height), Color.White);
            RenderingDevice.SpriteBatch.DrawString(font, confirmationStrings[0], new Vector2(RenderingDevice.Width * 0.5f, RenderingDevice.Height * 0.3f), Color.White, 0.0f, confirmStringMeasurements[0] * 0.5f, RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
            for(int i = 1; i < confirmationStrings.Length; i++)
                RenderingDevice.SpriteBatch.DrawString(font, confirmationStrings[i], new Vector2(RenderingDevice.Width * 0.5f, RenderingDevice.Height * 0.3f + confirmStringMeasurements[i-1].Y + 5), Color.White, 0.0f, confirmStringMeasurements[i] * 0.5f, RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
            base.Draw(gameTime);
        }

        public void Reset()
        {
            Finished = false;
            selectedControl = controlArray[1];
            controlArray[1].IsSelected = null;
            controlArray[0].IsSelected = false;
        }
    }
}
