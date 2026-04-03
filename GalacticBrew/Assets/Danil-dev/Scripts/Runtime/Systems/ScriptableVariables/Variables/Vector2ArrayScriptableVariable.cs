using UnityEngine;

namespace D_Dev.ScriptableVaiables
{
    [CreateAssetMenu(menuName = "D-Dev/Variables/Vector2ArrayVariable")]
    public class Vector2ArrayScriptableVariable : BaseScriptableVariable<Vector2[]>
    {
        public override void ResetValue() => Value = null;
    }

}