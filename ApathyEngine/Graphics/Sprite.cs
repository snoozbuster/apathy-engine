using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace ApathyEngine.Graphics
{
    public delegate Texture2D TextureDelegate();

    public class Sprite
    {
        public Vector2 Scale { get { return RenderingDevice.TextureScaleFactor; } }

        public float Width { get { return TargetArea.Width * Scale.X; } }
        public float Height { get { return TargetArea.Height * Scale.Y; } }

        public Texture2D Texture { get { return textureDelegate(); } }
        protected TextureDelegate textureDelegate;
        protected Vector2 upperLeft;
        /// <summary>
        /// Returns the upper left point of the texture after applying scale.
        /// </summary>
        public Vector2 UpperLeft { get { return upperLeft;}}// *scale; } }
        protected Vector2 lowerRight;
        /// <summary>
        /// Returns the lower right point of the texture after applying scale.
        /// </summary>
        public Vector2 LowerRight { get { return lowerRight;}}// *scale; } }
        protected Vector2 center;
        /// <summary>
        /// Returns the center point of the texture after applying scale.
        /// </summary>
        public Vector2 Center { get { return center;}}// *scale; } }
        public Rectangle TargetArea { get; private set; }
        public RenderPoint Point { get; private set; }
        private Vector2 relativeOrigin = Vector2.Zero;

        public bool Moving { get; private set; }
        private Vector2 speed = Vector2.Zero;
        private Vector2 target = Vector2.Zero;
        private Vector2 originalCenter;
        private float timeInSeconds;
        private float timer;
        private const float epsilon = 0.01f;

        /// <summary>
        /// Forces the texture to draw without scale. It will also fail to resize itself upon resolution changes.
        /// </summary>
        public bool DrawUnscaled { get; set; }

        private Vector2 relativeScreenPosition; // probably between 0 and 1, based on upper left

        public enum RenderPoint
        {
            UpLeft,
            Center
        }

        /// <summary>
        /// This is a special SuperTextor used as a hitbox. 
        /// </summary>
        /// <param name="upLeft">Upper left corner (in screen coords) of the hitbox.</param>
        /// <param name="heightWidth">A vector determining the height/width of the hitbox.
        /// The X value of the vector will be used as the width, the Y value as the height.</param>
        public Sprite(Vector2 upLeft, Vector2 heightWidth)
        {
            textureDelegate = delegate { return null; };
            Point = RenderPoint.UpLeft;
            upperLeft = upLeft;
            lowerRight = new Vector2(upLeft.X + heightWidth.X, upLeft.Y + heightWidth.Y);
            center = new Vector2((upLeft.X + LowerRight.X) / 2, (upLeft.Y + LowerRight.Y) / 2);

            relativeScreenPosition = new Vector2(upperLeft.X / RenderingDevice.Width, upperLeft.Y / RenderingDevice.Height);

            RenderingDevice.GDM.DeviceReset += rebuildCenters;
        }

        /// <summary>
        /// A Texture2D with coords relative to the screen. Generates the center of the 
        /// texture as well.
        /// </summary>
        /// <param name="tex">A delegate that returns a Texture2D.</param>
        /// <param name="position">Position of texture. Defined by point; e.g. if point is Center this value will be the center of the texture.</param>
        /// <param name="renderBox">If using a compiled texture, the rectangle that contains the
        /// desired texture. Use null for all of the given texture.</param>
        /// <param name="point">This is the point at which to render the sprite.</param>
        public Sprite(TextureDelegate d, Vector2 position, Rectangle? renderBox, RenderPoint point)
        {
            textureDelegate = d;
            if(renderBox.HasValue)
                TargetArea = renderBox.Value;
            else
                TargetArea = new Rectangle(0, 0, Texture.Width, Texture.Height);
            Point = point;

            if(point == RenderPoint.UpLeft)
            {
                upperLeft = position;
                lowerRight = new Vector2(upperLeft.X + Width, upperLeft.Y + Height);
                center = new Vector2((upperLeft.X + lowerRight.X) * 0.5f, (upperLeft.Y + lowerRight.Y) * 0.5f);
            }
            else // center
            {
                center = position;
                lowerRight = new Vector2(center.X + TargetArea.Width * 0.5f, center.Y + TargetArea.Height * 0.5f);
                upperLeft = new Vector2(center.X - TargetArea.Width * 0.5f, center.Y - TargetArea.Height * 0.5f);
                relativeOrigin = new Vector2(TargetArea.Width * 0.5f, TargetArea.Height * 0.5f);
            }

            originalCenter = Center;
            relativeScreenPosition = new Vector2(upperLeft.X / RenderingDevice.Width, upperLeft.Y / RenderingDevice.Height);

            RenderingDevice.GDM.DeviceReset += rebuildCenters;
        }

        /// <summary>
        /// Draws the SuperTextor with a tint. Call SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.LinearClamp, null, null) before this and 
        /// SpriteBatch.End() after!
        /// </summary>
        /// <param name="tint">The color to tint the sprite with.</param>
        public void Draw(Color tint)
        {
            if(Texture != null)
            {
                if(Point == RenderPoint.UpLeft)
                    RenderingDevice.SpriteBatch.Draw(Texture, upperLeft, TargetArea, tint, 0, Vector2.Zero, DrawUnscaled ? Vector2.One : Scale, SpriteEffects.None, 0);
                else
                    RenderingDevice.SpriteBatch.Draw(Texture, center, TargetArea, tint, 0.0f, relativeOrigin, DrawUnscaled ? Vector2.One : Scale, SpriteEffects.None, 0);
            }
        }
        /// <summary>
        /// Draws the SuperTextor. Call SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.LinearClamp, null, null) before this and SpriteBatch.End()
        /// after!
        /// </summary>
        public void Draw()
        {
            Draw(Color.White);
        }

        /// <summary>
        /// Moves a texture given a speed and a time.
        /// </summary>
        /// <param name="speed">Speed to move at.</param>
        /// <param name="timeInSeconds">Time to move for.</param>
        public void Move(Vector2 speed, float timeInSeconds)
        {
            if(Moving)
                GameManager.Space.DuringForcesUpdateables.Starting -= update;

            if(timeInSeconds == 0)
                return;

            this.timeInSeconds = timeInSeconds;            
            this.speed = speed;
            Moving = true;

            GameManager.Space.DuringForcesUpdateables.Starting += update;
        }

        /// <summary>
        /// Moves the SuperTextor to a new location.
        /// </summary>
        /// <param name="newPosition">The new location of the SuperTextor. It will be compared based on the point selected upon
        /// creation of the SuperTextor.</param>
        /// <param name="timeInSeconds">The time to take to move there. Speed will be calculated.</param>
        public void MoveTo(Vector2 newPosition, float timeInSeconds)
        {
            if(Moving)
                GameManager.Space.DuringForcesUpdateables.Starting -= update;
            if(timeInSeconds == 0)
            {
                TeleportTo(newPosition);
                return;
            }
            target = newPosition;
            // rate = distance / time
            Vector2 distance;
            if(Point == RenderPoint.UpLeft)
                distance = target - UpperLeft;
            else if(Point == RenderPoint.Center)
                distance = target - Center;
            else
                distance = target - LowerRight;
            speed = distance * GameManager.Space.TimeStepSettings.TimeStepDuration / timeInSeconds;
            Moving = true;

            GameManager.Space.DuringForcesUpdateables.Starting += update;
        }

        private void update()
        {
            if(!Moving)
                return;
            upperLeft += speed;
            center += speed;
            lowerRight += speed;
            if(timeInSeconds != 0)
            {
                timer += GameManager.Space.TimeStepSettings.TimeStepDuration;
                if(timer >= timeInSeconds)
                    Moving = false;
            }
            else
            {
                if(Point == RenderPoint.UpLeft && Moving && compareVectors(UpperLeft, target))
                    Moving = false;
                if(Point == RenderPoint.Center && Moving && compareVectors(Center, target))
                    Moving = false;
            }

            if(!Moving)
                GameManager.Space.DuringForcesUpdateables.Starting -= update;
        }

        private bool compareVectors(Vector2 lhs, Vector2 rhs)
        {
            return Math.Abs(lhs.X - rhs.X) < epsilon && Math.Abs(lhs.Y - rhs.Y) < epsilon;
        }

        /// <summary>
        /// Teleports the SuperTextor to its starting position and resets movement.
        /// </summary>
        public void Reset()
        {
            target = center = originalCenter;
            lowerRight = new Vector2(Center.X + Width * 0.5f, Center.Y + Height * 0.5f);
            upperLeft = new Vector2(Center.X - Width * 0.5f, Center.Y - Height * 0.5f);
            Moving = false;
            speed = Vector2.Zero;
            timeInSeconds = timer = 0;
            GameManager.Space.DuringForcesUpdateables.Starting -= update;
        }

        /// <summary>
        /// Teleports the SuperTextor to a given location based on the center.
        /// </summary>
        /// <param name="location"></param>
        public void TeleportTo(Vector2 location)
        {
            originalCenter = center = location;
            lowerRight = new Vector2(Center.X + Width * 0.5f, Center.Y + Height * 0.5f);
            upperLeft = new Vector2(Center.X - Width * 0.5f, Center.Y - Height * 0.5f);
        }

        public void ForceMoveUpdate(GameTime gameTime)
        {
            if(!Moving)
                return;
            upperLeft += speed * (float)(gameTime.ElapsedGameTime.TotalSeconds / GameManager.Space.TimeStepSettings.TimeStepDuration);
            center += speed;
            lowerRight += speed;

            if(timeInSeconds != 0)
            {
                timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if(timer >= timeInSeconds)
                    Moving = false;
            }
            else
            {
                if(Point == RenderPoint.UpLeft && Moving && compareVectors(UpperLeft, target))
                    Moving = false;
                if(Point == RenderPoint.Center && Moving && compareVectors(Center, target))
                    Moving = false;
            }

            if(!Moving)
                GameManager.Space.DuringForcesUpdateables.Starting -= update;
        }

        /// <summary>
        /// Call this to force a resize based on the current resolution. Safe to call on the same resolution multiple times.
        /// You'll want to do this if you need to make sure a texture has been resized before resizing other things based on the texture.
        /// This will not force a resize if DrawUnscaled is set to true.
        /// </summary>
        public void ForceResize()
        {
            rebuildCenters(this, EventArgs.Empty);
        }

        private void rebuildCenters(object caller, EventArgs e)
        {
            if(DrawUnscaled)
                return;

            Vector2 distance = Center - originalCenter;
            const float epsilon = 0.0001f;
            if(Math.Abs(distance.X) < epsilon && Math.Abs(distance.Y) < epsilon)
                distance = Vector2.Zero;
            distance *= RenderingDevice.TextureScaleFactor;

            upperLeft = new Vector2(RenderingDevice.Width, RenderingDevice.Height) * relativeScreenPosition;
            lowerRight = new Vector2(upperLeft.X + Width, upperLeft.Y + Height);
            center = new Vector2((upperLeft.X + lowerRight.X) * 0.5f, (upperLeft.Y + lowerRight.Y) * 0.5f);

            originalCenter = Center;

            // move the texture to where it was prior
            center = distance + Center;
            lowerRight = new Vector2(Center.X + Width * 0.5f, Center.Y + Height * 0.5f);
            upperLeft = new Vector2(Center.X - Width * 0.5f, Center.Y - Height * 0.5f);
        }
    }
}
