using ApathyEngine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Menus
{
    /// <summary>
    /// Provides a default implementation for a credits sequences that scrolls a 
    /// series of images from bottom to top underneath a small gradient at the top and bottom
    /// of the screen.
    /// </summary>
    public class CreditsMenu : Menu
    {
        protected Sprite[] credits;
        protected float alpha;
        protected float time;
        protected bool moved;
        protected bool fadingOut;

        protected float screenAlpha = 255;

        protected const float deltaA = 3;
        protected const float secondsToPause = 3;

        protected Texture2D gradient;

        protected Rectangle topR;
        protected Rectangle bottomR;

        /// <summary>
        /// Creates a menu that can be used to display a series of textures as scrolling credits.
        /// </summary>
        /// <param name="game">Owning game.</param>
        /// <param name="credits">List of textures to use as credits.</param>
        public CreditsMenu(BaseGame game, params Sprite[] credits)
            :base(game)
        {
            this.credits = credits;
            onGDMReset(this, EventArgs.Empty);
            RenderingDevice.GDM.DeviceReset += onGDMReset;
        }

        protected virtual void onGDMReset(object sender, EventArgs e)
        {
            gradient = new Texture2D(RenderingDevice.GraphicsDevice, 1, 20);
            Color[] colors = new Color[20];
            for(int i = 0; i < 5; i++)
                colors[i] = new Color(0, 0, 0, 255);
            for(int i = 5; i < 20; i++)
                colors[i] = new Color(0, 0, 0, 255 - ((255 / 15) * (i - 4)));
            gradient.SetData(colors);
        }

        public override void Update(GameTime gameTime)
        {
            if(!credits[0].Moving && !moved)
            {
                moved = true;
                credits[0].Move(new Vector2(0, -0.55f), 115);
                credits[1].Move(new Vector2(0, -0.55f), 115);
                topR = new Rectangle(0, 0, (int)RenderingDevice.Width, 20);
                bottomR = new Rectangle(0, (int)RenderingDevice.Height - 20, (int)RenderingDevice.Width, 20);
            }

            for(int i = 0; i < credits.Length - 1; i++)
                credits[i].ForceMoveUpdate(gameTime);

            if(!credits[0].Moving)
            {
                if(alpha + deltaA > 255 && !fadingOut)
                {
                    alpha = 255;
                    time += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if(time >= secondsToPause)
                    {
                        fadingOut = true;
                        time = 0;
                    }
                }
                else if(alpha - deltaA < 0 && fadingOut)
                {
                    alpha = 0;
                    time += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if(time >= secondsToPause)
                    {
                        if(screenAlpha - deltaA < 0) // black background
                        {
                            reset();
                            this.game.ChangeState(GameState.MainMenu);
                            MediaSystem.PlayTrack(SongOptions.Menu);
                        }
                        else
                            screenAlpha -= deltaA;
                    }
                }
                else
                {
                    alpha += deltaA * (fadingOut ? -1 : 1);
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            RenderingDevice.SpriteBatch.Begin();

            RenderingDevice.SpriteBatch.Draw(loader.EmptyTex, new Rectangle(0, 0, (int)RenderingDevice.Width, (int)RenderingDevice.Height),
                new Color(0, 0, 0, screenAlpha) * (screenAlpha / 255f));
            for(int i = 0; i < credits.Length - 1; i++)
                credits[i].Draw();
            credits[credits.Length - 1].Draw(new Color(255, 255, 255, alpha) * (alpha / 255f));

            Color whiteWithScreenAlpha = new Color(255, 255, 255, screenAlpha) * (screenAlpha / 255f);
            RenderingDevice.SpriteBatch.Draw(gradient, topR, null, whiteWithScreenAlpha, 0, Vector2.Zero, SpriteEffects.None, 0);
            RenderingDevice.SpriteBatch.Draw(gradient, bottomR, null, whiteWithScreenAlpha, 0, Vector2.Zero, SpriteEffects.FlipVertically, 0);

            RenderingDevice.SpriteBatch.End();
        }

        protected virtual void reset()
        {
            time = alpha = 0;
            screenAlpha = 255;
            moved = false;
            foreach(Sprite s in credits)
                s.Reset();
        }
    }
}
