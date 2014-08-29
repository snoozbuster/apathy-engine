using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics;
using System.Threading;
using ApathyEngine.Utilities;

namespace ApathyEngine.Graphics
{
    public delegate SpriteFont FontDelegate();

    // I've temporarily renamed this to make all references to RenderingDevice invalid; I want to 
    // look for alternatives where possible as well as significantly rewrite this class.
    public static class SecretRenderingDevice
    {
        private static readonly List<ModelData> texturedModels = new List<ModelData>();
        //private static readonly Dictionary<BaseModel, bool> untexturedModels = new Dictionary<BaseModel, bool>();
        private static readonly List<ModelData> glassModels = new List<ModelData>();
        private static readonly List<PrimData> primitives = new List<PrimData>();
        private static readonly List<ShaderModel> shaderModels = new List<ShaderModel>();
        /// <summary>
        /// Gets or sets the current camera in use.
        /// </summary>
        public static ICamera Camera { get; set; }
        private static LightingSystem lights;
        public static LightingSystem Lights { get { return lights; } }
        private static RenderTarget2D renderTargetReflection;
        private static RenderTarget2D renderTargetRefraction;
        //private static Texture2D shadowMap;

        private static Dictionary<Model, List<Texture2D>> masterDict = new Dictionary<Model, List<Texture2D>>();

        public static Effect Shader { get { return lights.Shader; } }

        private class Device : Log.INameable { string Log.INameable.Name { get { return "RenderingDevice"; } } }
        private static Device d = new Device();

        public static SpriteBatch SpriteBatch { get; private set; }
        public static GraphicsDeviceManager GDM { get; private set; }
        public static GraphicsDevice GraphicsDevice { get { return GDM.GraphicsDevice; } }
        private static Space Space;

#if WINDOWS
        /// <summary>
        /// A short form of graphics.GraphicsDevice.Viewport.Height.
        /// </summary>
        public static float Height { get { return GraphicsDevice.Viewport.Height; } }
        /// <summary>
        /// A short form of graphics.GraphicsDevice.Viewport.Width.
        /// </summary>
        public static float Width { get { return GraphicsDevice.Viewport.Width; } }
#elif XBOX
        /// <summary>
        /// A short form of graphics.GraphicsDevice.Viewport.Height.
        /// </summary>
        public static float Height { get { return GraphicsDevice.Viewport.Height * 0.9f; } }
        /// <summary>
        /// A short form of graphics.GraphicsDevice.Viewport.Width.
        /// </summary>
        public static float Width { get { return GraphicsDevice.Viewport.Width * 0.9f; } }

#endif

        public static int ScreenWidth { get { return GraphicsDevice.DisplayMode.Width; } }
        public static int ScreenHeight { get { return GraphicsDevice.DisplayMode.Height; } }

        public const int PreferredScreenHeight = 720;
        public const int PreferredScreenWidth = 1280;

        public static float RawHeight { get { return GraphicsDevice.Viewport.Height; } }
        public static float RawWidth { get { return GraphicsDevice.Viewport.Width; } }

        public static Vector2 TextureScaleFactor { get { return new Vector2(RawWidth / PreferredScreenWidth, RawHeight / PreferredScreenHeight); } }

        /// <summary>
        /// Gets the screen's aspect ratio. Same as graphics.GraphicsDevice.Viewport.AspectRatio.
        /// </summary>
        public static float AspectRatio { get { return GraphicsDevice.Viewport.AspectRatio; } }
        
        public static bool HiDef { get; private set; }

        public static void Initialize(GraphicsDeviceManager gdm, Space s, Effect shader)
        {
            Space = s;
            GDM = gdm;
            OnGDMCreation(shader);
            GDM.DeviceReset += onGDMReset;
            HiDef = GDM.GraphicsProfile == GraphicsProfile.HiDef;
            Camera = new ArcBallCamera(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1f, 10000f);
        }

        #region add
        public static void Add(BaseModel m)
        {
            try
            {
                if(m == null || m.Model == null)
                {
                    Log.WriteToLog(d, "Warning: Add(m) was called with a null parameter. Ensure this is correct behavior.");
                    return;
                }
                if(m.IsInvisible)
                    return;

                //if(m.UsesLaserSound)
                //{
                //    m.AddToRenderer();
                //    return;
                //}

                if(!m.RenderAsGlass)
                {
                    foreach(ModelData d in texturedModels)
                        if(d == m)
                        {
                            d.MakeActive();
                            return;
                        }
                    //if(untexturedModels.ContainsKey(m))
                    //{
                    //    untexturedModels[m] = true;
                    //    return;
                    //}
                }
                else
                    foreach(ModelData d in glassModels)
                        if(d == m)
                        {
                            d.MakeActive();
                            return;
                        }

                List<Texture2D> list;
                BasicEffect currentEffect;

                list = new List<Texture2D>();
                foreach(ModelMesh mesh in m.Model.Meshes)
                {
                    bool alreadyConverted = false;
                    for(int i = 0; i < mesh.Effects.Count; i++)
                    {
                        currentEffect = mesh.Effects[i] as BasicEffect;
                        EffectParameter e;
                        if(currentEffect != null)
                            list.Add(currentEffect.Texture);
                        else if((e = mesh.Effects[i].Parameters.FirstOrDefault(v => { return v.Name == "Texture"; })) != null)
                        {
                            list.Add(e.GetValueTexture2D());
                            alreadyConverted = true;
                        }
                        else
                        {
                            if(masterDict.ContainsKey(m.Model))
                                list.AddRange(masterDict[m.Model]);
                            alreadyConverted = true;
                            break;
                        }
                    }

                    if(!masterDict.ContainsKey(m.Model))
                        masterDict.Add(m.Model, list);
                    if(!alreadyConverted)
                    {
                        foreach(ModelMeshPart meshPart in mesh.MeshParts)
                            meshPart.Effect = lights.Shader.Clone();
                    }
                }

                if((list.Count == 0) || (list.Count > 0 && list[0] == null))
                    throw new ArgumentException("Models that are textureless can't be added to the renderer. Offending model name: " + m.Model.Meshes[0].Name);
                else if(m.RenderAsGlass)
                    glassModels.Add(new ModelData(m, list));
                else
                    texturedModels.Add(new ModelData(m, list));
            }
            catch(OutOfMemoryException)
            {
                //foreach(BaseModel b in untexturedModels.Keys)
                //    if(!untexturedModels[b])
                //        untexturedModels.Remove(b);
                for(int i = 0; i < texturedModels.Count; i++)
                    if(!texturedModels[i].IsActive)
                        texturedModels.RemoveAt(i);
                for(int i = 0; i < primitives.Count; i++)   
                    if(!primitives[i].IsActive)
                        primitives.RemoveAt(i);
                for(int i = 0; i < glassModels.Count; i++)
                    if(!glassModels[i].IsActive)
                        glassModels.RemoveAt(i);
                for(int i = 0; i < shaderModels.Count; i++)
                    if(!shaderModels[i].IsActive)
                        shaderModels.RemoveAt(i);
                GC.Collect(); // force a collection of released objects
            }
        }

        public static void Add(BaseModel m, List<Texture2D> textures)
        {
            try
            {
                if(m == null || textures == null || m.Model == null)// || textures.Count == 0)
                {
                    Log.WriteToLog(d, "Warning: Add(m, textures) was called with a null parameter. Ensure this is correct behavior.");
                    return;
                }
                if(m.IsInvisible)
                    return;

                foreach(ModelData d in texturedModels)
                    if(d == m)
                    {
                        d.MakeActive();
                        return;
                    }
                texturedModels.Add(new ModelData(m, textures));
            }
            catch(OutOfMemoryException)
            {
                //foreach(BaseModel b in untexturedModels.Keys)
                //    if(!untexturedModels[b])
                //        untexturedModels.Remove(b);
                for(int i = 0; i < texturedModels.Count; i++)
                    if(!texturedModels[i].IsActive)
                        texturedModels.RemoveAt(i);
                for(int i = 0; i < primitives.Count; i++)
                    if(!primitives[i].IsActive)
                        primitives.RemoveAt(i);
                for(int i = 0; i < glassModels.Count; i++)
                    if(!glassModels[i].IsActive)
                        glassModels.RemoveAt(i);
                for(int i = 0; i < shaderModels.Count; i++)
                    if(!shaderModels[i].IsActive)
                        shaderModels.RemoveAt(i);
                GC.Collect();
            }
        }

        public static void Add(Model m, Effect shader, Dictionary<string, object> objects, params Texture2D[] samplers)
        {
            try
            {
                if(m == null || shader == null || objects == null)
                {
                    Log.WriteToLog(d, "Warning: Add(m, shader, objects, samplers) was called with a null parameter. Ensure this is correct behavior.");
                    return;
                }

                foreach(ShaderModel s in shaderModels)
                    if(s == m)
                    {
                        s.MakeActive();
                        return;
                    }
                shaderModels.Add(new ShaderModel(m, shader, objects, samplers));
            }
            catch(OutOfMemoryException)
            {
                //foreach(BaseModel b in untexturedModels.Keys)
                //    if(!untexturedModels[b])
                //        untexturedModels.Remove(b);
                for(int i = 0; i < texturedModels.Count; i++)
                    if(!texturedModels[i].IsActive)
                        texturedModels.RemoveAt(i);
                for(int i = 0; i < primitives.Count; i++)
                    if(!primitives[i].IsActive)
                        primitives.RemoveAt(i);
                for(int i = 0; i < glassModels.Count; i++)
                    if(!glassModels[i].IsActive)
                        glassModels.RemoveAt(i);
                for(int i = 0; i < shaderModels.Count; i++)
                    if(!shaderModels[i].IsActive)
                        shaderModels.RemoveAt(i);
                GC.Collect();
            }
        }

        public static void Add(VertexBuffer vertBuffer, Texture2D texture)
        {
            try
            {
                if(vertBuffer == null || texture == null)
                {
                    Log.WriteToLog(d, "Warning: Add(vertBuffer, texture) was called with a null parameter. Ensure this is correct behavior.");
                    return;
                }

                foreach(PrimData d in primitives)
                    if(d == vertBuffer)
                    {
                        d.MakeActive();
                        return;
                    }

                primitives.Add(new PrimData(vertBuffer, texture));
            }
            catch(OutOfMemoryException)
            {
                //foreach(BaseModel b in untexturedModels.Keys)
                //    if(!untexturedModels[b])
                //        untexturedModels.Remove(b);
                for(int i = 0; i < texturedModels.Count; i++)
                    if(!texturedModels[i].IsActive)
                        texturedModels.RemoveAt(i);
                for(int i = 0; i < primitives.Count; i++)
                    if(!primitives[i].IsActive)
                        primitives.RemoveAt(i);
                for(int i = 0; i < glassModels.Count; i++)
                    if(!glassModels[i].IsActive)
                        glassModels.RemoveAt(i);
                for(int i = 0; i < shaderModels.Count; i++)
                    if(!shaderModels[i].IsActive)
                        shaderModels.RemoveAt(i);
                GC.Collect();
            }
        }

        public static void Add(IRenderableObject obj)
        {
            obj.AddToRenderer();
        }
        #endregion

        #region remove
        public static void Remove(Model m)
        {
            if(m == null)
                return;

            foreach(ShaderModel s in shaderModels)
                if(s == m)
                {
                    s.MakeInactive();
                    return;
                }
        }

        public static void Remove(BaseModel m)
        {
            if(m == null || m.Model == null)
                return;

            //if(m.UsesLaserSound)
            //{
            //    m.RemoveFromRenderer();
            //    return;
            //}
            
            //if(untexturedModels.ContainsKey(m))
            //{
            //    untexturedModels[m] = false;
            //    return;
            //}
            foreach(ModelData d in texturedModels)
                if(d == m)
                {
                    d.MakeInactive();
                    return;
                }
            foreach(ModelData d in glassModels)
                if(d == m)
                {
                    d.MakeInactive();
                    return;
                }
        }

        public static void Remove(VertexBuffer vertBuffer)
        {
            if(vertBuffer == null)
                return;

            foreach(PrimData d in primitives)
                if(d == vertBuffer)
                {
                    d.MakeInactive();
                    return;
                }
        }

        public static void RemovePermanent(BaseModel m)
        {
            if(m == null || m.Model == null)
                return;

            //if(untexturedModels.ContainsKey(m))
            //{
            //    untexturedModels.Remove(m);
            //    return;
            //}
            foreach(ModelData d in texturedModels)
                if(d == m)
                {
                    texturedModels.Remove(d);
                    return;
                }
            foreach(ModelData d in glassModels)
                if(d == m)
                {
                    glassModels.Remove(d);
                    return;
                }
        }

        public static void RemovePermanent(Model m)
        {
            if(m == null)
                return;

            foreach(ShaderModel s in shaderModels)
                if(s == m)
                {
                    shaderModels.Remove(s);
                    return;
                }
        }

        public static void Remove(IRenderableObject obj)
        {
            obj.RemoveFromRenderer();
        }
        #endregion

        public static bool Contains(BaseModel m)
        {
            if(m == null || m.Model == null)
                return false;

            foreach(ModelData model in texturedModels)
                if(model.Model == m)
                    return true;
            //foreach(KeyValuePair<BaseModel, bool> k in untexturedModels)
            //    if(k.Key == m)
            //        return true;
            return false;
        }

        public static void SetShaderValues(Model m, string[] keys, object[] values)
        {
            if(m == null || keys == null || values == null)
            {
                Log.WriteToLog(d, "Warning: SetShaderValues() was called with a null parameter. Ensure this is correct behavior.");
                return;
            }

            foreach(ShaderModel s in shaderModels)
                if(s == m)
                    s.SetValues(keys, values);
        }

        public static void Update(GameTime gameTime)
        {
            Camera.Update(gameTime);
        }

        public static void SetUpLighting(LightingData l)
        {
            lights.SetUpLighting(l.LightPower, l.AmbientPower, l.Position, l.Target, l.LightColor);
        }

        public static void Draw()
        {
            GraphicsDevice.Clear(Color.Black);
            draw(null, Camera.View);
        }

        private static Matrix boxWorld = Matrix.Identity;
        public static void DrawSpinningBox()
        {
            Vector2 screenCenter = new Vector2(Width * 0.5f, Height * 0.5f);

            Matrix view = Matrix.CreateLookAt(Vector3.UnitX * 20, Vector3.Zero, Vector3.UnitZ);
            Matrix proj = Matrix.CreateOrthographic(40, 40, 15f, 40);

            boxWorld = Matrix.Transform(boxWorld, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.1f));

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;

            Model boxModel = Resources.boxModel;
            Matrix[] transforms = new Matrix[boxModel.Bones.Count];
            boxModel.CopyAbsoluteBoneTransformsTo(transforms);
            int i = 0;
            foreach(ModelMesh mesh in boxModel.Meshes)
            {
                foreach(Effect currentEffect in mesh.Effects)
                {
                    currentEffect.CurrentTechnique = currentEffect.Techniques["OrthographicProjection"];

                    currentEffect.Parameters["Texture"].SetValue(masterDict[boxModel][i++]);
                    currentEffect.Parameters["xCamerasViewProjection"].SetValue(view * proj);
                    currentEffect.Parameters["xWorld"].SetValue(transforms[mesh.ParentBone.Index] * boxWorld * Matrix.CreateTranslation(new Vector3(0, 15, -15)));
                }
                mesh.Draw();
            }
        }

        private static void onGDMReset(object sender, EventArgs e)
        {
            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            renderTargetReflection = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            renderTargetRefraction = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            boxWorld = Matrix.Transform(Matrix.Identity, Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(30)));
        }

        /// <summary>
        /// Be careful when you use this.
        /// </summary>
        public static void OnGDMCreation(Effect shader)
        {
#if DEBUG
            vertices = new VertexPositionColor[6];
            vertices[0] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Red);
            vertices[1] = new VertexPositionColor(new Vector3(10000, 0, 0), Color.Red);
            vertices[2] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Green);
            vertices[3] = new VertexPositionColor(new Vector3(0, 10000, 0), Color.Green);
            vertices[4] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Blue);
            vertices[5] = new VertexPositionColor(new Vector3(0, 0, 10000), Color.Blue);
            xyz = new BasicEffect(GraphicsDevice);
            vertexBuff = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, vertices.Length, BufferUsage.None);
#endif
            lights = new LightingSystem(shader);
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            //untexturedModels.Clear();
            texturedModels.Clear();
            shaderModels.Clear();
            glassModels.Clear();
            masterDict.Clear();
            if(Resources.boxModel != null)
            {
                Box b = new Box(Vector3.Zero); // necessary because otherwise it looks for the current level's
                Add(b); RemovePermanent(b); // this gets the box model in the masterDict for the spinning saving box
            }
            primitives.Clear();
            onGDMReset(new object(), EventArgs.Empty);
            if(GameManager.CurrentLevel != null)
                GameManager.CurrentLevel.AddModels();
        }

        private static void draw(Plane? clipPlane, Matrix view)
        {
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;

            foreach(ModelData m in texturedModels)
                if(m.IsActive && !m.Model.UseCustomAlpha)
                    drawSingleModel(m.Model, m.Textures, "ShadowedScene", clipPlane, view, null);
            //foreach(KeyValuePair<BaseModel, bool> key in untexturedModels)
            //    if(key.Value && !key.Key.UseCustomAlpha)
            //        drawSingleModel(key.Key, null, "ShadowedSceneColor", clipPlane, view, null);
            foreach(PrimData p in primitives)
            {
                if(p.IsActive)
                {
                    lights.Shader.CurrentTechnique = lights.Shader.Techniques["ShadowedScene"];
                    lights.Shader.Parameters["xCamerasViewProjection"].SetValue(view * Camera.Projection);
                    lights.Shader.Parameters["xLightsViewProjection"].SetValue(lights.ViewProjection);
                    lights.Shader.Parameters["Texture"].SetValue(p.Texture);
                    lights.Shader.Parameters["xWorld"].SetValue(Matrix.Identity);
                    lights.Shader.Parameters["xLightPos"].SetValue(lights.LightPosition);
                    lights.Shader.Parameters["xLightPower"].SetValue(lights.LightPower);
                    lights.Shader.Parameters["xAmbient"].SetValue(lights.AmbientPower);
                    lights.Shader.Parameters["xLightDir"].SetValue(lights.LightDirection);
                    //lights.Shader.Parameters["xCarLightTexture"].SetValue(lights.LightMap);
                    lights.Shader.Parameters["xColor"].SetValue(lights.LightColor);
                    if(clipPlane.HasValue)
                    {
                        lights.Shader.Parameters["xEnableClipping"].SetValue(true);
                        lights.Shader.Parameters["xClipPlane"].SetValue(new Vector4(clipPlane.Value.Normal, clipPlane.Value.D));
                    }
                    else
                        lights.Shader.Parameters["xEnableClipping"].SetValue(false);
                    lights.Shader.Parameters["xEnableCustomAlpha"].SetValue(false);
                    //lights.Shader.Parameters["xModelPos"].SetValue(Vector3.Zero); // generally we don't want prims to be transparent
                    //lights.Shader.Parameters["xFarPlane"].SetValue(CameraFarPlane);
                    //lights.Shader.Parameters["xCamPos"].SetValue(Camera.Position);

                    foreach(EffectPass pass in lights.Shader.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        GraphicsDevice.SetVertexBuffer(p.Buffer);
                        GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                    }
                }
            }
#if DEBUG
            drawAxes();
            if(Camera.debugCamera)
                foreach(Entity e in Space.Entities)
                    e.CollisionInformation.BoundingBox.Draw();
#endif
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            try
            {
                if(!clipPlane.HasValue)
                    foreach(ShaderModel s in shaderModels)
                        if(s.IsActive)
                        {
                            for(int i = 0; i < s.Samplers.Length; i++)
                                GraphicsDevice.Textures[i] = s.Samplers[i];
                            
                            foreach(ModelMesh mesh in s.Model.Meshes)
                                foreach(Effect e in mesh.Effects)
                                    mesh.Draw();
                        }
            }
            catch(NotSupportedException e) // we've probably tried to use reflections in Reach, so perform emergency anti-fancy switch
            {
                if(GameManager.Manager.CurrentSave.Options.FancyGraphics)
                {
                    GameManager.Manager.CurrentSave.Options.FancyGraphics = false;
                    //GDM.GraphicsProfile = GraphicsProfile.Reach;
                    //GDM.ApplyChanges();
                }
                else
                    throw e; // no idea what's going on in this case.
            }

            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            Dictionary<ModelData, List<ModelMesh>> meshes = new Dictionary<ModelData, List<ModelMesh>>();
            foreach(ModelData m in glassModels)
                if(m.IsActive)
                    meshes.Add(m, new List<ModelMesh>(prepareMeshes(m.Model)));
            foreach(ModelData m in texturedModels)
                if(m.IsActive && m.Model.UseCustomAlpha)
                    meshes.Add(m, new List<ModelMesh>(prepareMeshes(m.Model)));
            //foreach(KeyValuePair<BaseModel, bool> key in untexturedModels)
            //    if(key.Value && key.Key.UseCustomAlpha)
            //        drawSingleModel(key.Key, null, "ShadowedSceneColor", clipPlane, view, key.Key.CustomAlpha);
            foreach(KeyValuePair<ModelData, List<ModelMesh>> m in meshes)
            {
                m.Value.Sort(new Comparison<ModelMesh>(sortGlassList));
                m.Value.Reverse();
                drawMeshes(m.Value.ToArray(), m.Key.Model, m.Key.Textures, "ShadowedScene", clipPlane, view, m.Key.Model.UseCustomAlpha ? m.Key.Model.CustomAlpha : (float?)null);
            }
        }

        private static int sortGlassList(ModelMesh x, ModelMesh y)
        {
            Vector3 pos1, pos2;
            pos1 = x.ParentBone.Transform.Translation;
            pos2 = y.ParentBone.Transform.Translation;
            float pos1Distance = Vector3.Distance(pos1, RenderingDevice.Camera.Position);
            float pos2Distance = Vector3.Distance(pos2, RenderingDevice.Camera.Position);
            return pos1Distance.CompareTo(pos2Distance);
        }

        private static void drawSingleModel(BaseModel model, List<Texture2D> textures, string tech, Plane? clipPlane, Matrix view, float? customAlpha)
        {
            ModelMesh[] meshes = prepareMeshes(model);
            drawMeshes(meshes, model, textures, tech, clipPlane, view, customAlpha);
        }

        private static ModelMesh[] prepareMeshes(BaseModel model)
        {
            if(model.IsInvisible)
                return new ModelMesh[] { };

            List<ModelMesh> meshes = new List<ModelMesh>();
            foreach(ModelMesh mesh in model.Model.Meshes)
                    meshes.Add(mesh);

            return meshes.ToArray();
        }

        private static void drawMeshes(ModelMesh[] meshes, BaseModel model, List<Texture2D> textures, string tech, Plane? clipPlane, Matrix view, float? customAlpha)
        {
            int i = 0;
            Matrix entityWorld = Matrix.Identity;
            if(!model.IsTerrain)
                entityWorld = ConversionHelper.MathConverter.Convert(model.Ent.CollisionInformation.WorldTransform.Matrix);
            foreach(ModelMesh mesh in meshes)
            {
                foreach(Effect currentEffect in mesh.Effects)
                {
                    currentEffect.CurrentTechnique = currentEffect.Techniques[tech];

                    if(textures != null)
                        currentEffect.Parameters["Texture"].SetValue(textures[i++]);

                    currentEffect.Parameters["xCamerasViewProjection"].SetValue(view * Camera.Projection);
                    currentEffect.Parameters["xLightsViewProjection"].SetValue(lights.ViewProjection);
                    currentEffect.Parameters["xWorld"].SetValue(mesh.ParentBone.Transform * model.Transform * entityWorld);// * Camera.World);
                    currentEffect.Parameters["xLightPos"].SetValue(lights.LightPosition);
                    currentEffect.Parameters["xLightPower"].SetValue(lights.LightPower);
                    currentEffect.Parameters["xAmbient"].SetValue(lights.AmbientPower);
                    currentEffect.Parameters["xLightDir"].SetValue(lights.LightDirection);

                    if(clipPlane.HasValue)
                    {
                        currentEffect.Parameters["xEnableClipping"].SetValue(true);
                        currentEffect.Parameters["xClipPlane"].SetValue(new Vector4(clipPlane.Value.Normal, clipPlane.Value.D));
                    }
                    else
                        currentEffect.Parameters["xEnableClipping"].SetValue(false);

                    if(customAlpha.HasValue)
                    {
                        currentEffect.Parameters["xEnableCustomAlpha"].SetValue(true);
                        currentEffect.Parameters["xCustomAlpha"].SetValue(customAlpha.Value);
                    }
                    else
                        currentEffect.Parameters["xEnableCustomAlpha"].SetValue(false);
                }
                mesh.Draw();
            }
        }


        #region Pre-draw operations
        public static Texture2D CreateRefractionMap(Plane refractionPlane)
        {
            GraphicsDevice.SetRenderTarget(renderTargetRefraction);
            GraphicsDevice.Clear(Color.Black);
            draw(refractionPlane, Camera.View);

            GraphicsDevice.SetRenderTarget(null);
            Texture2D output = (Texture2D)renderTargetRefraction;
            
            //System.IO.Stream s = null;
            //try
            //{
            //    s = System.IO.File.Open(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Accelerated Delivery/refraction.png", System.IO.FileMode.Create);
            //    output.SaveAsPng(s, output.Width, output.Height);
            //    s.Close();
            //}
            //catch { if(s != null) s.Close(); }

            return output;
        }

        public static Texture2D CreateReflectionMap(Matrix reflectedView, Plane reflectionPlane)
        {
            GraphicsDevice.SetRenderTarget(renderTargetReflection);
            GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 0, 1);
            draw(reflectionPlane, reflectedView);
            
            GraphicsDevice.SetRenderTarget(null);
            Texture2D output = (Texture2D)renderTargetReflection;
            //try
            //{
            //    System.IO.Stream s = System.IO.File.Open(Program.SavePath + "reflection.png", System.IO.FileMode.CreateNew);
            //    renderTarget.SaveAsPng(s, output.Width, output.Height);
            //    s.Close();
            //}
            //catch { }
            //renderTarget = null;
            return output;
        }

        /// <summary>
        /// Creates a reflection at a certain Z height.
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Matrix CreateReflectionMatrix(float height)
        {
            Vector3 reflectedCameraPos = Camera.Position;
            reflectedCameraPos.Z = -Camera.Position.Z + height * 2;
            Vector3 reflectedCameraTarget = Camera.TargetPosition;
            reflectedCameraTarget.Z = -Camera.TargetPosition.Z + height * 2;
            Vector3 cameraRight = Camera.Rotation.Right;
            Vector3 inverseUp = Vector3.Cross(cameraRight, reflectedCameraTarget - reflectedCameraPos);

            return Matrix.CreateLookAt(reflectedCameraPos, reflectedCameraTarget, inverseUp);
        }

        /// <summary>
        /// Creates a clip plane.
        /// </summary>
        /// <param name="height">Height of clip plane.</param>
        /// <param name="planeNormal">Normal of plane.</param>
        /// <param name="clipSide">False = below, True = above</param>
        /// <returns></returns>
        public static Plane CreatePlane(float height, Vector3 planeNormal, bool clipSide)
        {
            planeNormal.Normalize();
            Vector4 planeCoeffs = new Vector4(planeNormal, height);
            if(clipSide)
                planeCoeffs *= -1;

            //Matrix inverseWorldViewProj = Matrix.Transpose(Matrix.Invert(Camera.WorldViewProj));
            //planeCoeffs = Vector4.Transform(planeCoeffs, inverseWorldViewProj);
            Plane finalPlane = new Plane(planeCoeffs);

            return finalPlane;
        }
        #endregion

        #region Debug - Axes
#if DEBUG
        /// <summary>
        /// This is for drawing axes. Handy.
        /// </summary>
        private static VertexPositionColor[] vertices;
        /// <summary>
        /// This is for drawing axes. Handy.
        /// </summary>
        private static VertexBuffer vertexBuff;
        /// <summary>
        /// This is for drawing axes. Handy.
        /// </summary>
        private static BasicEffect xyz;

        private static void drawAxes()
        {
            GraphicsDevice.SetVertexBuffer(vertexBuff);
            xyz.VertexColorEnabled = true;
            xyz.World = Matrix.Identity;
            xyz.View = Camera.View;
            xyz.Projection = Camera.Projection;
            xyz.TextureEnabled = false;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            foreach(EffectPass pass in xyz.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.LineList, vertices, 0, 3);
            }
        }
#endif
        #endregion

        public struct LightingData
        {
            public float LightPower { get; private set; }
            public float AmbientPower { get; private set; }
            public Vector3 Position { get; private set; }
            public Vector3 Target { get; private set; }
            public Color LightColor { get; private set; }

            private LightingData(float lightPower, float ambientPower, Vector3 pos, Vector3 targetPos, Color color)
                :this()
            {
                LightPower = lightPower;
                AmbientPower = ambientPower;
                Position = pos;
                Target = targetPos;
                LightColor = color;
            }

            public static LightingData Lava { get { return new LightingData(0.6f, 0.5f, new Vector3(-6f, 0, 20), Vector3.Zero, Color.White); } }
            public static LightingData Results { get { return new LightingData(0.85f, 0.85f, new Vector3(0, 0, 20), Vector3.Zero, Color.White); } }
            public static LightingData Beach { get { return new LightingData(0.65f, 0.45f, new Vector3(-10, 0, 35), Vector3.Zero, Color.White); } }
            public static LightingData Ice { get { return new LightingData(0.4f, 0.55f, new Vector3(-2, -6, 30), new Vector3(-2, -6, 0), Color.White); } }
            public static LightingData Space { get { return new LightingData(0.45f, 0.3f, new Vector3(0f, -10, 70), Vector3.Zero, Color.White); } }
            public static LightingData Generic { get { return new LightingData(0.4f, 0.4f, new Vector3(0, 0, 60), Vector3.Zero, Color.White); } }
            public static LightingData Sky { get { return new LightingData(0.4f, 0.6f, new Vector3(0, 0, -100), Vector3.Zero, Color.White); } }
        }

        public struct LightingSystem
        {
            public Effect Shader { get; private set; }
            public float LightPower { get; private set; }
            public float AmbientPower { get; private set; }
            public Vector3 LightPosition { get; private set; }
            private Matrix view;
            private Matrix projection;
            public Matrix ViewProjection { get { return view * projection; } }
            public Vector3 LightColor { get; private set; }
            public Vector3 LightDirection { get; private set; }

            public LightingSystem(Effect shader)
                : this()
            {
                Shader = shader;
                projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, GraphicsDevice.Viewport.AspectRatio, 1f, 10000f);
            }

            public void SetUpLighting(float lightPower, float ambientPower, Vector3 pos, Vector3 targetPos, Color color)
            {
                this.LightPower = lightPower;
                this.AmbientPower = ambientPower;
                this.LightPosition = pos;
                this.LightColor = color.ToVector3();
                this.LightDirection = pos - targetPos;

                view = Matrix.CreateLookAt(pos, targetPos, Vector3.UnitZ);
            }
        }

        private class ShaderModel
        {
            public Model Model { get; private set; }
            public Dictionary<string, object> Parameters { get; private set; }
            public bool IsActive { get; private set; }
            public Effect Shader { get; private set; }
            public Texture2D[] Samplers { get; private set; }

            public ShaderModel(Model m, Effect shader, Dictionary<string, object> objects, params Texture2D[] samplerTextures)
            {
                Model = m;
                Parameters = objects;
                Shader = shader;
                IsActive = true;
                Samplers = samplerTextures;

                foreach(ModelMesh mesh in Model.Meshes)
                    foreach(ModelMeshPart meshPart in mesh.MeshParts)
                        meshPart.Effect = Shader;
            }

            public void SetValue(string name, object value)
            {
                if(Parameters.ContainsKey(name))
                    Parameters[name] = value;
                commitChanges();
            }

            public void SetValues(string[] names, object[] values)
            {
                if(names.Length != values.Length)
                    throw new ArgumentException("SetValues requires arrays of identical lengths.");
                for(int i = 0; i < names.Length; i++)
                    if(Parameters.ContainsKey(names[i]))
                        Parameters[names[i]] = values[i];
                commitChanges();
            }

            private object GetValue(string name)
            {
                if(Parameters.ContainsKey(name))
                    return Parameters[name];
                else throw new ArgumentException("The parameter name given to GetValue does not exist!");
            }

            private void commitChanges()
            {
                for(int i = 0; i < Parameters.Count; i++)
                {
                    KeyValuePair<string, object> key = Parameters.ElementAt(i);
                    if(key.Value is Matrix)
                        Shader.Parameters[key.Key].SetValue((Matrix)key.Value);
                    else if(key.Value is bool)
                        Shader.Parameters[key.Key].SetValue((bool)key.Value);
                    else if(key.Value is Vector2)
                        Shader.Parameters[key.Key].SetValue((Vector2)key.Value);
                    else if(key.Value is Vector3)
                        Shader.Parameters[key.Key].SetValue((Vector3)key.Value);
                    else if(key.Value is float)
                        Shader.Parameters[key.Key].SetValue((float)key.Value);
                    else if(key.Value is Quaternion)
                        Shader.Parameters[key.Key].SetValue((Quaternion)key.Value);
                    else if(key.Value is Vector4)
                        Shader.Parameters[key.Key].SetValue((Vector4)key.Value);
                    else if(key.Value is int)
                        Shader.Parameters[key.Key].SetValue((int)key.Value);
                    else if(key.Value is string)
                        Shader.Parameters[key.Key].SetValue((string)key.Value);
                    else if(key.Value is Texture)
                        Shader.Parameters[key.Key].SetValue((Texture)key.Value);
                    else if(key.Value is Matrix[])
                        Shader.Parameters[key.Key].SetValue((Matrix[])key.Value);
                    else if(key.Value is bool[])
                        Shader.Parameters[key.Key].SetValue((bool[])key.Value);
                    else if(key.Value is Vector2[])
                        Shader.Parameters[key.Key].SetValue((Vector2[])key.Value);
                    else if(key.Value is Vector3[])
                        Shader.Parameters[key.Key].SetValue((Vector3[])key.Value);
                    else if(key.Value is float[])
                        Shader.Parameters[key.Key].SetValue((float[])key.Value);
                    else if(key.Value is Quaternion[])
                        Shader.Parameters[key.Key].SetValue((Quaternion[])key.Value);
                    else if(key.Value is Vector4[])
                        Shader.Parameters[key.Key].SetValue((Vector4[])key.Value);
                    else if(key.Value is int[])
                        Shader.Parameters[key.Key].SetValue((int[])key.Value);
                    else if(key.Value is Color)
                    {
                        Color c = (Color)key.Value;
                        Shader.Parameters[key.Key].SetValue(c.ToVector4());
                    }
                    else
                        throw new ArgumentException("There was a value in Parameters that didn't make sense. Value was " + key.Value.GetType().Name);
                }
            }

            public void MakeActive()
            {
                IsActive = true;
            }
            public void MakeInactive()
            {
                IsActive = false;
            }

            public static bool operator==(ShaderModel lhs, Model rhs)
            {
                return lhs.Model == rhs;
            }
            public static bool operator !=(ShaderModel lhs, Model rhs)
            {
                return !(lhs == rhs);
            }
            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        private class ModelData
        {
            public List<Texture2D> Textures { get { return textures; } }
            private List<Texture2D> textures;
            public bool IsActive { get { return isActive; } }
            private bool isActive;
            public BaseModel Model { get; private set; }

            public ModelData(BaseModel m, List<Texture2D> textures)
            {
                if(textures.Count == 0)
                    throw new ArgumentException("Can't create a ModelData without any textures.");
                isActive = true;
                this.textures = new List<Texture2D>(textures);
                Model = m;
            }

            public void MakeActive()
            {
                isActive = true;
            }

            public void MakeInactive()
            {
                isActive = false;
            }

            public static bool operator==(ModelData lhs, BaseModel rhs)
            {
                return lhs.Model == rhs;
            }
            public static bool operator!=(ModelData lhs, BaseModel rhs)
            {
                return !(lhs.Model == rhs);
            }
            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        private class PrimData
        {
            public Texture2D Texture { get { return texture; } }
            private Texture2D texture;
            public bool IsActive { get { return isActive; } }
            private bool isActive;
            public VertexBuffer Buffer { get; private set; }

            public PrimData(VertexBuffer b, Texture2D texture)
            {
                isActive = true;
                this.texture = texture;
                Buffer = b;
            }

            public void MakeActive()
            {
                isActive = true;
            }

            public void MakeInactive()
            {
                isActive = false;
            }

            public static bool operator ==(PrimData lhs, VertexBuffer rhs)
            {
                return lhs.Buffer == rhs;
            }
            public static bool operator !=(PrimData lhs, VertexBuffer rhs)
            {
                return !(lhs.Buffer == rhs);
            }
            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}
