using D_Dev.ColliderEvents.Extensions.ScriptableVariables;
using D_Dev.EntityVariable;
using D_Dev.PolymorphicValueSystem;
using D_Dev.RuntimeEntityVariables;
using D_Dev.ScriptableVaiables;
using UnityEngine;

namespace D_Dev.ColliderEvents.Extensions
{
    [System.Serializable]
    public abstract class CollisionColliderObservableValue : PolymorphicValue<CollisionColliderObservable> {}

    [System.Serializable]
    public sealed class CollisionColliderObservableConstantValue : CollisionColliderObservableValue
    {
        #region Fields

        [SerializeField] private CollisionColliderObservable _value;

        #endregion

        #region Properties

        public override CollisionColliderObservable Value
        {
            get => _value;
            set => _value = value;
        }

        #endregion

        #region Cloning

        public override PolymorphicValue<CollisionColliderObservable> Clone()
        {
            return new CollisionColliderObservableConstantValue { _value = _value };
        }

        #endregion
    }

    [System.Serializable]
    public sealed class CollisionColliderObservableScriptableVariableValue : CollisionColliderObservableValue
    {
        #region Fields

        [SerializeField] private CollisionColliderObservableScriptableVariable _variable;

        #endregion

        #region Properties

        public override CollisionColliderObservable Value
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

        public override PolymorphicValue<CollisionColliderObservable> Clone()
        {
            return new CollisionColliderObservableScriptableVariableValue { _variable = _variable };
        }

        #endregion
    }

    [System.Serializable]
    public sealed class CollisionColliderObservableRuntimeVariableValue : CollisionColliderObservableValue
    {
        #region Fields

        [SerializeField] private StringScriptableVariable _variableID;
        [SerializeField] private RuntimeEntityVariablesContainer _runtimeEntityVariablesContainer;
        
        private CollisionColliderObservableEntityVariable _cachedVariable;

        #endregion

        #region Properties

        public override CollisionColliderObservable Value
        {
            get
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<CollisionColliderObservableEntityVariable>(_variableID);
                
                if (_cachedVariable == null)
                    return null;
                
                return _cachedVariable.Value;
            }
            set
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<CollisionColliderObservableEntityVariable>(_variableID);
                
                if (_cachedVariable != null)
                    _cachedVariable.Value = value;
            }
        }

        #endregion

        #region Cloning

        public override PolymorphicValue<CollisionColliderObservable> Clone()
        {
            return new CollisionColliderObservableRuntimeVariableValue
            {
                _variableID = _variableID,
                _runtimeEntityVariablesContainer = _runtimeEntityVariablesContainer
            };
        }

        #endregion
    }

    [System.Serializable]
    public class CollisionColliderObservableEntityVariable : EntityVariable<CollisionColliderObservable>
    {
        #region Constructors

        public CollisionColliderObservableEntityVariable() {}
        
        public CollisionColliderObservableEntityVariable(StringScriptableVariable id, CollisionColliderObservable value) : base(id, value) {}

        #endregion
        
        #region Overrides
        
        public override BaseEntityVariable Clone()
        {
            return new CollisionColliderObservableEntityVariable(_variableID, _value);
        }
        
        #endregion
    }
}
