using UnityEngine;

namespace D_Dev.Utility
{
    public class RotationHandler
    {
        #region Fields

        private Transform _transform;
        private float _rotationSpeed;
        private bool _isRotating;

        #endregion

        #region Properties

        public bool IsRotating => _isRotating;
        public float RotationSpeed => _rotationSpeed;

        #endregion

        #region Initialization

        public void Initialize(Transform transform, float rotationSpeed = 5f)
        {
            _transform = transform;
            _rotationSpeed = rotationSpeed;
        }

        #endregion

        #region Public Methods

        public void RotateTowards(Vector3 direction, float rotationSpeed = -1f, bool ignoreY = true)
        {
            if (_transform == null || direction == Vector3.zero)
                return;

            float speed = rotationSpeed > 0 ? rotationSpeed : _rotationSpeed;
            Vector3 targetDirection = ignoreY 
                ? new Vector3(direction.x, 0, direction.z) 
                : direction;

            if (targetDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

                _transform.rotation = Quaternion.Slerp(
                    _transform.rotation,
                    targetRotation,
                    speed * Time.deltaTime
                );

                _isRotating = true;
            }
            else
            {
                _isRotating = false;
            }
        }
        
        public void FaceMovementDirection(Vector3 movementDirection, float rotationSpeed = -1f)
        {
            RotateTowards(movementDirection, rotationSpeed);
        }

        
        public void SetRotation(Quaternion targetRotation)
        {
            if (_transform == null)
                return;

            _transform.rotation = targetRotation;
            _isRotating = false;
        }

        
        public void InstantRotate(Vector3 direction)
        {
            if (_transform == null || direction == Vector3.zero)
                return;

            Vector3 targetDirection = new Vector3(direction.x, 0, direction.z);

            if (targetDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
                _transform.rotation = targetRotation;
            }

            _isRotating = false;
        }

        
        public bool IsRotationComplete(Vector3 targetDirection, float tolerance = 5f)
        {
            if (_transform == null || targetDirection == Vector3.zero)
                return true;

            Vector3 targetDir = new Vector3(targetDirection.x, 0, targetDirection.z);
            if (targetDir.magnitude < 0.1f)
                return true;

            Quaternion targetRotation = Quaternion.LookRotation(targetDir, Vector3.up);
            float angle = Quaternion.Angle(_transform.rotation, targetRotation);

            return angle <= tolerance;
        }

        
        public Vector3 GetCurrentForwardDirection()
        {
            return _transform != null ? _transform.forward : Vector3.forward;
        }

        
        public void SetRotationSpeed(float rotationSpeed)
        {
            _rotationSpeed = rotationSpeed;
        }

        #endregion
    }
}