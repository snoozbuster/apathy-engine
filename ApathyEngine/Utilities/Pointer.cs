using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Utilities
{
    /// <summary>
    /// A class that uses delegates to simulate a C-style pointer.
    /// </summary>
    /// <typeparam name="T">The type of the thing pointed to.</typeparam>
    public sealed class Pointer<T>
    {
        private Func<T> getter;
        private Action<T> setter;

        /// <summary>
        /// Creates a new Pointer.
        /// </summary>
        /// <param name="getter">Delegate that will be used when the pointer is read from.</param>
        /// <param name="setter">Delegate that will be used when the pointer is written to.</param>
        public Pointer(Func<T> getter, Action<T> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }

        /// <summary>
        /// Gets or sets the pointed to value, using the delegates provided in the constructor.
        /// </summary>
        public T Value
        {
            get { return getter(); }
            set { setter(value); }
        }
    }
}
