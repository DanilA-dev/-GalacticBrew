using D_Dev.ScriptableVaiables;
using UnityEngine;

namespace D_Dev.PolymorphicValueSystem
{
    [System.Serializable]
    public abstract class ScriptableVariableValue<TVariable, T> : PolymorphicValue<T>
        where TVariable : BaseScriptableVariable<T>
    {
        #region Fields

        [SerializeField] protected TVariable _variable;

        #endregion

        #region Properties

        public override T Value
        {
            get
            {
                if (_variable == null)
                    return default;
                
                return _variable.Value;
            }
            set
            {
                if(_variable == null)
                    return;
                
                _variable.Value = value;
                OnValueChanged?.Invoke(_variable.Value);
            }
        }

        #endregion
    }
}