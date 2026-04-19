using System;
using System.Collections.Generic;
using D_Dev.EntityVariable.Types;
using D_Dev.RuntimeEntityVariables;
using D_Dev.ScriptableVaiables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Core.Queue
{
    public class VisitorQueue : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Transform _servePoint;
        [SerializeField] private Transform _exitPoint;
        [Space]
        [SerializeField] private StringScriptableVariable _targetPositionID;
        [Space]
        [SerializeField] private VisitorQueueContainer _queueContainer;
        [SerializeField] private Transform[] _queueSlots;
        [SerializeField, ReadOnly] private List<RuntimeEntityVariablesContainer> _queue = new();

        #endregion

        #region Properties

        public bool IsFull => _queue.Count >= _queueSlots.Length + 1;
        public bool IsEmpty => _queue.Count == 0;
        public int Count => _queue.Count;
        public int Capacity => _queueSlots.Length + 1;

        #endregion

        #region Events

        public event Action<RuntimeEntityVariablesContainer> OnVisitorEnqueued;
        public event Action<RuntimeEntityVariablesContainer> OnVisitorDequeued;

        #endregion

        #region Monobehaviour

        private void OnEnable()
        {
            if (_queueContainer != null)
                _queueContainer.Add(this);
        }

        private void OnDisable()
        {
            if (_queueContainer != null)
                _queueContainer.Remove(this);
        }

        #endregion

        #region Public

        public bool TryEnqueue(RuntimeEntityVariablesContainer visitor)
        {
            if (IsFull)
                return false;

            _queue.Add(visitor);
            int index = _queue.Count - 1;
            var slot = GetSlotTransform(index);
            SetTargetPosition(visitor, slot.position);

            OnVisitorEnqueued?.Invoke(visitor);
            return true;
        }

        public void MoveQueue()
        {
            if (_queue.Count == 0)
                return;

            var leaving = _queue[0];
            _queue.RemoveAt(0);

            SetTargetPosition(leaving, _exitPoint.position);
            OnVisitorDequeued?.Invoke(leaving);

            for (int i = 0; i < _queue.Count; i++)
            {
                var slot = GetSlotTransform(i);
                SetTargetPosition(_queue[i], slot.position);
            }
        }

        #endregion

        #region Private

        private Transform GetSlotTransform(int index)
        {
            return index == 0 ? _servePoint : _queueSlots[index - 1];
        }

        private void SetTargetPosition(RuntimeEntityVariablesContainer container, Vector3 pos)
        {
            var variable = container.GetVariable<Vector3EntityVariable>(_targetPositionID);
            if (variable?.Value != null)
                variable.Value.Value = pos;
        }

        #endregion
    }
}
