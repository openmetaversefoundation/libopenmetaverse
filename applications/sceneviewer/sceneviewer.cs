/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

#region Using Statements
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using libsecondlife;
using sceneviewer.Prims;
#endregion

namespace sceneviewer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class sceneviewer : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager Graphics;
        ContentManager Content;

        const short WATER_ROWS = 50;
        const short WATER_COLS = 50;
        const float WATER_WIDTH = 256.0f;
        const float WATER_HEIGHT = 256.0f;

        // 3d world
        private Camera Camera;
        private Matrix World;
        private Matrix ViewMatrix;
        private Matrix ViewProjectionMatrix;
        private Matrix ViewInverseMatrix;

        // Shaders
        private Effect EffectBasicPrim;
        private Effect EffectWater;

        // Input
        KeyboardState CurrentKeyboardState;
        MouseState CurrentMouseState;
        Keys[] KeysHeldDown;
        List<Keys> KeysPressedThisFrame;
        List<Keys> KeysReleasedThisFrame;

        // Second Life
        private SecondLife Client;

        // Prims
        private Dictionary<uint, PrimVisual> Prims;
        private VertexDeclaration PrimVertexDeclaration;

        // Water
        private VertexPosTexNormalTanBitan[] WaterVertexArray;
        //private VertexPositionTexture[] WaterVertexArray;
        private VertexBuffer WaterVertexBuffer;
        private IndexBuffer WaterIndexBuffer;
        private VertexDeclaration WaterVertexDeclaration;
        private Texture2D WaterNormalMap;
        private TextureCube WaterReflectionCubemap;

        // Timer related
        private Timer CameraUpdateTimer;
        private float deltaFPSTime = 0;

        // State tracking
        bool Wireframe = false;


        /// <summary>
        /// 
        /// </summary>
        public sceneviewer()
        {
            Graphics = new GraphicsDeviceManager(this);
            Graphics.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;
            Graphics.PreferMultiSampling = false;
            Graphics.SynchronizeWithVerticalRetrace = false;

            Content = new ContentManager(Services);

            KeysPressedThisFrame = new List<Keys>();
            KeysReleasedThisFrame = new List<Keys>();
            Prims = new Dictionary<uint, PrimVisual>();
            Client = new SecondLife();

            this.IsMouseVisible = true;
            Window.AllowUserResizing = true;

            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();
            KeysHeldDown = Keyboard.GetState().GetPressedKeys();

            Window.ClientSizeChanged += new EventHandler(Window_ClientSizeChanged);
            this.Exiting += new EventHandler(sceneviewer_Exiting);

            Camera = new Camera(this.Window, new Vector3(-10, -10, 40), new Vector3(255, 255, 40));
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Register libsl callbacks
            Client.Objects.OnNewPrim += new NewPrimCallback(OnNewPrim);
            Client.Objects.OnPrimMoved += new PrimMovedCallback(OnPrimMoved);
            Client.Objects.OnObjectKilled += new KillObjectCallback(OnObjectKilled);

            if (!Client.Network.Login("Ron", "Hubbard", "radishman", "sceneviewer", "jhurliman@wsu.edu"))
            {
                Exit();
            }

            // Wait for basic information to be retrieved from the current sim
            while (
                Client.Network.CurrentSim == null || 
                Client.Network.CurrentSim.Region == null || 
                Client.Network.CurrentSim.Region.Name == null)
            {
                System.Threading.Thread.Sleep(10);
            }

            // Initialize the engine
            InitializeTransform();
            InitializeScene();
            InitializeWater();

            // Start the timer
            CameraUpdateTimer = new Timer(new TimerCallback(SendCameraUpdate), null, 0, 500);

            base.Initialize();
        }

        
        /// <summary>
        /// 
        /// </summary>
        private void InitializeTransform()
        {
            World = Matrix.CreateTranslation(Vector3.Zero);
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeScene()
        {
            PrimVertexDeclaration = new VertexDeclaration(Graphics.GraphicsDevice, VertexPositionColor.VertexElements);
            //WaterVertexDeclaration = new VertexDeclaration(Graphics.GraphicsDevice, VertexPositionTexture.VertexElements);
            WaterVertexDeclaration = new VertexDeclaration(Graphics.GraphicsDevice, VertexPosTexNormalTanBitan.VertexElements);
        }


        /// <summary>
        /// 
        /// </summary>
        private void InitializeWater()
        {
            int i = 0;
            short p = 0;
            short[] indices = new short[WATER_ROWS * WATER_COLS * 6];
            WaterVertexArray = new VertexPosTexNormalTanBitan[(WATER_ROWS + 1) * (WATER_COLS + 1)];
            //WaterVertexArray = new VertexPositionTexture[(WATER_ROWS + 1) * (WATER_COLS + 1)];
            WaterIndexBuffer = new IndexBuffer(Graphics.GraphicsDevice, typeof(short), WATER_ROWS * WATER_COLS * 6,
                ResourceUsage.WriteOnly, ResourceManagementMode.Automatic);

            for (int y = 0; y <= WATER_COLS; y++)
            {
                for (int x = 0; x <= WATER_ROWS; x++)
                {
                    WaterVertexArray[p] = new VertexPosTexNormalTanBitan(
                        new Vector3(((float)x / WATER_ROWS) * WATER_WIDTH, ((float)y / WATER_COLS) * WATER_HEIGHT, 0),
                        new Vector2(((float)x / WATER_ROWS), (float)(WATER_COLS - y) / WATER_COLS),
                        Vector3.UnitZ,
                        Vector3.UnitX,
                        Vector3.UnitY
                        );

                    if (y != WATER_COLS && x != WATER_ROWS)
                    {
                        indices[i++] = p;
                        indices[i++] = (short)(p + 1);
                        indices[i++] = (short)(p + WATER_COLS + 1);

                        indices[i++] = (short)(p + WATER_COLS + 1);
                        indices[i++] = (short)(p + WATER_COLS + 2);
                        indices[i++] = (short)(p + 1);
                    }

                    p++;
                }
            }

            WaterVertexBuffer = new VertexBuffer(Graphics.GraphicsDevice, typeof(VertexPosTexNormalTanBitan),
                WATER_ROWS * WATER_COLS * 6, ResourceUsage.WriteOnly, ResourceManagementMode.Automatic);
            WaterVertexBuffer.SetData<VertexPosTexNormalTanBitan>(WaterVertexArray);

            WaterIndexBuffer.SetData<short>(indices);
        }


        /// <summary>
        /// Load your graphics content.  If loadAllContent is true, you should
        /// load content from both ResourceManagementMode pools.  Otherwise, just
        /// load ResourceManagementMode.Manual content.
        /// </summary>
        /// <param name="loadAllContent">Which type of content to load.</param>
        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            if (loadAllContent)
            {
                EffectBasicPrim = Content.Load<Effect>("Shaders/basicprim");
                EffectWater = Content.Load<Effect>("Shaders/ocean");

                WaterNormalMap = Content.Load<Texture2D>("Textures/wavenormalmap");
                WaterReflectionCubemap = Content.Load<TextureCube>("Textures/cubemap");

                EffectWater.Parameters["normalMap"].SetValue(WaterNormalMap);
                EffectWater.Parameters["cubeMap"].SetValue(WaterReflectionCubemap);
                EffectWater.Parameters["bumpHeight"].SetValue(1.4f);
            }

            // Load any ResourceManagementMode.Manual content here
        }


        /// <summary>
        /// Unload your graphics content.  If unloadAllContent is true, you should
        /// unload content from both ResourceManagementMode pools.  Otherwise, just
        /// unload ResourceManagementMode.Manual content.  Manual content will get
        /// Disposed by the GraphicsDevice during a Reset.
        /// </summary>
        /// <param name="unloadAllContent">Which type of content to unload.</param>
        protected override void UnloadGraphicsContent(bool unloadAllContent)
        {
            if (unloadAllContent == true)
            {
                Content.Unload();
            }
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedRealTime.TotalSeconds;
            
            // Allows the default game to exit on Xbox 360 and Windows
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // Update the FPS counter
            float fps = 1.0f / elapsed;
            deltaFPSTime += elapsed;
            if (deltaFPSTime > 0.1f)
            {
                Window.Title = fps.ToString() + " FPS";
                deltaFPSTime -= 0.1f;
            }

            // Update the keyboard and mouse state
            UpdateInput();

            // Check for keypresses and keys that are currently held down
            HandleInput();

            ViewMatrix = Camera.ViewMatrix;
            ViewInverseMatrix = Matrix.Invert(ViewMatrix);
            ViewProjectionMatrix = Camera.ViewProjectionMatrix;

            base.Update(gameTime);
        }


        /// <summary>
        /// 
        /// </summary>
        void UpdateInput()
        {
            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();

            // Clear our pressed and released lists.
            KeysPressedThisFrame.Clear();
            KeysReleasedThisFrame.Clear();

            // Interpret pressed key data between arrays to
            // figure out just-pressed and just-released keys.
            Keys[] currentKeys = CurrentKeyboardState.GetPressedKeys();

            // First loop, looking for keys just pressed.
            for (int currentKey = 0; currentKey < currentKeys.Length; currentKey++)
            {
                bool found = false;
                for (int previousKey = 0; previousKey < KeysHeldDown.Length; previousKey++)
                {
                    if (currentKeys[currentKey] == KeysHeldDown[previousKey])
                    {
                        // The key was pressed both this frame and last; ignore.
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    // The key was pressed this frame, but not last frame; it was just pressed.
                    KeysPressedThisFrame.Add(currentKeys[currentKey]);
                }
            }

            // Second loop, looking for keys just released.
            for (int previousKey = 0; previousKey < KeysHeldDown.Length; previousKey++)
            {
                bool found = false;
                for (int currentKey = 0; currentKey < currentKeys.Length; currentKey++)
                {
                    if (KeysHeldDown[previousKey] == currentKeys[currentKey])
                    {
                        // The key was pressed both this frame and last; ignore.
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    // The key was pressed last frame, but not this frame; it was just released.
                    KeysReleasedThisFrame.Add(KeysHeldDown[previousKey]);
                }
            }

            // Set the held state to the current state.
            KeysHeldDown = currentKeys;
        }


        /// <summary>
        /// 
        /// </summary>
        protected void HandleInput()
        {
            //
            // Mouse
            //

            if (CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                // Test for intersections with objects in the grid
                Ray pickRay = GetPickRay();

                // TODO: Can we optimize this, insted of looping through every object there is?
                lock (Prims)
                {
                    foreach (PrimVisual prim in Prims.Values)
                    {
                        Nullable<float> result = pickRay.Intersects(prim.BoundBox);

                        if (result.HasValue)
                        {
                            // TODO: This should be added to a temporary list of prims that will be 
                            // depth sorted and possibly checked for per-face intersection

                            prim.Select();
                        }
                    }
                }


                // TODO: If there were no object intersections, test for intersections
                // with the water and terrain
            }

            //
            // Keyboard
            //

            if (CurrentKeyboardState.IsKeyDown(Keys.W))
            {
                Camera.Translate(new Vector3(0, 0.8f, 0));
            }

            if (CurrentKeyboardState.IsKeyDown(Keys.A))
            {
                Camera.Rotate(0.05f);
            }

            if (CurrentKeyboardState.IsKeyDown(Keys.S))
            {
                Camera.Translate(new Vector3(0, -1, 0));
            }

            if (CurrentKeyboardState.IsKeyDown(Keys.D))
            {
                Camera.Rotate(-0.05f);
            }

            if (CurrentKeyboardState.IsKeyDown(Keys.PageUp))
            {
                Camera.Translate(new Vector3(0, 0, 1));
            }

            if (CurrentKeyboardState.IsKeyDown(Keys.PageDown))
            {
                Camera.Translate(new Vector3(0, 0, -1));
            }

            if (KeyPressedThisFrame(Keys.D1))
            {
                Wireframe = !Wireframe;
            }
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            Graphics.GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
            Graphics.GraphicsDevice.RenderState.FillMode = (Wireframe) ? FillMode.WireFrame : FillMode.Solid;
            //graphics.GraphicsDevice.RenderState.MultiSampleAntiAlias = true;

            RenderWater(gameTime);
            //RenderBasicPrims();

            base.Draw(gameTime);
        }


        /// <summary>
        /// 
        /// </summary>
        protected void RenderWater(GameTime gameTime)
        {
            Matrix worldOffset = Matrix.CreateTranslation(new Vector3(0, 0, Client.Network.CurrentSim.Region.WaterHeight));

            Graphics.GraphicsDevice.VertexDeclaration = WaterVertexDeclaration;
            Graphics.GraphicsDevice.Vertices[0].SetSource(WaterVertexBuffer, 0, VertexPosTexNormalTanBitan.SizeInBytes);
            Graphics.GraphicsDevice.Indices = WaterIndexBuffer;

            EffectWater.Begin();

            EffectWater.Parameters["worldMatrix"].SetValue(worldOffset);
            EffectWater.Parameters["wvpMatrix"].SetValue(worldOffset * ViewProjectionMatrix);
            EffectWater.Parameters["worldViewMatrix"].SetValue(worldOffset * ViewMatrix);
            EffectWater.Parameters["viewInverseMatrix"].SetValue(ViewInverseMatrix);
            EffectWater.Parameters["time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            EffectWater.CommitChanges();

            foreach (EffectPass pass in EffectWater.CurrentTechnique.Passes)
            {
                pass.Begin();

                Graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                    (WATER_ROWS + 1) * (WATER_COLS + 1),
                    0, WATER_COLS * WATER_ROWS * 2);

                pass.End();
            }

            EffectWater.End();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ViewProjectionMatrix"></param>
        protected void RenderBasicPrims()
        {
            Graphics.GraphicsDevice.VertexDeclaration = PrimVertexDeclaration;

            EffectBasicPrim.Begin();

            lock (Prims)
            {
                foreach (EffectPass pass in EffectBasicPrim.CurrentTechnique.Passes)
                {
                    pass.Begin();

                    foreach (PrimVisual prim in Prims.Values)
                    {
                        if (prim.Prim.ParentID != 0)
                        {
                            if (!Prims.ContainsKey(prim.Prim.ParentID))
                            {
                                // We don't have the base position for this child prim, can't render it
                                continue;
                            }
                            else
                            {
                                // Child prim in a linkset

                                // Add the base position of the parent prim and the offset position of this child
                                LLVector3 llBasePosition = Prims[prim.Prim.ParentID].Prim.Position;
                                LLQuaternion llBaseRotation = Prims[prim.Prim.ParentID].Prim.Rotation;

                                Vector3 basePosition = new Vector3(llBasePosition.X, llBasePosition.Y, llBasePosition.Z);

                                Matrix worldOffset = Matrix.CreateTranslation(basePosition);
                                Matrix rootRotation = Matrix.CreateFromQuaternion(new Quaternion(llBaseRotation.X, 
                                    llBaseRotation.Y, llBaseRotation.Z, llBaseRotation.W));

                                EffectBasicPrim.Parameters["WorldViewProj"].SetValue(prim.Matrix * rootRotation * worldOffset * ViewProjectionMatrix);
                                EffectBasicPrim.CommitChanges();

                                Graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList,
                                    prim.VertexArray, 0, prim.VertexArray.Length / 3);
                            }
                        }
                        else
                        {
                            // Root prim or not part of a linkset

                            EffectBasicPrim.Parameters["WorldViewProj"].SetValue(prim.Matrix * ViewProjectionMatrix);
                            EffectBasicPrim.CommitChanges();

                            Graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList,
                                prim.VertexArray, 0, prim.VertexArray.Length / 3);
                        }
                    }

                    pass.End();
                }
            }

            EffectBasicPrim.End();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Ray GetPickRay()
        {
            int mouseX = CurrentMouseState.X;
            int mouseY = CurrentMouseState.Y;
            float nearClip = Camera.NearClip;
            float farClip = Camera.FarClip;

            System.Console.WriteLine("mouseState.X: " + mouseX + ", mouseState.Y: " + mouseY);

            // Determine the mouse position in screen space.
            double screenSpaceX = ((float)mouseX / ((float)Window.ClientBounds.Width / 2.0f) - 1.0f) * 
                ((float)Window.ClientBounds.Width / (float)Window.ClientBounds.Height);
            double screenSpaceY = (1.0f - (float)mouseY / ((float)Window.ClientBounds.Height / 2.0f));

            System.Console.WriteLine("ScreenSpaceX: " + screenSpaceX + ", ScreenSpaceY: " + screenSpaceY);

            // Calculating the tangent in this method is for clarity. Normally, the
            // tangent would be calculated only once at start up and recalculated
            // if the camera field of view changes.
            double viewRatio = Math.Tan(Camera.FOV / 2.0f);
            screenSpaceX = screenSpaceX * viewRatio;
            screenSpaceY = screenSpaceY * viewRatio;

            // Determine the mouse position in camera space on the near clip plane.
            Vector3 cameraSpaceNear = new Vector3((float)(screenSpaceX * nearClip),
                (float)(screenSpaceY * nearClip), (float)(-nearClip));

            // Deetermine the mouse position in camera space on the far clip plane.
            Vector3 cameraSpaceFar = new Vector3((float)(screenSpaceX * farClip),
                (float)(screenSpaceY * farClip), (float)(-farClip));

            Vector3 worldSpaceNear = Vector3.Transform(cameraSpaceNear, ViewInverseMatrix);
            Vector3 worldSpaceFar = Vector3.Transform(cameraSpaceFar, ViewInverseMatrix);

            // Create a ray from the near clip plane to the far clip plane.
            Ray pickRay = new Ray(worldSpaceNear, worldSpaceFar - worldSpaceNear);

            return pickRay;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool KeyPressedThisFrame(Keys key)
        {
            if (KeysPressedThisFrame.Contains(key))
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool KeyReleasedThisFrame(Keys key)
        {
            if (KeysReleasedThisFrame.Contains(key))
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Keys[] KeysHeldDownThisFrame()
        {
            return KeysHeldDown;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="prim"></param>
        /// <param name="regionHandle"></param>
        /// <param name="timeDilation"></param>
        void OnNewPrim(Simulator simulator, PrimObject prim, ulong regionHandle, ushort timeDilation)
        {
            PrimVisual primVisual = PrimVisual.BuildPrimVisual(prim);

            if (primVisual != null &&
                (primVisual.GetType() == typeof(PrimVisualBox) ||
                primVisual.GetType() == typeof(PrimVisualCylinder)))
            {
                lock (Prims)
                {
                    if (Prims.ContainsKey(prim.LocalID))
                    {
                        Prims.Remove(prim.LocalID);
                    }

                    Prims.Add(prim.LocalID, primVisual);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="primUpdate"></param>
        /// <param name="regionHandle"></param>
        /// <param name="timeDilation"></param>
        void OnPrimMoved(Simulator simulator, PrimUpdate primUpdate, ulong regionHandle, ushort timeDilation)
        {
            if (Prims.ContainsKey(primUpdate.LocalID))
            {
                Prims[primUpdate.LocalID].Update(primUpdate);
            }
            else
            {
                Client.Objects.RequestObject(simulator, primUpdate.LocalID);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        void OnObjectKilled(Simulator simulator, uint localID)
        {
            lock (Prims)
            {
                Prims.Remove(localID);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void sceneviewer_Exiting(object sender, EventArgs e)
        {
            if (Client.Network.Connected)
            {
                Client.Network.Logout();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            Camera.UpdateProjection(this.Window);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        void SendCameraUpdate(object obj)
        {
            Client.Self.UpdateCamera(false);
        }
    }
}
