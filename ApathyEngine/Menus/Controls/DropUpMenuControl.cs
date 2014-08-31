using ApathyEngine.Graphics;
using ApathyEngine.Input;
using ApathyEngine.Media;
using ApathyEngine.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Menus.Controls
{
    public class DropUpMenuControl : MenuControl
    {
        protected readonly List<MenuButton> controls = new List<MenuButton>();
        protected const float dropTime = 1.1f;
        protected const int deltaA = 4;

        public bool IsActive { get; private set; }
        protected bool isFading;
        protected int alpha;

        protected Vector2 lowerRight { get { return Texture.LowerRight; } }
        protected Vector2 upperLeft { get { if(controls.Count > 0) return controls[controls.Count - 1].Texture.UpperLeft; return Texture.UpperLeft; } }

        protected Pointer<MenuControl> onLeft, onUp, onRight, onDown;
        protected MenuControl selectedLastFrame;
        protected MenuControl selectedLastFrameMouse;
        protected bool playedClickSound = false;

        public bool MouseWithin { get { return InputManager.MouseState.IsWithinCoordinates(upperLeft, lowerRight); } }

        public override bool? IsSelected
        {
            get { return base.IsSelected; }
            set
            {
                if((value == null && IsActive && MenuHandler.MouseTempDisabled) || // we need to close
                    (!MenuHandler.MouseTempDisabled && IsActive && value.HasValue && value == false))
                {
                    moveTextures(false);
                    IsActive = false;
                }
                else if(value == null && !IsActive)
                    this.invoke();
                else if(value.HasValue && value.Value)
                    value = null;
                base.IsSelected = value;
            }
        }

        public override MenuControl OnUp
        {
            get
            {
                MenuHandler.MouseTempDisabled = true;
                if(IsActive && !isFading)
                    return controls[0];
                else if(!IsActive)
                    this.invoke();
                return null;
            }
            protected set { onUp.Value = value; }
        }

        public override MenuControl OnDown
        {
            get
            {
                if(IsActive)
                {
                    moveTextures(false);
                    IsActive = false;
                }
                if(onDown != null)
                    return onDown.Value;
                return null;
            }
            protected set { onDown.Value = value; }
        }
        public override MenuControl OnLeft
        {
            get
            {
                if(IsActive)
                {
                    moveTextures(false);
                    IsActive = false;
                }
                if(onLeft != null)
                    return onLeft.Value;
                return null;
            }
            protected set { onLeft.Value = value; }
        }
        public override MenuControl OnRight
        {
            get
            {
                if(IsActive)
                {
                    moveTextures(false);
                    IsActive = false;
                }
                if(onRight != null)
                    return onRight.Value;
                return null;
            }
            protected set { onRight.Value = value; }
        }

        public override Action OnSelect
        {
            get
            {
                if(!IsActive)
                    return this.invoke;
                else return delegate
                {
                    IsActive = false;
                    moveTextures(false);
                };
            }
            protected set { }
        }

        public DropUpMenuControl(Sprite texture)
        {
            Texture = texture;
            this.HelpfulText = String.Empty;
            IsSelected = false;
        }

        public DropUpMenuControl(Sprite texture, string helpfulText)
            : this(texture)
        {
            HelpfulText = helpfulText;
        }

        /// <summary>
        /// The first will be the closest to the parent, and so on.
        /// </summary>
        /// <param name="controls">They don't need to have their directionals set. They should also have the same
        /// original position and HelpfulText (if applicable) as the parent.</param>
        public void SetInternalMenu(IList<MenuButton> controls)
        {
            this.controls.Clear();
            this.controls.AddRange(controls);
            for(int i = 0; i < controls.Count - 1; i++)
                this.controls[i].SetDirectionals(null, null, this.controls[i + 1], (i == 0 ? this as MenuControl : this.controls[i - 1]));
            this.controls[this.controls.Count - 1].SetDirectionals(null, null, null, this.controls[this.controls.Count - 2]);
        }

        /// <summary>
        /// Updates the control and its children. Automatically performs necessary mouse input for its children
        /// (the controls' own CheckMouseInput must still be called for it to update itself).
        /// </summary>
        /// <param name="selected">The currently selected control.</param>
        /// <param name="gameTime">The gameTime.</param>
        /// <returns>A reference to one of its children if one is selected and null if it is selected.</returns>
        public MenuControl Update(MenuControl selected, GameTime gameTime)
        {
            if(isFading)
            {
                if(!IsActive)
                {
                    alpha -= deltaA;
                    if(alpha < 0)
                    {
                        alpha = 0;
                        isFading = false;
                    }
                }
                else
                {
                    alpha += deltaA;
                    if(alpha > 255)
                    {
                        alpha = 255;
                        isFading = false;
                    }
                }
            }

            bool selectedWasChanged = false;

            if(IsActive || isFading)
                foreach(MenuControl c in controls)
                    c.Texture.ForceMoveUpdate(gameTime);
            if(IsActive && alpha >= 215)
            {
                foreach(MenuButton m in controls)
                {
                    if(MenuHandler.MouseTempDisabled)
                        break;

                    bool? old = m.IsSelected;
                    bool? current = m.CheckMouseInput(selected);
                    if((current == null || (current.HasValue && current.Value)) && m != selectedLastFrameMouse)
                    {
                        selected = m;
                        selectedLastFrameMouse = m;
                        selectedWasChanged = true;
                        current = m.CheckMouseInput(selected);
                    }

                    if(old.HasValue && !old.Value && !current.HasValue)
                        MediaSystem.PlaySoundEffect(SFXOptions.Button_Rollover);
                    else if(!old.HasValue && current.HasValue && current.Value)
                    {
                        if(!playedClickSound)
                            MediaSystem.PlaySoundEffect(SFXOptions.Button_Press);
                        playedClickSound = true;
                    }
                    else if(old.HasValue && old.Value && !current.HasValue && InputManager.MouseState.LeftButton == ButtonState.Released)
                    {
                        MediaSystem.PlaySoundEffect(SFXOptions.Button_Release);
                        m.OnSelect();
                        playedClickSound = false;
                        return null; // necessary?
                    }
                    else if(InputManager.MouseState.LeftButton == ButtonState.Released)
                        playedClickSound = false;
                }
            }
            else if(!IsActive)
            {
                playedClickSound = false;
                foreach(MenuButton b in controls)
                    b.IsSelected = false;
                if(selected == this || controls.Contains(selected))
                {
                    base.IsSelected = null;
                    return this;
                }
            }

            if(selectedWasChanged)
                return selected;

            foreach(MenuControl c in controls)
                if(!c.IsSelected.HasValue || (c.IsSelected.HasValue && c.IsSelected.Value))
                {
                    selectedLastFrameMouse = c;
                    return c;
                }
            return null;
        }

        public override void Draw(MenuControl selected)
        {
            if(IsActive || isFading)
                foreach(MenuControl c in controls)
                    c.Draw(selected, new Color(255, 255, 255, alpha));
            base.Draw(selected);
        }

        public void SetPointerDirectionals(Pointer<MenuControl> left, Pointer<MenuControl> right,
            Pointer<MenuControl> up, Pointer<MenuControl> down)
        {
            onLeft = left;
            onRight = right;
            onUp = up;
            onDown = down;
        }

        public override void SetDirectionals(MenuControl left, MenuControl right, MenuControl up, MenuControl down)
        {
            if(left != null)
                onLeft = new Pointer<MenuControl>(() => left, v => { });
            else
                onLeft = null;

            if(right != null)
                onRight = new Pointer<MenuControl>(() => right, v => { });
            else
                onRight = null;

            if(up != null)
                onUp = new Pointer<MenuControl>(() => up, v => { });
            else
                onUp = null;

            if(down != null)
                onDown = new Pointer<MenuControl>(() => down, v => { });
            else
                onDown = null;
        }

        protected void invoke()
        {
            IsActive = true;
            moveTextures(true);
        }

        protected void moveTextures(bool forward)
        {
            isFading = true;
            if(forward)
            {
                float offset = Texture.Height * RenderingDevice.TextureScaleFactor.Y * 0.1f; // tenth of texture height
                Vector2 temp = (controls[0].Texture.Point == Sprite.RenderPoint.Center ? Texture.Center : Texture.UpperLeft) - new Vector2(0, offset + controls[0].Texture.Height);
                controls[0].Texture.MoveTo(temp, dropTime);
                for(int i = 1; i < controls.Count; i++)
                {
                    temp = (controls[i].Texture.Point == Sprite.RenderPoint.Center ? Texture.Center : Texture.UpperLeft) - new Vector2(0, (offset + controls[i - 1].Texture.Height) * (i + 1));
                    controls[i].Texture.MoveTo(temp, dropTime);
                }
            }
            else
                foreach(MenuControl c in controls)
                    c.Texture.MoveTo(c.Texture.Point == Sprite.RenderPoint.Center ? Texture.Center : Texture.UpperLeft,
                       dropTime);
        }

        public override bool? CheckMouseInput(MenuControl selected)
        {
            bool eligible = Program.Game.IsActive && !MenuHandler.MouseTempDisabled && !IsDisabled;
            if(!eligible)
                return IsSelected;

            if(!InputManager.MouseState.IsWithinCoordinates(upperLeft, lowerRight) && IsActive)
            {
                IsActive = false;
                moveTextures(false);
            }
            else if(InputManager.MouseState.IsWithinCoordinates(upperLeft, lowerRight) && !IsActive && alpha > 0)
            {
                IsActive = true;
                moveTextures(true);
            }

            bool withinCoords = InputManager.MouseState.IsWithinCoordinates(Texture);
            if(withinCoords && eligible && !IsActive)// && this != selectedLastFrame)
            {
                selectedLastFrame.IsSelected = false;
                base.IsSelected = null;
                invoke();
            }
            else if((withinCoords && IsActive) || this == selected)
                base.IsSelected = null;
            else if(!withinCoords && IsActive)
                base.IsSelected = false;
            else
                base.IsSelected = false;

            selectedLastFrame = selected;
            return IsSelected;
        }

        public void SetNewPosition(Vector2 pos)
        {
            Texture.TeleportTo(pos);
            foreach(MenuControl c in controls)
                c.Texture.TeleportTo(pos);
        }
    }
}
