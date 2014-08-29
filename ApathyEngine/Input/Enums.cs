using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Input
{
    /// <summary>
    /// Defines the control schemes available for use.
    /// </summary>
    public enum ControlScheme
    {
        /// <summary>
        /// Indicates the current control scheme uses a keyboard and mouse.
        /// </summary>
        Keyboard,
        /// <summary>
        /// Indicates the current control scheme uses a Xbox controller.
        /// </summary>
        XboxController,
        /// <summary>
        /// Indicates that there is no currently selected control scheme.
        /// </summary>
        None
    }
}
