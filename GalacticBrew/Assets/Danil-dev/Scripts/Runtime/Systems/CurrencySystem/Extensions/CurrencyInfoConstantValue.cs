using D_Dev.PolymorphicValueSystem;
using D_Dev.RuntimeEntityVariables;
using D_Dev.ScriptableVaiables;
using UnityEngine;

namespace D_Dev.CurrencySystem.Extensions
{
    [System.Serializable]
    public abstract class CurrencyInfoValue : PolymorphicValue<CurrencyInfo>{}
    
    [System.Serializable]
    public class CurrencyInfoConstantValue : ConstantValue<CurrencyInfo>
    {
        #region Clone

        public override PolymorphicValue<CurrencyInfo> Clone()
        {
            return new CurrencyInfoConstantValue() { _value = _value };
        }

        #endregion
    }

    [System.Serializable]
    public class CurrencyInfoRuntimeVariableValue : CurrencyInfoValue
    {
        #region Fields

        [SerializeField] private StringScriptableVariable _variableID;
        [SerializeField] private RuntimeEntityVariablesContainer _runtimeEntityVariablesContainer;
        
        private CurrencyInfoEntityVariable _cachedVariable;

        #endregion

        #region Properties

        public override CurrencyInfo Value
        {
            get
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<CurrencyInfoEntityVariable>(_variableID);

                return _cachedVariable != null ? _cachedVariable.Value : null;
            }
            set
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<CurrencyInfoEntityVariable>(_variableID);
                
                if (_cachedVariable != null)
                {
                    _cachedVariable.Value = value;
                }
            }
        }

        #endregion

        #region Clone

        public override PolymorphicValue<CurrencyInfo> Clone()
        {
            return new CurrencyInfoRuntimeVariableValue
            {
                _variableID = _variableID,
                _runtimeEntityVariablesContainer = _runtimeEntityVariablesContainer
            };
        }

        #endregion
    }
}