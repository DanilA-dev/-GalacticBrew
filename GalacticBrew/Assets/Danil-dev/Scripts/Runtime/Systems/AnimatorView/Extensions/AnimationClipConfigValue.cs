using D_Dev.PolymorphicValueSystem;
using D_Dev.RuntimeEntityVariables;
using D_Dev.ScriptableVaiables;
using UnityEngine;

namespace D_Dev.AnimatorView.Extensions
{
    [System.Serializable]
    public abstract class AnimationClipConfigValue : PolymorphicValue<AnimationClipConfig> {}

    [System.Serializable]
    public sealed class AnimationClipConfigConstantValue : AnimationClipConfigValue
    {
        #region Fields

        [SerializeField] private AnimationClipConfig _value;

        #endregion

        #region Properties

        public override AnimationClipConfig Value
        {
            get => _value;
            set => _value = value;
        }

        #endregion

        #region Cloning

        public override PolymorphicValue<AnimationClipConfig> Clone()
        {
            return new AnimationClipConfigConstantValue { _value = _value };
        }

        #endregion
    }

    [System.Serializable]
    public class AnimationClipConfigRuntimeVariableValue : AnimationClipConfigValue
    {
        #region Fields

        [SerializeField] private StringScriptableVariable _variableID;
        [SerializeField] private RuntimeEntityVariablesContainer _runtimeEntityVariablesContainer;
        
        private AnimationClipConfigEntityVariable _cachedVariable;

        #endregion

        #region Properties

        public override AnimationClipConfig Value
        {
            get
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<AnimationClipConfigEntityVariable>(_variableID);
                
                if (_cachedVariable == null)
                    return null;
                
                return _cachedVariable.Value;
            }
            set
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<AnimationClipConfigEntityVariable>(_variableID);
                
                if (_cachedVariable != null)
                    _cachedVariable.Value = value;
            }
        }

        #endregion

        #region Clone

        public override PolymorphicValue<AnimationClipConfig> Clone()
        {
            return new AnimationClipConfigRuntimeVariableValue
            {
                _variableID = _variableID,
                _runtimeEntityVariablesContainer = _runtimeEntityVariablesContainer
            };
        }
        #endregion
    }
}