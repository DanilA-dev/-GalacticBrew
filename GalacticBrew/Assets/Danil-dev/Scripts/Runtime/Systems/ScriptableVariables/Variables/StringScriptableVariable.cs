using UnityEngine;

namespace D_Dev.ScriptableVaiables
{
    [CreateAssetMenu(menuName = "D-Dev/Variables/StringVariable")]
    public class StringScriptableVariable : BaseScriptableVariable<string>
    {
        public override void ResetValue() => Value = null;
    }
    
}
