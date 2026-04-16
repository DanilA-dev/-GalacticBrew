using D_Dev.MovementHandler;
using D_Dev.PolymorphicValueSystem;
using D_Dev.PositionRotationConfig;
using D_Dev.StateMachineBehaviour;
using D_Dev.Utility;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Core.States
{
    public class CharacterIdleState : BaseComponentState
    {
        #region Fields

        [SerializeField] private BaseMovementController _movementController;
        [Space]
        [SerializeField] private bool _lookAt;
        [ShowIf(nameof(_lookAt))]
        [SerializeReference] private BasePositionSettings _lookAtTarget;
        [ShowIf(nameof(_lookAt))]
        [SerializeField] private Transform _rotationRoot;
        [ShowIf(nameof(_lookAt))]
        [SerializeReference] private PolymorphicValue<float> _rotationSpeed;

        private RotationHandler _rotationHandler;

        #endregion

        #region Overrides

        public override void OnEnter()
        {
            _movementController.StopMovement();

            if (_lookAt)
            {
                _rotationHandler ??= new RotationHandler();
                _rotationHandler.Initialize(_rotationRoot);
            }
        }

        public override void OnUpdate()
        {
            if (_lookAt)
                UpdateLookAt();
        }

        #endregion

        #region Private

        private void UpdateLookAt()
        {
            var direction = _lookAtTarget.GetPosition() - _rotationRoot.position;
            _rotationHandler.RotateTowards(direction, _rotationSpeed.Value);
        }

        #endregion
    }
}