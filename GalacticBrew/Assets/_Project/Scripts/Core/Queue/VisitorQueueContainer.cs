using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Core.Queue
{
    [CreateAssetMenu(menuName = "Game/Queue/Visitor Queue Container", fileName = "VisitorQueueContainer")]
    public class VisitorQueueContainer : ScriptableObject
    {
        #region Fields

        [ShowInInspector, ReadOnly]
        private readonly List<VisitorQueue> _queues = new();

        #endregion

        #region Events

        public event Action<VisitorQueue> OnQueueAdded;
        public event Action<VisitorQueue> OnQueueRemoved;
        public event Action OnCleared;

        #endregion

        #region Properties

        public IReadOnlyList<VisitorQueue> Queues => _queues;
        public int Count => _queues.Count;

        #endregion

        #region Monobehaviour

        private void OnEnable()
        {
            _queues.Clear();
        }

        private void OnDisable()
        {
            Clear();
        }

        #endregion

        #region Public

        public void Add(VisitorQueue queue)
        {
            if (queue == null || _queues.Contains(queue))
                return;

            _queues.Add(queue);
            OnQueueAdded?.Invoke(queue);
        }

        public bool Remove(VisitorQueue queue)
        {
            if (queue == null)
                return false;

            if (!_queues.Remove(queue))
                return false;

            OnQueueRemoved?.Invoke(queue);
            return true;
        }

        public bool Contains(VisitorQueue queue)
        {
            return queue != null && _queues.Contains(queue);
        }

        public void Clear()
        {
            if (_queues.Count == 0)
                return;

            _queues.Clear();
            OnCleared?.Invoke();
        }

        #endregion
    }
}
