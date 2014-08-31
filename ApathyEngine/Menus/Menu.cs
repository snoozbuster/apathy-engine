using ApathyEngine.Input;
using ApathyEngine.Menus.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Menus
{
    public abstract class Menu
    {
        /// <summary>
        /// A dictionary of all the controls. The key is the control, and the value is false if not selected, null if selected but
        /// no buttons are down, and true if there's a button down and it's selected.
        /// </summary>
        protected List<MenuControl> controlArray;

        /// <summary>
        /// The currently selected control.
        /// </summary>
        protected MenuControl selectedControl;

        protected bool enterLetGo = false;
        protected bool holdingSelection = false;

        private bool playedClickSound = false;

        /// <summary>
        /// You NEED to set selectedControl in your constructor. Absolutely need. Just set it to
        /// controlArray.ElementAt(0).Key. That's all you have to remember to do. If you don't,
        /// things WILL crash. Boom. Also, set controlArray.IsSelected to null.
        /// </summary>
        protected Menu()
        {
            controlArray = new List<MenuControl>();
        }

        /// <summary>
        /// Calls DetectKeyboardInput and DetectMouseInput and if either is true invokes the selected control.
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values.</param>
        public virtual void Update(GameTime gameTime)
        {
            if(detectKeyboardInput() || detectMouseInput())
                selectedControl.OnSelect();
        }

        /// <summary>
        /// Draws each of the controls in controlArray.
        /// </summary>
        public virtual void Draw(GameTime gameTime)
        {
            foreach(MenuControl m in controlArray)
                m.Draw(selectedControl);
        }

        /// <summary>
        /// True denotes do something. False denotes don't.
        /// </summary>
        /// <returns></returns>
        protected virtual bool detectKeyboardInput()
        {
            if((InputManager.KeyboardState.WasKeyJustPressed(Program.Game.Manager.CurrentSaveWindowsOptions.MenuLeftKey) ||
                InputManager.CurrentPad.WasButtonJustPressed(Program.Game.Manager.CurrentSaveXboxOptions.MenuLeftKey)) && selectedControl.OnLeft != null)
            {
                MenuHandler.MouseTempDisabled = true;
                selectedControl.IsSelected = false;

                MenuControl initial = selectedControl;
                do
                {
                    selectedControl = selectedControl.OnLeft;
                    if(selectedControl == null)
                    {
                        selectedControl = initial;
                        break;
                    }
                } while(selectedControl.IsDisabled);
                //} while(loader.levelArray[controlArray.IndexOf(selectedControl)] == null);

                if(initial != selectedControl)
                    MediaSystem.PlaySoundEffect(SFXOptions.Button_Rollover);

                selectedControl.IsSelected = null;
                return false;
            }
            else if((InputManager.KeyboardState.WasKeyJustPressed(Program.Game.Manager.CurrentSaveWindowsOptions.MenuRightKey) ||
                     InputManager.CurrentPad.WasButtonJustPressed(Program.Game.Manager.CurrentSaveXboxOptions.MenuRightKey)) && selectedControl.OnRight != null)
            {
                MenuHandler.MouseTempDisabled = true;
                selectedControl.IsSelected = false;

                MenuControl initial = selectedControl;

                do
                {
                    selectedControl = selectedControl.OnRight;
                    if(selectedControl == null)
                    {
                        selectedControl = initial;
                        break;
                    }
                } while(selectedControl.IsDisabled);
                //} while(loader.levelArray[controlArray.IndexOf(selectedControl)] == null);

                if(initial != selectedControl)
                    MediaSystem.PlaySoundEffect(SFXOptions.Button_Rollover);

                selectedControl.IsSelected = null;
                return false;
            }
            else if((InputManager.KeyboardState.WasKeyJustPressed(Program.Game.Manager.CurrentSaveWindowsOptions.MenuUpKey) ||
                     InputManager.CurrentPad.WasButtonJustPressed(Program.Game.Manager.CurrentSaveXboxOptions.MenuUpKey)) && selectedControl.OnUp != null)
            {
                MenuHandler.MouseTempDisabled = true;
                selectedControl.IsSelected = false;

                MenuControl initial = selectedControl;
                do
                {
                    if(selectedControl.OnUp == null)
                    {
                        selectedControl = initial;
                        break;
                    }
                    selectedControl = selectedControl.OnUp;
                    if(selectedControl.IsDisabled)
                    {
                        MenuControl initialLeft = selectedControl;
                        do
                        {
                            if(selectedControl.OnLeft == null && selectedControl.OnUp == null)
                            {
                                selectedControl = initial;
                                break;
                            }
                            if(selectedControl.OnLeft != null)
                                selectedControl = selectedControl.OnLeft;
                            else
                                break; // this'll get us out of all the loops
                            if(initialLeft == selectedControl) // we've looped, time to move on
                                break;
                        } while(selectedControl.IsDisabled);
                    }
                } while(selectedControl.IsDisabled);
                //} while(loader.levelArray[controlArray.IndexOf(selectedControl)] == null);

                if(initial != selectedControl)
                    MediaSystem.PlaySoundEffect(SFXOptions.Button_Rollover);

                selectedControl.IsSelected = null;
                return false;
            }
            else if((InputManager.KeyboardState.WasKeyJustPressed(Program.Game.Manager.CurrentSaveWindowsOptions.MenuDownKey) ||
                     InputManager.CurrentPad.WasButtonJustPressed(Program.Game.Manager.CurrentSaveXboxOptions.MenuDownKey)) && selectedControl.OnDown != null)
            {
                MenuHandler.MouseTempDisabled = true;
                selectedControl.IsSelected = false;
                MenuControl initial = selectedControl;
                do
                {
                    if(selectedControl.OnDown == null)
                    {
                        selectedControl = initial;
                        break;
                    }
                    selectedControl = selectedControl.OnDown;
                    if(selectedControl.IsDisabled)
                    {
                        MenuControl initialLeft = selectedControl;
                        do
                        {
                            if(selectedControl.OnLeft == null && selectedControl.OnDown == null)
                            {
                                selectedControl = initial;
                                break;
                            }
                            if(selectedControl.OnLeft != null)
                                selectedControl = selectedControl.OnLeft;
                            else
                                break;
                            if(initialLeft == selectedControl) // we've looped, time to move on
                                break;
                        } while(selectedControl.IsDisabled);
                    }
                } while(selectedControl.IsDisabled);
                //} while(loader.levelArray[controlArray.IndexOf(selectedControl)] == null);

                if(initial != selectedControl)
                    MediaSystem.PlaySoundEffect(SFXOptions.Button_Rollover);

                selectedControl.IsSelected = null;
                return false;
            }

            bool? old = selectedControl.IsSelected;

            if(InputManager.KeyboardState.WasKeyJustPressed(Program.Game.Manager.CurrentSaveWindowsOptions.SelectionKey) ||
                InputManager.CurrentPad.WasButtonJustPressed(Program.Game.Manager.CurrentSaveXboxOptions.SelectionKey))
            {
                holdingSelection = true;
                MenuHandler.MouseTempDisabled = true;
            }
            else if(InputManager.KeyboardState.IsKeyUp(Program.Game.Manager.CurrentSaveWindowsOptions.SelectionKey) &&
                InputManager.CurrentPad.IsButtonUp(Program.Game.Manager.CurrentSaveXboxOptions.SelectionKey) && holdingSelection)
                holdingSelection = false;

            bool buttonDown = holdingSelection && !selectedControl.IsDisabled;
            //loader.levelArray[controlArray.IndexOf(selectedControl)] != null;
            if(buttonDown)
                MenuHandler.MouseTempDisabled = true;

            if(!old.HasValue && buttonDown && MenuHandler.MouseTempDisabled)
            {
                if(!(selectedControl is DropUpMenuControl))
                    MediaSystem.PlaySoundEffect(SFXOptions.Button_Press);
                selectedControl.IsSelected = true;
                return false;
            }
            else if(old.HasValue && old.Value && !buttonDown && MenuHandler.MouseTempDisabled)
            {
                MediaSystem.PlaySoundEffect(SFXOptions.Button_Release);
                selectedControl.IsSelected = null;
                holdingSelection = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// True denotes do something. False denotes don't.
        /// </summary>
        /// <returns></returns>
        protected virtual bool detectMouseInput()
        {
#if WINDOWS
            if(!MenuHandler.MouseTempDisabled)
            {
                if(InputManager.MouseState.LeftButton == ButtonState.Released)
                    playedClickSound = false;

                foreach(MenuControl m in controlArray)
                {
                    bool? old = m.IsSelected;
                    bool? current = m.CheckMouseInput(selectedControl);

                    if(old.HasValue && !old.Value && !current.HasValue)
                    {
                        MediaSystem.PlaySoundEffect(SFXOptions.Button_Rollover);
                        selectedControl = m;
                        return false;
                    }
                    else if(!old.HasValue && current.HasValue && current.Value)
                    {
                        if(!playedClickSound)
                            MediaSystem.PlaySoundEffect(SFXOptions.Button_Press);
                        playedClickSound = true;
                        selectedControl = m;
                        return false;
                    }
                    else if(old.HasValue && old.Value && !current.HasValue && InputManager.MouseState.LeftButton == ButtonState.Released)
                    {
                        MediaSystem.PlaySoundEffect(SFXOptions.Button_Release);
                        selectedControl = m;
                        return true;
                    }
                }
            }
#endif
            return false;
        }
    }
}
