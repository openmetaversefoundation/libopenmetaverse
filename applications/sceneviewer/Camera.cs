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
        #region Constants
        private static readonly float MAX_RADIANS = (float)(Math.PI * 2.0);
        private static readonly float MAX_PHI = (float)((Math.PI) * 2.0);
        private static readonly float MIN_PHI = 0;
        #endregion Constants

        #region Private Fields
        private Vector3 _cameraPosition;
        private Vector3 _lookatPosition;
        private Matrix _projection;
        private GameWindow _window;

        private float _theta;
        private float _phi;
        private float _zoom;
        #endregion Private Fields

        #region Constructors
        /// <summary>
        /// Default constructor. Assumes the camera should be oriented up.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="lookAt"></param>
        public Camera(GameWindow window, Vector3 pos, Vector3 lookAt)
        {
            _window = window;
            _cameraPosition = pos;
            _lookatPosition = lookAt;

            UpdateProjection();
        }
        #endregion Constructors

        #region Properties
        /// <summary>
        /// Generate the current view matrix for the camera
        /// </summary>
        public Matrix ViewMatrix
        {
            get 
            {
                Vector3 newCameraPosition = _cameraPosition;
                Matrix rotationMatrix;

                // Apply zoom
                newCameraPosition += GetZoomVector();

                // Apply Z rotation
                rotationMatrix = Matrix.CreateRotationX(_phi);
                newCameraPosition = Vector3.Transform(newCameraPosition, 
                    rotationMatrix);

                // Apply Y rotation
                rotationMatrix = Matrix.CreateRotationY( _theta);
                newCameraPosition = Vector3.Transform(newCameraPosition,
                    rotationMatrix);

                return Matrix.CreateLookAt(newCameraPosition, _lookatPosition,
                    new Vector3(0.0f, 0.0f, 1.0f)); 
            }
        }

        public Matrix ProjectionMatrix
        {
            get
            {
                return _projection;
            }
        }

        /// <summary>
        /// Gets the view and projection matrices multiplied together
        /// </summary>
        public Matrix ViewProjectionMatrix
        {
            get
            {
                return ViewMatrix * _projection;
            }
        }

        /// <summary>
        /// Gets the camera position
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return _cameraPosition;
            }
        }

        /// <summary>
        /// Get or set the current angle of rotation in radians about the
        /// Y-axis for the camera.
        /// </summary>
        public float Theta
        {
            get { return _theta; }
            set 
            {
                _theta = value % MAX_RADIANS;
            }
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
            set 
            {
                if (value > MAX_PHI)
                {
                    _phi = MAX_PHI;
                }
                else if (value < MIN_PHI)
                {
                    _phi = MIN_PHI;
                }
                else
                {
                    _phi = value;
                }
            }
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
            set { _zoom = value; }
        }
        #endregion Properties

        /// <summary>
        /// Call this method any time the client window changes.
        /// </summary>
        public void UpdateProjection()
        {
            _projection = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                (float)_window.ClientWidth / (float)_window.ClientHeight,
                1.0f, 512.0f);
        }

        /// <summary>
        /// Determine how far the camera should move forward
        /// or backward based on the current zoom value.
        /// 
        /// This is calculated by finding the normal vector
        /// between the camera's position and the LookAt
        /// point and multiplying this vector by the zoom
        /// amount.
        /// </summary>
        /// <returns></returns>
        private Vector3 GetZoomVector()
        {
            Vector3 diff = _cameraPosition - _lookatPosition;

            diff.Normalize();
            diff *= _zoom;

            return diff;
        }
    }
}