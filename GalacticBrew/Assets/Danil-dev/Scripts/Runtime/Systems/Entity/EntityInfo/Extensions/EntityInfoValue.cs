using D_Dev.PolymorphicValueSystem;
using D_Dev.RuntimeEntityVariables;
using D_Dev.ScriptableVaiables;
using UnityEngine;

namespace D_Dev.Entity.Extensions
{
    [System.Serializable]
    public abstract class EntityInfoValue : PolymorphicValue<EntityInfo> {}

    [System.Serializable]
    public sealed class EntityInfoConstantValue : EntityInfoValue
    {
        #region Fields

        [SerializeField] private EntityInfo _value;

        #endregion

        #region Properties

        public override EntityInfo Value
        {
            get => _value;
            set => _value = value;
        }

        #endregion

        #region Cloning

        public override PolymorphicValue<EntityInfo> Clone()
        {
            return new EntityInfoConstantValue { _value = _value };
        }

        #endregion
    }

    [System.Serializable]
    public class EntityInfoRuntimeVariableValue : EntityInfoValue
    {
        #region Fields

        [SerializeField] private StringScriptableVariable _variableID;
        [SerializeField] private RuntimeEntityVariablesContainer _runtimeEntityVariablesContainer;
        
        private EntityInfoEntityVariable _cachedVariable;

        #endregion

        #region Properties

        public override EntityInfo Value
        {
            get
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<EntityInfoEntityVariable>(_variableID);
                
                if (_cachedVariable == null)
                    return null;
                
                return _cachedVariable.Value;
            }
            set
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<EntityInfoEntityVariable>(_variableID);
                
                if (_cachedVariable != null)
                    _cachedVariable.Value = value;
            }
        }

        #endregion

        #region Clone

        public override PolymorphicValue<EntityInfo> Clone()
        {
            return new EntityInfoRuntimeVariableValue
            {
                _variableID = _variableID,
                _runtimeEntityVariablesContainer = _runtimeEntityVariablesContainer
            };
        }

        #endregion
    }
}