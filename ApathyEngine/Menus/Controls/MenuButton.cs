using ApathyEngine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Menus.Controls
{
    public class MenuButton : MenuControl
    {
        public MenuButton(Sprite t, Action action)
            : this(t, action, String.Empty)
        { }

        public MenuButton(Sprite t, Action action, string helpfulText)
            : base(t, helpfulText, action)
        { }
    }
}
