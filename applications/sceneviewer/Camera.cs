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
        public Vector3 _position;
        private Vector3 _lookatPosition;
        private Matrix _projection;
        private Matrix _view;

        private float _theta;
        private float _phi;
        private float _zoom;

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
            _projection = Matrix.CreatePerspectiveFieldOfView((float)MathHelper.PiOver4,
                (float)window.ClientWidth / (float)window.ClientHeight,
                1.0f, 1024.0f);
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