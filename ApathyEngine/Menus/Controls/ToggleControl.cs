using ApathyEngine.Graphics;
using ApathyEngine.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Menus.Controls
{
    /// <summary>
    /// This technically isn't a GreedyControl because it doesn't command attention, but oh well.
    /// </summary>
    public class ToggleControl : GreedyControl<bool>
    {
        private readonly Sprite checkmark;
        private Action optionalAction;

        public ToggleControl(Sprite checkmark, Sprite border, Vector2 textV, string text, FontDelegate font,
            Pointer<bool> variable, string helpfulText)
            : base(variable, border, text, textV, font)
        {
            this.checkmark = checkmark;
            HelpfulText = helpfulText;
        }

        protected override void invoke()
        {
            if((IsSelected.HasValue && IsSelected.Value) || IsDisabled)
                return; // cause we're still holding the button down.

            IsActive = false;
            variable.Value = !variable.Value;

            if(optionalAction != null)
                optionalAction();
        }

        public void SetAction(Action a)
        {
            optionalAction = a;
        }

        public override void Draw(MenuControl selected)
        {
            base.Draw(selected);
            if(variable.Value)
                checkmark.Draw();
        }

        public override MenuControl OnDown { get { if(onDown != null) return onDown.Value; return null; } protected set { if(onDown != null) onDown.Value = value; onDown = new Pointer<MenuControl>(() => value, v => { value = v; }); OnDown = value; } }
        protected Pointer<MenuControl> onDown;
        public override MenuControl OnUp { get { if(onUp != null) return onUp.Value; return null; } protected set { if(onUp != null) onUp.Value = value; onUp = new Pointer<MenuControl>(() => value, v => { value = v; }); OnUp = value; } }
        protected Pointer<MenuControl> onUp;
        public override MenuControl OnLeft { get { if(onLeft != null) return onLeft.Value; return null; } protected set { if(onLeft != null) onLeft.Value = value; onLeft = new Pointer<MenuControl>(() => value, v => { value = v; }); OnLeft = value; } }
        protected Pointer<MenuControl> onLeft;
        public override MenuControl OnRight { get { if(onRight != null) return onRight.Value; return null; } protected set { if(onRight != null) onRight.Value = value; onRight = new Pointer<MenuControl>(() => value, v => { value = v; }); OnRight = value; } }
        protected Pointer<MenuControl> onRight;
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
            throw new NotSupportedException("Cannot be called on this object.");
        }
    }
}
