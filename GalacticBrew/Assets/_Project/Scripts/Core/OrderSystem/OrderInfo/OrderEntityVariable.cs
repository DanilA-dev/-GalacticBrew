using D_Dev.EntityVariable;
using D_Dev.EntityVariable.Types;
using D_Dev.PolymorphicValueSystem;
using D_Dev.ScriptableVaiables;

namespace _Project.Scripts.Core.OrderSystem
{
    [System.Serializable]
    public class OrderEntityVariable : PolymorphicEntityVariable<PolymorphicValue<OrderInfo>>
    {
        #region Constructors

        public OrderEntityVariable() {}

        public OrderEntityVariable(StringScriptableVariable id, PolymorphicValue<OrderInfo> value) : base(id, value) {}

        #endregion

        #region Overrides

        public override BaseEntityVariable Clone()
        {
            return new OrderEntityVariable(_variableID, _value?.Clone());
        }

        #endregion
    }
}
