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
    public class MenuSlider : GreedyControl<float>
    {
        /// <summary>
        /// Background (immobile) part of the slider.
        /// </summary>
        public Sprite BackgroundTexture { get; private set; }

        /// <summary>
        /// If this is true, you should hold everything and call OnSelect(), because the user wants to send input to this slider.
        /// </summary>
        public override bool IsActive { get { return isActive; } protected set { isActive = value; if(!isActive) justInvoked = false; } }

        private readonly float minValue;
        private readonly float maxValue;

        private readonly float distance; // rhsbound - lhsbound
        private readonly float lhsBound; // in screenspace
        private readonly float rhsBound; // in screenspace

        private const int frameLapse = 20; // number of frames to wait until accepting input again
        private const int delta = 5; // amount to advance or devance when using keyboard

        /// <summary>
        /// This is to be set only by the property. PERIOD.
        /// </summary>
        private bool isActive = false;

        private int framesHeld = 0; // number of frames the button has been held down

        // true if using mouse, false if using keyboard/xbox, null if we don't know yet.
        private bool? usingMouse;
        private bool justInvoked;

        private Direction direction = Direction.None;

        private enum Direction
        {
            Left,
            Right,
            None
        }

        /// <summary>
        /// Creates a slider that can be used to modify a floating-point value.
        /// </summary>
        /// <param name="backgroundTexture">The background of the slider (immobile part).</param>
        /// <param name="foreGroundTexture">The foreground of the slider (mobile part). Should be created with the center
        /// as the render point.</param>
        /// <param name="min">The minimum value of the variable.</param>
        /// <param name="max">The maximum value of the variable.</param>
        /// <param name="distance">The distance (in pixels) the slider can travel.</param>
        /// <param name="offset">Offset of distance (in pixels) from backgroundTexture.UpperLeft.X.</param>
        /// <param name="variable">The variable to get and set.</param>
        public MenuSlider(Sprite backgroundTexture, Sprite foreGroundTexture,
            float min, float max, float distance, float offset, string text, Vector2 textVector, FontDelegate font,
            Pointer<float> variable)
            : base(variable, foreGroundTexture, text, textVector, font)
        {
            minValue = min;
            maxValue = max;
            BackgroundTexture = backgroundTexture;
            this.distance = distance;

            lhsBound = Texture.UpperLeft.X + offset;
            rhsBound = lhsBound + distance;

            HelpfulText = "Press %s% and use %lr% to adjust then press %s% again to confirm, or drag with the mouse.";
        }

        protected override void invoke()
        {
            if(IsSelected.HasValue && IsSelected.Value)
                return; // cause we're still holding the button down.

            IsActive = true;

            if(!usingMouse.HasValue)
            {
                justInvoked = true;
                if(InputManager.KeyboardState.WasKeyJustReleased(Program.Game.Manager.CurrentSaveWindowsOptions.SelectionKey) ||
                    InputManager.CurrentPad.WasButtonJustReleased(Program.Game.Manager.CurrentSaveXboxOptions.SelectionKey))
                    usingMouse = false;
                else // we got in here with the mouse. Probably.
                    usingMouse = true;
            }
            else
                justInvoked = false;

            if((InputManager.MouseState.LeftButton == ButtonState.Released && usingMouse.GetValueOrDefault()) || !Program.Game.IsActive)
            {
                IsActive = false;
                return;
            }
            else if((InputManager.KeyboardState.WasKeyJustReleased(Program.Game.Manager.CurrentSaveWindowsOptions.SelectionKey) ||
                    InputManager.CurrentPad.WasButtonJustReleased(Program.Game.Manager.CurrentSaveXboxOptions.SelectionKey)) && !justInvoked &&
                    !usingMouse.GetValueOrDefault())
            {
                IsActive = false;
                return;
            }

            #region variable updates
            if(usingMouse.GetValueOrDefault())
            {
                float x = InputManager.MouseState.X;
                if(x < lhsBound)
                    x = lhsBound;
                if(x > rhsBound)
                    x = rhsBound;
                Texture.TeleportTo(new Vector2(x, Texture.Center.Y));
                variable.Value = ((x - lhsBound) / distance) * (maxValue - minValue);
            }
            else // using keyboard
            {
                if(direction == Direction.Left)
                {
                    if(InputManager.KeyboardState.IsKeyDown(Program.Game.Manager.CurrentSaveWindowsOptions.MenuLeftKey) ||
                       InputManager.CurrentPad.IsButtonDown(Program.Game.Manager.CurrentSaveXboxOptions.MenuLeftKey))
                    {
                        if(framesHeld < frameLapse) // if the button hasn't been held down long enough, increase frames by one and return.
                            framesHeld++;
                        else
                        {
                            framesHeld = 0;
                            variable.Value -= delta;
                            if(variable.Value < minValue)
                                variable.Value = minValue;
                            Texture.TeleportTo(new Vector2(Texture.Center.Y - delta, Texture.Center.Y));
                        }
                    }
                    else // left is no longer down
                        direction = Direction.None;
                }
                else if(direction == Direction.Right)
                {
                    if(InputManager.KeyboardState.IsKeyDown(Program.Game.Manager.CurrentSaveWindowsOptions.MenuRightKey) ||
                          InputManager.CurrentPad.IsButtonDown(Program.Game.Manager.CurrentSaveXboxOptions.MenuRightKey))
                    {
                        if(framesHeld < frameLapse) // if the button hasn't been held down long enough, increase frames by one and return.
                            framesHeld++;
                        else
                        {
                            framesHeld = 0;
                            variable.Value += delta;
                            if(variable.Value > maxValue)
                                variable.Value = maxValue;
                            Texture.TeleportTo(new Vector2(Texture.Center.Y + delta, Texture.Center.Y));
                        }
                    }
                    else // left is no longer down
                        direction = Direction.None;
                }
                else // direction is none
                    framesHeld = 0; // so this can be reset
            }
            #endregion
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

        public override void Draw(MenuControl selected)
        {
            base.Draw(selected);
            BackgroundTexture.Draw();
        }
    }
}
