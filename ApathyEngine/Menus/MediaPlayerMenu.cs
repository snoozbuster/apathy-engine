using ApathyEngine.Graphics;
using ApathyEngine.Input;
using ApathyEngine.Media;
using ApathyEngine.Menus.Controls;
using ApathyEngine.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Menus
{
    /// <summary>
    /// Provides a default implementation for a menu that provides controls for playing a custom soundtrack.
    /// </summary>
    public class MediaPlayerMenu : Menu
    {
        private readonly MenuButton play, stop;
        private Color fill = Color.Transparent;
        private readonly Sprite background;
        private readonly Sprite scissorBackground;
        private Rectangle scissorRectangle;
        private readonly RasterizerState rs = new RasterizerState();
        private readonly SpriteFont font;
        private const byte deltaA = 3;
        private const byte fadeBoundary = 185;
        private string scrollingText = string.Empty;
        private string scrollingTextLastFrame = string.Empty;
        private MenuButton playPause;

        private enum FadeDirection { In, Out, None }
        private FadeDirection fade;
        private bool scrolling = false;
        private Vector2 textPos;
        private int timer;
        private const int waitTime = 1200;

        public string ErrorString { private get; set; }

        private enum MenuState { Normal, Artists, Albums }
        private MenuState state = MenuState.Normal;
        private AlbumCollection albums;
        private ArtistCollection artists;
        private string[] names;
        private float sliderStepDistance;
        private int startingIndex = -3;
        private int selectedIndex = 0;
        private Color backgroundTint = new Color(255, 255, 255, 0);
        private bool holdingEnter = false;
        private bool sliding = false;

        private readonly Rectangle mainRenderRect = new Rectangle(188, 235, 630, 249);
        private readonly Rectangle sliderRenderRect = new Rectangle(803, 0, 16, 39);
        private const int sliderSlideDistance = 140;
        private readonly Vector2 mainRenderPos;
        private readonly Vector2 sliderBaseRenderPos;
        private readonly Vector2[] textPositions;
        private Vector2 sliderPos;
        private const float epsilon = 0.001f;

        public MediaPlayerMenu()
        {
            font = loader.SmallerFont;
            MenuButton artists, shuffle, albums;
            VariableButton prev, next;
            rs.CullMode = CullMode.None;
            rs.ScissorTestEnable = true;

            Sprite playTex, stopTex, artistsTex, shuffleTex, albumsTex, prevTex, nextTex;
            Texture2D black = new Texture2D(RenderingDevice.GraphicsDevice, 172, 28);
            Color[] col = new Color[172 * 28];
            for(int i = 0; i < col.Length; i++)
                col[i] = Color.Black;
            black.SetData(col);

            background = new Sprite(delegate { return loader.MediaTexture; }, new Vector2(RenderingDevice.Width, RenderingDevice.Height) * 0.5f, new Rectangle(213, 0, 447, 235), Sprite.RenderPoint.Center);
            scissorBackground = new Sprite(delegate { return black; }, new Vector2(background.UpperLeft.X + 17 * RenderingDevice.TextureScaleFactor.X, background.LowerRight.Y - 55 * RenderingDevice.TextureScaleFactor.Y), null, Sprite.RenderPoint.UpLeft);
            albumsTex = new Sprite(delegate { return loader.MediaTexture; }, background.UpperLeft + new Vector2(205, 24) * RenderingDevice.TextureScaleFactor, new Rectangle(0, 0, 210, 51), Sprite.RenderPoint.UpLeft);
            artistsTex = new Sprite(delegate { return loader.MediaTexture; }, new Vector2(albumsTex.UpperLeft.X, albumsTex.LowerRight.Y) + new Vector2(0, 11) * RenderingDevice.TextureScaleFactor, new Rectangle(0, 51, 210, 51), Sprite.RenderPoint.UpLeft);
            shuffleTex = new Sprite(delegate { return loader.MediaTexture; }, new Vector2(artistsTex.UpperLeft.X, artistsTex.LowerRight.Y) + new Vector2(0, 11) * RenderingDevice.TextureScaleFactor, new Rectangle(0, 102, 210, 51), Sprite.RenderPoint.UpLeft);
            playTex = new Sprite(delegate { return loader.MediaTexture; }, background.UpperLeft + new Vector2(45, 36) * RenderingDevice.TextureScaleFactor, new Rectangle(0, 154, 112, 112), Sprite.RenderPoint.UpLeft);
            stopTex = new Sprite(delegate { return loader.MediaTexture; }, background.UpperLeft + new Vector2(45, 36) * RenderingDevice.TextureScaleFactor, new Rectangle(0, 266, 112, 112), Sprite.RenderPoint.UpLeft);
            prevTex = new Sprite(delegate { return loader.MediaTexture; }, background.UpperLeft + new Vector2(17, 10) * RenderingDevice.TextureScaleFactor, new Rectangle(112, 154, 76, 162), Sprite.RenderPoint.UpLeft);
            nextTex = new Sprite(delegate { return loader.MediaTexture; }, new Vector2(prevTex.LowerRight.X, prevTex.UpperLeft.Y) + new Vector2(12, 0) * RenderingDevice.TextureScaleFactor, new Rectangle(112, 317, 76, 162), Sprite.RenderPoint.UpLeft);

            play = new MenuButton(playTex, delegate { swapPlayPause(); string s = MediaSystem.StartShuffleCustomMusic(); if(s != string.Empty) ErrorString = s; });
            stop = new MenuButton(stopTex, delegate { swapPlayPause(); MediaSystem.StopCustomMusic(); });
            artists = new MenuButton(artistsTex, doArtistsClick);
            shuffle = new MenuButton(shuffleTex, delegate { string s = MediaSystem.StartShuffleCustomMusic(); if(s != string.Empty) ErrorString = s; });
            albums = new MenuButton(albumsTex, doAlbumsClick);
            prev = new VariableButton(prevTex, delegate { string s = MediaSystem.MovePrevious(); if(s != string.Empty) ErrorString = s; }, string.Empty);
            next = new VariableButton(nextTex, delegate { string s = MediaSystem.MoveNext(); if(s != string.Empty) ErrorString = s; }, string.Empty);
            play.MakeTransparencySensitive();
            stop.MakeTransparencySensitive();
            prev.MakeTransparencySensitive();
            next.MakeTransparencySensitive();

            prev.SetPointerDirectionals(null, new Pointer<MenuControl>(() => playPause, v => { }), null, null);
            next.SetPointerDirectionals(new Pointer<MenuControl>(() => playPause, v => { }), new Pointer<MenuControl>(() => artists, v => { }), null, null);
            play.SetDirectionals(prev, next, null, null);
            stop.SetDirectionals(prev, next, null, null);
            albums.SetDirectionals(next, prev, shuffle, artists);
            artists.SetDirectionals(next, prev, albums, shuffle);
            shuffle.SetDirectionals(next, prev, artists, albums);

            mainRenderPos = new Vector2(BaseGame.PreferredScreenWidth - mainRenderRect.Width, BaseGame.PreferredScreenHeight - mainRenderRect.Height) * 0.5f;
            sliderPos = sliderBaseRenderPos = mainRenderPos + new Vector2(578, 36);
            textPositions = new Vector2[7];
            textPositions[0] = mainRenderPos + new Vector2(46, 40);
            for(int i = 1; i < textPositions.Length; i++)
                textPositions[i] = textPositions[i - 1] + new Vector2(0, 23);

            RenderingDevice.GraphicsDevice.DeviceReset += onGDMReset;
            startDarkFade();
            scissorRectangle = new Rectangle((int)scissorBackground.UpperLeft.X, (int)scissorBackground.UpperLeft.Y, (int)scissorBackground.Width, (int)scissorBackground.Height);

            selectedControl = playPause = play;
            selectedControl.IsSelected = null;
            controlArray.AddRange(new[] { play, prev, next, artists, shuffle, albums });
            scrollingText = "Playing: (nothing)";
            textPos = scissorBackground.UpperLeft + new Vector2(0, (scissorBackground.Height - font.MeasureString(scrollingText).Y) / 2);
            ErrorString = "";
            MediaPlayer.ActiveSongChanged += onSongChanged;
        }

        private void startDarkFade()
        {
            fade = FadeDirection.In;
        }

        private void startFadeBack()
        {
            fade = FadeDirection.Out;
        }

        private void swapPlayPause()
        {
            if(controlArray.Contains(play))
            {
                selectedControl = playPause = stop;
                if(play.IsSelected == null)
                    stop.IsSelected = null;
                play.IsSelected = false;
                controlArray.Remove(play);
                controlArray.Add(stop);
            }
            else if(controlArray.Contains(stop))
            {
                selectedControl = playPause = play;
                if(stop.IsSelected == null)
                    play.IsSelected = null;
                stop.IsSelected = false;
                controlArray.Remove(stop);
                controlArray.Add(play);
            }
        }

        private void onGDMReset(object sender, EventArgs e)
        {
            scissorBackground.ForceResize();
            scissorRectangle = new Rectangle((int)scissorBackground.UpperLeft.X, (int)scissorBackground.UpperLeft.Y, (int)scissorBackground.Width, (int)scissorBackground.Height);
        }

        public override void Draw(GameTime gameTime)
        {
            if(GameManager.PreviousState == GameState.Running)
                GameManager.DrawLevel(gameTime);

            RenderingDevice.SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.LinearClamp, null, null);
            RenderingDevice.SpriteBatch.Draw(loader.EmptyTex, new Rectangle(0, 0, (int)RenderingDevice.Width, (int)RenderingDevice.Height), fill);
            if(fade != FadeDirection.None)
            {
                RenderingDevice.SpriteBatch.End();
                return;
            }

            scissorBackground.Draw();
            if(scrolling)
            {
                RasterizerState prev = RenderingDevice.GraphicsDevice.RasterizerState;
                Rectangle r = RenderingDevice.GraphicsDevice.ScissorRectangle;
                RenderingDevice.GraphicsDevice.RasterizerState = rs;
                RenderingDevice.GraphicsDevice.ScissorRectangle = scissorRectangle;
                RenderingDevice.SpriteBatch.DrawString(font, scrollingText, textPos, Color.SlateGray);
                RenderingDevice.GraphicsDevice.RasterizerState = prev;
                RenderingDevice.GraphicsDevice.ScissorRectangle = r;
            }
            else // just draw normally
                RenderingDevice.SpriteBatch.DrawString(font, scrollingText, textPos, Color.SlateGray);

            background.Draw();
            base.Draw(gameTime);

            if(state != MenuState.Normal || backgroundTint.A > 0)
            {
                Color selectionTint = Color.Red;
                selectionTint.A = backgroundTint.A;
                Color downTint = new Color(180, 0, 0, backgroundTint.A);

                RenderingDevice.SpriteBatch.Draw(loader.MediaTexture, new Vector2(RenderingDevice.Width, RenderingDevice.Height) * 0.5f, mainRenderRect, backgroundTint,
                    0, new Vector2(mainRenderRect.Width, mainRenderRect.Height) * 0.5f, RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);

                if(sliding)
                    RenderingDevice.SpriteBatch.Draw(loader.MediaTexture, sliderPos, sliderRenderRect, downTint,
                        0, Vector2.Zero, RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
                else if(InputManager.MouseState.IsWithinCoordinates(sliderPos, sliderPos + new Vector2(sliderRenderRect.Width, sliderRenderRect.Height) * RenderingDevice.TextureScaleFactor))
                    RenderingDevice.SpriteBatch.Draw(loader.MediaTexture, sliderPos, sliderRenderRect, selectionTint,
                        0, Vector2.Zero, RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
                else
                    RenderingDevice.SpriteBatch.Draw(loader.MediaTexture, sliderPos, sliderRenderRect, backgroundTint,
                        0, Vector2.Zero, RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);

                int step = 0;
                if(startingIndex < 0)
                    step = -startingIndex; // step is now positive
                else if(startingIndex > names.Length - 7)
                    step = -(names.Length - startingIndex);
                for(int i = startingIndex + (step < 0 ? 0 : step); i < (startingIndex + (step < 0 ? -step : 7)); i++)
                    RenderingDevice.SpriteBatch.DrawString(loader.Font, names[i], textPositions[i - startingIndex] * RenderingDevice.TextureScaleFactor,
                        i == selectedIndex ? (holdingEnter ? downTint : selectionTint) : backgroundTint, 0,
                        Vector2.Zero, RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
            }

            string s = "To return, press       .";

            Vector2 dim = loader.Font.MeasureString(s) * RenderingDevice.TextureScaleFactor;
            Vector2 firstPart = loader.Font.MeasureString("To return, press ") * RenderingDevice.TextureScaleFactor;

            RenderingDevice.SpriteBatch.DrawString(loader.Font, s, (mainRenderPos + new Vector2(mainRenderRect.Width / 2.0f + 5, mainRenderRect.Height)) * RenderingDevice.TextureScaleFactor - new Vector2(dim.X, 0),
                Color.White, 0, Vector2.Zero, RenderingDevice.TextureScaleFactor, SpriteEffects.None, 0);
            if(InputManager.ControlScheme == ControlScheme.Keyboard)
                SymbolWriter.WriteKeyboardIcon(InputManager.WindowsOptions.MusicKey,
                    (mainRenderPos + new Vector2(mainRenderRect.Width / 2.0f + 25, mainRenderRect.Height + 15)) * RenderingDevice.TextureScaleFactor - dim + firstPart, true);
            else
                SymbolWriter.WriteXboxIcon(InputManager.XboxOptions.MusicKey,
                    (mainRenderPos + new Vector2(mainRenderRect.Width / 2.0f + 25, mainRenderRect.Height + 15)) * RenderingDevice.TextureScaleFactor - dim + firstPart, true);

            RenderingDevice.SpriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            if(state == MenuState.Normal)
            {
                if(backgroundTint.A > 0)
                {
                    if(backgroundTint.A - deltaA * 2 < 0)
                    {
                        backgroundTint.A = 0;
                        albums = null;
                        artists = null;
                        names = null;
                        sliding = false;
                        holdingEnter = false;
                        startingIndex = -3;
                        selectedIndex = 0;
                    }
                    else
                        backgroundTint.A -= deltaA * 2;
                }

                if(ErrorString != string.Empty)
                {
                    scrollingText = ErrorString;
                    ErrorString = string.Empty;
                }

                if(InputManager.KeyboardState.WasKeyJustPressed(Program.Game.Manager.CurrentSaveWindowsOptions.MusicKey) || InputManager.CurrentPad.WasButtonJustPressed(Program.Game.Manager.CurrentSaveXboxOptions.MusicKey))
                    startFadeBack();

                #region Fading
                if(fade == FadeDirection.In)
                {
                    fill.A += deltaA;
                    if(fill.A >= fadeBoundary)
                        fade = FadeDirection.None;
                }
                else if(fade == FadeDirection.Out)
                {
                    if(fill.A - deltaA > 0)
                        fill.A -= deltaA;
                    else
                        fill.A = 0;
                    if(fill.A == 0)
                    {
                        fade = FadeDirection.In;
                        MediaSystem.PlayAll();
                        GameManager.State = GameManager.PreviousState;
                    }
                    else
                        return;
                }
                #endregion
            }
            else
            {
                if(backgroundTint.A < 255)
                {
                    if(backgroundTint.A + deltaA * 2 > 255)
                        backgroundTint.A = 255;
                    else
                        backgroundTint.A += deltaA * 2;
                }
                else
                {
                    #region keyboard input
                    if(InputManager.CurrentPad.WasButtonJustReleased(Program.Game.Manager.CurrentSaveXboxOptions.SelectionKey) ||
                        InputManager.KeyboardState.WasKeyJustReleased(Program.Game.Manager.CurrentSaveWindowsOptions.SelectionKey))
                    {
                        if(names[selectedIndex] != "(Empty)")
                        {
                            switch(state)
                            {
                                case MenuState.Albums:
                                    playAlbum(albums[selectedIndex]); // this should work
                                    break;
                                case MenuState.Artists:
                                    playArtist(artists[selectedIndex]);
                                    break;
                            }
                            MediaSystem.PlaySoundEffect(SFXOptions.Button_Release);
                            holdingEnter = false;
                        }
                    }
                    if(!holdingEnter || sliding)
                    {
                        if(InputManager.CurrentPad.WasButtonJustPressed(Program.Game.Manager.CurrentSaveXboxOptions.SelectionKey) ||
                            InputManager.KeyboardState.WasKeyJustPressed(Program.Game.Manager.CurrentSaveWindowsOptions.SelectionKey))
                        {
                            MediaSystem.PlaySoundEffect(SFXOptions.Button_Press);
                            holdingEnter = true;
                            MenuHandler.MouseTempDisabled = true;
                        }

                        else if(InputManager.CurrentPad.WasButtonJustPressed(Buttons.Back) || InputManager.KeyboardState.WasKeyJustPressed(Keys.Escape))
                        {
                            state = MenuState.Normal;
                            MenuHandler.MouseTempDisabled = true;
                        }
                        else if(InputManager.KeyboardState.WasKeyJustPressed(Program.Game.Manager.CurrentSaveWindowsOptions.MenuUpKey) ||
                            InputManager.CurrentPad.WasButtonJustPressed(Buttons.LeftThumbstickUp))
                        {
                            if(selectedIndex > 0)
                            {
                                selectedIndex--;
                                startingIndex--;
                                if(names[selectedIndex] == "(Empty)")
                                {
                                    selectedIndex++;
                                    startingIndex++;
                                }
                                else
                                    MediaSystem.PlaySoundEffect(SFXOptions.Button_Rollover);
                            }
                            MenuHandler.MouseTempDisabled = true;
                        }
                        else if(InputManager.KeyboardState.WasKeyJustPressed(Program.Game.Manager.CurrentSaveWindowsOptions.MenuDownKey) ||
                            InputManager.CurrentPad.WasButtonJustPressed(Buttons.LeftThumbstickDown))
                        {
                            if(selectedIndex < names.Length - 1)
                            {
                                startingIndex++;
                                selectedIndex++;
                                if(names[selectedIndex] == "(Empty)")
                                {
                                    selectedIndex--;
                                    startingIndex--;
                                }
                                else
                                    MediaSystem.PlaySoundEffect(SFXOptions.Button_Rollover);
                            }
                            MenuHandler.MouseTempDisabled = true;
                        }
                        else if(InputManager.KeyboardState.WasKeyJustPressed(Program.Game.Manager.CurrentSaveWindowsOptions.MenuLeftKey) ||
                            InputManager.CurrentPad.WasButtonJustPressed(Buttons.LeftThumbstickLeft))
                        {
                            if(selectedIndex > 0)
                            {
                                startingIndex -= 7;
                                selectedIndex -= 7;
                                if(selectedIndex < 0)
                                    selectedIndex = 0;
                                if(startingIndex < -3)
                                    startingIndex = -3;
                                MediaSystem.PlaySoundEffect(SFXOptions.Button_Rollover);
                            }
                            MenuHandler.MouseTempDisabled = true;
                        }
                        else if(InputManager.KeyboardState.WasKeyJustPressed(Program.Game.Manager.CurrentSaveWindowsOptions.MenuRightKey) ||
                            InputManager.CurrentPad.WasButtonJustPressed(Buttons.LeftThumbstickRight))
                        {
                            if(selectedIndex < names.Length - 1)
                            {
                                startingIndex += 7;
                                selectedIndex += 7;
                                if(selectedIndex > names.Length - 1)
                                    selectedIndex = names.Length - 1;
                                if(startingIndex > names.Length - 4)
                                    startingIndex = names.Length - 4;
                                MediaSystem.PlaySoundEffect(SFXOptions.Button_Rollover);
                            }
                            MenuHandler.MouseTempDisabled = true;
                        }
                    }
                    #endregion

                    if(sliding && MenuHandler.MouseTempDisabled)
                        MenuHandler.MouseTempDisabled = false;

                    #region slider
                    if(InputManager.MouseState.IsWithinCoordinates(sliderPos * RenderingDevice.TextureScaleFactor, (sliderPos + new Vector2(sliderRenderRect.Width, sliderRenderRect.Height)) * RenderingDevice.TextureScaleFactor) &&
                        !wasMouseWithinCoordinates(sliderPos * RenderingDevice.TextureScaleFactor, (sliderPos + new Vector2(sliderRenderRect.Width, sliderRenderRect.Height)) * RenderingDevice.TextureScaleFactor))
                        MediaSystem.PlaySoundEffect(SFXOptions.Button_Rollover);
                    if(!sliding && InputManager.MouseState.IsWithinCoordinates(sliderPos, sliderPos + new Vector2(sliderRenderRect.Width, sliderRenderRect.Height)) &&
                        InputManager.MouseState.LeftButton == ButtonState.Pressed)
                    {
                        sliding = true;
                        MediaSystem.PlaySoundEffect(SFXOptions.Button_Press);
                    }
                    if(InputManager.MouseState.LeftButton != ButtonState.Pressed && sliding)
                    {
                        sliding = false;
                        MediaSystem.PlaySoundEffect(SFXOptions.Button_Release);
                    }
                    if(!sliding)
                    {
                        sliderPos.Y = (sliderBaseRenderPos.Y + selectedIndex * sliderStepDistance) * RenderingDevice.TextureScaleFactor.Y;
                        if(sliderPos.Y < sliderBaseRenderPos.Y)
                            sliderPos.Y = sliderBaseRenderPos.Y * RenderingDevice.TextureScaleFactor.Y;
                        else if(sliderPos.Y > sliderBaseRenderPos.Y + sliderSlideDistance)
                            sliderPos.Y = (sliderBaseRenderPos.Y + sliderSlideDistance) * RenderingDevice.TextureScaleFactor.Y;
                    }
                    else
                    {
                        if(InputManager.MouseState.Y > sliderBaseRenderPos.Y * RenderingDevice.TextureScaleFactor.Y &&
                            InputManager.MouseState.Y < (sliderBaseRenderPos.Y + sliderSlideDistance) * RenderingDevice.TextureScaleFactor.Y)
                            sliderPos.Y = InputManager.MouseState.Y;
                        else if(InputManager.MouseState.Y < sliderBaseRenderPos.Y * RenderingDevice.TextureScaleFactor.Y)
                            sliderPos.Y = sliderBaseRenderPos.Y * RenderingDevice.TextureScaleFactor.Y;
                        else
                            sliderPos.Y = (sliderBaseRenderPos.Y + sliderSlideDistance) * RenderingDevice.TextureScaleFactor.Y;

                        if(Math.Abs(sliderPos.Y - sliderBaseRenderPos.Y * RenderingDevice.TextureScaleFactor.Y) < epsilon)
                            startingIndex = -3;
                        else if(Math.Abs(sliderPos.Y - ((sliderBaseRenderPos.Y + sliderSlideDistance) * RenderingDevice.TextureScaleFactor.Y)) < epsilon)
                            startingIndex = names.Length - 4;
                        else
                        {
                            int i;
                            float step = sliderStepDistance * RenderingDevice.TextureScaleFactor.Y;
                            float top = sliderBaseRenderPos.Y * RenderingDevice.TextureScaleFactor.Y;
                            for(i = 0; i < names.Length - 2; i++)
                            {
                                if(i * step + top < InputManager.MouseState.Y && (i + 1) * step + top > InputManager.MouseState.Y)
                                {
                                    float upper = (i * step + top) - InputManager.MouseState.Y;
                                    float lower = InputManager.MouseState.Y - ((i + 1) * step + top);
                                    if(lower > upper)
                                        i = i + 1;
                                    break;
                                }
                            }
                            startingIndex = i - 3;
                        }

                        selectedIndex = startingIndex + 3;
                    }
                    #endregion

                    #region mouse
                    if(!MenuHandler.MouseTempDisabled)
                    {
                        if(!sliding && !holdingEnter)
                        {
                            Vector2 upperLeftBlack = (mainRenderPos + new Vector2(44, 110)) * RenderingDevice.TextureScaleFactor;
                            Vector2 lowerRightBlack = (mainRenderPos + new Vector2(572, 130)) * RenderingDevice.TextureScaleFactor;
                            if(InputManager.MouseState.IsWithinCoordinates(upperLeftBlack, lowerRightBlack) && InputManager.MouseState.WasMouseJustClicked())
                            {
                                holdingEnter = true;
                                MediaSystem.PlaySoundEffect(SFXOptions.Button_Press);
                            }

                            int step = InputManager.MouseState.ScrollWheelDelta;
                            if(step != 0)
                            {
                                step = Math.Sign(step);
                                MediaSystem.PlaySoundEffect(SFXOptions.Button_Rollover);
                            }
                            startingIndex -= step;
                            if(startingIndex < -3)
                                startingIndex = -3;
                            if(startingIndex > names.Length - 4)
                                startingIndex = names.Length - 4;
                            selectedIndex = startingIndex + 3;
                        }
                        if(holdingEnter && InputManager.MouseState.WasMouseJustReleased())
                        {
                            if(names[selectedIndex] != "(Empty)")
                            {
                                switch(state)
                                {
                                    case MenuState.Albums:
                                        playAlbum(albums[selectedIndex]); // this should work
                                        break;
                                    case MenuState.Artists:
                                        playArtist(artists[selectedIndex]);
                                        break;
                                }
                                MediaSystem.PlaySoundEffect(SFXOptions.Button_Release);
                                holdingEnter = false;
                            }
                        }
                    }
                    #endregion
                }
            }

            // update scrolling text
            scrolling = scrollingText != null && font.MeasureString(scrollingText).X > scissorBackground.Width;
            if(scrolling)
            {
                if(timer < waitTime)
                    timer += gameTime.ElapsedGameTime.Milliseconds;
                else
                    textPos.X -= 1f;
            }
            if(font.MeasureString(scrollingText).X + textPos.X <= scissorBackground.UpperLeft.X)
                textPos.X = scissorBackground.LowerRight.X;

            if(fade == FadeDirection.None && state == MenuState.Normal && backgroundTint.A == 0) // stop updating the buttons when we're selecting other stuff
                base.Update(gameTime);
        }

        private void onSongChanged(object sender, EventArgs e)
        {
            timer = 0;
            scrollingText = "Playing: " + MediaSystem.GetPlayingSong();
            textPos = scissorBackground.UpperLeft + new Vector2(0, (scissorBackground.Height - font.MeasureString(scrollingText).Y) / 2);
        }

        private void doArtistsClick()
        {
            ArtistCollection artists = MediaSystem.GetArtistsInLibrary();
            if(artists.Count == 0)
            {
                scrollingText = "No artists found. Try running Windows Media Player to create the required library.";
                textPos = scissorBackground.UpperLeft + new Vector2(0, (scissorBackground.Height - font.MeasureString(scrollingText).Y) / 2);
                return;
            }

            this.artists = artists;
            names = new string[artists.Count < 7 ? 7 : artists.Count];
            int i = 0;
            foreach(Artist a in artists)
                names[i++] = a.Name;
            if(names.Length < 7)
                for(i = 6 - names.Length; i < 6; i++)
                    names[i] = "(Empty)";

            float maxWidth = 523 * RenderingDevice.TextureScaleFactor.X;
            for(i = 0; i < names.Length; i++)
            {
                string temp = names[i];
                while(loader.Font.MeasureString(temp).X * RenderingDevice.TextureScaleFactor.X > maxWidth)
                    temp = temp.Substring(0, temp.Length - 5) + "..."; // chop off last 4 characters and add "..."
                names[i] = temp;
            }

            sliderStepDistance = ((float)sliderSlideDistance / (names.Length - 4)) * RenderingDevice.TextureScaleFactor.Y;
            sliderPos = sliderBaseRenderPos * RenderingDevice.TextureScaleFactor;

            state = MenuState.Artists;
        }

        private void doAlbumsClick()
        {
            AlbumCollection albums = MediaSystem.GetAlbumsInLibrary();
            if(albums.Count == 0)
            {
                scrollingText = "No albums found. Try running Windows Media Player to create the required library.";
                textPos = scissorBackground.UpperLeft + new Vector2(0, (scissorBackground.Height - font.MeasureString(scrollingText).Y) / 2);
                return;
            }

            this.albums = albums;
            names = new string[artists.Count < 7 ? 7 : artists.Count];
            int i = 0;
            foreach(Album a in albums)
                names[i++] = a.Name;
            if(names.Length < 7)
                for(i = 6 - names.Length; i < 6; i++)
                    names[i] = "(Empty)";

            float maxWidth = 523 * RenderingDevice.TextureScaleFactor.X;
            for(i = 0; i < names.Length; i++)
            {
                string temp = names[i];
                while(loader.Font.MeasureString(temp).X * RenderingDevice.TextureScaleFactor.X > maxWidth)
                    temp = temp.Substring(0, temp.Length - 5) + "..."; // chop off last 4 characters and add "..."
                names[i] = temp;
            }

            sliderStepDistance = ((float)sliderSlideDistance / (names.Length - 4)) * RenderingDevice.TextureScaleFactor.Y;
            sliderPos = sliderBaseRenderPos * RenderingDevice.TextureScaleFactor;

            state = MenuState.Albums;
        }

        private void playArtist(Artist a)
        {
            string s = MediaSystem.StartArtistsCustomMusic(a);
            if(s != string.Empty)
                ErrorString = s;
            state = MenuState.Normal;
            swapPlayPause();
        }

        private void playAlbum(Album a)
        {
            string s = MediaSystem.StartAlbumCustomMusic(a);
            if(s != string.Empty)
                ErrorString = s;
            state = MenuState.Normal;
            swapPlayPause();
        }
    }
}
