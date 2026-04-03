using UnityEngine;

namespace D_Dev.ScriptableVaiables
{
    [CreateAssetMenu(menuName = "D-Dev/Variables/FloatArrayVariable")]
    public class FloatArrayScriptableVariable : BaseScriptableVariable<float[]>
    {
        public override void ResetValue() => Value = null;
    }

}