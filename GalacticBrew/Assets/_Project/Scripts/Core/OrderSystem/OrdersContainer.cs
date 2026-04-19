using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Core.OrderSystem
{
    [CreateAssetMenu(menuName = "Game/Order/Orders Container", fileName = "OrdersContainer")]
    public class OrdersContainer : ScriptableObject
    {
        #region Fields

        [ShowInInspector, ReadOnly]
        private readonly List<OrderInfo> _orders = new();

        #endregion

        #region Events

        public event Action<OrderInfo> OnOrderAdded;
        public event Action<OrderInfo> OnOrderRemoved;
        public event Action OnCleared;

        #endregion

        #region Properties

        public IReadOnlyList<OrderInfo> Orders => _orders;
        public int Count => _orders.Count;

        #endregion

        #region Monobehaviour

        private void OnEnable()
        {
            _orders.Clear();
        }

        private void OnDisable()
        {
            Clear();
        }

        #endregion

        #region Public

        public void Add(OrderInfo order)
        {
            if (order == null || _orders.Contains(order))
                return;

            _orders.Add(order);
            OnOrderAdded?.Invoke(order);
        }

        public bool Remove(OrderInfo order)
        {
            if (order == null)
                return false;

            if (!_orders.Remove(order))
                return false;

            OnOrderRemoved?.Invoke(order);
            return true;
        }

        public bool Contains(OrderInfo order)
        {
            return order != null && _orders.Contains(order);
        }

        public void Clear()
        {
            if (_orders.Count == 0)
                return;

            _orders.Clear();
            OnCleared?.Invoke();
        }

        #endregion
    }
}
