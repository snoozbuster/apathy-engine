using ApathyEngine.Graphics;
using ApathyEngine.Input;
using ApathyEngine.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Menus.Controls
{
    public abstract class MenuControl
    {
        public static readonly Color SelectionTint = Color.Red;
        public static readonly Color DownTint = new Color(180, 0, 0);
        public static readonly Color DisabledTint = new Color(128, 128, 128, 128);
        /// <summary>
        /// The MenuControl that should be selected when Left is pressed and this is the selected MenuControl.
        /// </summary>
        public virtual MenuControl OnLeft { get; protected set; }
        /// <summary>
        /// The MenuControl that should be selected when Right is pressed and this is the selected MenuControl.
        /// </summary>
        public virtual MenuControl OnRight { get; protected set; }
        /// <summary>
        /// The MenuControl that should be selected when Up is pressed and this is the selected MenuControl.
        /// </summary>
        public virtual MenuControl OnUp { get; protected set; }
        /// <summary>
        /// The MenuControl that should be selected when Down is pressed and this is the selected MenuControl.
        /// </summary>
        public virtual MenuControl OnDown { get; protected set; }
        /// <summary>
        /// An Action defining what should be done when this control is clicked on.
        /// </summary>
        public virtual Action OnSelect { get; protected set; }
        public Sprite Texture { get; protected set; }

        public virtual bool? IsSelected { get; set; }
        public virtual bool IsDisabled { get; set; }

        /// <summary>
        /// This string displays a helpful bit of text about what the control does. %s% means print selection keys, %b% means 
        /// print Escape and Back, %lr% means print left and right icons.
        /// </summary>
        public string HelpfulText { get; protected set; }

        protected bool hasTransparency = false;

        /// <summary>
        /// Creates a new MenuControl.
        /// </summary>
        /// <param name="t">The texture to use.</param>
        /// <param name="onSelect">The action to invoke on selection.</param>
        protected MenuControl(Sprite t, string tooltip, Action onSelect)
        {
            Texture = t;
            OnSelect = onSelect;
            IsSelected = false;
            HelpfulText = tooltip;
        }

        /// <summary>
        /// Only GreedyControl is allowed to call this.
        /// </summary>
        protected internal MenuControl() { }

        /// <summary>
        /// Sets the directionals of the control. I'd recommend calling this.
        /// </summary>
        /// <param name="directionals">
        /// [0] - OnLeft
        /// [1] - OnRight
        /// [2] - OnUp
        /// [3] - OnDown</param>
        public virtual void SetDirectionals(MenuControl left, MenuControl right, MenuControl up, MenuControl down)
        {
            OnLeft = left;
            OnRight = right;
            OnUp = up;
            OnDown = down;
        }

        public void MakeTransparencySensitive()
        {
            hasTransparency = true;
        }

        /// <summary>
        /// Checks for mouse input.
        /// </summary>
        /// <returns>Returns false if nothing, null if rolled over, true if down. The value it returns will
        /// be the same as the one in IsSelected.</returns>
        public virtual bool? CheckMouseInput(MenuControl selected)
        {
            bool transparent = false;
            if(hasTransparency)
            {
                Vector2 antiscale = new Vector2(1 / Texture.Scale.X, 1 / Texture.Scale.Y);
                Color[] pixel = new Color[] { new Color(1, 1, 1, 1) };
                int relativeX = (int)((InputManager.MouseState.X - Texture.UpperLeft.X) * antiscale.X) + (Texture.TargetArea == Texture.Texture.Bounds ? 0 : Texture.TargetArea.X);
                int relativeY = (int)((InputManager.MouseState.Y - Texture.UpperLeft.Y) * antiscale.Y) + (Texture.TargetArea == Texture.Texture.Bounds ? 0 : Texture.TargetArea.Y);
                if(relativeX > 0 && relativeY > 0 && relativeX < Texture.Texture.Width && relativeY < Texture.Texture.Height)
                    Texture.Texture.GetData(0, new Rectangle(relativeX, relativeY, 1, 1), pixel, 0, 1);
                if(pixel[0].A == 0)
                    transparent = true;
            }

            bool withinCoords = InputManager.MouseState.IsWithinCoordinates(Texture) && !transparent;
            bool eligible = Program.Game.IsActive && !MenuHandler.MouseTempDisabled && !IsDisabled;
            if(!eligible)
                return IsSelected;

            if(withinCoords && eligible && InputManager.MouseState.LeftButton == ButtonState.Pressed && this == selected)
                IsSelected = true;
            else if(withinCoords && eligible && InputManager.MouseState.LeftButton != ButtonState.Pressed)
                IsSelected = null;
            else if(!withinCoords && this == selected)
                IsSelected = null;
            else
                IsSelected = false;
            return IsSelected;
        }

        /// <summary>
        /// Draws the control.
        /// </summary>
        public virtual void Draw(MenuControl selected)
        {
            this.Draw(selected, Color.White);
        }

        internal virtual void Draw(MenuControl selected, Color tint)
        {
            if(IsDisabled)
                Texture.Draw(new Color(new Vector4(DisabledTint.ToVector3(), tint.A / 255f)));
            else if(IsSelected.HasValue && IsSelected.Value)
                Texture.Draw(new Color(new Vector4(DownTint.ToVector3(), tint.A / 255f)));
            else if(!IsSelected.HasValue)
                Texture.Draw(new Color(new Vector4(SelectionTint.ToVector3(), tint.A / 255f)));
            else
                Texture.Draw(tint);
        }
    }
}
