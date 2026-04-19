using D_Dev.ColliderEvents;
using D_Dev.EntityPool;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public class ColliderTriggerPoolableRelease : MonoBehaviour
    {
        #region Fields

        [SerializeField] private TriggerColliderObservable _trigger;

        #endregion

        #region Monobehaviour

        private void Reset()
        {
            _trigger = GetComponent<TriggerColliderObservable>();
        }

        private void Awake()
        {
            if (_trigger == null)
                _trigger = GetComponent<TriggerColliderObservable>();
        }

        private void OnEnable()
        {
            if (_trigger != null)
                _trigger.OnEnter.AddListener(HandleEnter);
        }

        private void OnDisable()
        {
            if (_trigger != null)
                _trigger.OnEnter.RemoveListener(HandleEnter);
        }

        #endregion

        #region Private

        private void HandleEnter(Collider other)
        {
            var poolable = other.GetComponentInParent<PoolableObject>();
            if (poolable != null)
                poolable.Release();
        }

        #endregion
    }
}
