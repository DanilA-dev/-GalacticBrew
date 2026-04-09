using D_Dev.MovementHandler;
using D_Dev.StateMachineBehaviour;
using UnityEngine;

namespace _Project.Scripts.Core.States
{
    public class CharacterIdleState : BaseComponentState
    {
        #region Fields

        [SerializeField] private BaseMovementController _movementController;
        
        #endregion
        
        #region Overrides

        public override void OnEnter()
        {
            _movementController.StopMovement();
        }
        
        #endregion
    }
}