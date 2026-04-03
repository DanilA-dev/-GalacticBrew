using UnityEngine;

namespace D_Dev.ScriptableVaiables
{
    [CreateAssetMenu(menuName = "D-Dev/Variables/GameObjectArrayVariable")]
    public class GameObjectArrayScriptableVariable : BaseScriptableVariable<GameObject[]>
    {
        public override void ResetValue() => Value = null;
    }

}