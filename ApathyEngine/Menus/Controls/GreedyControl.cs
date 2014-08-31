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
    public abstract class GreedyControl<T> : GreedyControl
    {
        protected Pointer<T> variable { get; private set; }
        protected readonly string controlText;
        protected Vector2 textVector;
        protected readonly FontDelegate font;
        protected Vector2 stringLength;
        protected Vector2 relativeScreenSpace;

        public override Action OnSelect { get { return this.invoke; } protected set { } }

        /// <summary>
        /// Creates a generic GreedyControl: a control that requires invocations when it is selected.
        /// </summary>
        /// <param name="variable">The variable to get/set.</param>
        /// <param name="backgroundTex">The background texture of the control.</param>
        /// <param name="text">The control's display text.</param>
        /// <param name="textV">The upper-left corner of the text.</param>
        /// <param name="f">The font to use.</param>
        protected GreedyControl(Pointer<T> variable, Sprite backgroundTex,
            string text, Vector2 textV, FontDelegate f)
        {
            this.variable = variable;
            Texture = backgroundTex;
            font = f;
            controlText = text;
            textVector = textV;
            upperLeft = new Vector2();
            lowerRight = new Vector2();
            upperLeft.X = backgroundTex.UpperLeft.X < textVector.X ? backgroundTex.UpperLeft.X : textVector.X;
            upperLeft.Y = backgroundTex.UpperLeft.Y < textVector.Y ? backgroundTex.UpperLeft.Y : textVector.Y;
            stringLength = font().MeasureString(controlText) * RenderingDevice.TextureScaleFactor;
            lowerRight.X = backgroundTex.LowerRight.X > textVector.X + stringLength.X ? backgroundTex.LowerRight.X : textVector.X + stringLength.X;
            lowerRight.Y = backgroundTex.LowerRight.Y > textVector.Y + stringLength.Y ? backgroundTex.LowerRight.Y : textVector.Y + stringLength.Y;
            IsSelected = false;

            relativeScreenSpace = textVector / new Vector2(RenderingDevice.Width, RenderingDevice.Height);
            Program.Game.GDM.DeviceReset += GDM_device_reset;
        }

        public override void Draw(MenuControl selected)
        {
            RenderingDevice.SpriteBatch.DrawString(font(), controlText, textVector, IsDisabled ? DisabledTint : textColor, 0, Vector2.Zero, RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
            base.Draw(selected);
        }

        protected virtual void GDM_device_reset(object caller, EventArgs e)
        {
            textVector = new Vector2(RenderingDevice.Width, RenderingDevice.Height) * relativeScreenSpace;

            Texture.ForceResize();
            Sprite backgroundTex = Texture;
            stringLength = font().MeasureString(controlText) * RenderingDevice.TextureScaleFactor;

            upperLeft.X = backgroundTex.UpperLeft.X < textVector.X ? backgroundTex.UpperLeft.X : textVector.X;
            upperLeft.Y = backgroundTex.UpperLeft.Y < textVector.Y ? backgroundTex.UpperLeft.Y : textVector.Y;
            lowerRight.X = backgroundTex.LowerRight.X > textVector.X + stringLength.X ? backgroundTex.LowerRight.X : textVector.X + stringLength.X;
            lowerRight.Y = backgroundTex.LowerRight.Y > textVector.Y + stringLength.Y ? backgroundTex.LowerRight.Y : textVector.Y + stringLength.Y;
        }
    }

    /// <summary>
    /// The only purpose for this class is to provide a solution for iterating through GreedyControls
    /// when the generic part doesn't matter.
    /// </summary>
    public abstract class GreedyControl : MenuControl
    {
        protected readonly Color textColor = Color.White;
        protected readonly Color invocationColor = Color.Green;

        /// <summary>
        /// This will be the upper-left of the text's vector and the background texture. Test it against any other applicable textures
        /// in the control.
        /// </summary>
        protected Vector2 upperLeft;
        /// <summary>
        /// This will be the lower-right of the text's vector and the background texture. Test it against any other applicable textures
        /// in the control.
        /// </summary>
        protected Vector2 lowerRight;

        public virtual bool IsActive { get; protected set; }

        protected abstract void invoke();

        /// <summary>
        /// Draws texture differently from MenuControl.Draw(). Does not call MenuControl.Draw(). Draws text.
        /// </summary>
        public override void Draw(MenuControl selected)
        {
            if(IsDisabled)
            {
                Texture.Draw(DisabledTint);
                return;
            }
            if(IsSelected.HasValue && IsSelected.Value && !IsActive)
                Texture.Draw(DownTint);
            else if(!IsSelected.HasValue && !IsActive)
                Texture.Draw(SelectionTint);
            else if(IsActive && !IsSelected.HasValue)
                Texture.Draw(invocationColor);
            else
                Texture.Draw();
        }

        /// <summary>
        /// Checks the mouse within the upper-left and lower-right of the sum of all drawn controls.
        /// </summary>
        /// <param name="selected">The currently selected control.</param>
        /// <returns>True if mouse is held down, null if rolled over or the mouse hasn't rolled over anything else, false if
        /// not selected. Also sets the return value to IsSelected.</returns>
        public override bool? CheckMouseInput(MenuControl selected)
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

            bool withinCoords = InputManager.MouseState.IsWithinCoordinates(upperLeft, lowerRight) && !transparent;
            bool eligible = Program.Game.IsActive && !MenuHandler.MouseTempDisabled && !IsDisabled;
            if(!eligible)
                return IsSelected;
            if(withinCoords && eligible && InputManager.MouseState.LeftButton == ButtonState.Pressed)
                IsSelected = true;
            else if(withinCoords && eligible && InputManager.MouseState.LeftButton != ButtonState.Pressed)
                IsSelected = null;
            else if(!withinCoords && this == selected)
                IsSelected = null;
            else
                IsSelected = false;
            return IsSelected;
        }
    }
}
