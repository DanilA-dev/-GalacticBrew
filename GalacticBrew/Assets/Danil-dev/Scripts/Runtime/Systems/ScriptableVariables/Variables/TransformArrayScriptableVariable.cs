using UnityEngine;

namespace D_Dev.ScriptableVaiables
{
    [CreateAssetMenu(menuName = "D-Dev/Variables/TransformArrayVariable")]
    public class TransformArrayScriptableVariable : BaseScriptableVariable<Transform[]>
    {
        public override void ResetValue() => Value = null;
    }

}