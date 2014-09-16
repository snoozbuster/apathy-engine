using ApathyEngine.Graphics;
using BEPUphysics.CollisionShapes;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Collections.Generic;

namespace ApathyEngine
{
    /// <summary>
    /// Simple single-thread loader using enumeration. Any <pre>GraphicsResource</pre>s loaded by this
    /// loader should be accessed through a delegate. 
    /// </summary>
    public abstract class Loader : IEnumerable<float>
    {
        private ContentManager content;
        private int loadedItems;
        private int totalItems;

        #region staples
        public Texture2D halfBlack;
        public Texture2D Instructions_PC;
        public Texture2D mainMenuLogo;
        public Texture2D mainMenuBackground;
        public Texture2D EmptyTex;
        #endregion

        #region font
        public SpriteFont SmallerFont;
        public SpriteFont Font;
        public SpriteFont BiggerFont;
        #endregion

        #region Buttons
        public Sprite resumeButton;
        public Sprite startButton;
        public Sprite quitButton;
        public Sprite mainMenuButton;
        public Sprite yesButton;
        public Sprite noButton;
        public Sprite pauseQuitButton;
        public Sprite instructionsButton;
        #endregion

        public Loader(ContentManager content)
        {
            this.content = content;
        }

        /// <summary>
        /// Put your things to be loaded here.
        /// When FullReload() is called, it will iterate through this entire function; be sure that
        /// this is a safe operation.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator<float> GetEnumerator();

        /// <summary>
        /// Call this each time you yield return from GetEnumerator; ie <pre>yield return progress();</pre>.
        /// </summary>
        /// <returns>A float between 0 and 1 indicating percentage of progress loaded.</returns>
        protected float progress()
        {
            ++loadedItems;
            return (float)loadedItems / totalItems;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Does a full reload of all assets loaded with this Loader. Blocks the calling thread
        /// until complete. Useful when the graphics device is lost and needs to be reset.
        /// </summary>
        public void FullReload()
        {
            var enumerator = GetEnumerator();
            // MoveNext() returns true while there's still items; there's nothing to do but just
            // iterate the whole thing out. No while body is needed.
            while(enumerator.MoveNext())
                ;
        }
    }
}
