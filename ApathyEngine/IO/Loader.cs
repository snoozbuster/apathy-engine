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

        public abstract IEnumerator<float> GetEnumerator();

        protected float progress()
        {
            ++loadedItems;
            return (float)loadedItems / totalItems;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
