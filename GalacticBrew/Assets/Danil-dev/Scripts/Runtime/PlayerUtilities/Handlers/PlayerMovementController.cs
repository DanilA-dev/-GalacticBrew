using D_Dev.MovementHandler;
using D_Dev.PolymorphicValueSystem;
using UnityEngine;

namespace D_Dev.PlayerStateController
{
    public class PlayerMovementController : AutonomousMovementController
    {
        #region Fields

        [SerializeReference] private PolymorphicValue<Transform> _directionRoot;
        [SerializeReference] private PolymorphicValue<Vector3> _rawInputDirection;
        [SerializeReference] private PolymorphicValue<Vector3> _moveInputDirection;
        
        #endregion

        #region Overrides

        protected override void Update()
        {
            base.Update();
            _moveInputDirection.Value = GetMovementDirection();
        }

        #endregion
        
        #region Private

        private Vector3 GetMovementDirection()
        {
            var rootRight = _directionRoot.Value.right;
            var rootForward = _directionRoot.Value.forward;
            rootRight.y = 0f;
            rootForward.y = 0f;
            
            return rootRight * _rawInputDirection.Value.x + rootForward * _rawInputDirection.Value.z;
        }

        #endregion
    }
}