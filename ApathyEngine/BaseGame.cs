using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.IO;
using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Win32;
using ApathyEngine.IO;
using ApathyEngine.Input;
using ApathyEngine.Media;

namespace ApathyEngine
{
    public class BaseGame : Game
    {
        #region Graphics
        /// <summary>
        /// Gets the primary GraphicsDeviceManager.
        /// </summary>
        public GraphicsDeviceManager GDM { get; private set; }
            
        /// <summary>
        /// Defines the preferred screen height for the game.
        /// </summary>
        public virtual static int PreferredScreenHeight { get { return 720; } }
        /// <summary>
        /// Defines the preferred screen width for the game.
        /// </summary>
        public virtual static int PreferredScreenWidth { get { return 1280; } }
        #endregion

        #region I/O
        public IOManager<T> Manager { get; private set; }
        #endregion

        #region Loading
        protected LoadingScreen LoadingScreen { get; private set; }
        public Loader Loader { get; private set; }
        public bool Loading { get; private set; }

        protected Texture2D backgroundTex;
        protected int alpha = 0;

        protected bool readyToLoad = false;
        protected bool beenDrawn = false;
        #endregion

        /// <summary>
        /// Gets a value indicating if the game can current pause while running.
        /// </summary>
        protected virtual bool canPause
        {
            get
            {
                return InputManager.KeyboardState.WasKeyJustPressed(Manager.CurrentSaveWindowsOptions.PauseKey) ||
                       InputManager.CurrentPad.WasButtonJustPressed(Manager.CurrentSaveXboxOptions.PauseKey);
            }
        }
        /// <summary>
        /// Gets the screen clear color.
        /// </summary>
        protected virtual Color backgroundColor { get { return Color.Black; } }

        protected bool successfulRender;
        protected bool locked = false;
        protected GameState stateLastFrame;

        public BaseGame()
        {
            IsMouseVisible = true;
#if XBOX
            Components.Add(new GamerServicesComponent(this));
#endif
            Content = new ContentManager(Services);
            GDM = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            GDM.PreferredBackBufferWidth = PreferredScreenWidth;
            GDM.PreferredBackBufferHeight = PreferredScreenHeight;

            // supported resolutions are 800x600, 1280x720, 1366x768, 1980x1020, and 1024x768
            int width = GraphicsDevice.DisplayMode.Width;
            int height = GraphicsDevice.DisplayMode.Height;
            if(width > 800) // this setup doesn't account for aspect ratios, which may be a problem
            {
                if(width > 1024)
                {
                    if(width > 1280)
                    {
                        if(width > 1366)
                        {
                            GDM.PreferredBackBufferWidth = 1366;
                            GDM.PreferredBackBufferHeight = 768;
                        }
                        else
                        {
                            GDM.PreferredBackBufferWidth = 1280;
                            GDM.PreferredBackBufferHeight = 720;
                        }
                    }
                    else
                    {
                        GDM.PreferredBackBufferWidth = 1024;
                        GDM.PreferredBackBufferHeight = 768;
                    }
                }
            }
            else if(width < 800)
            {
                ShowMissingRequirementMessage(new Exception("The game requires at least an 800x600 screen resolution."));
                Exit();
            }
            else
            {
                GDM.PreferredBackBufferWidth = 800;
                GDM.PreferredBackBufferHeight = 600;
            }

            GameManager.FirstStageInitialization(this);

            AccomplishmentManager.InitAchievements();

            try { Manager = new IOManager(); }
            catch { }

            if(Manager != null && Manager.SuccessfulLoad)
                MenuHandler.SaveLoaded();

            GDM.ApplyChanges();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            RenderingDevice.Initialize(GDM, GameManager.Space, Content.Load<Effect>("Shaders/shadowmap"));

            LoadingScreen = new LoadingScreen(Content, GraphicsDevice);

            backgroundTex = Content.Load<Texture2D>("2D/Splashes and Overlays/Logo");

            ApathyExtensions.Initialize(GraphicsDevice);
            Resources.Initialize(Content);
#if WINDOWS
            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            GDM.DeviceCreated += onGDMCreation;
#endif
        }

        /// <summary>
        /// Provides boilerplate updating code for a game using the Apathy Engine.
        /// </summary>
        /// <remarks>Put actual update code inside <pre>updateRunning(gameTime)</pre>.</remarks>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if(stateLastFrame == GameState.Running && GameManager.State == GameState.Paused)
                MediaSystem.PlaySoundEffect(SFXOptions.Pause);

            stateLastFrame = GameManager.State;

            if((!IsActive && Loader != null) || locked)
            {
                base.Update(gameTime);
                return;
            }

            InputManager.Update(gameTime, MenuHandler.IsSelectingSave);
            MediaSystem.Update(gameTime, IsActive);

            if(Manager.SaveLoaded)
                AccomplishmentManager.Update(gameTime);

            if(LoadingScreen != null)
            {
                IsMouseVisible = false;

                if(LoadingScreen.FadeComplete)
                {
                    Loading = true;
                    IsFixedTimeStep = false;

                    Loader = LoadingScreen.Update(gameTime);
                    if(Loader != null)
                        onLoadComplete();
                }
            }
            else
            {
                if(GameManager.State == GameState.MainMenu && Loading)
                    this.IsMouseVisible = false;
                else
                    this.IsMouseVisible = true;

                GameState statePrior = GameManager.State;
                MenuHandler.Update(gameTime);
                bool stateChanged = GameManager.State != statePrior; 

                if(GameManager.State == GameState.Running)
                {
                    if(canPause && !stateChanged)
                        GameManager.State = GameState.Paused;
                    else
                        updateRunning(gameTime);
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Provides standard boilerplate for games using the Apathy Engine. Put actual draw code in
        /// <pre>drawRunning()</pre>.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if(!beenDrawn)
            {
                MediaSystem.Ready(Content);
                beenDrawn = true;
            }

            if(Loading || !readyToLoad)
            {
                LoadingScreen.Draw();
                return;
            }

            GraphicsDevice.Clear(backgroundColor);

            if(GameManager.State == GameState.Running)
                drawRunning(gameTime);

            MenuHandler.Draw(gameTime);
            AccomplishmentManager.Draw();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Called during Update() when the game is running and needs to be updated.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected virtual void updateRunning(GameTime gameTime)
        {
            Manager.CurrentSave.Playtime += gameTime.ElapsedGameTime;
            GameManager.Space.Update((float)(gameTime.ElapsedGameTime.TotalSeconds));
#if DEBUG
            if(InputManager.KeyboardState.IsKeyDown(Keys.Q))
                GameManager.Space.Update((float)(gameTime.ElapsedGameTime.TotalSeconds));
#endif
            RenderingDevice.Update(gameTime);
            if(GameManager.CurrentLevel != null)
                GameManager.CurrentLevel.Update(gameTime);
        }

        /// <summary>
        /// Called during Draw() when the game is running and needs to be drawn.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected virtual void drawRunning(GameTime gameTime)
        {
            GameManager.DrawLevel();
        }

        /// <summary>
        /// Executed when loading is completed.
        /// </summary>
        protected virtual void onLoadComplete()
        {
            // Content loaded.  Use loader members to get at the loaded content.
            LoadingScreen = null;
            IsFixedTimeStep = true;
            Loading = false;
            MenuHandler.Create(Loader);
            GameManager.Initialize(Loader.Font, Manager);
            AccomplishmentManager.Ready();
        }

        #region Miscellaneous
        public void RestartLoad()
        {
            LoadingScreen = new LoadingScreen(Content, GraphicsDevice);

            backgroundTex = Content.Load<Texture2D>("2D/Splashes and Overlays/Logo");

            Extensions.Initialize(GraphicsDevice);
            Resources.Initialize(Content);
        }

        protected void onGDMCreation(object sender, EventArgs e)
        {
            Loader.ReloadALLtheThings();
            Goal.GDMReset();
            Theme.OnGDMReset();
            Level.OnGDMReset();
            RenderingDevice.OnGDMCreation(Content.Load<Effect>("Shaders/shadowmap"));
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            try 
            { 
                Manager.Save(false);
            }
            // Ignore errors, it's too late to do anything about them.
            catch { }

#if INDIECITY
            //Make call to end the session.
            Session.EndSession();

            do
            {
                Session.UpdateSession();
                System.Threading.Thread.Sleep(100);
            } while(Session.IsSessionStarted());
#endif

            base.OnExiting(sender, args);
        }

        protected override void OnActivated(object sender, EventArgs args)
        {
#if INDIECITY
            if(error)
                return;
#endif

            if(GameManager.PreviousState == GameState.Running && Manager.CurrentSaveWindowsOptions.ResumeOnFocus)
                GameManager.State = GameState.Running;
            if(GameManager.State == GameState.Running)
                MediaSystem.PlayAll();
            else
                MediaSystem.ResumeBGM();
            base.OnActivated(sender, args);
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
            if(GameManager.State == GameState.Running)
                if(GameManager.CurrentLevel == null)
                    GameManager.State = GameState.Paused;
                else if(!GameManager.CurrentLevel.ShowingOverlay)
                    GameManager.State = GameState.Paused;

            MediaSystem.PauseAll();
            base.OnDeactivated(sender, args);
        }

#if WINDOWS
        protected void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if(e.Reason == SessionSwitchReason.SessionLock)
            {
                OnDeactivated(sender, e);
                locked = true;
            }
            else if(e.Reason == SessionSwitchReason.SessionUnlock)
            {
                OnActivated(sender, e);
                locked = false;
            }
        }
#endif
        #endregion
    }
}
