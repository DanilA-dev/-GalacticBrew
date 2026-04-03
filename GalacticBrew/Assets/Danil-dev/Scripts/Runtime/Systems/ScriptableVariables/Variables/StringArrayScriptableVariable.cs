using UnityEngine;

namespace D_Dev.ScriptableVaiables
{
    [CreateAssetMenu(menuName = "D-Dev/Variables/StringArrayVariable")]
    public class StringArrayScriptableVariable : BaseScriptableVariable<string[]>
    {
        public override void ResetValue() => Value = null;
    }

}