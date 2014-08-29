using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ApathyEngine.IO
{
    public class LoadingScreen
    {
        /// <summary>
        /// If the loading screen is set to fade in the background texture, gets if the fade is complete.
        /// Returns true if not set to fade.
        /// </summary>
        public bool FadeComplete { get; protected set; }

        /// <summary>
        /// Current alpha level of the background texture.
        /// </summary>
        protected float alpha = 0;
        /// <summary>
        /// Indicates if we should fade in the background texture.
        /// </summary>
        protected bool shouldFade;
        /// <summary>
        /// Speed at which to fade in the texture.
        /// </summary>
        protected int delta;

        /// <summary>
        /// Loader that will contain loaded content.
        /// </summary>
        protected Loader loader;
        /// <summary>
        /// Spritebatch to draw the textures.
        /// </summary>
        protected SpriteBatch sb;
        /// <summary>
        /// Enumerator that the loader returns.
        /// </summary>
        protected IEnumerator<float> enumerator;
        
        /// <summary>
        /// Texture to display 
        /// </summary>
        protected Texture2D backgroundTex;
        /// <summary>
        /// Color the screen will be cleared with. Defaults to color of upper-left pixel of backgroundTex.
        /// </summary>
        protected Color screenBackgroundColor;

        /// <summary>
        /// Foreground loading bar color.
        /// </summary>
        protected Color barColor = Color.DeepSkyBlue;
        /// <summary>
        /// Background loading bar color.
        /// </summary>
        protected Color barBackgroundColor = Color.Gray;
        /// <summary>
        /// Width of background bar in pixels.
        /// </summary>
        protected int barBackgroundExpand = 2;
        /// <summary>
        /// One-pixel white texture for use in filling the bar colors.
        /// </summary>
        protected Texture2D barTex;

        /// <summary>
        /// Size and position of the loading bar in screen coordinates.
        /// </summary>
        protected Rectangle loadingBarPos;

        /// <summary>
        /// Creates a new LoadingScreen to manage loading.
        /// </summary>
        /// <param name="loader">Unloaded Loader subclass.</param>
        /// <param name="gd">GraphicsDevice for the game.</param>
        /// <param name="backgroundTex">Background texture to use.</param>
        /// <param name="fadeTex">Indicates if the background textures should fade in or not.</param>
        /// <param name="fadeDelta">Sets the speed in alpha units at which the texture should fade in.</param>
        public LoadingScreen(Loader loader, GraphicsDevice gd, Texture2D backgroundTex, bool fadeTex = true, int fadeDelta = 3)
        {
            this.loader = loader;
            this.backgroundTex = backgroundTex;
            this.shouldFade = fadeTex;
            this.delta = fadeDelta;
            if(!shouldFade)
                alpha = 255;
            sb = new SpriteBatch(gd);
            enumerator = loader.GetEnumerator();

            // get upper-left pixel color
            Color[] backgroundTexData = new Color[backgroundTex.Width * backgroundTex.Height];
            backgroundTex.GetData(backgroundTexData);
            screenBackgroundColor = backgroundTexData[0];

            // 1-pixel white texture for solid bars
            barTex = new Texture2D(gd, 1, 1);
            barTex.SetData(new uint[] { 0xffffffff });

            loadingBarPos = new Rectangle();
            loadingBarPos.Width = 200;
            loadingBarPos.Height = 10;
            loadingBarPos.X = sb.GraphicsDevice.Viewport.TitleSafeArea.Width - loadingBarPos.Width - 30;
            loadingBarPos.Y = sb.GraphicsDevice.Viewport.TitleSafeArea.Y + 30;
        }

        /// <summary>
        /// Loads one item. If loading is complete, returns a Loader; otherwise, returns null.
        /// </summary>
        /// <returns>Loader if loading is done, else null.</returns>
        public Loader Update()
        {
            if(shouldFade)
            {
                if(alpha + delta >= 255)
                {
                    alpha = 255;
                    FadeComplete = true;
                }
                else
                    alpha += delta;
            }

            // enumerator.MoveNext() will load one item and return false when all done.
            return enumerator.MoveNext() ? null : loader;
        }

        /// <summary>
        /// Draws the LoadingScreen.
        /// </summary>
        public virtual void Draw()
        {
            sb.GraphicsDevice.Clear(screenBackgroundColor);

            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null);

            sb.Draw(backgroundTex, new Vector2(sb.GraphicsDevice.Viewport.Width * 0.5f, sb.GraphicsDevice.Viewport.Height * 0.5f), null, new Color(255, 255, 255) * (alpha / 255f), 0, new Vector2(backgroundTex.Width * 0.5f, backgroundTex.Height * 0.5f), 1, SpriteEffects.None, 0);

            Rectangle barBackground = loadingBarPos;
            barBackground.Inflate(barBackgroundExpand, barBackgroundExpand);
            sb.Draw(barTex, barBackground, barBackgroundColor);

            Rectangle bar = loadingBarPos;
            float completeness = enumerator.Current;
            bar.Width = (int)(loadingBarPos.Width * completeness);

            sb.Draw(barTex, bar, barColor);
            sb.End();
        }
    }
}
