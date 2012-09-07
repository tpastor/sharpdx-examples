using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpExamples;
using System.Windows.Forms;

namespace SharpExamples
{
    /// <summary>
    /// First Person Camera
    /// To be used in Ploobs Demos and in Debug Mode.
    /// It is not a camera to be used in Production Environment
    /// </summary>
    public class CameraFirstPerson : ICamera
    {
        int width;
        int height;
        Input input;
        public CameraFirstPerson(Input input, float lrRot, float udRot, Vector3 startingPos, int width, int height)
        {
            this.height = height;
            this.width = width;
            this.input = input;
            this.leftrightRot = lrRot;
            this.updownRot = udRot;
            _position = startingPos;            
            UpdateViewMatrix();            
            _projection = Matrix.PerspectiveFovLH(_fieldOdView, _aspectRatio, _nearPlane, _farPlane);
        }
        
        #region Fields

        private Matrix viewProjection;
        private Vector3 _position = Vector3.UnitX;        
        private Vector3 _target = Vector3.Zero;        
        private Vector3 _up = Vector3.UnitY;
        private Quaternion _rotation = Quaternion.Identity;
        private float _fieldOdView =(float) Math.PI / 4;
        private float _aspectRatio = 4f / 3f;
        private float _nearPlane = 1.0f;
        private float _farPlane = 2000f;
        private Matrix _view;
        private Matrix _projection;
        private float leftrightRot;
        private float updownRot;
        private float rotationSpeed = 0.005f;
        private float sensibility = 0.5f;
        private float moveSpeed = 1f;
        #endregion

        public float MoveSpeed
        {
            get { return moveSpeed; }
            set
            {
                if (value <= 0)
                {
                }
                else
                {
                    moveSpeed = value;
                }
            }
        }

        public float RotationSpeed
        {
            set
            {
                if (value <= 0)
                {
                }
                else
                {
                    this.rotationSpeed = value;
                }
            }
            get
            {
                return rotationSpeed;
            }
        }



        public float Sensibility
        {
            set
            {
                if (value <= 0)
                {
                }
                else
                {
                    this.sensibility = value;
                }
            }
            get
            {
                return sensibility;
            }
        }



        #region ICamera Members


        public override Vector3 Position
        {

            get
            {
                return _position;
            }
            set
            {
                _position = value;
                UpdateViewMatrix();
            }
        }

        public override Vector3 Up
        {
            get
            {
                return this._up;
            }
            set
            {
                this._up = value;
                UpdateViewMatrix();
            }
        }

        public override Quaternion Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
            }
        }

        public override float FieldOfView
        {
            get
            {
                return this._fieldOdView;
            }
            set
            {
                if (value <= 0)
                {
                }
                else
                {
                    this._fieldOdView = value;
                }

            }
        }

        public override float AspectRatio
        {
            get
            {
                return _aspectRatio;
            }
            set
            {
                if (value <= 0)
                {
                }
                else
                {
                    this._aspectRatio = value;
                }
            }
        }

        public override float NearPlane
        {
            get
            {
                return this._nearPlane;
            }
            set
            {
                if (value <= 0)
                {
                }
                else
                {
                    this._nearPlane = value;
                }
            }
        }

        public override float FarPlane
        {
            get
            {
                return this._farPlane;
            }
            set
            {
                if (value <= 0)
                {
                }
                else
                {
                    this._farPlane = value;
                }
            }
        }

        public override Matrix View
        {
            get { return _view; }
        }

        public override Matrix Projection
        {
            get { return _projection; }
        }

        private void UpdateViewMatrix()
        {
            Matrix cameraRotation = Matrix.RotationX(updownRot) * Matrix.RotationY(leftrightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);

            Vector3 cameraRotatedTarget = Vector3.TransformCoordinate(cameraOriginalTarget, cameraRotation);
            _target = _position + cameraRotatedTarget;

            _up = Vector3.TransformCoordinate(cameraOriginalUpVector, cameraRotation);

            _view = Matrix.LookAtLH(_position, _target, _up);
        }


        public override void Update(float elapsed)
        {
            UpdateCamera();
            viewProjection = View * Projection;
        }

        
        private void UpdateCamera()
        {
            
            float xDifference = input.MousePoint.X - width / 2;
            float yDifference = input.MousePoint.Y - height / 2;
            leftrightRot += rotationSpeed * xDifference;
            updownRot -= rotationSpeed * yDifference;                
            UpdateViewMatrix();

            input.SetMousePosition(width / 2, height / 2);

            if (input.KeysDown.Contains(Keys.Up) || input.KeysDown.Contains(Keys.W))      //Forward
            {
                AddToCameraPosition(new Vector3(0, 0, -sensibility));
            }
            if (input.KeysDown.Contains(Keys.Down) || input.KeysDown.Contains(Keys.S))    //Backward
            {
                AddToCameraPosition(new Vector3(0, 0, sensibility));
                
            }
            if (input.KeysDown.Contains(Keys.Right) || input.KeysDown.Contains(Keys.D))   //Right
            {
                AddToCameraPosition(new Vector3(-sensibility, 0, 0));
                
            }
            if (input.KeysDown.Contains(Keys.Left) || input.KeysDown.Contains(Keys.A))    //Left
            {
                AddToCameraPosition(new Vector3(sensibility, 0, 0));
                
            }
            if (input.KeysDown.Contains(Keys.Q))                                     //Up
            {
                AddToCameraPosition(new Vector3(0, sensibility, 0));
                
            }
            if (input.KeysDown.Contains(Keys.Z))                                     //Down
            {
                AddToCameraPosition(new Vector3(0, -sensibility, 0));
                
            }
        }
        
        #endregion

#if WINDOWS_PHONE
        TouchCollection tcpressed;        
#endif

        public float UpDownRot
        {
            get { return updownRot; }
            set
            {
                if (value <= 0)
                {
                }
                else
                {
                    updownRot = value;
                }
            }
        }

        public float LeftRightRot
        {
            get { return leftrightRot; }
            set
            {
                if (value <= 0)
                {
                }
                else
                {
                    leftrightRot = value;
                }
            }
        }

        public override Matrix ViewProjection
        {
            get { return viewProjection; }
        }

        public override Vector3 Target
        {
            get
            {
                return _target;
            }
            set
            {
                Vector3 floorProjection = new Vector3(value.X, 0, value.Z);
                float directionLength = floorProjection.Length();
                updownRot = (float)Math.Atan2(value.Y, value.Length());
                leftrightRot = -(float)Math.Atan2(value.X, -value.Z);
            }
        }

        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.RotationX(updownRot) * Matrix.RotationY(leftrightRot);
            Vector3 rotatedVector = Vector3.TransformCoordinate(vectorToAdd, cameraRotation);
            _position += moveSpeed * rotatedVector;
            UpdateViewMatrix();
        }
    }
}
