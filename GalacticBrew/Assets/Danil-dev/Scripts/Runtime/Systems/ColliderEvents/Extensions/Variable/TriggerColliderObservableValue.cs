using D_Dev.ColliderEvents.Extensions.ScriptableVariables;
using D_Dev.EntityVariable;
using D_Dev.PolymorphicValueSystem;
using D_Dev.RuntimeEntityVariables;
using D_Dev.ScriptableVaiables;
using UnityEngine;

namespace D_Dev.ColliderEvents.Extensions
{
    [System.Serializable]
    public abstract class TriggerColliderObservableValue : PolymorphicValue<TriggerColliderObservable> {}

    [System.Serializable]
    public sealed class TriggerColliderObservableConstantValue : TriggerColliderObservableValue
    {
        #region Fields

        [SerializeField] private TriggerColliderObservable _value;

        #endregion

        #region Properties

        public override TriggerColliderObservable Value
        {
            get => _value;
            set => _value = value;
        }

        #endregion

        #region Cloning

        public override PolymorphicValue<TriggerColliderObservable> Clone()
        {
            return new TriggerColliderObservableConstantValue { _value = _value };
        }

        #endregion
    }

    [System.Serializable]
    public sealed class TriggerColliderObservableScriptableVariableValue : TriggerColliderObservableValue
    {
        #region Fields

        [SerializeField] private TriggerColliderObservableScriptableVariable _variable;

        #endregion

        #region Properties

        public override TriggerColliderObservable Value
        {
            get => _variable?.Value;
            set
            {
                if (_variable != null)
                    _variable.Value = value;
            }
        }

        #endregion

        #region Cloning

        public override PolymorphicValue<TriggerColliderObservable> Clone()
        {
            return new TriggerColliderObservableScriptableVariableValue { _variable = _variable };
        }

        #endregion
    }

    [System.Serializable]
    public sealed class TriggerColliderObservableRuntimeVariableValue : TriggerColliderObservableValue
    {
        #region Fields

        [SerializeField] private StringScriptableVariable _variableID;
        [SerializeField] private RuntimeEntityVariablesContainer _runtimeEntityVariablesContainer;
        
        private TriggerColliderObservableEntityVariable _cachedVariable;

        #endregion

        #region Properties

        public override TriggerColliderObservable Value
        {
            get
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<TriggerColliderObservableEntityVariable>(_variableID);
                
                if (_cachedVariable == null)
                    return null;
                
                return _cachedVariable.Value;
            }
            set
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<TriggerColliderObservableEntityVariable>(_variableID);
                
                if (_cachedVariable != null)
                    _cachedVariable.Value = value;
            }
        }

        #endregion

        #region Cloning

        public override PolymorphicValue<TriggerColliderObservable> Clone()
        {
            return new TriggerColliderObservableRuntimeVariableValue
            {
                _variableID = _variableID,
                _runtimeEntityVariablesContainer = _runtimeEntityVariablesContainer
            };
        }

        #endregion
    }

    [System.Serializable]
    public class TriggerColliderObservableEntityVariable : EntityVariable<TriggerColliderObservable>
    {
        #region Constructors

        public TriggerColliderObservableEntityVariable() {}
        
        public TriggerColliderObservableEntityVariable(StringScriptableVariable id, TriggerColliderObservable value) : base(id, value) {}

        #endregion
        
        #region Overrides
        
        public override BaseEntityVariable Clone()
        {
            return new TriggerColliderObservableEntityVariable(_variableID, _value);
        }
        
        #endregion
    }
}
