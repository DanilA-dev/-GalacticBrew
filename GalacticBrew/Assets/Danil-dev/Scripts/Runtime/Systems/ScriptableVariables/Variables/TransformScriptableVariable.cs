using UnityEngine;

namespace D_Dev.ScriptableVaiables
{
    [CreateAssetMenu(menuName = "D-Dev/Variables/TransformVariable")]
    public class TransformScriptableVariable : BaseScriptableVariable<Transform>
    {
        public override void ResetValue() => Value = null;
    }
}