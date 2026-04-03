using UnityEngine;

namespace D_Dev.MovementHandler
{
    [System.Serializable]
    public class RigidbodyMovementHandler : BaseMovementHandler
    {
        #region Fields

        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private ForceMode _forceMode;

        #endregion

        #region Overrides

        public override void OnFixedUpdate()
        {
            if (_rigidbody == null)
                return;
 
            if (Direction.magnitude > 0.1f)
            {
                Vector3 force = Direction.normalized * Acceleration;
                _rigidbody.AddForce(force, _forceMode);
            }

            if (_rigidbody.linearVelocity.magnitude > MaxVelocity)
                _rigidbody.linearVelocity = Vector3.ClampMagnitude(_rigidbody.linearVelocity, MaxVelocity);
        }

        public override void StopMovement()
        {
            if (_rigidbody == null)
                return;
 
            _rigidbody.linearVelocity = Vector3.zero;
            Direction = Vector3.zero;
        }

        public override float GetVelocity() => _rigidbody != null ? _rigidbody.linearVelocity.magnitude : 0f;
        public override bool IsMoving() => _rigidbody != null && _rigidbody.linearVelocity.magnitude > 0.1f;

        #endregion
    }
}