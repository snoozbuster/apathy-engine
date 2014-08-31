using ApathyEngine.Graphics;
using ApathyEngine.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Menus.Controls
{
    public class DualValueControl<T> : GreedyControl<T>
    {
        protected T value1, value2;

        protected Action drawV1, drawV2;

        /// <summary>
        /// Creates a control that can only hold two pre-provided values.
        /// </summary>
        /// <param name="t">The texture to use.</param>
        /// <param name="text"></param>
        /// <param name="textV"></param>
        /// <param name="font"></param>
        /// <param name="variable"></param>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="drawValue1">The function to execute if the value of the variable is value1.</param>
        /// <param name="drawValue2">The function to execute if the value of the variable is value2.</param>
        public DualValueControl(Sprite t, string text, Vector2 textV, FontDelegate font, Pointer<T> variable,
            T value1, T value2, Action drawValue1, Action drawValue2)
            : base(variable, t, text, textV, font)
        {
            this.value1 = value1;
            this.value2 = value2;
            drawV1 = drawValue1;
            drawV2 = drawValue2;
            HelpfulText = "Press %s% or click the box to toggle between values.";
        }

        public override void Draw(MenuControl selectedControl)
        {
            if(variable.Value.Equals(value1))
                drawV1();
            else
                drawV2();
            base.Draw(selectedControl);
        }

        protected override void invoke()
        {
            if(IsSelected.HasValue && IsSelected.Value)
                return; // cause we're still holding the button down.

            if(variable.Value.Equals(value1))
                variable.Value = value2;
            else
                variable.Value = value1;
            IsActive = false;
        }
    }

}
