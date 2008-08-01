using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Tao.OpenGl;
using Tao.Platform.Windows;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Imaging;
using OpenMetaverse.Rendering;

// NOTE: Batches are divided by texture, fullbright, shiny, transparent, and glow

namespace PrimWorkshop
{
    public struct FaceData
    {
        public float[] Vertices;
        public ushort[] Indices;
        public float[] TexCoords;
        public int TexturePointer;
        public System.Drawing.Image Texture;
        // TODO: Normals / binormals?
    }

    public static class Render
    {
        public static IRendering Plugin;
    }

    public partial class frmPrimWorkshop : Form
    {
        #region Form Globals

        List<FacetedMesh> Prims = null;
        FacetedMesh CurrentPrim = null;
        ProfileFace? CurrentFace = null;

        bool DraggingTexture = false;
        bool Wireframe = true;
        int[] TexturePointers = new int[1];

        #endregion Form Globals

        public frmPrimWorkshop()
        {
            InitializeComponent();
            glControl.InitializeContexts();

            Gl.glShadeModel(Gl.GL_SMOOTH);
            Gl.glClearColor(0f, 0f, 0f, 0f);

            Gl.glClearDepth(1.0f);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glDepthMask(Gl.GL_TRUE);
            Gl.glDepthFunc(Gl.GL_LEQUAL);
            Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_NICEST);

            TexturePointers[0] = 0;

            // Call the resizing function which sets up the GL drawing window
            // and will also invalidate the GL control
            glControl_Resize(null, null);
        }

        private void frmPrimWorkshop_Shown(object sender, EventArgs e)
        {
            // Get a list of rendering plugins
            List<string> renderers = RenderingLoader.ListRenderers(".");

            foreach (string r in renderers)
            {
                DialogResult result = MessageBox.Show(
                    String.Format("Use renderer {0}?", r), "Select Rendering Plugin", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    Render.Plugin = RenderingLoader.LoadRenderer(r);
                    break;
                }
            }

            if (Render.Plugin == null)
            {
                MessageBox.Show("No valid rendering plugin loaded, exiting...");
                Application.Exit();
            }
        }

        #region GLControl Callbacks

        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glLoadIdentity();

            // Setup wireframe or solid fill drawing mode
            if (Wireframe)
                Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_LINE);
            else
                Gl.glPolygonMode(Gl.GL_FRONT, Gl.GL_FILL);

            Vector3 center = Vector3.Zero;

            Glu.gluLookAt(
                    center.X, (double)scrollZoom.Value * 0.1d + center.Y, center.Z,
                    center.X, center.Y, center.Z,
                    0d, 0d, 1d);

            // Push the world matrix
            Gl.glPushMatrix();

            Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);

            // World rotations
            Gl.glRotatef((float)scrollRoll.Value, 1f, 0f, 0f);
            Gl.glRotatef((float)scrollPitch.Value, 0f, 1f, 0f);
            Gl.glRotatef((float)scrollYaw.Value, 0f, 0f, 1f);

            if (Prims != null)
            {
                for (int i = 0; i < Prims.Count; i++)
                {
                    Primitive prim = Prims[i].Prim;

                    if (i == cboPrim.SelectedIndex)
                        Gl.glColor3f(1f, 0f, 0f);
                    else
                        Gl.glColor3f(1f, 1f, 1f);

                    // Individual prim matrix
                    Gl.glPushMatrix();

                    // The root prim position is sim-relative, while child prim positions are
                    // parent-relative. We want to apply parent-relative translations but not
                    // sim-relative ones
                    if (Prims[i].Prim.ParentID != 0)
                    {
                        Gl.glTranslatef(prim.Position.X, prim.Position.Y, prim.Position.Z);

                        // Prim rotation
                        // Using euler angles because I have no clue what I'm doing
                        float roll, pitch, yaw;

                        Matrix4 rotation = Matrix4.CreateFromQuaternion(prim.Rotation);
                        rotation.GetEulerAngles(out roll, out pitch, out yaw);

                        Gl.glRotatef(roll * 57.2957795f, 1f, 0f, 0f);
                        Gl.glRotatef(pitch * 57.2957795f, 0f, 1f, 0f);
                        Gl.glRotatef(yaw * 57.2957795f, 0f, 0f, 1f);
                    }

                    // Prim scaling
                    Gl.glScalef(prim.Scale.X, prim.Scale.Y, prim.Scale.Z);

                    // Draw the prim faces
                    for (int j = 0; j < Prims[i].Faces.Count; j++)
                    {
                        if (i == cboPrim.SelectedIndex)
                        {
                            // This prim is currently selected in the dropdown
                            //Gl.glColor3f(0f, 1f, 0f);
                            Gl.glColor3f(1f, 1f, 1f);

                            if (j == cboFace.SelectedIndex)
                            {
                                // This face is currently selected in the dropdown
                            }
                            else
                            {
                                // This face is not currently selected in the dropdown
                            }
                        }
                        else
                        {
                            // This prim is not currently selected in the dropdown
                            Gl.glColor3f(1f, 1f, 1f);
                        }

                        #region Texturing

                        Face face = Prims[i].Faces[j];
                        FaceData data = (FaceData)face.UserData;

                        if (data.TexturePointer != 0)
                        {
                            // Set the color to solid white so the texture is not altered
                            //Gl.glColor3f(1f, 1f, 1f);
                            // Enable texturing for this face
                            Gl.glEnable(Gl.GL_TEXTURE_2D);
                        }
                        else
                        {
                            Gl.glDisable(Gl.GL_TEXTURE_2D);
                        }

                        // Bind the texture
                        Gl.glBindTexture(Gl.GL_TEXTURE_2D, data.TexturePointer);

                        #endregion Texturing

                        Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, data.TexCoords);
                        Gl.glVertexPointer(3, Gl.GL_FLOAT, 0, data.Vertices);
                        Gl.glDrawElements(Gl.GL_TRIANGLES, data.Indices.Length, Gl.GL_UNSIGNED_SHORT, data.Indices);
                    }

                    // Pop the prim matrix
                    Gl.glPopMatrix();
                }
            }
            /*else if (CurrentMesh != null)
            {
                Gl.glColor3f(1f, 1f, 1f);

                GLMesh glmesh = CurrentMesh.Value;
                LLMesh llmesh = glmesh.Mesh;

                Gl.glRotatef(llmesh.RotationAngles.X, 1f, 0f, 0f);
                Gl.glRotatef(llmesh.RotationAngles.Y, 0f, 1f, 0f);
                Gl.glRotatef(llmesh.RotationAngles.Z, 0f, 0f, 1f);

                Gl.glScalef(llmesh.Scale.X, llmesh.Scale.Y, llmesh.Scale.Z);

                // Push the mesh data
                Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, glmesh.TexCoords);
                Gl.glVertexPointer(3, Gl.GL_FLOAT, 0, glmesh.Vertices);
                Gl.glDrawElements(Gl.GL_TRIANGLES, glmesh.Indices.Length, Gl.GL_UNSIGNED_SHORT, glmesh.Indices);
            }*/

            // Pop the world matrix
            Gl.glPopMatrix();

            Gl.glDisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
            Gl.glDisableClientState(Gl.GL_VERTEX_ARRAY);

            Gl.glFlush();
        }

        private void glControl_Resize(object sender, EventArgs e)
        {
            Gl.glClearColor(0.39f, 0.58f, 0.93f, 1.0f);

            Gl.glViewport(0, 0, glControl.Width, glControl.Height);

            Gl.glPushMatrix();
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();

            Glu.gluPerspective(50.0d, 1.0d, 0.1d, 50d);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPopMatrix();
        }

        #endregion GLControl Callbacks

        #region Menu Callbacks

        private void openPrimXMLToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Prims = null;
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LLSD llsd = null;

                try { llsd = LLSDParser.DeserializeXml(File.ReadAllText(dialog.FileName)); }
                catch (Exception ex) { MessageBox.Show(ex.Message); }

                if (llsd != null && llsd.Type == LLSDType.Map)
                {
                    List<Primitive> primList = Helpers.LLSDToPrimList(llsd);
                    Prims = new List<FacetedMesh>(primList.Count);

                    for (int i = 0; i < primList.Count; i++)
                    {
                        // TODO: Can't render sculpted prims without the textures
                        if (primList[i].Sculpt.SculptTexture != UUID.Zero)
                            continue;

                        Primitive prim = primList[i];
                        FacetedMesh mesh = Render.Plugin.GenerateFacetedMesh(prim, DetailLevel.Highest);

                        // Create a FaceData struct for each face that stores the 3D data
                        // in a Tao.OpenGL friendly format
                        for (int j = 0; j < mesh.Faces.Count; j++)
                        {
                            Face face = mesh.Faces[j];
                            FaceData data = new FaceData();
                            
                            // Vertices for this face
                            data.Vertices = new float[face.Vertices.Count * 3];
                            for (int k = 0; k < face.Vertices.Count; k++)
                            {
                                data.Vertices[k * 3 + 0] = face.Vertices[k].Position.X;
                                data.Vertices[k * 3 + 1] = face.Vertices[k].Position.Y;
                                data.Vertices[k * 3 + 2] = face.Vertices[k].Position.Z;
                            }

                            // Indices for this face
                            data.Indices = face.Indices.ToArray();

                            // Texture transform for this face
                            LLObject.TextureEntryFace teFace = prim.Textures.GetFace((uint)j);
                            Render.Plugin.TransformTexCoords(face.Vertices, face.Center, teFace);

                            // Texcoords for this face
                            data.TexCoords = new float[face.Vertices.Count * 2];
                            for (int k = 0; k < face.Vertices.Count; k++)
                            {
                                data.TexCoords[k * 2 + 0] = face.Vertices[k].TexCoord.X;
                                data.TexCoords[k * 2 + 1] = face.Vertices[k].TexCoord.Y;
                            }

                            // Texture for this face
                            if (LoadTexture(System.IO.Path.GetDirectoryName(dialog.FileName), teFace.TextureID, ref data.Texture))
                            {
                                Bitmap bitmap = new Bitmap(data.Texture);
                                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                                Rectangle rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                                BitmapData bitmapData = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                                Gl.glGenTextures(1, out data.TexturePointer);
                                Gl.glBindTexture(Gl.GL_TEXTURE_2D, data.TexturePointer);

                                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
                                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
                                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
                                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
                                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_GENERATE_MIPMAP, Gl.GL_TRUE);

                                Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, Gl.GL_RGB8, bitmap.Width, bitmap.Height, Gl.GL_BGR, Gl.GL_UNSIGNED_BYTE, bitmapData.Scan0);

                                bitmap.UnlockBits(bitmapData);
                                bitmap.Dispose();
                            }

                            // Set the UserData for this face to our FaceData struct
                            face.UserData = data;
                            mesh.Faces[j] = face;
                        }

                        Prims.Add(mesh);
                    }

                    // Setup the dropdown list of prims
                    PopulatePrimCombobox();

                    glControl.Invalidate();
                }
                else
                {
                    MessageBox.Show("Failed to load LLSD formatted primitive data from " + dialog.FileName);
                }
            }
        }

        private bool LoadTexture(string basePath, UUID textureID, ref System.Drawing.Image texture)
        {
            if (File.Exists(textureID.ToString() + ".tga"))
            {
                try
                {
                    texture = (Image)LoadTGAClass.LoadTGA(
                        basePath + System.IO.Path.DirectorySeparatorChar + textureID.ToString() + ".tga");
                    return true;
                }
                catch (Exception)
                {
                }
            }

            return false;
        }

        private void textureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            picTexture.Image = null;
            TexturePointers[0] = 0;

            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    picTexture.Image = System.Drawing.Image.FromFile(dialog.FileName);
                    Bitmap bitmap = new Bitmap(picTexture.Image);
                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

                    // Create the GL texture space
                    Gl.glGenTextures(1, TexturePointers);
                    Rectangle rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                    BitmapData bitmapData = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                    Gl.glBindTexture(Gl.GL_TEXTURE_2D, TexturePointers[0]);

                    Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
                    Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
                    Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_EDGE);
                    Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_EDGE);
                    Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_GENERATE_MIPMAP, Gl.GL_TRUE);

                    Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, Gl.GL_RGB8, bitmap.Width, bitmap.Height, Gl.GL_BGR, Gl.GL_UNSIGNED_BYTE, bitmapData.Scan0);

                    bitmap.UnlockBits(bitmapData);
                    bitmap.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load image from file " + dialog.FileName + ": " + ex.Message);
                }
            }
        }

        private void savePrimXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void saveTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void oBJToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "OBJ files (*.obj)|*.obj";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (!MeshToOBJ.MeshesToOBJ(Prims, dialog.FileName))
                {
                    MessageBox.Show("Failed to save file " + dialog.FileName +
                        ". Ensure that you have permission to write to that file and it is currently not in use");
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Written by John Hurliman <jhurliman@jhurliman.org> (http://www.jhurliman.org/)");
        }

        #endregion Menu Callbacks

        #region Scrollbar Callbacks

        private void scroll_ValueChanged(object sender, EventArgs e)
        {
            glControl.Invalidate();
        }

        private void scrollZoom_ValueChanged(object sender, EventArgs e)
        {
            glControl_Resize(null, null);
            glControl.Invalidate();
        }

        #endregion Scrollbar Callbacks

        #region PictureBox Callbacks

        private void picTexture_MouseDown(object sender, MouseEventArgs e)
        {
            DraggingTexture = true;
        }

        private void picTexture_MouseUp(object sender, MouseEventArgs e)
        {
            DraggingTexture = false;
        }

        private void picTexture_MouseLeave(object sender, EventArgs e)
        {
            DraggingTexture = false;
        }

        private void picTexture_MouseMove(object sender, MouseEventArgs e)
        {
            if (DraggingTexture)
            {
                // What is the current action?
                // None, DraggingEdge, DraggingCorner, DraggingWhole
            }
            else
            {
                // Check if the mouse is close to the edge or corner of a selection
                // rectangle

                // If so, change the cursor accordingly
            }
        }

        private void picTexture_Paint(object sender, PaintEventArgs e)
        {
            // Draw the current selection rectangles
        }

        #endregion PictureBox Callbacks

        private void cboPrim_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentPrim = (FacetedMesh)cboPrim.Items[cboPrim.SelectedIndex];
            PopulateFaceCombobox();

            glControl.Invalidate();
        }

        private void cboFace_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentFace = (ProfileFace)cboFace.Items[cboFace.SelectedIndex];

            glControl.Invalidate();
        }

        private void PopulatePrimCombobox()
        {
            cboPrim.Items.Clear();

            if (Prims != null)
            {
                for (int i = 0; i < Prims.Count; i++)
                    cboPrim.Items.Add(Prims[i]);
            }

            if (cboPrim.Items.Count > 0)
                cboPrim.SelectedIndex = 0;
        }

        private void PopulateFaceCombobox()
        {
            cboFace.Items.Clear();

            if (CurrentPrim != null)
            {
                for (int i = 0; i < CurrentPrim.Profile.Faces.Count; i++)
                    cboFace.Items.Add(CurrentPrim.Profile.Faces[i]);
            }

            if (cboFace.Items.Count > 0)
                cboFace.SelectedIndex = 0;
        }

        private void wireframeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wireframeToolStripMenuItem.Checked = !wireframeToolStripMenuItem.Checked;
            Wireframe = wireframeToolStripMenuItem.Checked;

            glControl.Invalidate();
        }

        private void worldBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmBrowser browser = new frmBrowser();
            browser.ShowDialog();
        }
    }
}
