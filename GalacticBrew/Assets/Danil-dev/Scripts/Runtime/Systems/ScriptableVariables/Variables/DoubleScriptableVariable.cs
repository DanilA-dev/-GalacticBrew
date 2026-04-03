using UnityEngine;

namespace D_Dev.ScriptableVaiables
{
    [CreateAssetMenu(menuName = "D-Dev/Variables/DoubleVariable")]
    public class DoubleScriptableVariable : BaseScriptableVariable<double>
    {
        public override void ResetValue() => Value = 0.0;
    }
}