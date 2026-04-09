using D_Dev.MovementHandler;
using D_Dev.PolymorphicValueSystem;
using D_Dev.PositionRotationConfig;
using D_Dev.StateMachineBehaviour;
using D_Dev.Utility;
using Sirenix.OdinInspector;
using UnityEngine;

namespace  _Project.Scripts.Core.States
{
    public class CharacterMoveState : BaseComponentState
    {
        #region Fields

        [SerializeReference] private BasePositionSettings _movementDirection;
        [SerializeField] private BaseMovementController _movementController;
        [SerializeField] private bool _rotateToDirection;
        [Space]
        [SerializeReference] private PolymorphicValue<float> _accelerationSpeed;
        [SerializeReference] private PolymorphicValue<float> _maxVelocity;
        [ShowIf(nameof(_rotateToDirection))]
        [SerializeField] private Transform _rotationRoot;
        [ShowIf(nameof(_rotateToDirection))]
        [SerializeReference] private PolymorphicValue<float> _rotationSpeed;

        private RotationHandler _rotationHandler;

        #endregion

        #region Overrides

        public override void OnEnter()
        {
            _rotationHandler ??= new RotationHandler();
            _rotationHandler.Initialize(_rotationRoot);

            _movementController.ResumeMovement();
            _movementController.SetMaxVelocity(_maxVelocity.Value);
            _movementController.SetAcceleration(_accelerationSpeed.Value);
        }

        public override void OnUpdate()
        {
            UpdateMovement();
        }

        #endregion

        #region Private

        private void UpdateMovement()
        {
            var direction = _movementDirection.GetPosition();

            if (_rotateToDirection)
                _rotationHandler.FaceMovementDirection(direction, _rotationSpeed.Value);

            _movementController?.SetDirection(direction);
        }

        #endregion
    }
}