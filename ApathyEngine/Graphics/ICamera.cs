using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApathyEngine.Graphics
{
    public interface ICamera
    {
        Matrix World { get; }
        Matrix View { get; }
        Matrix Projection { get; }

        Matrix WorldViewProj { get; }

        Vector3 Position { get; }
        Vector3 TargetPosition { get; set; }
        Matrix Rotation { get; }

        float Zoom { get; set; }

#if DEBUG
        bool debugCamera { get; }
#endif

        void Update(GameTime gameTime);
        void SetForResultsScreen();
        void Reset();
        void LookAt(Vector3 target);
    }
}
