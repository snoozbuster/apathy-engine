using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework.Input;

namespace ApathyEngine
{
    public static class ApathyExtensions
    {
        public static void Initialize(GraphicsDevice device)
        {
#if DEBUG
            effect = new BasicEffect(device);
            RenderingDevice.GDM.DeviceCreated += delegate { effect = new BasicEffect(RenderingDevice.GraphicsDevice); };
            effect.VertexColorEnabled = true;
            effect.LightingEnabled = false;
#endif
        }

        #region BoundingBox extension - Draw()
#if DEBUG
        static VertexPositionColor[] verts = new VertexPositionColor[8];
        static BasicEffect effect;

        static int[] indices = new int[]
        {
            0, 1,
            1, 2,
            2, 3,
            3, 0,
            0, 4,
            1, 5,
            2, 6,
            3, 7,
            4, 5,
            5, 6,
            6, 7,
            7, 4,
        };

        public static void Draw(this BEPUutilities.BoundingBox box)
        {
            if(RenderingDevice.HiDef)
            {
                effect.View = RenderingDevice.Camera.View;
                effect.Projection = RenderingDevice.Camera.Projection;

                BEPUutilities.Vector3[] corners = box.GetCorners();
                for(int i = 0; i < 8; i++)
                {
                    verts[i].Position = new Vector3(corners[i].X, corners[i].Y, corners[i].Z);
                    verts[i].Color = Color.Goldenrod;
                }

                foreach(EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    RenderingDevice.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, verts, 0, 8, indices, 0, indices.Length / 2);
                }
            }
        }
#endif
        #endregion

        #region Vector3 extension - ToRadians
        /// <summary>
        /// Converts each member of a Vector3 into a radian representation.
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static Vector3 ToRadians(this Vector3 degrees)
        {
            return new Vector3(MathHelper.ToRadians(degrees.X), MathHelper.ToRadians(degrees.Y), MathHelper.ToRadians(degrees.Z));
        }
        #endregion

        #region Vector3 extension - AngleBetween()
        public static float AngleBetween(this Vector3 v1, Vector3 v2)
        {
            float dot = Vector3.Dot(v1, v2);
            float mag1 = Math.Abs(v1.Length());
            float mag2 = Math.Abs(v2.Length());
            return (float)Math.Acos(dot / (mag1 * mag2));
        }
        #endregion

        #region GamePadState extension - GetPressedButtons()
        public static Buttons[] GetPressedButtons(this GamePadState state)
        {
            List<Buttons> pressedButtons = new List<Buttons>();

            GamePadButtons buttons = state.Buttons;
            if(buttons.A == ButtonState.Pressed)
                pressedButtons.Add(Buttons.A);
            if(buttons.B == ButtonState.Pressed)
                pressedButtons.Add(Buttons.B);
            if(buttons.LeftShoulder == ButtonState.Pressed)
                pressedButtons.Add(Buttons.LeftShoulder);
            if(buttons.LeftStick == ButtonState.Pressed)
                pressedButtons.Add(Buttons.LeftStick);
            if(buttons.RightShoulder == ButtonState.Pressed)
                pressedButtons.Add(Buttons.RightShoulder);
            if(buttons.RightStick == ButtonState.Pressed)
                pressedButtons.Add(Buttons.RightStick);
            if(buttons.X == ButtonState.Pressed)
                pressedButtons.Add(Buttons.X);
            if(buttons.Y == ButtonState.Pressed)
                pressedButtons.Add(Buttons.Y);

            GamePadDPad dpad = state.DPad;
            if(dpad.Down == ButtonState.Pressed)
                pressedButtons.Add(Buttons.DPadDown);
            if(dpad.Up == ButtonState.Pressed)
                pressedButtons.Add(Buttons.DPadUp);
            if(dpad.Left == ButtonState.Pressed)
                pressedButtons.Add(Buttons.DPadLeft);
            if(dpad.Right == ButtonState.Pressed)
                pressedButtons.Add(Buttons.DPadRight);

            return pressedButtons.ToArray();
        }
        #endregion
    }
}
