using System;
using System.Collections.Generic;
using D_Dev.EntityVariable;
using D_Dev.ScriptableVaiables;
using UnityEngine;

namespace D_Dev.RuntimeEntityVariables
{
    public class RuntimeEntityVariablesContainer : MonoBehaviour
    {
        #region Fields

        [SerializeField] private bool _initLocalVariablesOnStart;
        [SerializeReference] private List<BaseEntityVariable> _variables = new();

        private Dictionary<StringScriptableVariable, BaseEntityVariable> _variableMap = new();
        
        public event Action OnInitialized;
            
        #endregion

        #region Monobehaviour

        private void Start()
        {
            if (_initLocalVariablesOnStart)
            {
                foreach (var runtimeVariables in _variables)
                    _variableMap.TryAdd(runtimeVariables.VariableID, runtimeVariables);
                
                OnInitialized?.Invoke();
            }
        }

        #endregion

        #region Public

        public void Init(List<BaseEntityVariable> variablesFromInfo)
        {
            if(variablesFromInfo == null || variablesFromInfo.Count == 0)
                return;

            foreach (var runtimeVariables in _variables)
                _variableMap.TryAdd(runtimeVariables.VariableID, runtimeVariables);
            
            foreach (var variable in variablesFromInfo)
            {
                if(variable == null)
                    continue;
                
                var clonedVar = variable.Clone();
                _variables.Add(clonedVar);
                _variableMap.TryAdd(variable.VariableID, clonedVar);
            }
            OnInitialized?.Invoke();
        }

        public T GetVariable<T>(StringScriptableVariable variableID) where T : BaseEntityVariable
        {
            if (_variableMap.TryGetValue(variableID, out var variable))
            {
                return variable as T;
            }
        
            Debug.LogError($"Variable '{variableID}' not found in RuntimeEntityVariablesContainer on GameObject {gameObject.name}");
            return null;
        }

        public bool TryGetValue<T>(StringScriptableVariable variableID, out T value)
            where T : BaseEntityVariable
        {
            var variable = GetVariable<T>(variableID);
            if (variable == null)
            {
                value = null;
                return false;
            }

            var valueRaw = variable.GetValueRaw() as T;
            if(valueRaw == null)
            {
                value = null;
                return false;
            }
            value = valueRaw;
            return true;
        }
        
        public void SetValue<T>(StringScriptableVariable variableID, T value) where T : BaseEntityVariable
        {
            var variable = GetVariable<T>(variableID);
            if (variable != null)
                variable.SetValueRaw(value);
        }
        
        #endregion
    }
}