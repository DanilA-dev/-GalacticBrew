using System;
using D_Dev.EntityVariable.Types;
using D_Dev.PolymorphicValueSystem;
using D_Dev.ScriptableVaiables;
using UnityEngine;

namespace D_Dev.RuntimeEntityVariables.Extensions
{
    [System.Serializable]
    public class Vector3RuntimeVariableValue : Vector3Value
    {
        #region Fields

        [SerializeField] private StringScriptableVariable _variableID;
        [SerializeField] private RuntimeEntityVariablesContainer _runtimeEntityVariablesContainer;
        
        private Vector3EntityVariable _cachedVariable;

        #endregion

        #region Properties

        public override Vector3 Value
        {
            get
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<Vector3EntityVariable>(_variableID);
                
                return _cachedVariable != null ? _cachedVariable.Value.Value : Vector3.zero;
            }
            set
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<Vector3EntityVariable>(_variableID);
                
                if (_cachedVariable != null)
                {
                    _cachedVariable.Value.Value = value;
                }
            }
        }

        #endregion

        #region Clone

        public override PolymorphicValue<Vector3> Clone()
        {
            return new Vector3RuntimeVariableValue
            {
                _variableID = _variableID,
                _runtimeEntityVariablesContainer = _runtimeEntityVariablesContainer
            };
        }

        #endregion
    }
}
