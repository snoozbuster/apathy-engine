using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using ApathyEngine.Input;

namespace ApathyEngine.Graphics
{
    public class ArcBallCamera : ICamera
    {
        private Matrix rotation = Matrix.Identity;
        public Matrix Rotation { get { return rotation; } }
        public Vector3 Position { get; private set; }

        // Simply feed this camera the position of whatever you want its target to be
        protected Vector3 targetPosition = Vector3.Zero;
        public Vector3 TargetPosition { get { return targetPosition; } set { targetPosition = value; } }
        protected Vector3 posLastFrame;

        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }
        private float zoom = 100.0f;
        public float Zoom { get { return zoom; } set { zoom = MathHelper.Clamp(value, zoomMin, zoomMax); } }

        private float horizontalAngle = MathHelper.PiOver2;
        public float HorizontalAngle
        {
            get { return horizontalAngle; }
            set
            {
                if(value >= MathHelper.Pi || value <= -MathHelper.Pi)
                {
                    horizontalAngle = -horizontalAngle + (value % MathHelper.Pi);
                    return;
                }
                horizontalAngle = value % MathHelper.Pi; 
            }
        }

        private float verticalAngle = (float)Math.Sqrt(3) / 2;
        public float VerticalAngle{ get { return verticalAngle; } set { verticalAngle = MathHelper.Clamp(value, verticalAngleMin, verticalAngleMax); } }

#if DEBUG || INTERNAL
        private float verticalAngleMin = 0;
        private float verticalAngleMax = MathHelper.TwoPi;
        private float zoomMin = 0;
        private float zoomMax = 10000;
        public bool debugCamera { get; private set; }
#else
        private const float verticalAngleMin = 0.7f;
        private const float verticalAngleMax = 1.25f;
        private const float zoomMin = 50.0f;
        private const float zoomMax = 125.0f;
#endif

        public Matrix WorldViewProj { get { return World * ViewProj; } }
        public Matrix InverseView { get { return Matrix.Invert(View); } }
        public Matrix ViewProj { get { return View * Projection; } }
        public Matrix World { get { return Matrix.Identity; } }
        public AudioListener Ears { get; private set; }

        /// <summary>
        /// Creates a camera.
        /// </summary>
        /// <param name="fieldOfView">The field of view in radians.</param>
        /// <param name="aspectRatio">The aspect ratio of the game.</param>
        /// <param name="nearPlane">The near plane.</param>
        /// <param name="farPlane">The far plane.</param>
        public ArcBallCamera(float fieldOfView, float aspectRatio, float nearPlane, float farPlane)
        {
            if(nearPlane < 0.1f)
                throw new ArgumentException("nearPlane must be greater than 0.1.");

            Position = new Vector3(20, 20, 20);

            HorizontalAngle = MathHelper.PiOver4;
            VerticalAngle = (float)Math.Sqrt(3) / 2;

            this.Projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio,
                                                                        nearPlane, farPlane);
            this.LookAt(TargetPosition);
            this.View = Matrix.CreateLookAt(this.Position,
                                            this.Position + this.rotation.Forward,
                                            this.rotation.Up);
            
            Ears = new AudioListener();
            Ears.Up = Vector3.UnitZ;
            Ears.Position = Position;
            Ears.Forward = rotation.Forward;
        }

        public void Update(GameTime gameTime)
        {
            posLastFrame = Position;
            Vector3 cameraPosition = new Vector3(0.0f, 0.0f, zoom);

            HandleInput(gameTime);

            // Rotate vertically
            cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateRotationX(verticalAngle));

            // Rotate horizontally
            cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateRotationZ(horizontalAngle));
            
            Position = cameraPosition + TargetPosition;

            this.LookAt(TargetPosition);

            // Compute view matrix
            this.View = Matrix.CreateLookAt(this.Position,
                                            this.Position + this.rotation.Forward,
                                            this.rotation.Up);

            Ears.Position = Position;
            Ears.Forward = rotation.Forward;
            Ears.Velocity = (Position - cameraPosition) / gameTime.ElapsedGameTime.Milliseconds;
        }

        /// <summary>
        /// Points camera in direction of any position.
        /// </summary>
        /// <param name="targetPos">Target position for camera to face.</param>
        public void LookAt(Vector3 targetPos)
        {
            Vector3 newForward = targetPos - this.Position;
            newForward.Normalize();
            this.rotation.Forward = newForward;

            Vector3 referenceVector = Vector3.UnitZ;

            this.rotation.Right = Vector3.Cross(this.rotation.Forward, referenceVector);
            this.rotation.Up = Vector3.Cross(this.rotation.Right, this.rotation.Forward);
        }

        private void HandleInput(GameTime gameTime)
        {
            if(game.State == GameState.Results)
                return;

#if DEBUG
            if(InputManager.KeyboardState.WasKeyJustPressed(Keys.Space))
                debugCamera = !debugCamera;

            if(!debugCamera)
            {
                verticalAngleMin = 0.7f;
                verticalAngleMax = 1.25f;
                zoomMin = 50.0f;
                zoomMax = 125.0f;
            }
            else
            {
                verticalAngleMin = 0;
                verticalAngleMax = MathHelper.TwoPi;
                zoomMin = 0;
                zoomMax = 10000;
            }

#endif
            if(InputManager.ControlScheme == ControlScheme.Keyboard)
            {
                if(!InputManager.WindowsOptions.SwapCamera)
                {
                    if(InputManager.KeyboardState.IsKeyDown(InputManager.WindowsOptions.CameraDownKey))
                        VerticalAngle += gameTime.ElapsedGameTime.Milliseconds * MathHelper.ToRadians(0.1f);
                    if(InputManager.KeyboardState.IsKeyDown(InputManager.WindowsOptions.CameraUpKey))
                        this.VerticalAngle -= gameTime.ElapsedGameTime.Milliseconds * MathHelper.ToRadians(0.1f);
                }
                else
                {
                    if(InputManager.KeyboardState.IsKeyDown(InputManager.WindowsOptions.CameraDownKey))
                        this.VerticalAngle -= gameTime.ElapsedGameTime.Milliseconds * MathHelper.ToRadians(0.1f);
                    if(InputManager.KeyboardState.IsKeyDown(InputManager.WindowsOptions.CameraUpKey))
                        this.VerticalAngle += gameTime.ElapsedGameTime.Milliseconds * MathHelper.ToRadians(0.1f);
                }
                if(InputManager.KeyboardState.IsKeyDown(InputManager.WindowsOptions.CameraLeftKey))
                    this.HorizontalAngle -= gameTime.ElapsedGameTime.Milliseconds * MathHelper.ToRadians(0.1f);
                if(InputManager.KeyboardState.IsKeyDown(InputManager.WindowsOptions.CameraRightKey))
                    this.HorizontalAngle += gameTime.ElapsedGameTime.Milliseconds * MathHelper.ToRadians(0.1f);

                if(InputManager.KeyboardState.IsKeyDown(InputManager.WindowsOptions.CameraZoomPlusKey) ||
                    (InputManager.WindowsOptions.CameraZoomPlusKey == Keys.Add && InputManager.KeyboardState.IsKeyDown(Keys.OemPlus)))
                    this.Zoom -= (float)gameTime.ElapsedGameTime.TotalSeconds * 50f;

                if(InputManager.KeyboardState.IsKeyDown(InputManager.WindowsOptions.CameraZoomMinusKey) ||
                    (InputManager.WindowsOptions.CameraZoomMinusKey == Keys.Subtract && InputManager.KeyboardState.IsKeyDown(Keys.OemMinus)))
                    this.Zoom += (float)gameTime.ElapsedGameTime.TotalSeconds * 50f;
#if WINDOWS
                Zoom -= MathHelper.ToRadians(InputManager.MouseState.ScrollWheelDelta);
                if(InputManager.MouseState.RightButton == ButtonState.Pressed)
                {
                    Vector2 delta = InputManager.MouseState.MousePositionDelta;
                    HorizontalAngle += MathHelper.ToRadians(delta.X);
                    VerticalAngle += MathHelper.ToRadians((delta.Y) * 0.5f);
                }
#endif
            }
            else
            {
                Vector2 sticks = new Vector2(MathHelper.Clamp(InputManager.CurrentPad.ThumbSticks.Left.X + InputManager.CurrentPad.ThumbSticks.Right.X, -1, 1),
                    MathHelper.Clamp(InputManager.CurrentPad.ThumbSticks.Left.Y + InputManager.CurrentPad.ThumbSticks.Right.Y, -1, 1));

                if(InputManager.WindowsOptions.SwapCamera)
                    sticks.Y = -sticks.Y;

                this.HorizontalAngle += sticks.X * (float)gameTime.ElapsedGameTime.TotalSeconds * MathHelper.ToRadians(0.1f);
                this.VerticalAngle -= sticks.Y * (float)gameTime.ElapsedGameTime.TotalSeconds * MathHelper.ToRadians(0.1f);

                if(InputManager.CurrentPad.IsButtonDown(InputManager.XboxOptions.CameraZoomPlusKey))
                    this.Zoom -= (float)gameTime.ElapsedGameTime.TotalSeconds * 50f;

                if(InputManager.CurrentPad.IsButtonDown(InputManager.XboxOptions.CameraZoomMinusKey))
                    this.Zoom += (float)gameTime.ElapsedGameTime.TotalSeconds * 50f;
            }
        }

        public void SetForResultsScreen()
        {
            zoom = 40f;
            horizontalAngle = MathHelper.ToRadians(45);
            verticalAngle = (float)Math.Sqrt(3) / 2;
            TargetPosition = Vector3.Zero;
        }

        public void Reset()
        {
            zoom = 100f;
            verticalAngle = (float)Math.Sqrt(3) / 2;
            horizontalAngle = MathHelper.PiOver2;
            TargetPosition = Vector3.Zero;
        }
    }
}
