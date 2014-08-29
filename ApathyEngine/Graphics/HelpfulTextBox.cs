using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Net;
using ApathyEngine.Input;

namespace ApathyEngine.Graphics
{
    public sealed class HelpfulTextBox
    {
        private Rectangle screenSpace;
        private Color color = Color.White;
        private readonly FontDelegate font;
        private readonly Vector2 stringPos;
        private readonly Vector2 replaceLR1, replaceLR2, replaceSorBV, replaceCam1, spacesVector;

        private const string spacesPerIcon = "      ";

        private const string printLR = "%lr%";
        private const string printS = "%s%";
        private const string printB = "%b%";
        private const string printCam = "%c%";
        private const string printNum1 = "%1%";
        private const string printTilde = "%~%";
        private const string printRes = "%r%";
        private const string printZoom = "%z%";
        private const string printHelp = "%h%";

        private const string replaceLR = spacesPerIcon + " " + spacesPerIcon;
        private const string replaceSorB = spacesPerIcon;
#if WINDOWS
        private const string replaceCam = spacesPerIcon;
#elif XBOX
        private const string replaceCam = spacesPerIcon;
#endif

        private string previousFrameText = "";
        private string[] previousFrameFormattedText;

        private float newlineHalfHeight { get { return font().MeasureString("\n").Y * RenderingDevice.TextureScaleFactor.Y * 0.5f; } }
        private Vector2 upperLeft { get { return new Vector2(screenSpace.X, screenSpace.Y); } }

        private Vector2 scale = Vector2.One;

        /// <summary>
        /// Creates a text box that does nice things.
        /// </summary>
        /// <param name="space">The screen space this text box has available.</param>
        public HelpfulTextBox(Rectangle space, FontDelegate font)
        {
            screenSpace = space;
            this.font = font;
            previousFrameFormattedText = new string[0];

            spacesVector = font().MeasureString(spacesPerIcon);
            replaceLR1 = new Vector2(spacesVector.X * 0.5f, spacesVector.Y * 0.5f);
            replaceLR2 = new Vector2(spacesVector.X * 1.5f + font().MeasureString(" ").X, spacesVector.Y * 0.5f);
            replaceSorBV = new Vector2(spacesVector.X * 0.5f, spacesVector.Y * 0.5f);
            replaceCam1 = new Vector2(spacesVector.X * 0.5f, spacesVector.Y * 0.5f);

            stringPos = new Vector2(screenSpace.X, screenSpace.Y);
        }

        public void SetSpace(Rectangle r)
        {
            screenSpace = r;
        }

        public void SetTextColor(Color c)
        {
            color = c;
        }

        /// <summary>
        /// Warning: This does not scale icons.
        /// Warning: If the HelpfulTextBox detects that the text will overrun the vertical boundary, it will attempt to set this until
        /// the text no longer does. It will reset to the original scale after drawing.
        /// </summary>
        /// <param name="s"></param>
        public void SetScaling(Vector2 s)
        {
            scale = s;
        }

        /// <summary>
        /// Draws text. 
        /// </summary>
        /// <param name="text">Expects the text to be in the same form as the HelpfulText property in MenuControl.</param>
        public void Draw(string text)
        {
            string[] formattedText = convertString(text);
            string temp = "";
            Vector2 previous;
            int numNewlines = 0;
            for(int i = 0; i < formattedText.Length; i++)
            {
                if(formattedText[i] == printS)
                {
                    previous = font().MeasureString(temp) * RenderingDevice.TextureScaleFactor * scale;
                    if(InputManager.ControlScheme == ControlScheme.Keyboard)
                        SymbolWriter.WriteKeyboardIcon(InputManager.WindowsOptions.SelectionKey,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + spacesVector * 0.5f * RenderingDevice.TextureScaleFactor + upperLeft, color.A);
                    else
                        SymbolWriter.WriteXboxIcon(InputManager.XboxOptions.SelectionKey,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + spacesVector * 0.5f * RenderingDevice.TextureScaleFactor + upperLeft, color.A);
                    formattedText[i] = spacesPerIcon;
                    temp += spacesPerIcon + " ";
                }
                else if(formattedText[i] == printB)
                {
                    previous = font().MeasureString(temp) * RenderingDevice.TextureScaleFactor * scale;
                    if(InputManager.ControlScheme == ControlScheme.Keyboard)
                        SymbolWriter.WriteKeyboardIcon(Keys.Escape,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + spacesVector * 0.5f * RenderingDevice.TextureScaleFactor + upperLeft, color.A);
                    else
                        SymbolWriter.WriteXboxIcon(Buttons.Back,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + spacesVector * 0.5f * RenderingDevice.TextureScaleFactor + upperLeft, color.A);
                    formattedText[i] = spacesPerIcon;
                    temp += spacesPerIcon + " ";
                }
                else if(formattedText[i] == printLR)
                {
                    previous = font().MeasureString(temp) * RenderingDevice.TextureScaleFactor * scale;
                    if(InputManager.ControlScheme == ControlScheme.Keyboard)
                    {
                        SymbolWriter.WriteKeyboardIcon(InputManager.WindowsOptions.MenuLeftKey,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + upperLeft + replaceLR1 * RenderingDevice.TextureScaleFactor, color.A);
                        SymbolWriter.WriteKeyboardIcon(InputManager.WindowsOptions.MenuRightKey,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + upperLeft + replaceLR2 * RenderingDevice.TextureScaleFactor, color.A);
                    }
                    else
                        SymbolWriter.WriteXboxIcon(InputManager.XboxOptions.MenuLeftKey,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + upperLeft + (replaceLR1 + replaceLR2) * 0.5f * RenderingDevice.TextureScaleFactor, color.A);
                    formattedText[i] = replaceLR;
                    temp += replaceLR + " ";
                }
                else if(formattedText[i] == printCam)
                {
                    previous = font().MeasureString(temp) * RenderingDevice.TextureScaleFactor * scale;
                    if(InputManager.ControlScheme == ControlScheme.XboxController)
                    {
                        SymbolWriter.WriteXboxIcon(Buttons.LeftThumbstickLeft,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + replaceLR1 * RenderingDevice.TextureScaleFactor + upperLeft, color.A);
                        SymbolWriter.WriteXboxIcon(Buttons.RightThumbstickLeft,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + replaceLR2 * RenderingDevice.TextureScaleFactor + upperLeft, color.A);
                        formattedText[i] = replaceCam;
                        temp += replaceCam + " ";
                    }
                    else // Windows
                    {
                        SymbolWriter.WriteKeyboardIcon(InputManager.WindowsOptions.CameraUpKey,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + upperLeft + replaceLR1 * RenderingDevice.TextureScaleFactor, color.A);
                        SymbolWriter.WriteKeyboardIcon(InputManager.WindowsOptions.CameraLeftKey,
                            new Vector2(previous.X - replaceLR1.X * 0.5f * RenderingDevice.TextureScaleFactor.X, numNewlines * newlineHalfHeight) + upperLeft + replaceLR2 * RenderingDevice.TextureScaleFactor, color.A);
                        SymbolWriter.WriteKeyboardIcon(InputManager.WindowsOptions.CameraDownKey,
                            new Vector2(previous.X + replaceLR2.X * 1.1f * RenderingDevice.TextureScaleFactor.X, numNewlines * newlineHalfHeight) + upperLeft + replaceLR1 * RenderingDevice.TextureScaleFactor, color.A);
                        SymbolWriter.WriteKeyboardIcon(InputManager.WindowsOptions.CameraRightKey,
                            new Vector2(previous.X + replaceLR2.X * 0.95f * RenderingDevice.TextureScaleFactor.X, numNewlines * newlineHalfHeight) + upperLeft + replaceLR2 * RenderingDevice.TextureScaleFactor, color.A);
                        formattedText[i] = replaceLR + replaceLR;
                        temp += replaceLR + replaceLR + " ";
                    }
                }
                else if(formattedText[i] == printNum1)
                {
                    previous = font().MeasureString(temp) * RenderingDevice.TextureScaleFactor * scale;
                    if(InputManager.ControlScheme == ControlScheme.XboxController)
                        SymbolWriter.WriteXboxIcon(InputManager.XboxOptions.Machine1Key,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + spacesVector * 0.5f * RenderingDevice.TextureScaleFactor + upperLeft, color.A);
                    else
                        SymbolWriter.WriteKeyboardIcon(InputManager.WindowsOptions.Machine1Key,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + spacesVector * 0.5f * RenderingDevice.TextureScaleFactor + upperLeft, color.A);
                    formattedText[i] = spacesPerIcon;
                    temp += spacesPerIcon + " ";
                }
                else if(formattedText[i] == printTilde)
                {
                    previous = font().MeasureString(temp) * RenderingDevice.TextureScaleFactor * scale;
                    if(InputManager.ControlScheme == ControlScheme.XboxController)
                        SymbolWriter.WriteXboxIcon(InputManager.XboxOptions.QuickBoxKey,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + spacesVector * 0.5f * RenderingDevice.TextureScaleFactor + upperLeft, color.A);
                    else
                        SymbolWriter.WriteKeyboardIcon(InputManager.WindowsOptions.QuickBoxKey,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + spacesVector * 0.5f * RenderingDevice.TextureScaleFactor + upperLeft, color.A);
                    formattedText[i] = spacesPerIcon;
                    temp += spacesPerIcon + " ";
                }
                else if(formattedText[i] == printRes)
                {
                    formattedText[i] = RenderingDevice.Width + "x" + RenderingDevice.Height;
                    temp += formattedText[i] + " ";
                }
                else if(formattedText[i] == printZoom)
                {
                    previous = font().MeasureString(temp) * RenderingDevice.TextureScaleFactor * scale;
                    if(InputManager.ControlScheme == ControlScheme.Keyboard)
                    {
                        SymbolWriter.WriteKeyboardIcon(InputManager.WindowsOptions.CameraZoomPlusKey,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + upperLeft + replaceLR1 * RenderingDevice.TextureScaleFactor, color.A);
                        SymbolWriter.WriteKeyboardIcon(InputManager.WindowsOptions.CameraZoomMinusKey,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + upperLeft + replaceLR2 * RenderingDevice.TextureScaleFactor, color.A);
                    }
                    else
                    {
                        SymbolWriter.WriteXboxIcon(InputManager.XboxOptions.CameraZoomPlusKey,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + upperLeft + replaceLR1 * RenderingDevice.TextureScaleFactor, color.A);
                        SymbolWriter.WriteXboxIcon(InputManager.XboxOptions.CameraZoomMinusKey,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + upperLeft + replaceLR2 * RenderingDevice.TextureScaleFactor, color.A);
                    }
                    formattedText[i] = replaceLR;
                    temp += replaceLR + " ";
                }
                else if(formattedText[i] == printHelp)
                {
                    previous = font().MeasureString(temp) * RenderingDevice.TextureScaleFactor * scale;
                    if(InputManager.ControlScheme == ControlScheme.XboxController)
                        SymbolWriter.WriteXboxIcon(Buttons.Back,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + spacesVector * 0.5f * RenderingDevice.TextureScaleFactor + upperLeft, color.A);
                    else
                        SymbolWriter.WriteKeyboardIcon(Keys.Tab,
                            new Vector2(previous.X, numNewlines * newlineHalfHeight) + spacesVector * 0.5f * RenderingDevice.TextureScaleFactor + upperLeft, color.A);
                    formattedText[i] = spacesPerIcon;
                    temp += spacesPerIcon + " ";
                }
                else if(formattedText[i] != "\n")
                    temp += formattedText[i] + " ";
                else
                {
                    temp = "";
                    numNewlines++;
                }
            }

            string draw = "";

            foreach(string s in formattedText)
                draw += s + (s == "\n" ? "" : " ");

            if(checkScale(draw))
            {
                Vector2 originalScale = scale;
                do
                {
                    SetScaling(new Vector2(0.9f));
                } while(checkScale(draw));
                RenderingDevice.SpriteBatch.DrawString(font(), draw, upperLeft, color * (color.A / 255f), 0, Vector2.Zero, RenderingDevice.TextureScaleFactor * scale, SpriteEffects.None, 0);
                scale = originalScale;
                return;
            }

            RenderingDevice.SpriteBatch.DrawString(font(), draw, upperLeft, color * (color.A / 255f), 0, Vector2.Zero, RenderingDevice.TextureScaleFactor * scale, SpriteEffects.None, 0);

        }

        private bool checkScale(string toDraw)
        {
            return font().MeasureString(toDraw).Y * scale.Y * RenderingDevice.TextureScaleFactor.Y > screenSpace.Height;
        }

        /// <summary>
        /// Expands a given string to add spaces where needed and add newlines.
        /// </summary>
        /// <param name="text">The text from Draw().</param>
        private string[] convertString(string text)
        {
            if(previousFrameText == text)
                return (string[])previousFrameFormattedText.Clone();

            previousFrameText = text;

            List<string> words = new List<string>();
            string temp = "";

            foreach(char c in text)
            {
                if(c != ' ')
                    temp += c;
                else
                {
                    words.Add(temp);
                    temp = "";
                }
            }
            words.Add(temp); // adds the last word.

            double cumulativeX = 0;
            for(int i = 0; i < words.Count; i++)
            {
                string measure;
                if(words[i] == printB || words[i] == printS)
                    measure = replaceSorB;
                else if(words[i] == printLR)
                    measure = replaceLR;
                else
                    measure = words[i];

                Vector2 tempLength = font().MeasureString(measure + " ") * RenderingDevice.TextureScaleFactor * scale;
                if(cumulativeX + tempLength.X > screenSpace.Width)
                {
                    words.Insert(i++, "\n"); // increase i because we already add the X value for this word.
                    cumulativeX = tempLength.X;
                }
                else
                    cumulativeX += tempLength.X;
            }

            previousFrameFormattedText = words.ToArray();
            return words.ToArray();
        }
    }
}
