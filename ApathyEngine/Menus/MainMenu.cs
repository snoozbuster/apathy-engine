using ApathyEngine.Graphics;
using ApathyEngine.Input;
using ApathyEngine.Media;
using ApathyEngine.Menus.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Menus
{
    /// <summary>
    /// Provides a default implementation of a main menu that optionally includes a "press start"
    /// input selection mechanism.
    /// </summary>
    /// <remarks>This implementation only includes buttons for start, instructions, and quit.</remarks>
    public class MainMenu : Menu
    {
        private float timerInMilliseconds = 0;

        private bool startBeenPressed;
        private int timer = 1200;
        private const int smallerTime = 300;
        private const int largerTime = 750;

        private readonly MenuControl start, instructions;

        private bool can_click = false;

        public MainMenu()
        {
            MenuButton quit;
            start = new MenuButton(loader.startButton, delegate { GameManager.State = GameState.Running; timerInMilliseconds = 0; Program.Game.Start(); });
            instructions = new MenuButton(loader.instructionsButton, delegate { GameManager.State = GameState.Menuing_HiS; can_click = false; });
            quit = new MenuButton(loader.quitButton, delegate { GameManager.State = GameState.Exiting; });

            start.SetDirectionals(null, instructions, null, null);
            instructions.SetDirectionals(start, quit, null, null);
            quit.SetDirectionals(instructions, null, null, null);
            controlArray.AddRange(new MenuControl[] { instructions, start, quit });

            selectedControl = start;
            selectedControl.IsSelected = null;
        }

        public override void Draw(GameTime gameTime)
        {
            RenderingDevice.GraphicsDevice.Clear(Color.White);
            if(InputManager.MessagePad == null || (startBeenPressed && timer > 0))
            {
                int time = startBeenPressed ? smallerTime : largerTime;

                RenderingDevice.SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.LinearClamp, null, null);
                RenderingDevice.GraphicsDevice.Clear(Color.White);
                RenderingDevice.SpriteBatch.Draw(loader.mainMenuBackground, new Vector2(0, 0), null, Color.White, 0, Vector2.Zero, RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
                RenderingDevice.SpriteBatch.Draw(loader.mainMenuLogo, new Vector2(RenderingDevice.Width * 0.5f, RenderingDevice.Height * 0.15f), null, Color.White, 0.0f, new Vector2(loader.mainMenuLogo.Width, loader.mainMenuLogo.Height) * 0.5f * RenderingDevice.TextureScaleFactor, 0.75f * RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
                if(timerInMilliseconds % time <= time * 0.5f)
                {
                    Vector2 screenSpot = new Vector2(RenderingDevice.Width * 0.5f, RenderingDevice.Height * 0.8f);
                    RenderingDevice.SpriteBatch.Draw(Program.Game.Loader.pressStart, screenSpot, null, Color.White, 0, new Vector2(Program.Game.Loader.pressStart.Width, Program.Game.Loader.pressStart.Height) * 0.5f * RenderingDevice.TextureScaleFactor,
                        RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
                }
                RenderingDevice.SpriteBatch.End();
            }
            else
            {
                startBeenPressed = false;
                RenderingDevice.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, null, null);
                RenderingDevice.SpriteBatch.Draw(loader.mainMenuBackground, new Vector2(0, 0), null, Color.White, 0, Vector2.Zero, RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
                RenderingDevice.SpriteBatch.Draw(loader.mainMenuLogo, new Vector2(RenderingDevice.Width * 0.5f, RenderingDevice.Height * 0.15f), null, Color.White, 0.0f, new Vector2(loader.mainMenuLogo.Width / 2, loader.mainMenuLogo.Height / 2), 0.75f * RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
                base.Draw(gameTime);
                RenderingDevice.SpriteBatch.End();
            }
        }

        public override void Update(GameTime gameTime)
        {
            timerInMilliseconds += gameTime.ElapsedGameTime.Milliseconds;
            if(startBeenPressed)
                timer -= gameTime.ElapsedGameTime.Milliseconds;

            if(!Program.Game.Loading)
            {
                if(InputManager.MessagePad == null)
                {
                    InputManager.PlayerSelect(Program.Game.IsActive);
                    if(InputManager.MessagePad != null)
                    {
                        MediaSystem.PlaySoundEffect(SFXOptions.Box_Success);
                        MenuHandler.MouseTempDisabled = true;
                        startBeenPressed = true;
                        timer = 1200;
                        timerInMilliseconds %= smallerTime;
                    }
                }
                else if(InputManager.MessagePad != null && timer <= 0)
                {
                    if(InputManager.KeyboardState.WasKeyJustPressed(Keys.Escape) || InputManager.CurrentPad.WasButtonJustPressed(Buttons.B))
                    {
                        ReturnToPressStart();
                        return;
                    }
                    if(InputManager.MouseState.LeftButton == ButtonState.Released)
                        can_click = true;

                    if(can_click)
                        base.Update(gameTime);
                }
            }
        }

        private void ReturnToPressStart()
        {
            InputManager.RemoveInputDevices();
            startBeenPressed = false;
            ResetTimer();
            MediaSystem.PlaySoundEffect(SFXOptions.Box_Death);
        }

        internal void ResetTimer()
        {
            timerInMilliseconds = 0;
        }
    }
}
