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
    public class KeyControl : GreedyControl<Keys>
    {
        private readonly bool allowDuplicates;

        public KeyControl(Sprite borderTex, bool allowDuplicates, Vector2 stringV, string text,
            FontDelegate font, Pointer<Keys> variable)
            : base(variable, borderTex, text, stringV, font)
        {
            this.allowDuplicates = allowDuplicates;
            HelpfulText = "Press %s% or click the box and then press the desired keyboard key. Press %b% to cancel selection of a new key.";
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

            Keys[] pressedKeys = InputManager.KeyboardState.GetPressedKeys();
            if(pressedKeys.Length > 0)
            {
                Keys firstPressedKey = pressedKeys[0];
                if(Program.Game.Manager.CurrentSaveWindowsOptions.DetermineIfSupported(firstPressedKey) &&
                    !Program.Game.Manager.CurrentSaveWindowsOptions.FindDuplicateKeys(firstPressedKey, allowDuplicates))
                    variable.Value = firstPressedKey;

                IsActive = false;
            }
        }

        public override void Draw(MenuControl selected)
        {
            base.Draw(selected);
            SymbolWriter.WriteKeyboardIcon(variable.Value, Texture.Center, true);
        }
    }
}
