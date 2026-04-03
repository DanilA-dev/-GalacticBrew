using UnityEngine;

namespace D_Dev.ScriptableVaiables
{
    [CreateAssetMenu(menuName = "D-Dev/Variables/DoubleArrayVariable")]
    public class DoubleArrayScriptableVariable : BaseScriptableVariable<double[]>
    {
        public override void ResetValue() => Value = null;
    }

}