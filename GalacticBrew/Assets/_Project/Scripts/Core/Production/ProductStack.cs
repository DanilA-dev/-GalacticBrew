using System.Collections.Generic;
using D_Dev.EntityPool;
using D_Dev.PolymorphicValueSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Core.Production
{
    public class ProductStack : MonoBehaviour
    {
        #region Fields

        [Title("Slots")]
        [SerializeReference] private PolymorphicValue<Transform[]> _positionPoints = new TransformArrayConstantValue();
        [SerializeReference] private PolymorphicValue<Vector3> _stackOffset = new Vector3ConstantValue();
        [SerializeReference] private PolymorphicValue<int> _limit = new IntConstantValue();

        private readonly List<GameObject> _products = new List<GameObject>();
        private int _reservedCount;

        #endregion

        #region Properties

        public int Count => _products.Count;
        public int Limit => _limit?.Value ?? 0;
        public bool HasLimit => Limit > 0;
        public int FreeSlots => HasLimit ? Mathf.Max(0, Limit - _products.Count - _reservedCount) : int.MaxValue;
        public bool IsFull => HasLimit && (_products.Count + _reservedCount) >= Limit;
        public bool IsEmpty => _products.Count == 0;
        public IReadOnlyList<GameObject> Products => _products;

        #endregion

        #region Public

        public bool TryReserveSlot(out Transform parent, out Vector3 localPosition)
        {
            parent = null;
            localPosition = Vector3.zero;

            var points = _positionPoints?.Value;
            if (IsFull || points == null || points.Length == 0)
                return false;

            int slotIndex = _products.Count + _reservedCount;
            int pointIndex = slotIndex % points.Length;
            int stackLevel = slotIndex / points.Length;

            parent = points[pointIndex];
            if (parent == null)
                return false;

            localPosition = (_stackOffset?.Value ?? Vector3.zero) * stackLevel;
            _reservedCount++;
            return true;
        }

        public void CommitReserved(GameObject product)
        {
            if (_reservedCount > 0)
                _reservedCount--;
            if (product != null)
                _products.Add(product);
        }

        public void ReleaseReserved()
        {
            if (_reservedCount > 0)
                _reservedCount--;
        }

        public bool Remove(GameObject product)
        {
            return product != null && _products.Remove(product);
        }

        public bool TryPopTop(out GameObject product)
        {
            product = null;
            if (_products.Count == 0)
                return false;

            int last = _products.Count - 1;
            product = _products[last];
            _products.RemoveAt(last);
            return true;
        }

        public GameObject PeekTop()
        {
            return _products.Count == 0 ? null : _products[_products.Count - 1];
        }

        public void Clear()
        {
            _products.Clear();
            _reservedCount = 0;
        }

        public void ClearAndReleaseProducts()
        {
            _reservedCount = 0;

            if (_products.Count == 0)
                return;

            for (int i = _products.Count - 1; i >= 0; i--)
            {
                var product = _products[i];
                if (product == null)
                    continue;

                product.transform.SetParent(null);
                if (product.TryGetComponent(out PoolableObject poolableObject))
                    poolableObject.Release();
            }

            _products.Clear();
        }

        #endregion
    }
}
