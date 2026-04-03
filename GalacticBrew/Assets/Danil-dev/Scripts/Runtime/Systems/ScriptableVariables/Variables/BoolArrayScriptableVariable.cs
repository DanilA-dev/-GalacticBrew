using UnityEngine;

namespace D_Dev.ScriptableVaiables
{
    [CreateAssetMenu(menuName = "D-Dev/Variables/BoolArrayVariable")]
    public class BoolArrayScriptableVariable : BaseScriptableVariable<bool[]>
    {
        public override void ResetValue() => Value = null;
    }

}