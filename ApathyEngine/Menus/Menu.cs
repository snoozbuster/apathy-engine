using ApathyEngine.Input;
using ApathyEngine.Menus.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Menus
{
    public abstract class Menu
    {
        /// <summary>
        /// Fires when either the mouse enters a control or the control is given focus by the keyboard.
        /// </summary>
        public event Action<MenuControl> ControlEntered;
        /// <summary>
        /// Fires when a control is clicked on or the selection key is pressed but not released.
        /// </summary>
        public event Action<MenuControl> ControlClicked;
        /// <summary>
        /// Fires after a control is clicked on, when the mouse or selection button is released.
        /// </summary>
        public event Action<MenuControl> ControlReleased;

        /// <summary>
        /// A dictionary of all the controls. The key is the control, and the value is false if not selected, null if selected but
        /// no buttons are down, and true if there's a button down and it's selected.
        /// </summary>
        protected List<MenuControl> controlArray;

        /// <summary>
        /// Gets the currently selected control.
        /// </summary>
        protected MenuControl selectedControl;

        protected bool enterLetGo = false;
        protected bool holdingSelection = false;

        /// <summary>
        /// Gets the owning BaseGame.
        /// </summary>
        protected BaseGame game;

        private bool firedMouseClickEvent = false;

        /// <summary>
        /// You NEED to set selectedControl in your constructor, as well as set selectedControl.IsSelected to null.
        /// </summary>
        /// <param name="game">Owning game.</param>
        protected Menu(BaseGame game)
        {
            this.game = game;
            controlArray = new List<MenuControl>();

            ControlEntered += (c) => MediaSystem.PlaySoundEffect(SFXOptions.Button_Rollover);
            ControlClicked += (c) => MediaSystem.PlaySoundEffect(SFXOptions.Button_Press);
            ControlReleased += (c) => MediaSystem.PlaySoundEffect(SFXOptions.Button_Release);
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
            if((InputManager.KeyboardState.WasKeyJustPressed(game.Manager.CurrentSaveWindowsOptions.MenuLeftKey) ||
                InputManager.CurrentPad.WasButtonJustPressed(game.Manager.CurrentSaveXboxOptions.MenuLeftKey)) && selectedControl.OnLeft != null)
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

                if(initial != selectedControl)
                    if(ControlEntered != null)
                        ControlEntered(selectedControl);

                selectedControl.IsSelected = null;
                return false;
            }
            else if((InputManager.KeyboardState.WasKeyJustPressed(game.Manager.CurrentSaveWindowsOptions.MenuRightKey) ||
                     InputManager.CurrentPad.WasButtonJustPressed(game.Manager.CurrentSaveXboxOptions.MenuRightKey)) && selectedControl.OnRight != null)
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

                if(initial != selectedControl)
                    if(ControlEntered != null)
                        ControlEntered(selectedControl);

                selectedControl.IsSelected = null;
                return false;
            }
            else if((InputManager.KeyboardState.WasKeyJustPressed(game.Manager.CurrentSaveWindowsOptions.MenuUpKey) ||
                     InputManager.CurrentPad.WasButtonJustPressed(game.Manager.CurrentSaveXboxOptions.MenuUpKey)) && selectedControl.OnUp != null)
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

                if(initial != selectedControl)
                    if(ControlEntered != null)
                        ControlEntered(selectedControl);

                selectedControl.IsSelected = null;
                return false;
            }
            else if((InputManager.KeyboardState.WasKeyJustPressed(game.Manager.CurrentSaveWindowsOptions.MenuDownKey) ||
                     InputManager.CurrentPad.WasButtonJustPressed(game.Manager.CurrentSaveXboxOptions.MenuDownKey)) && selectedControl.OnDown != null)
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

                if(initial != selectedControl)
                    if(ControlEntered != null)
                        ControlEntered(selectedControl);

                selectedControl.IsSelected = null;
                return false;
            }

            bool? old = selectedControl.IsSelected;

            if(InputManager.KeyboardState.WasKeyJustPressed(game.Manager.CurrentSaveWindowsOptions.SelectionKey) ||
                InputManager.CurrentPad.WasButtonJustPressed(game.Manager.CurrentSaveXboxOptions.SelectionKey))
            {
                holdingSelection = true;
                MenuHandler.MouseTempDisabled = true;
            }
            else if(InputManager.KeyboardState.IsKeyUp(game.Manager.CurrentSaveWindowsOptions.SelectionKey) &&
                InputManager.CurrentPad.IsButtonUp(game.Manager.CurrentSaveXboxOptions.SelectionKey) && holdingSelection)
                holdingSelection = false;

            bool buttonDown = holdingSelection && !selectedControl.IsDisabled;
            if(buttonDown)
                MenuHandler.MouseTempDisabled = true;

            if(!old.HasValue && buttonDown && MenuHandler.MouseTempDisabled)
            {
                selectedControl.IsSelected = true;
                if(!(selectedControl is DropUpMenuControl))
                    if(ControlClicked != null)
                        ControlClicked(selectedControl);
                return false;
            }
            else if(old.HasValue && old.Value && !buttonDown && MenuHandler.MouseTempDisabled)
            {
                selectedControl.IsSelected = null;
                holdingSelection = false;
                if(ControlReleased != null)
                    ControlReleased(selectedControl);
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
                    firedMouseClickEvent = false;

                foreach(MenuControl m in controlArray)
                {
                    bool? old = m.IsSelected;
                    bool? current = m.CheckMouseInput(selectedControl);

                    if(old.HasValue && !old.Value && !current.HasValue)
                    {
                        selectedControl = m;
                        if(ControlEntered != null)
                            ControlEntered(selectedControl);
                        return false;
                    }
                    else if(!old.HasValue && current.HasValue && current.Value)
                    {
                        selectedControl = m;
                        if(!firedMouseClickEvent)
                        {
                            if(ControlClicked != null)
                                ControlClicked(selectedControl);
                            firedMouseClickEvent = true;
                        }
                        return false;
                    }
                    else if(old.HasValue && old.Value && !current.HasValue && InputManager.MouseState.LeftButton == ButtonState.Released)
                    {
                        selectedControl = m;
                        if(ControlReleased != null)
                            ControlReleased(selectedControl);
                        return true;
                    }
                }
            }
#endif
            return false;
        }
    }
}
