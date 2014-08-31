using ApathyEngine.Graphics;
using ApathyEngine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Menus.Controls
{
    public class VariableButton : MenuButton
    {
        public override MenuControl OnDown { get { if(onDown != null) return onDown.Value; return null; } protected set { if(onDown != null) onDown.Value = value; onDown = new Pointer<MenuControl>(() => value, v => { value = v; }); OnDown = value; } }
        protected Pointer<MenuControl> onDown;
        public override MenuControl OnUp { get { if(onUp != null) return onUp.Value; return null; } protected set { if(onUp != null) onUp.Value = value; onUp = new Pointer<MenuControl>(() => value, v => { value = v; }); OnUp = value; } }
        protected Pointer<MenuControl> onUp;
        public override MenuControl OnLeft { get { if(onLeft != null) return onLeft.Value; return null; } protected set { if(onLeft != null) onLeft.Value = value; onLeft = new Pointer<MenuControl>(() => value, v => { value = v; }); OnLeft = value; } }
        protected Pointer<MenuControl> onLeft;
        public override MenuControl OnRight { get { if(onRight != null) return onRight.Value; return null; } protected set { if(onRight != null) onRight.Value = value; onRight = new Pointer<MenuControl>(() => value, v => { value = v; }); OnRight = value; } }
        protected Pointer<MenuControl> onRight;

        public VariableButton(Sprite tex, Action a, string tooltip)
            : base(tex, a, tooltip)
        { }

        /// <summary>
        /// Call this instead of the other one.
        /// </summary>
        /// <param name="onL"></param>
        /// <param name="onR"></param>
        /// <param name="onU"></param>
        /// <param name="onD"></param>
        public void SetPointerDirectionals(Pointer<MenuControl> onL, Pointer<MenuControl> onR, Pointer<MenuControl> onU, Pointer<MenuControl> onD)
        {
            onLeft = onL;
            onRight = onR;
            onUp = onU;
            onDown = onD;
        }

        public override void SetDirectionals(MenuControl left, MenuControl right, MenuControl up, MenuControl down)
        {
            throw new InvalidOperationException("Can't call the normal SetDirectionals() on a VariableControl!");
        }
    }
}
