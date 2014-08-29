using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.IO
{
    /// <summary>
    /// Defines a savefile structure.
    /// </summary>
    /// <typeparam name="T">The type of save data contained in the file.</typeparam>
    public abstract class SaveFile<T> : ICloneable
    {
        /// <summary>
        /// Gets the saves stored in this save file.
        /// </summary>
        public virtual T[] Saves { get; protected set; }

        /// <summary>
        /// Creates a save file for use in an IOManager.
        /// </summary>
        /// <param name="numSaves">The number of saves this SaveFile can hold.</param>
        public SaveFile(int numSaves)
        {
            Saves = new T[numSaves];
        }

        /// <summary>
        /// Clones the save file into a new object.
        /// </summary>
        /// <returns>A deep copy of the current SaveFile.</returns>
        public abstract object Clone();
    }
}
