using UnityEngine;

namespace D_Dev.PolymorphicValueSystem
{
    [System.Serializable]
    public abstract class ConstantValue<T> : PolymorphicValue<T>
    {
        #region Fields

        [SerializeField] protected T _value;

        #endregion

        #region Properties

        public override T Value
        {
            get => _value;
            set
            {
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }

        #endregion
    }
}