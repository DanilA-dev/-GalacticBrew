using UnityEngine;

namespace D_Dev.ScriptableVaiables
{
    [CreateAssetMenu(menuName = "D-Dev/Variables/IntArrayVariable")]
    public class IntArrayScriptableVariable : BaseScriptableVariable<int[]>
    {
        public override void ResetValue() => Value = null;
    }

}