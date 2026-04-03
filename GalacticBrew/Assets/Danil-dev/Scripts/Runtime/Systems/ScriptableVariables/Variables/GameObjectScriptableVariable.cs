using UnityEngine;

namespace D_Dev.ScriptableVaiables
{
    [CreateAssetMenu(menuName = "D-Dev/Variables/GameObjectVariable")]
    public class GameObjectScriptableVariable : BaseScriptableVariable<GameObject>
    {
        public override void ResetValue() => Value = null;
    }
    
}