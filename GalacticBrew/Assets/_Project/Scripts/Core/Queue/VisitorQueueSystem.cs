using System;
using System.Collections.Generic;
using D_Dev.EntityVariable.Types;
using D_Dev.RuntimeEntityVariables;
using D_Dev.ScriptableVaiables;
using UnityEngine;

namespace _Project.Scripts.Core.Queue
{
    public class VisitorQueueSystem : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Transform _servicePoint;
        [SerializeField] private Transform[] _queueSlots;
        [SerializeField] private Transform _exitPoint;
        [Space]
        [SerializeField] private StringScriptableVariable _targetPositionID;
        [SerializeField] private StringScriptableVariable _isServedID;
        [SerializeField] private StringScriptableVariable _isAtRegisterID;

        private readonly List<RuntimeEntityVariablesContainer> _queue = new();

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

        #region Public

        public bool TryEnqueue(RuntimeEntityVariablesContainer visitor)
        {
            if (IsFull)
                return false;

            _queue.Add(visitor);
            int index = _queue.Count - 1;
            var slot = GetSlotTransform(index);
            SetTargetPosition(visitor, slot);

            OnVisitorEnqueued?.Invoke(visitor);
            return true;
        }

        public void ServeFirst()
        {
            if (_queue.Count == 0)
                return;

            var served = _queue[0];
            _queue.RemoveAt(0);

            SetIsServed(served, true);
            SetTargetPosition(served, _exitPoint);
            OnVisitorDequeued?.Invoke(served);

            for (int i = 0; i < _queue.Count; i++)
            {
                var slot = GetSlotTransform(i);
                SetTargetPosition(_queue[i], slot);
            }
        }

        #endregion

        #region Private

        private Transform GetSlotTransform(int index)
        {
            return index == 0 ? _servicePoint : _queueSlots[index - 1];
        }

        private void SetTargetPosition(RuntimeEntityVariablesContainer container, Transform t)
        {
            var variable = container.GetVariable<TransformEntityVariable>(_targetPositionID);
            if (variable?.Value != null)
                variable.Value.Value = t;
        }

        private void SetIsServed(RuntimeEntityVariablesContainer container, bool value)
        {
            var variable = container.GetVariable<BoolEntityVariable>(_isServedID);
            if (variable?.Value != null)
                variable.Value.Value = value;
        }

        

        #endregion
    }
}
