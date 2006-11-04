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

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace sceneviewer
{
    /// <summary>
    /// Simple third-person camera class
    /// </summary>
    public class Camera
    {
        public const float FOV = MathHelper.PiOver4;
        public const float NearClip = 1.0f;
        public const float FarClip = 1024.0f;

        public Vector3 _position;
        private Vector3 _lookatPosition;
        private Matrix _projection;
        private Matrix _view;

        private float _theta = 0;
        private float _phi = 0;
        private float _zoom = 0;

        public Matrix ViewMatrix
        {
            get { return _view; }
        }

        public Matrix ProjectionMatrix
        {
            get { return _projection; }
        }

        /// <summary>
        /// Gets the view and projection matrices multiplied together
        /// </summary>
        public Matrix ViewProjectionMatrix
        {
            get { return _view * _projection; }
        }

        /// <summary>
        /// Gets the camera position
        /// </summary>
        public Vector3 Position
        {
            get { return _position; }
        }

        /// <summary>
        /// Get or set the current angle of rotation in radians about the
        /// Y-axis for the camera.
        /// </summary>
        public float Theta
        {
            get { return _theta; }
            //set 
            //{
            //    _theta = value; //% MAX_RADIANS;
            //}
        }

        /// <summary>
        /// Get or set the current angle of rotation in radians about the
        /// Z-axis for the camera.
        /// 
        /// Applies a hard cap on the minimum or maximum values of phi.
        /// </summary>
        public float Phi
        {
            get { return _phi; }
        }

        /// <summary>
        /// Get or set the current zoom of the camera.
        /// 
        /// TODO: I should set a hard value on how close
        /// the camera can zoom in; it should never be able to go
        /// through the look at point.
        /// </summary>
        public float Zoom
        {
            get { return _zoom; }

            // TODO: Add a hard limit on zoom amount.
            //set { _zoom = value; }
        }


        /// <summary>
        /// Default constructor. Assumes the camera should be oriented up.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="lookAt"></param>
        public Camera(GameWindow window, Vector3 pos, Vector3 lookAt)
        {
            _position = pos;
            _lookatPosition = lookAt;

            UpdateProjection(window);
            Update();
        }

        public void Rotate(float angle)
        {
            _theta += angle;
            Update();
        }

        public void Translate(Vector3 distance)
        {
            _position += Vector3.Transform(distance, Matrix.CreateRotationZ(_theta));
            Update();
        }

        /// <summary>
        /// Call this method any time the client window changes.
        /// </summary>
        public void UpdateProjection(GameWindow window)
        {
            _projection = Matrix.CreatePerspectiveFieldOfView(FOV,
                (float)window.ClientBounds.Width / (float)window.ClientBounds.Height,
                NearClip, FarClip);
        }

        public void Update()
        {
            Vector3 newCameraPosition = _position;
            Matrix rotationMatrix;

            rotationMatrix = Matrix.CreateRotationZ(_theta);

            Vector3 cameraReference = Vector3.UnitY;
            Vector3 transformedReference = Vector3.Transform(cameraReference, rotationMatrix);

            _lookatPosition = transformedReference + _position;

            _view = Matrix.CreateLookAt(newCameraPosition, _lookatPosition,
                Vector3.UnitZ);
        }
    }
}