using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace D_Dev.PolymorphicValueSystem
{
    [System.Serializable]
    public abstract class PolymorphicValue<T>
    {
        #region Fields

        [SerializeField, PropertyOrder(999)] protected bool _showEvent;
        [ShowIf("_showEvent")]
        [SerializeField, PropertyOrder(999)] protected UnityEvent<T> OnValueChanged;

        #endregion
        
        #region Properties

        public abstract T Value { get; set; }

        #endregion

        #region Cloning

        public abstract PolymorphicValue<T> Clone();

        #endregion

        #region Overrides

        public virtual void Dispose() { }

        #endregion
    }
}
