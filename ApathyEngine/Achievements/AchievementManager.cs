﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework.Input;
using ApathyEngine.Graphics;
using ApathyEngine.Utilities;
using ApathyEngine.Input;
using ApathyEngine.Media;

namespace ApathyEngine.Achievements
{
    public class AchievementManager
    {
        /// <summary>
        /// Fires when an achievement is unlocked.
        /// </summary>
        public event Action<Achievement> AchievementUnlocked;

        private static bool initialized = false;

        protected bool drawAchievementGotten;
        protected Achievement achToDraw;
        protected Queue<Achievement> achQueue = new Queue<Achievement>();

        protected Color textColor;
        protected Texture2D achievementTex;
        protected Vector2 scale = new Vector2(0, 0);
        protected Color tint = new Color(255, 255, 255, 0);
        protected Color titleColor { get { return new Color(194, 193, 97, tint.A); } }

        protected HelpfulTextBox text;

        protected Vector2 achievementPos;
        protected readonly Vector2 basePos;
        protected readonly Vector2 relativeScreenSpace;

        protected int height { get { return achievementTex.Height; } }
        protected int width { get { return achievementTex.Width; } }

        protected Timer timer;

        protected List<Achievement>[] achievementLists;

        protected const float deltaY = 0.1f;
        protected const float deltaX = 0.05f;
        protected const int deltaA = 13;

        protected List<Type> achTypes = new List<Type>();

        protected BaseGame game;

        public AchievementManager(BaseGame game)
        {
            if(initialized)
                throw new InvalidOperationException("AchievementManager is a singleton and has already been instantiated.");

            this.game = game;
            timer = new Timer(7000, onEvent, false);
            textColor = Color.Black;
            basePos = new Vector2(604, 620);
            relativeScreenSpace = new Vector2(basePos.X / 1280, basePos.Y / 720);

            GameManager.Manager.OnSaveDeleted += initializeList;

            RenderingDevice.GDM.DeviceReset += onGDMReset;
            achievementTex = Resources.AchievementToastTexture;
            achievementPos = new Vector2(RenderingDevice.Width, RenderingDevice.Height) * relativeScreenSpace;
            text = new HelpfulTextBox(new Rectangle((int)(achievementPos.X + (143 - achievementTex.Width * 0.5f) * RenderingDevice.TextureScaleFactor.X), (int)(achievementPos.Y + (50 - achievementTex.Height * 0.5f) * RenderingDevice.TextureScaleFactor.Y),
                (int)(332 * RenderingDevice.TextureScaleFactor.X), (int)(50 * RenderingDevice.TextureScaleFactor.Y)), () => Resources.Font);
            text.SetScaling(new Vector2(.8f, .8f));

            initialized = true;
        }

        /// <summary>
        /// After registering achievements, call this to setup the manager.
        /// </summary>
        public void Initialize()
        {
            achievementLists = new List<Achievement>[game.Manager.SaveCount];
            for(int i = 0; i < game.Manager.SaveCount; i++)
                initializeList(i);
        }

        protected void initializeList(int listNumber)
        {
            achievementLists[listNumber] = new List<Achievement>();
            foreach(Type t in achTypes)
            {
                achievementLists[listNumber].Add((Achievement)Activator.CreateInstance(t));
                if((from a in achievementLists[listNumber]
                    where a.ID == achievementLists[listNumber][achievementLists[listNumber].Count - 1].ID
                    select a).Count() > 1)
                    throw new InvalidOperationException("Achievement " + achievementLists[listNumber][achievementLists[listNumber].Count - 1].GetType().Name + " has a duplicate ID.");

            }
        }

        /// <summary>
        /// Registers an achievement type. Achievements subclassing from Achievement should have a parameterless
        /// constructor.
        /// </summary>
        /// <param name="t"></param>
        public void RegisterAccomplishment(Type t)
        {
            if(t.BaseType == typeof(Achievement))
                if(!achTypes.Contains(t))
                    achTypes.Add(t);
                else
                    throw new ArgumentException("Already exists.");
            else
                throw new ArgumentException("Not an Accomplishment.");
        }

        public virtual void Update(GameTime gameTime)
        {
#if DEBUG
            if(InputManager.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt) && InputManager.KeyboardState.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.Space))
                foreach(Achievement a in achievementLists[game.Manager.SaveCount])
                    a.DebugUnlock();
            unlockDebugAchievements();
#endif

            foreach(Achievement ach in achievementLists[game.Manager.CurrentSaveNumber].Where(a => !a.Completed))
            {
                int current, max;
                bool completed = ach.CheckProgress(gameTime, out current, out max);
                if(completed)
                {
                    if(AchievementUnlocked != null)
                        AchievementUnlocked(ach);
                    MediaSystem.PlaySoundEffect(SFXOptions.Achievement);
                    achQueue.Enqueue(ach);
                }
            }

            if(achQueue.Count > 0 && achToDraw == null)
            {
                drawAchievementGotten = true;
                timer.Start();
                achToDraw = achQueue.Dequeue();
            }

            if(drawAchievementGotten)
            {
                timer.Update(gameTime);

                if(scale.X + deltaX >= 1)
                    scale.X = 1;
                else
                    scale.X += deltaX;
                if(scale.Y + deltaY >= 1)
                    scale.Y = 1;
                else 
                    scale.Y += deltaY;
                if(tint.A + deltaA >= 255)
                    tint.A = 255;
                else
                    tint.A += deltaA;
            }
            else if(scale.X > 0)
            {
                if(scale.X - deltaX < 0)
                    scale.X = 0;
                else
                    scale.X -= deltaX;
                if(deltaX < 0.3f)
                {
                    if(scale.Y - deltaY < 0)
                        scale.Y = 0;
                    else
                        scale.Y -= deltaY;
                }
                if(tint.A - deltaA < 0)
                {
                    tint.A = 0;
                    achToDraw = null;
                }
                else
                    tint.A -= deltaA;
            }
        }

        public virtual void Draw()
        {
            if(!drawAchievementGotten && tint.A == 0)
                return;

            RenderingDevice.SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.LinearClamp, null, null);

            RenderingDevice.SpriteBatch.Draw(achievementTex, achievementPos, null, tint * (tint.A / 255f), 0, new Vector2(achievementTex.Width, achievementTex.Height) * 0.5f * RenderingDevice.TextureScaleFactor * scale, scale * RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
            
            RenderingDevice.SpriteBatch.Draw(achToDraw.Icon, (achievementPos - new Vector2(30, -6) * RenderingDevice.TextureScaleFactor), achToDraw.RenderRectangle, tint * (tint.A / 255f), 0, (new Vector2(achievementTex.Width, achievementTex.Height) * 0.5f) * RenderingDevice.TextureScaleFactor * scale, 0.78125f * scale * RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);

            Vector2 dim = Resources.BiggerFont.MeasureString(achToDraw.Title) * 0.85f;
            RenderingDevice.SpriteBatch.DrawString(Resources.BiggerFont, achToDraw.Title, achievementPos + new Vector2(-150 + achievementTex.Width * 0.5f, -2) * RenderingDevice.TextureScaleFactor, titleColor * (tint.A / 255f), 0, new Vector2(achievementTex.Width, achievementTex.Height) * 0.5f * RenderingDevice.TextureScaleFactor * scale / (dim.X > achievementTex.Width - 170 ? (achievementTex.Width - 180) / dim.X : 1), (dim.X > achievementTex.Width - 170 ? (achievementTex.Width - 180) / dim.X : 1) * 0.85f * scale * RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);

            text.SetTextColor(textColor * (tint.A / 255f));
            text.Draw(achToDraw.Text);

            Vector2 length = Resources.Font.MeasureString("Progress: Completed!") * RenderingDevice.TextureScaleFactor;
            RenderingDevice.SpriteBatch.DrawString(Resources.Font, "Progress: Completed!", achievementPos + new Vector2(achievementTex.Width * 0.5f - 42, achievementTex.Height * 0.5f - 10) * RenderingDevice.TextureScaleFactor, textColor * (tint.A / 255f), 0, length, RenderingDevice.TextureScaleFactor * scale, SpriteEffects.None, 0);

            RenderingDevice.SpriteBatch.End();
        }

        /// <summary>
        /// Gets an accomplishment from the list based on its ID.
        /// </summary>
        /// <param name="ID">The unique ID to get an achievement by.</param>
        /// <returns>The accomplishment attached to that ID, or null if that ID is not in use. Searches the current list.</returns>
        public Achievement GetAchievementByID(string ID, int saveSlot)
        {
            foreach(Achievement a in achievementLists[saveSlot-1])
                if(a.ID == ID)
                    return a;
            return null;
        }

        protected void onEvent()
        {
            drawAchievementGotten = false;
            timer.Reset();
        }

        protected virtual void onGDMReset(object sender, EventArgs e)
        {
            achievementPos = new Vector2(RenderingDevice.Width, RenderingDevice.Height) * relativeScreenSpace;
            text.SetSpace(new Rectangle((int)(achievementPos.X + (143 - achievementTex.Width * 0.5f) * RenderingDevice.TextureScaleFactor.X), 
                (int)(achievementPos.Y + (50 - achievementTex.Height * 0.5f) * RenderingDevice.TextureScaleFactor.Y),
                (int)(332 * RenderingDevice.TextureScaleFactor.X), (int)(50 * RenderingDevice.TextureScaleFactor.Y)));
        }
    }
}
