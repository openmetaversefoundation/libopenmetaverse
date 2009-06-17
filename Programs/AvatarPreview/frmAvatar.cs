using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

using Tao.OpenGl;
using Tao.Platform.Windows;

using OpenMetaverse;
using OpenMetaverse.Imaging;
using OpenMetaverse.Rendering;
using OpenMetaverse.Assets;

namespace AvatarPreview
{
    public partial class frmAvatar : Form
    {
        GridClient _client = new GridClient();
        Dictionary<string, GLMesh> _meshes = new Dictionary<string, GLMesh>();
        bool _wireframe = true;
        bool _showSkirt = false;

        public frmAvatar()
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

            glControl_Resize(null, null);
        }

        private void lindenLabMeshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "avatar_lad.xml|avatar_lad.xml";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _meshes.Clear();

                try
                {
                    // Parse through avatar_lad.xml to find all of the mesh references
                    XmlDocument lad = new XmlDocument();
                    lad.Load(dialog.FileName);

                    XmlNodeList meshes = lad.GetElementsByTagName("mesh");

                    foreach (XmlNode meshNode in meshes)
                    {
                        string type = meshNode.Attributes.GetNamedItem("type").Value;
                        int lod = Int32.Parse(meshNode.Attributes.GetNamedItem("lod").Value);
                        string fileName = meshNode.Attributes.GetNamedItem("file_name").Value;
                        //string minPixelWidth = meshNode.Attributes.GetNamedItem("min_pixel_width").Value;

                        // Mash up the filename with the current path
                        fileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(dialog.FileName), fileName);

                        GLMesh mesh = (_meshes.ContainsKey(type) ? _meshes[type] : new GLMesh(type));

                        if (lod == 0)
                        {
                            mesh.LoadMesh(fileName);
                        }
                        else
                        {
                            mesh.LoadLODMesh(lod, fileName);
                        }

                        _meshes[type] = mesh;
                        glControl_Resize(null, null);
                        glControl.Invalidate();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load avatar mesh: " + ex.Message);
                }
            }
        }

        private void textureToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void wireframeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wireframeToolStripMenuItem.Checked = !wireframeToolStripMenuItem.Checked;
            _wireframe = wireframeToolStripMenuItem.Checked;

            glControl.Invalidate();
        }

        private void skirtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            skirtToolStripMenuItem.Checked = !skirtToolStripMenuItem.Checked;
            _showSkirt = skirtToolStripMenuItem.Checked;

            glControl.Invalidate();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Written by John Hurliman <jhurliman@jhurliman.org> (http://www.jhurliman.org/)");
        }

        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glLoadIdentity();

            // Setup wireframe or solid fill drawing mode
            if (_wireframe)
                Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_LINE);
            else
                Gl.glPolygonMode(Gl.GL_FRONT, Gl.GL_FILL);

            // Push the world matrix
            Gl.glPushMatrix();

            Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);

            // World rotations
            Gl.glRotatef((float)scrollRoll.Value, 1f, 0f, 0f);
            Gl.glRotatef((float)scrollPitch.Value, 0f, 1f, 0f);
            Gl.glRotatef((float)scrollYaw.Value, 0f, 0f, 1f);

            if (_meshes.Count > 0)
            {
                foreach (GLMesh mesh in _meshes.Values)
                {
                    if (!_showSkirt && mesh.Name == "skirtMesh")
                        continue;

                    Gl.glColor3f(1f, 1f, 1f);

                    // Individual prim matrix
                    Gl.glPushMatrix();

                    //Gl.glTranslatef(mesh.Position.X, mesh.Position.Y, mesh.Position.Z);

                    Gl.glRotatef(mesh.RotationAngles.X, 1f, 0f, 0f);
                    Gl.glRotatef(mesh.RotationAngles.Y, 0f, 1f, 0f);
                    Gl.glRotatef(mesh.RotationAngles.Z, 0f, 0f, 1f);

                    Gl.glScalef(mesh.Scale.X, mesh.Scale.Y, mesh.Scale.Z);

                    // TODO: Texturing

                    Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, mesh.RenderData.TexCoords);
                    Gl.glVertexPointer(3, Gl.GL_FLOAT, 0, mesh.RenderData.Vertices);
                    Gl.glDrawElements(Gl.GL_TRIANGLES, mesh.RenderData.Indices.Length, Gl.GL_UNSIGNED_SHORT, mesh.RenderData.Indices);
                }
            }

            // Pop the world matrix
            Gl.glPopMatrix();

            Gl.glDisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
            Gl.glDisableClientState(Gl.GL_VERTEX_ARRAY);

            Gl.glFlush();
        }

        private void glControl_Resize(object sender, EventArgs e)
        {
            //Gl.glClearColor(0.39f, 0.58f, 0.93f, 1.0f); // Cornflower blue anyone?
            Gl.glClearColor(0f, 0f, 0f, 1f);

            Gl.glPushMatrix();
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();

            Gl.glViewport(0, 0, glControl.Width, glControl.Height);

            Glu.gluPerspective(50.0d, 1.0d, 0.001d, 50d);

            Vector3 center = Vector3.Zero;
            GLMesh head, lowerBody;
            if (_meshes.TryGetValue("headMesh", out head) && _meshes.TryGetValue("lowerBodyMesh", out lowerBody))
                center = (head.RenderData.Center + lowerBody.RenderData.Center) / 2f;

            Glu.gluLookAt(
                    center.X, (double)scrollZoom.Value * 0.1d + center.Y, center.Z,
                    center.X, (double)scrollZoom.Value * 0.1d + center.Y + 1d, center.Z,
                    0d, 0d, 1d);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
        }

        private void scroll_ValueChanged(object sender, EventArgs e)
        {
            glControl_Resize(null, null);
            glControl.Invalidate();
        }

        private void pic_MouseClick(object sender, MouseEventArgs e)
        {
            PictureBox control = (PictureBox)sender;

            OpenFileDialog dialog = new OpenFileDialog();
            // TODO: Setup a dialog.Filter for supported image types

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    System.Drawing.Image image = System.Drawing.Image.FromFile(dialog.FileName);

                    #region Dimensions Check

                    if (control == picEyesBake)
                    {
                        // Eyes texture is 128x128
                        if (Width != 128 || Height != 128)
                        {
                            Bitmap resized = new Bitmap(128, 128, image.PixelFormat);
                            Graphics graphics = Graphics.FromImage(resized);

                            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(image, 0, 0, 128, 128);

                            image.Dispose();
                            image = resized;
                        }
                    }
                    else
                    {
                        // Other textures are 512x512
                        if (Width != 128 || Height != 128)
                        {
                            Bitmap resized = new Bitmap(512, 512, image.PixelFormat);
                            Graphics graphics = Graphics.FromImage(resized);

                            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(image, 0, 0, 512, 512);

                            image.Dispose();
                            image = resized;
                        }
                    }

                    #endregion Dimensions Check

                    // Set the control image
                    control.Image = image;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load image: " + ex.Message);
                }
            }
            else
            {
                control.Image = null;
            }

            #region Baking

            Dictionary<int, float> paramValues = GetParamValues();
            Dictionary<AppearanceManager.TextureIndex, AssetTexture> layers =
                    new Dictionary<AppearanceManager.TextureIndex, AssetTexture>();
            int textureCount = 0;

            if ((string)control.Tag == "Head")
            {
                if (picHair.Image != null)
                {
                    layers.Add(AppearanceManager.TextureIndex.Hair,
                        new AssetTexture(new ManagedImage((Bitmap)picHair.Image)));
                    ++textureCount;
                }
                if (picHeadBodypaint.Image != null)
                {
                    layers.Add(AppearanceManager.TextureIndex.HeadBodypaint,
                        new AssetTexture(new ManagedImage((Bitmap)picHeadBodypaint.Image)));
                    ++textureCount;
                }

                // Compute the head bake
                Baker baker = new Baker(
                    _client, AppearanceManager.BakeType.Head, textureCount, paramValues);

                foreach (KeyValuePair<AppearanceManager.TextureIndex, AssetTexture> kvp in layers)
                    baker.AddTexture(kvp.Key, kvp.Value, false);

                if (baker.BakedTexture != null)
                {
                    AssetTexture bakeAsset = baker.BakedTexture;
                    // Baked textures use the alpha layer for other purposes, so we need to not use it
                    bakeAsset.Image.Channels = ManagedImage.ImageChannels.Color;
                    picHeadBake.Image = LoadTGAClass.LoadTGA(new MemoryStream(bakeAsset.Image.ExportTGA()));
                }
                else
                {
                    MessageBox.Show("Failed to create the bake layer, unknown error");
                }
            }
            else if ((string)control.Tag == "Upper")
            {
                if (picUpperBodypaint.Image != null)
                {
                    layers.Add(AppearanceManager.TextureIndex.UpperBodypaint,
                        new AssetTexture(new ManagedImage((Bitmap)picUpperBodypaint.Image)));
                    ++textureCount;
                }
                if (picUpperGloves.Image != null)
                {
                    layers.Add(AppearanceManager.TextureIndex.UpperGloves,
                        new AssetTexture(new ManagedImage((Bitmap)picUpperGloves.Image)));
                    ++textureCount;
                }
                if (picUpperUndershirt.Image != null)
                {
                    layers.Add(AppearanceManager.TextureIndex.UpperUndershirt,
                        new AssetTexture(new ManagedImage((Bitmap)picUpperUndershirt.Image)));
                    ++textureCount;
                }
                if (picUpperShirt.Image != null)
                {
                    layers.Add(AppearanceManager.TextureIndex.UpperShirt,
                        new AssetTexture(new ManagedImage((Bitmap)picUpperShirt.Image)));
                    ++textureCount;
                }
                if (picUpperJacket.Image != null)
                {
                    layers.Add(AppearanceManager.TextureIndex.UpperJacket,
                        new AssetTexture(new ManagedImage((Bitmap)picUpperJacket.Image)));
                    ++textureCount;
                }

                // Compute the upper body bake
                Baker baker = new Baker(
                    _client, AppearanceManager.BakeType.UpperBody, textureCount, paramValues);

                foreach (KeyValuePair<AppearanceManager.TextureIndex, AssetTexture> kvp in layers)
                    baker.AddTexture(kvp.Key, kvp.Value, false);

                if (baker.BakedTexture != null)
                {
                    AssetTexture bakeAsset = baker.BakedTexture;
                    // Baked textures use the alpha layer for other purposes, so we need to not use it
                    bakeAsset.Image.Channels = ManagedImage.ImageChannels.Color;
                    picUpperBodyBake.Image = LoadTGAClass.LoadTGA(new MemoryStream(bakeAsset.Image.ExportTGA()));
                }
                else
                {
                    MessageBox.Show("Failed to create the bake layer, unknown error");
                }
            }
            else if ((string)control.Tag == "Lower")
            {
                if (picLowerBodypaint.Image != null)
                {
                    layers.Add(AppearanceManager.TextureIndex.LowerBodypaint,
                        new AssetTexture(new ManagedImage((Bitmap)picLowerBodypaint.Image)));
                    ++textureCount;
                }
                if (picLowerUnderpants.Image != null)
                {
                    layers.Add(AppearanceManager.TextureIndex.LowerUnderpants,
                        new AssetTexture(new ManagedImage((Bitmap)picLowerUnderpants.Image)));
                    ++textureCount;
                }
                if (picLowerSocks.Image != null)
                {
                    layers.Add(AppearanceManager.TextureIndex.LowerSocks,
                        new AssetTexture(new ManagedImage((Bitmap)picLowerSocks.Image)));
                    ++textureCount;
                }
                if (picLowerShoes.Image != null)
                {
                    layers.Add(AppearanceManager.TextureIndex.LowerShoes,
                        new AssetTexture(new ManagedImage((Bitmap)picLowerShoes.Image)));
                    ++textureCount;
                }
                if (picLowerPants.Image != null)
                {
                    layers.Add(AppearanceManager.TextureIndex.LowerPants,
                        new AssetTexture(new ManagedImage((Bitmap)picLowerPants.Image)));
                    ++textureCount;
                }

                // Compute the lower body bake
                Baker baker = new Baker(
                    _client, AppearanceManager.BakeType.LowerBody, textureCount, paramValues);

                foreach (KeyValuePair<AppearanceManager.TextureIndex, AssetTexture> kvp in layers)
                    baker.AddTexture(kvp.Key, kvp.Value, false);

                if (baker.BakedTexture != null)
                {
                    AssetTexture bakeAsset = baker.BakedTexture;
                    // Baked textures use the alpha layer for other purposes, so we need to not use it
                    bakeAsset.Image.Channels = ManagedImage.ImageChannels.Color;
                    picLowerBodyBake.Image = LoadTGAClass.LoadTGA(new MemoryStream(bakeAsset.Image.ExportTGA()));
                }
                else
                {
                    MessageBox.Show("Failed to create the bake layer, unknown error");
                }
            }
            else if ((string)control.Tag == "Bake")
            {
                // Bake image has been set manually, no need to manually calculate a bake
                // FIXME:
            }

            #endregion Baking
        }

        private Dictionary<int, float> GetParamValues()
        {
            Dictionary<int, float> paramValues = new Dictionary<int, float>(VisualParams.Params.Count);

            foreach (KeyValuePair<int, VisualParam> kvp in VisualParams.Params)
            {
                VisualParam vp = kvp.Value;
                paramValues.Add(vp.ParamID, vp.DefaultValue);
            }

            return paramValues;
        }
    }
}
