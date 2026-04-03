using UnityEngine;

namespace D_Dev.ScriptableVaiables
{
    [CreateAssetMenu(menuName = "D-Dev/Variables/CanvasVariable")]
    public class CanvasScriptableVariable : BaseScriptableVariable<Canvas>
    {
        public override void ResetValue() => Value = null;
    }
}