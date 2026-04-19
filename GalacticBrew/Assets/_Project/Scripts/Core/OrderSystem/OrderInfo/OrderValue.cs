using D_Dev.PolymorphicValueSystem;
using D_Dev.RuntimeEntityVariables;
using D_Dev.ScriptableVaiables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Core.OrderSystem
{
    [System.Serializable]
    public abstract class OrderValue : PolymorphicValue<OrderInfo> {}

    [System.Serializable]
    public sealed class OrderConstantValue : OrderValue
    {
        #region Fields

        [SerializeField] private OrderInfo _value;

        #endregion

        #region Properties

        public override OrderInfo Value
        {
            get => _value;
            set => _value = value;
        }

        #endregion

        #region Cloning

        public override PolymorphicValue<OrderInfo> Clone()
        {
            return new OrderConstantValue { _value = _value };
        }

        #endregion
    }

    [System.Serializable]
    public class OrderRuntimeVariableValue : OrderValue
    {
        #region Fields

        [SerializeField] private StringScriptableVariable _variableID;
        [SerializeField] private RuntimeEntityVariablesContainer _runtimeEntityVariablesContainer;

        private OrderEntityVariable _cachedVariable;

        #endregion

        #region Properties

        public override OrderInfo Value
        {
            get
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<OrderEntityVariable>(_variableID);

                if (_cachedVariable?.Value == null)
                    return null;

                return _cachedVariable.Value.Value;
            }
            set
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<OrderEntityVariable>(_variableID);

                if (_cachedVariable?.Value != null)
                    _cachedVariable.Value.Value = value;
            }
        }

        #endregion

        #region Clone

        public override PolymorphicValue<OrderInfo> Clone()
        {
            return new OrderRuntimeVariableValue
            {
                _variableID = _variableID,
                _runtimeEntityVariablesContainer = _runtimeEntityVariablesContainer
            };
        }

        #endregion
    }

    [System.Serializable]
    public class OrderAvailableContainerValue : OrderValue
    {
        #region Fields

        [SerializeField] private OrdersContainer _ordersContainer;
        [SerializeField] private bool _random;
        [HideIf(nameof(_random))]
        [SerializeField] private OrderInfo _order;

        #endregion

        #region Properties

        public override OrderInfo Value
        {
            get
            {
                if (_ordersContainer == null || _ordersContainer.Count == 0)
                    return null;

                if (_random)
                {
                    int index = Random.Range(0, _ordersContainer.Count);
                    return _ordersContainer.Orders[index];
                }

                return _ordersContainer.Contains(_order) ? _order : null;
            }
            set
            {
                if (_ordersContainer == null)
                    return;

                if (value != null)
                    _ordersContainer.Add(value);
            }
        }

        #endregion

        #region Clone

        public override PolymorphicValue<OrderInfo> Clone()
        {
            return new OrderAvailableContainerValue
            {
                _ordersContainer = _ordersContainer,
                _random = _random,
                _order = _order
            };
        }

        #endregion
    }
}
