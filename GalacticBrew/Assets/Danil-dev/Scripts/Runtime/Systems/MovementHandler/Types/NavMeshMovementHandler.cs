using UnityEngine;
using UnityEngine.AI;

namespace D_Dev.MovementHandler
{
    [System.Serializable]
    public class NavMeshMovementHandler : BaseMovementHandler
    {
        #region Fields

        [SerializeField] private NavMeshAgent _navMeshAgent;

        #endregion

        #region Overrides

        public override void OnUpdate()
        {
            if (_navMeshAgent == null)
                return;

            if (_navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.speed = MaxVelocity;
                _navMeshAgent.destination = Direction;
            }
        }

        public override void StopMovement()
        {
            if (_navMeshAgent == null)
                return;

            if (_navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
                _navMeshAgent.ResetPath();
            
            Direction = Vector3.zero;
        }

        public override float GetVelocity() => _navMeshAgent != null ? _navMeshAgent.velocity.magnitude : 0f;
        public override bool IsMoving() => _navMeshAgent != null && _navMeshAgent.velocity.magnitude > 0.1f;

        #endregion
    }
}