using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using D_Dev.CurrencySystem;
using D_Dev.PolymorphicValueSystem;
using D_Dev.RuntimeEntityVariables;
using D_Dev.ScriptableVaiables;
using Game.Core.Production;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace _Project.Scripts.Core.OrderSystem
{
    public class VisitorOrderHandler : MonoBehaviour
    {
        #region Fields

        [Title("Order")]
        [SerializeReference] private PolymorphicValue<OrderInfo> _order = new OrderConstantValue();
        [SerializeField] private bool _randomCount;
        [HideIf(nameof(_randomCount))]
        [SerializeField, Min(1)] private int _count = 1;
        [ShowIf(nameof(_randomCount))]
        [SerializeField] private Vector2Int _countRange = new Vector2Int(1, 3);

        [Title("Product Transfer")]
        [SerializeField] private ProductContainer _receiverContainer;
        [SerializeField] private StringScriptableVariable _productOrderVariableID;

        [Title("Currency")]
        [SerializeField] private CurrencyInfoSetter _currencySetter;

        [FoldoutGroup("Events")]
        public UnityEvent<OrderInfo> OnOrderGenerated;
        [FoldoutGroup("Events")]
        public UnityEvent OnOrderFulfilled;
        [FoldoutGroup("Events")]
        public UnityEvent OnOrderFailed;

        private ProductContainer _sourceContainer;
        private OrderInfo _currentOrder;
        private int _requiredCount;
        private readonly List<GameObject> _matchBuffer = new();

        #endregion

        #region Properties

        public OrderInfo CurrentOrder => _currentOrder;
        public int RequiredCount => _requiredCount;
        public ProductContainer SourceContainer => _sourceContainer;

        #endregion

        #region Public

        public void SetSourceContainer(ProductContainer sourceContainer) => _sourceContainer = sourceContainer;

        public void GenerateOrder()
        {
            _currentOrder = _order?.Value;
            _requiredCount = _randomCount
                ? Random.Range(_countRange.x, _countRange.y + 1)
                : _count;

            OnOrderGenerated?.Invoke(_currentOrder);
        }

        public void FulfillOrder() => TryFulfillOrderAsync().Forget();

        public async UniTask<bool> TryFulfillOrderAsync()
        {
            if (_currentOrder == null || _sourceContainer == null
                || _sourceContainer.Stack == null || _receiverContainer == null)
            {
                OnOrderFailed?.Invoke();
                return false;
            }

            var sourceStack = _sourceContainer.Stack;
            _matchBuffer.Clear();

            foreach (var product in sourceStack.Products)
            {
                if (GetProductOrder(product) != _currentOrder)
                    continue;

                _matchBuffer.Add(product);
                if (_matchBuffer.Count >= _requiredCount)
                    break;
            }

            if (_matchBuffer.Count < _requiredCount)
            {
                _matchBuffer.Clear();
                OnOrderFailed?.Invoke();
                return false;
            }

            int transferred = 0;
            for (int i = 0; i < _requiredCount; i++)
            {
                var product = _matchBuffer[i];
                if (!sourceStack.Remove(product))
                    continue;

                bool accepted = await _receiverContainer.PutIn(product);
                if (accepted)
                    transferred++;
            }
            _matchBuffer.Clear();

            if (transferred > 0 && _currencySetter != null)
                _currencySetter.TryDepositValue(_currentOrder.Price * transferred);

            if (transferred >= _requiredCount)
            {
                OnOrderFulfilled?.Invoke();
                return true;
            }

            OnOrderFailed?.Invoke();
            return false;
        }

        #endregion

        #region Private

        private OrderInfo GetProductOrder(GameObject product)
        {
            if (product == null || _productOrderVariableID == null)
                return null;

            var container = product.GetComponent<RuntimeEntityVariablesContainer>();
            if (container == null)
                return null;

            var variable = container.GetVariable<OrderEntityVariable>(_productOrderVariableID);
            return variable?.Value?.Value;
        }

        #endregion
    }
}
