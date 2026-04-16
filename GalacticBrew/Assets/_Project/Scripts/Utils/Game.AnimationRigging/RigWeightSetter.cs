using System;
using D_Dev.UpdateManagerSystem;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace _Project.Scripts.Utils.Game.AnimationRigging
{
    public class RigWeightSetter : MonoBehaviour, ILateTickable
    {
        #region Fields

        [SerializeField] private Rig _rig;

        private float? _pendingValue;

        #endregion

        #region Monobehaviour

        private void OnEnable()
        {
            LateUpdateManager.Add(this);
        }

        private void OnDisable()
        {
            LateUpdateManager.Remove(this);
        }

        #endregion
        
        #region Public

        public void SetWeight(float value) => _pendingValue = value;

        #endregion
        
        #region ITickable

        public void LateTick()
        {
            if (_pendingValue.HasValue)
            {
                _rig.weight = _pendingValue.Value;
            }
        }

        #endregion
        
    }
}