using System;
using D_Dev.EntityVariable.Types;
using D_Dev.PolymorphicValueSystem;
using D_Dev.ScriptableVaiables;
using UnityEngine;

namespace D_Dev.RuntimeEntityVariables.Extensions
{
    [System.Serializable]
    public class DoubleRuntimeVariableValue : DoubleValue
    {
        #region Fields

        [SerializeField] private StringScriptableVariable _variableID;
        [SerializeField] private RuntimeEntityVariablesContainer _runtimeEntityVariablesContainer;
        
        private DoubleEntityVariable _cachedVariable;

        #endregion

        #region Properties

        public override double Value
        {
            get
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<DoubleEntityVariable>(_variableID);
                
                return _cachedVariable != null ? _cachedVariable.Value.Value : 0.0;
            }
            set
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<DoubleEntityVariable>(_variableID);
                
                if (_cachedVariable != null)
                {
                    _cachedVariable.Value.Value = value;
                }
            }
        }

        #endregion

        #region Clone

        public override PolymorphicValue<double> Clone()
        {
            return new DoubleRuntimeVariableValue
            {
                _variableID = _variableID,
                _runtimeEntityVariablesContainer = _runtimeEntityVariablesContainer
            };
        }

        #endregion
    }
}
