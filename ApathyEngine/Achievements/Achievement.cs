using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApathyEngine.Achievements
{
    /// <summary>
    /// Base Achievement class.
    /// </summary>
    public abstract class Achievement : IXmlSerializable
    {
        /// <summary>
        /// This is where you set the texture for the achiemvements to read from.
        /// </summary>
        public static Texture2D AchievementTexture { protected get; set; }

        /// <summary>
        /// Gets the text of the achievement. This is exactly what needs to be done to get it, and should
        /// only be displayed after getting the achievement.
        /// </summary>
        public string Text { get; private set; }
        /// <summary>
        /// Gets the hint of the achievement. This is what should be displayed until the achievement is gotten.
        /// </summary>
        public string Hint { get; private set; }
        /// <summary>
        /// Gets the name of the achievement.
        /// </summary>
        public string Title { get; private set; }
        /// <summary>
        /// Indicates if the achievement has been completed. Does not update progress.
        /// </summary>
        public bool Completed { get { return currentProgress == maxProgress; } }
        /// <summary>
        /// Gets the achievement's icon.
        /// </summary>
        public Texture2D Icon { get { return AchievementTexture; } }
        /// <summary>
        /// Indicates how often the player should be notified of achievement progress, if applicable.
        /// The player is always notified the first time.
        /// </summary>
        public int NotifyEvery { get; protected set; }
        /// <summary>
        /// The Rectangle indicating the portion of Icon to draw.
        /// </summary>
        public Rectangle RenderRectangle { get; private set; }
        /// <summary>
        /// The max progress of the achievement.
        /// </summary>
        public int Max { get { return maxProgress; } }
        /// <summary>
        /// The current progress of the achievement.
        /// </summary>
        public int Current { get { return currentProgress; } }
        /// <summary>
        /// Gets the unique ID of the achievement.
        /// </summary>
        public string ID { get; private set; }

        protected int currentProgress = 0;
        protected int maxProgress;

        /// <summary>
        /// Create a new instance of the Achievement. This will be called exactly once per save file.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="achText"></param>
        /// <param name="hint"></param>
        /// <param name="ID"></param>
        /// <param name="renderSpace"></param>
        protected Achievement(string title, string achText, string hint, string ID, Rectangle renderSpace)
        {
            Text = achText;
            Title = title;
            NotifyEvery = 1;
            Hint = hint;

            if(renderSpace == null)
                RenderRectangle = new Rectangle(0, 0, Icon.Width, Icon.Height);
            else
                RenderRectangle = renderSpace;

            if(ID.Contains(' '))
                throw new ArgumentException("ID cannot contain whitespace.", "ID");
            this.ID = ID;
        }

        /// <summary>
        /// Checks the progress of an achievement.
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values.</param>
        /// <param name="current">Current progress.</param>
        /// <param name="max">Maximum progress.</param>
        /// <returns>If the achievement is gotten, ie if current == max.</returns>
        public abstract bool CheckProgress(GameTime gameTime, out int current, out int max);

        #region XML serialization boilerplate
        public sealed override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public sealed override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public sealed override string ToString()
        {
            return "Objective";
        }
        #endregion

        /// <summary>
        /// Reads XML from a file into the accomplishment.
        /// </summary>
        /// <param name="reader"></param>
        public void ReadXml(XmlReader reader)
        {
            //reader.ReadStartElement();
            currentProgress = reader.ReadElementContentAsInt();
            //reader.ReadEndElement();
        }

        /// <summary>
        /// Writes data from the accomplishment into XML.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(ID);
            writer.WriteString(currentProgress.ToString());
            writer.WriteEndElement();
        }

#if DEBUG
        /// <summary>
        /// Forces an unlock of the achievement. This will be saved and will be permanant.
        /// </summary>
        public void DebugUnlock()
        {
            currentProgress = maxProgress;
        }
#endif
    }
}
