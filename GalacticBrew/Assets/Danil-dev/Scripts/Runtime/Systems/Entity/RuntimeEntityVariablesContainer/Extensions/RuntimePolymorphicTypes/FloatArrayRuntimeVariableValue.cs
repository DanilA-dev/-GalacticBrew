using System;
using D_Dev.EntityVariable.Types;
using D_Dev.PolymorphicValueSystem;
using D_Dev.ScriptableVaiables;
using UnityEngine;

namespace D_Dev.RuntimeEntityVariables.Extensions
{
    [System.Serializable]
    public class FloatArrayRuntimeVariableValue : FloatArrayValue
    {
        #region Fields

        [SerializeField] private StringScriptableVariable _variableID;
        [SerializeField] private RuntimeEntityVariablesContainer _runtimeEntityVariablesContainer;
        
        private FloatArrayEntityVariable _cachedVariable;

        #endregion

        #region Properties

        public override float[] Value
        {
            get
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<FloatArrayEntityVariable>(_variableID);

                return _cachedVariable != null ? _cachedVariable.Value.Value : Array.Empty<float>();
            }
            set
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<FloatArrayEntityVariable>(_variableID);
                
                if (_cachedVariable != null)
                {
                    _cachedVariable.Value.Value = value;
                }
            }
        }

        #endregion

        #region Clone

        public override PolymorphicValue<float[]> Clone()
        {
            return new FloatArrayRuntimeVariableValue
            {
                _variableID = _variableID,
                _runtimeEntityVariablesContainer = _runtimeEntityVariablesContainer
            };
        }

        #endregion
    }
}
