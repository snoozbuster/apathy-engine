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
    public class ButtonControl : GreedyControl<Buttons>
    {
        protected bool allowDuplicates;

        public ButtonControl(Sprite borderTex, bool allowDupes, Vector2 stringV, string text,
            FontDelegate font, Pointer<Buttons> variable)
            : base(variable, borderTex, text, stringV, font)
        {
            allowDuplicates = allowDupes;
            HelpfulText = "Press %s% or click the box and then press the desired Xbox button. Press %b% to cancel.";
        }

        protected override void invoke()
        {
            if(IsSelected.HasValue && IsSelected.Value)
                return; // cause we're still holding the button down.

            IsActive = true;

            if(InputManager.KeyboardState.WasKeyJustPressed(Keys.Escape) || InputManager.CurrentPad.WasButtonJustPressed(Buttons.Back))
            {
                IsActive = false;
                return;
            }

            Buttons? pressedKey = getFirstButton(InputManager.CurrentPad);
            if(pressedKey.HasValue)
            {
                Program.Game.Manager.CurrentSaveXboxOptions.Swap(variable, pressedKey.Value);
                IsActive = false;
            }
        }

        protected Buttons? getFirstButton(IGamepad thisFrame)
        {
            if(thisFrame.Buttons.A == ButtonState.Pressed)
                return Buttons.A;
            if(thisFrame.Buttons.B == ButtonState.Pressed)
                return Buttons.B;
            if(thisFrame.Buttons.LeftShoulder == ButtonState.Pressed)
                return Buttons.LeftShoulder;
            if(thisFrame.Buttons.LeftStick == ButtonState.Pressed)
                return Buttons.LeftStick;
            if(thisFrame.Buttons.RightShoulder == ButtonState.Pressed)
                return Buttons.RightShoulder;
            if(thisFrame.Buttons.RightStick == ButtonState.Pressed)
                return Buttons.RightStick;
            if(thisFrame.Buttons.Start == ButtonState.Pressed)
                return Buttons.Start;
            if(thisFrame.Buttons.X == ButtonState.Pressed)
                return Buttons.X;
            if(thisFrame.Buttons.Y == ButtonState.Pressed)
                return Buttons.Y;
            if(thisFrame.DPad.Down == ButtonState.Pressed)
                return Buttons.DPadDown;
            if(thisFrame.DPad.Up == ButtonState.Pressed)
                return Buttons.DPadUp;
            if(thisFrame.DPad.Left == ButtonState.Pressed)
                return Buttons.DPadLeft;
            if(thisFrame.DPad.Right == ButtonState.Pressed)
                return Buttons.DPadRight;
            if(thisFrame.Triggers.Left > 0.75f)
                return Buttons.LeftTrigger;
            if(thisFrame.Triggers.Right > 0.75f)
                return Buttons.RightTrigger;
            if(thisFrame.ThumbSticks.Left.X > 0.75f)
                return Buttons.LeftThumbstickRight;
            if(thisFrame.ThumbSticks.Left.Y > 0.75f)
                return Buttons.LeftThumbstickUp;
            if(thisFrame.ThumbSticks.Left.X < -0.75f)
                return Buttons.LeftThumbstickLeft;
            if(thisFrame.ThumbSticks.Left.Y < -0.75f)
                return Buttons.LeftThumbstickDown;
            if(thisFrame.ThumbSticks.Right.X > 0.75f)
                return Buttons.RightThumbstickRight;
            if(thisFrame.ThumbSticks.Right.Y > 0.75f)
                return Buttons.RightThumbstickUp;
            if(thisFrame.ThumbSticks.Right.X < -0.75f)
                return Buttons.RightThumbstickLeft;
            if(thisFrame.ThumbSticks.Right.Y < -0.75f)
                return Buttons.RightThumbstickDown;
            return null;
        }

        public override void Draw(MenuControl selectedControl)
        {
            SymbolWriter.WriteXboxIcon(variable.Value, Texture.Center, true);
            base.Draw(selectedControl);
        }
    }

}
