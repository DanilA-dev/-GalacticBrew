using D_Dev.DamageableSystem;
using D_Dev.EntityVariable;
using D_Dev.PolymorphicValueSystem;
using D_Dev.RuntimeEntityVariables;
using D_Dev.ScriptableVaiables;
using UnityEngine;

namespace D_Dev.Extensions
{
    [System.Serializable]
    public abstract class DamageDataValue : PolymorphicValue<DamageData> {}

    [System.Serializable]
    public class DamageDataConstantValue : ConstantValue<DamageData>
    {
        #region Clone

        public override PolymorphicValue<DamageData> Clone()
        {
            return new DamageDataConstantValue { _value = Value };
        }

        #endregion
    }
    
    [System.Serializable]
    public class DamageDataRuntimeVariableValue : DamageDataValue
    {
        #region Fields

        [SerializeField] private StringScriptableVariable _variableID;
        [SerializeField] private RuntimeEntityVariablesContainer _runtimeEntityVariablesContainer;
        
        private DamageDataEntityVariable _cachedVariable;

        #endregion

        #region Properties

        public override DamageData Value
        {
            get
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<DamageDataEntityVariable>(_variableID);
                
                if (_cachedVariable == null)
                    return new DamageData();
                
                return _cachedVariable.Value;
            }
            set
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<DamageDataEntityVariable>(_variableID);
                
                if (_cachedVariable != null)
                    _cachedVariable.Value = value;
            }
        }

        #endregion

        #region Clone

        public override PolymorphicValue<DamageData> Clone()
        {
            return new DamageDataRuntimeVariableValue
            {
                _variableID = _variableID,
                _runtimeEntityVariablesContainer = _runtimeEntityVariablesContainer
            };
        }

        #endregion
    }

    [System.Serializable]
    public class DamageDataEntityVariable : EntityVariable<DamageData>
    {
        #region Constructors

        public DamageDataEntityVariable() {}
        
        public DamageDataEntityVariable(StringScriptableVariable id, DamageData value) : base(id, value) {}

        #endregion
        
        #region Overrides
        
        public override BaseEntityVariable Clone()
        {
            return new DamageDataEntityVariable(_variableID, _value);
        }
        
        #endregion
    }

   
}