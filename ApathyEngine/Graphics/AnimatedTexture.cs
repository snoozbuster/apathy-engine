using Accelerated_Delivery_Win;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Graphics
{
    public class AnimatedSprite : Sprite
    {
        private List<Rectangle> frames;
        private List<float> timeInSeconds;
        private int currentIndex = 0;

        private float timer = 0;

        /// <summary>
        /// Creates a new animated Sprite.
        /// </summary>
        /// <param name="spriteSheet">Spritesheet to use for the sprite.</param>
        /// <param name="position">Position to use for the sprite, relative to <paramref name="point"/></param>
        /// <param name="point">Origin point to use for positioning.</param>
        /// <param name="frames">Rectangles defining each frame on the sprite sheet. They should all have the same height and width.</param>
        /// <param name="timeInSeconds">Time in seconds to wait on each frame before moving to the next one.</param>
        public AnimatedSprite(Vector2 position, TextureDelegate spriteSheet, List<float> timeInSeconds, List<Rectangle> frames, RenderPoint point)
            :base(spriteSheet, position, frames[0], point)
        {
            if(frames.Count != timeInSeconds.Count)
                throw new ArgumentException("frame and timeInSeconds must all be the same length");

            this.frames = frames;
            this.timeInSeconds = timeInSeconds;
        }

        public void Reset()
        {
            timer = currentIndex = 0;
        }

        public void Update(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if(timer > timeInSeconds[currentIndex])
            {
                timer = 0;
                currentIndex++;
                if(currentIndex >= frames.Count)
                    currentIndex = 0;
                this.TargetArea = frames[currentIndex];
            }
        }
    }
}
