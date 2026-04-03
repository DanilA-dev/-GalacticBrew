using System;
using D_Dev.EntityVariable.Types;
using D_Dev.PolymorphicValueSystem;
using D_Dev.ScriptableVaiables;
using UnityEngine;

namespace D_Dev.RuntimeEntityVariables.Extensions
{
    [System.Serializable]
    public class GameObjectArrayRuntimeVariableValue : GameObjectArrayValue
    {
        #region Fields

        [SerializeField] private StringScriptableVariable _variableID;
        [SerializeField] private RuntimeEntityVariablesContainer _runtimeEntityVariablesContainer;
        
        private GameObjectArrayEntityVariable _cachedVariable;

        #endregion

        #region Properties

        public override GameObject[] Value
        {
            get
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<GameObjectArrayEntityVariable>(_variableID);
                
                return _cachedVariable != null ? _cachedVariable.Value.Value : Array.Empty<GameObject>();
            }
            set
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<GameObjectArrayEntityVariable>(_variableID);
                
                if (_cachedVariable != null)
                {
                    _cachedVariable.Value.Value = value;
                }
            }
        }

        #endregion

        #region Clone

        public override PolymorphicValue<GameObject[]> Clone()
        {
            return new GameObjectArrayRuntimeVariableValue
            {
                _variableID = _variableID,
                _runtimeEntityVariablesContainer = _runtimeEntityVariablesContainer
            };
        }

        #endregion
    }
}
