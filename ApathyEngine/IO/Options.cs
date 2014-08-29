using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Xml.Serialization;
using ApathyEngine.Input;

namespace ApathyEngine.IO
{
    [Serializable]
    public class WindowsOptions
    {
        #region Controls
        public Keys Machine1Key 
        {
            get 
            {
                return machine1Key;
            }
            set 
            {
                if(value == Keys.NumPad1)
                    value = Keys.D1;
                machine1Key = value; 
            }
        }
        public Keys Machine2Key
        {
            get
            {
                return machine2Key;
            }
            set
            {
                if(value == Keys.NumPad2)
                    value = Keys.D2;
                machine2Key = value;
            }
        }
        public Keys Machine3Key
        {
            get
            {
                return machine3Key;
            }
            set
            {
                if(value == Keys.NumPad3)
                    value = Keys.D3;
                machine3Key = value;
            }
        }
        public Keys Machine4Key
        {
            get
            {
                return machine4Key;
            }
            set
            {
                if(value == Keys.NumPad4)
                    value = Keys.D4;
                machine4Key = value;
            }
        }
        public Keys Machine5Key
        {
            get
            {
                return machine5Key;
            }
            set
            {
                if(value == Keys.NumPad5)
                    value = Keys.D5;
                machine5Key = value;
            }
        }
        public Keys Machine6Key
        {
            get
            {
                return machine6Key;
            }
            set
            {
                if(value == Keys.NumPad6)
                    value = Keys.D6;
                machine6Key = value;
            }
        }
        public Keys Machine7Key
        {
            get
            {
                return machine7Key;
            }
            set
            {
                if(value == Keys.NumPad7)
                    value = Keys.D7;
                machine7Key = value;
            }
        }
        public Keys Machine8Key
        {
            get
            {
                return machine8Key;
            }
            set
            {
                if(value == Keys.NumPad8)
                    value = Keys.D8;
                machine8Key = value;
            }
        }
        public Keys Machine9Key
        {
            get
            {
                return machine9Key;
            }
            set
            {
                if(value == Keys.NumPad9)
                    value = Keys.D9;
                machine9Key = value;
            }
        }
        public Keys Machine0Key
        {
            get
            {
                return machine0Key;
            }
            set
            {
                if(value == Keys.NumPad0)
                    value = Keys.D0;
                machine0Key = value;
            }
        }

        protected Keys machine1Key = Keys.D1;
        protected Keys machine2Key = Keys.D2;
        protected Keys machine3Key = Keys.D3;
        protected Keys machine4Key = Keys.D4;
        protected Keys machine5Key = Keys.D5;
        protected Keys machine6Key = Keys.D6;
        protected Keys machine7Key = Keys.D7;
        protected Keys machine8Key = Keys.D8;
        protected Keys machine9Key = Keys.D9;
        protected Keys machine0Key = Keys.D0;
        public Keys QuickBoxKey = Keys.OemTilde;

        public Keys CameraLeftKey = Keys.Left;
        public Keys CameraRightKey = Keys.Right;
        public Keys CameraUpKey = Keys.Up;
        public Keys CameraDownKey = Keys.Down;

        public Keys CameraZoomPlusKey
        {
            get { return cameraZoomPlusKey; }
            set
            {
                if(value == Keys.OemPlus)
                    value = Keys.Add;
                cameraZoomPlusKey = value;
            }
        }
        public Keys CameraZoomMinusKey
        {
            get { return cameraZoomMinusKey; }
            set
            {
                if(value == Keys.OemMinus)
                    value = Keys.Subtract;
                cameraZoomMinusKey = value;
            }
        }
        protected Keys cameraZoomPlusKey = Keys.Add;
        protected Keys cameraZoomMinusKey = Keys.Subtract;

        public Keys PauseKey = Keys.Escape;

        public Keys MenuLeftKey = Keys.Left;
        public Keys MenuRightKey = Keys.Right;
        public Keys MenuUpKey = Keys.Up;
        public Keys MenuDownKey = Keys.Down;
        public Keys SelectionKey = Keys.Enter;

        public Keys HelpKey = Keys.Tab;
        public Keys MuteKey = Keys.M;
        public Keys MusicKey = Keys.N;

        public float BGMVolume = 100;
        public float SFXVolume = 100;
        public float VoiceVolume = 100;

        /// <summary>
        /// A list of all the keys in the class. Use this for the AssignKey function.
        /// </summary>
        public enum KeyList
        {
            Machine1Key,
            Machine2Key,
            Machine3Key,
            Machine4Key,
            Machine5Key,
            Machine6Key,
            Machine7Key,
            Machine8Key,
            Machine9Key,
            Machine0Key,
            QuickBoxKey,
            CameraLeftKey,
            CameraRightKey,
            CameraUpKey,
            CameraDownKey,
            CameraZoomPlusKey,
            CameraZoomMinusKey,
            PauseKey,
            MenuLeftKey,
            MenuRightKey,
            MenuUpKey,
            MenuDownKey,
            MuteKey
        }
        #endregion

        #region Options
        [XmlIgnore]
        public StorageDevice PreferredDevice = null;

        //public enum Difficulty
        //{
        //    Hard = 2,
        //    Medium = 1,
        //    Easy = 0
        //}

        //public Difficulty DifficultyLevel = Difficulty.Medium;
        public bool VoiceClips = true;
        public bool SwapCamera = false;
        public bool UseKeypad = false;
        public bool ResumeOnFocus = false;
        public bool Muted = false;
        public int ScreenHeight = 720;
        public int ScreenWidth = 1280;
        public bool HighScoreMode = false;
        public bool FancyGraphics = true;
        #endregion

        /// <summary>
        /// Determines if a key is supported by this game.
        /// </summary>
        /// <param name="keyInQuestion">The key to check.</param>
        /// <returns>True if it is supported, false if it isn't.</returns>
        public bool DetermineIfSupported(Keys keyInQuestion)
        {
            return (keyInQuestion == Keys.A ||
                keyInQuestion == Keys.Add ||
                keyInQuestion == Keys.B ||
                keyInQuestion == Keys.Back ||
                keyInQuestion == Keys.C ||
                keyInQuestion == Keys.D ||
                keyInQuestion == Keys.D0 ||
                keyInQuestion == Keys.D1 ||
                keyInQuestion == Keys.D2 ||
                keyInQuestion == Keys.D3 ||
                keyInQuestion == Keys.D4 ||
                keyInQuestion == Keys.D5 ||
                keyInQuestion == Keys.D6 ||
                keyInQuestion == Keys.D7 ||
                keyInQuestion == Keys.D8 ||
                keyInQuestion == Keys.D9 ||
                keyInQuestion == Keys.Decimal ||
                keyInQuestion == Keys.Delete ||
                keyInQuestion == Keys.Divide ||
                keyInQuestion == Keys.Down ||
                keyInQuestion == Keys.E ||
                keyInQuestion == Keys.End ||
                keyInQuestion == Keys.Enter ||
                keyInQuestion == Keys.Escape ||
                keyInQuestion == Keys.F ||
                keyInQuestion == Keys.F1 ||
                keyInQuestion == Keys.F10 ||
                keyInQuestion == Keys.F11 ||
                keyInQuestion == Keys.F12 ||
                keyInQuestion == Keys.F2 ||
                keyInQuestion == Keys.F4 ||
                keyInQuestion == Keys.F3 ||
                keyInQuestion == Keys.F5 ||
                keyInQuestion == Keys.F6 ||
                keyInQuestion == Keys.F7 ||
                keyInQuestion == Keys.F8 ||
                keyInQuestion == Keys.F9 ||
                keyInQuestion == Keys.G ||
                keyInQuestion == Keys.H ||
                keyInQuestion == Keys.Home ||
                keyInQuestion == Keys.I ||
                keyInQuestion == Keys.Insert ||
                keyInQuestion == Keys.J ||
                keyInQuestion == Keys.K ||
                keyInQuestion == Keys.L ||
                keyInQuestion == Keys.Left ||
                keyInQuestion == Keys.LeftAlt ||
                keyInQuestion == Keys.LeftControl ||
                keyInQuestion == Keys.LeftShift ||
                keyInQuestion == Keys.M ||
                keyInQuestion == Keys.Multiply ||
                keyInQuestion == Keys.N ||
                keyInQuestion == Keys.NumPad0 ||
                keyInQuestion == Keys.NumPad1 ||
                keyInQuestion == Keys.NumPad2 ||
                keyInQuestion == Keys.NumPad3 ||
                keyInQuestion == Keys.NumPad4 ||
                keyInQuestion == Keys.NumPad5 ||
                keyInQuestion == Keys.NumPad6 ||
                keyInQuestion == Keys.NumPad7 ||
                keyInQuestion == Keys.NumPad8 ||
                keyInQuestion == Keys.NumPad9 ||
                keyInQuestion == Keys.O ||
                keyInQuestion == Keys.OemBackslash ||
                keyInQuestion == Keys.OemCloseBrackets ||
                keyInQuestion == Keys.OemComma ||
                keyInQuestion == Keys.OemMinus ||
                keyInQuestion == Keys.OemOpenBrackets ||
                keyInQuestion == Keys.OemPeriod ||
                keyInQuestion == Keys.OemPipe ||
                keyInQuestion == Keys.OemPlus ||
                keyInQuestion == Keys.OemQuestion ||
                keyInQuestion == Keys.OemQuotes ||
                keyInQuestion == Keys.OemSemicolon ||
                keyInQuestion == Keys.OemTilde ||
                keyInQuestion == Keys.P ||
                keyInQuestion == Keys.PageDown ||
                keyInQuestion == Keys.PageUp ||
                keyInQuestion == Keys.Q ||
                keyInQuestion == Keys.R ||
                keyInQuestion == Keys.Right ||
                keyInQuestion == Keys.RightAlt ||
                keyInQuestion == Keys.RightControl ||
                keyInQuestion == Keys.RightShift ||
                keyInQuestion == Keys.S ||
                keyInQuestion == Keys.Space ||
                keyInQuestion == Keys.Subtract ||
                keyInQuestion == Keys.T ||
                keyInQuestion == Keys.Tab ||
                keyInQuestion == Keys.U ||
                keyInQuestion == Keys.Up ||
                keyInQuestion == Keys.V ||
                keyInQuestion == Keys.W ||
                keyInQuestion == Keys.X ||
                keyInQuestion == Keys.Y ||
                keyInQuestion == Keys.Z);
        }

        /// <summary>
        /// Determines if a supplied key is already in use.
        /// </summary>
        /// <param name="keyToLocate">The key to try to find.</param>        
        /// <param name="assigningToMenu">If you are assigning to a menu key, set true.
        /// This is so a menu key can't share an assignment with another menu key, but can with
        /// anything else.</param>
        /// <returns>True if the key is in use.</returns>
        public bool FindDuplicateKeys(Keys keyToLocate, bool assigningToMenu)
        {
            if(assigningToMenu)
            {
                return (keyToLocate == MenuLeftKey ||
                        keyToLocate == MenuRightKey ||
                        keyToLocate == MenuUpKey ||
                        keyToLocate == MenuDownKey);
            }
            return (keyToLocate == Machine0Key ||
                     keyToLocate == Machine1Key ||
                     keyToLocate == Machine2Key ||
                     keyToLocate == Machine3Key ||
                     keyToLocate == Machine4Key ||
                     keyToLocate == Machine5Key ||
                     keyToLocate == Machine6Key ||
                     keyToLocate == Machine7Key ||
                     keyToLocate == Machine8Key ||
                     keyToLocate == Machine9Key ||
                     keyToLocate == QuickBoxKey ||
                     keyToLocate == PauseKey ||
                     keyToLocate == CameraDownKey ||
                     keyToLocate == CameraLeftKey ||
                     keyToLocate == CameraRightKey ||
                     keyToLocate == CameraUpKey ||
                     keyToLocate == CameraZoomPlusKey ||
                     keyToLocate == CameraZoomMinusKey ||
                     keyToLocate == SelectionKey ||
                     keyToLocate == MuteKey ||
                     keyToLocate == MusicKey);
        }

        internal void GetMachineInputs(int machineNo, out Keys key, out Buttons button)
        {
            switch(machineNo)
            {
                case 1: key = Machine1Key;
                    button = InputManager.XboxOptions.Machine1Key;
                    break;
                case 2: key = Machine2Key;
                    button = InputManager.XboxOptions.Machine2Key;
                    break;
                case 3: key = Machine3Key;
                    button = InputManager.XboxOptions.Machine3Key;
                    break;
                case 4: key = Machine4Key;
                    button = InputManager.XboxOptions.Machine4Key;
                    break;
                case 5: key = Machine5Key;
                    button = InputManager.XboxOptions.Machine5Key;
                    break;
                case 6: key = Machine6Key;
                    button = InputManager.XboxOptions.Machine6Key;
                    break;
                case 7: key = Machine7Key;
                    button = InputManager.XboxOptions.Machine7Key;
                    break;
                case 8: key = Machine8Key;
                    button = InputManager.XboxOptions.Machine8Key;
                    break;
                case 9: key = Machine9Key;
                    button = InputManager.XboxOptions.Machine9Key;
                    break;
                case 10: key = Machine0Key;
                    button = InputManager.XboxOptions.Machine0Key;
                    break;
                default:
                    key = Keys.None;
                    button = (Buttons)(-1);
                    break;
            }
        }
    }

    [Serializable]
    public class XboxOptions
    {
        #region Controls
        public Buttons Machine1Key = Buttons.A;
        public Buttons Machine2Key = Buttons.B;
        public Buttons Machine3Key = Buttons.X;
        public Buttons Machine4Key = Buttons.Y;
        public Buttons Machine5Key = Buttons.DPadDown;
        public Buttons Machine6Key = Buttons.DPadRight;
        public Buttons Machine7Key = Buttons.DPadLeft;
        public Buttons Machine8Key = Buttons.DPadUp;
        public Buttons Machine9Key = Buttons.LeftShoulder;
        public Buttons Machine0Key = Buttons.RightShoulder;
        public Buttons QuickBoxKey = Buttons.LeftStick;

        public Buttons CameraZoomPlusKey = Buttons.LeftTrigger;
        public Buttons CameraZoomMinusKey = Buttons.RightTrigger;

        public Buttons PauseKey = Buttons.Start;

        public Buttons MenuLeftKey = Buttons.LeftThumbstickLeft;
        public Buttons MenuRightKey = Buttons.LeftThumbstickRight;
        public Buttons MenuUpKey = Buttons.LeftThumbstickUp;
        public Buttons MenuDownKey = Buttons.LeftThumbstickDown;
        public Buttons SelectionKey = Buttons.A;

        public Buttons HelpKey = Buttons.Back;
        public Buttons MusicKey = Buttons.RightStick;

        public enum ButtonList
        {
            Machine1Key,
            Machine2Key,
            Machine3Key,
            Machine4Key,
            Machine5Key,
            Machine6Key,
            Machine7Key,
            Machine8Key,
            Machine9Key,
            Machine0Key,
            QuickBoxKey,
            CameraZoomPlusKey,
            CameraZoomMinusKey,
            MusicKey
        }

        /// <summary>
        /// Assigns a new value to a button.
        /// </summary>
        /// <param name="keyToAssignTo">The button to assign the new value to.</param>
        /// <param name="newKey">The new value of the first parameter.</param>
        /// <param name="assignedKey">The button that was assigned to.</param>
        /// <returns>The original value of the button that was assigned the new value.</returns>
        public Buttons AssignKey(ButtonList keyToAssignTo, Buttons newKey, out ButtonList assignedKey)
        {
            Buttons holdingKey;
            switch(keyToAssignTo)
            {
                case ButtonList.Machine1Key: holdingKey = Machine1Key;
                    Machine1Key = newKey;
                    assignedKey = ButtonList.Machine1Key;
                    break;
                case ButtonList.Machine2Key: holdingKey = Machine2Key;
                    Machine2Key = newKey;
                    assignedKey = ButtonList.Machine2Key;
                    break;
                case ButtonList.Machine3Key: holdingKey = Machine3Key;
                    Machine3Key = newKey;
                    assignedKey = ButtonList.Machine3Key;
                    break;
                case ButtonList.Machine4Key: holdingKey = Machine4Key;
                    Machine4Key = newKey;
                    assignedKey = ButtonList.Machine4Key;
                    break;
                case ButtonList.Machine5Key: holdingKey = Machine5Key;
                    Machine5Key = newKey;
                    assignedKey = ButtonList.Machine5Key;
                    break;
                case ButtonList.Machine6Key: holdingKey = Machine6Key;
                    Machine6Key = newKey;
                    assignedKey = ButtonList.Machine6Key;
                    break;
                case ButtonList.Machine7Key: holdingKey = Machine7Key;
                    Machine7Key = newKey;
                    assignedKey = ButtonList.Machine7Key;
                    break;
                case ButtonList.Machine8Key: holdingKey = Machine8Key;
                    Machine8Key = newKey;
                    assignedKey = ButtonList.Machine8Key;
                    break;
                case ButtonList.Machine9Key: holdingKey = Machine9Key;
                    Machine9Key = newKey;
                    assignedKey = ButtonList.Machine9Key;
                    break;
                case ButtonList.Machine0Key: holdingKey = Machine0Key;
                    Machine0Key = newKey;
                    assignedKey = ButtonList.Machine0Key;
                    break;
                case ButtonList.QuickBoxKey: holdingKey = QuickBoxKey;
                    QuickBoxKey = newKey;
                    assignedKey = ButtonList.QuickBoxKey;
                    break;
                case ButtonList.CameraZoomPlusKey: holdingKey = CameraZoomPlusKey;
                    CameraZoomPlusKey = newKey;
                    assignedKey = ButtonList.CameraZoomPlusKey;
                    break;
                case ButtonList.CameraZoomMinusKey: holdingKey = CameraZoomMinusKey;
                    CameraZoomMinusKey = newKey;
                    assignedKey = ButtonList.CameraZoomMinusKey;
                    break;
                case ButtonList.MusicKey: holdingKey = MusicKey;
                    MusicKey = newKey;
                    assignedKey = ButtonList.MusicKey;
                    break;
                default: throw new KeyNotFoundException("There was a programming error in Options.cs, line 179.");
            }
            return holdingKey;
        }

        /// <summary>
        /// Determines if a supplied button is already in use.
        /// </summary>
        /// <param name="buttonToLocate">The button to try to find.</param>
        /// <returns>True if the button is in use. However, it could return false if the button to
        /// check is assigned to the menu. This is intentional. Don't use this function if assigning to the 
        /// menu buttons, because they should be able to share button assignments with in-game buttons.</returns>
        public bool FindDuplicateButtons(Buttons buttonToLocate)
        {
            return (buttonToLocate == Machine0Key ||
                     buttonToLocate == Machine1Key ||
                     buttonToLocate == Machine2Key ||
                     buttonToLocate == Machine3Key ||
                     buttonToLocate == Machine4Key ||
                     buttonToLocate == Machine5Key ||
                     buttonToLocate == Machine6Key ||
                     buttonToLocate == Machine7Key ||
                     buttonToLocate == Machine8Key ||
                     buttonToLocate == Machine9Key ||
                     buttonToLocate == QuickBoxKey ||
                     buttonToLocate == PauseKey ||
                     buttonToLocate == Buttons.LeftThumbstickDown ||
                     buttonToLocate == Buttons.LeftThumbstickLeft ||
                     buttonToLocate == Buttons.LeftThumbstickRight ||
                     buttonToLocate == Buttons.LeftThumbstickUp ||
                     buttonToLocate == Buttons.RightThumbstickDown ||
                     buttonToLocate == Buttons.RightThumbstickLeft ||
                     buttonToLocate == Buttons.RightThumbstickRight ||
                     buttonToLocate == Buttons.RightThumbstickUp ||
                     buttonToLocate == CameraZoomPlusKey ||
                     buttonToLocate == CameraZoomMinusKey ||
                     buttonToLocate == SelectionKey ||
                     buttonToLocate == MusicKey);
        }

        #endregion

        /// <summary>
        /// Assigns the value in variable to the variable holding pressedKey, and then assigns pressedKey to variable.Value.
        /// </summary>
        /// <param name="variable">Pointer to a variable.</param>
        /// <param name="pressedKey">The new key.</param>
        public void Swap(Pointer<Buttons> variable, Buttons pressedKey)
        {
            if(pressedKey == Buttons.LeftThumbstickDown || pressedKey == Buttons.LeftThumbstickLeft ||
                pressedKey == Buttons.LeftThumbstickRight || pressedKey == Buttons.LeftThumbstickUp ||
                pressedKey == Buttons.RightThumbstickDown || pressedKey == Buttons.RightThumbstickLeft ||
                pressedKey == Buttons.RightThumbstickRight || pressedKey == Buttons.RightThumbstickUp)
                return;

            if(CameraZoomMinusKey == pressedKey)
            {
                CameraZoomMinusKey = variable.Value;
                variable.Value = pressedKey;
            }
            else if(CameraZoomPlusKey == pressedKey)
            {
                CameraZoomPlusKey = variable.Value;
                variable.Value = pressedKey;
            }
            else if(QuickBoxKey == pressedKey)
            {
                QuickBoxKey = variable.Value;
                variable.Value = pressedKey;
            }
            else if(Machine0Key == pressedKey)
            {
                Machine0Key = variable.Value;
                variable.Value = pressedKey;
            }
            else if(Machine1Key == pressedKey)
            {
                Machine1Key = variable.Value;
                variable.Value = pressedKey;
            }
            else if(Machine2Key == pressedKey)
            {
                Machine2Key = variable.Value;
                variable.Value = pressedKey;
            }
            else if(Machine3Key == pressedKey)
            {
                Machine3Key = variable.Value;
                variable.Value = pressedKey;
            }
            else if(Machine4Key == pressedKey)
            {
                Machine4Key = variable.Value;
                variable.Value = pressedKey;
            }
            else if(Machine5Key == pressedKey)
            {
                Machine5Key = variable.Value;
                variable.Value = pressedKey;
            }
            else if(Machine6Key == pressedKey)
            {
                Machine6Key = variable.Value;
                variable.Value = pressedKey;
            }
            else if(Machine7Key == pressedKey)
            {
                Machine7Key = variable.Value;
                variable.Value = pressedKey;
            }
            else if(Machine8Key == pressedKey)
            {
                Machine8Key = variable.Value;
                variable.Value = pressedKey;
            }
            else if(Machine9Key == pressedKey)
            {
                Machine9Key = variable.Value;
                variable.Value = pressedKey;
            }
            else if(MusicKey == pressedKey)
            {
                MusicKey = variable.Value;
                variable.Value = pressedKey;
            }
            else // nothing else is using that
                variable.Value = pressedKey; 
        }
    }
}
