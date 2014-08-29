using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Graphics
{
    public interface IRenderableObject
    {
        /// <summary>
        /// Adds an IRenderableObject to the RenderingDevice.
        /// </summary>
        void AddToRenderer();

        /// <summary>
        /// Adds an IRenderableObject to the RenderingDevice.
        /// </summary>
        void RemoveFromRenderer();
    }
}
