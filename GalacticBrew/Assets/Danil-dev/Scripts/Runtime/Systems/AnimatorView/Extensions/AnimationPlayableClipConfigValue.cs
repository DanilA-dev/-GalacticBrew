using D_Dev.AnimatorView.AnimationPlayableHandler;
using D_Dev.PolymorphicValueSystem;
using D_Dev.RuntimeEntityVariables;
using D_Dev.ScriptableVaiables;
using UnityEngine;

namespace D_Dev.AnimatorView.Extensions
{
    [System.Serializable]
    public abstract class AnimationPlayableClipConfigValue : PolymorphicValue<AnimationPlayableClipConfig> {}

    [System.Serializable]
    public sealed class AnimationPlayableClipConfigConstantValue : AnimationPlayableClipConfigValue
    {
        #region Fields

        [SerializeField] private AnimationPlayableClipConfig _value;

        #endregion

        #region Properties

        public override AnimationPlayableClipConfig Value
        {
            get => _value;
            set => _value = value;
        }

        #endregion

        #region Cloning

        public override PolymorphicValue<AnimationPlayableClipConfig> Clone()
        {
            return new AnimationPlayableClipConfigConstantValue { _value = _value };
        }

        #endregion
    }

    [System.Serializable]
    public class AnimationPlayableClipConfigRuntimeVariableValue : AnimationPlayableClipConfigValue
    {
        #region Fields

        [SerializeField] private StringScriptableVariable _variableID;
        [SerializeField] private RuntimeEntityVariablesContainer _runtimeEntityVariablesContainer;
        
        private AnimationPlayableClipConfigEntityVariable _cachedVariable;

        #endregion

        #region Properties

        public override AnimationPlayableClipConfig Value
        {
            get
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<AnimationPlayableClipConfigEntityVariable>(_variableID);
                
                if (_cachedVariable == null)
                    return null;
                
                return _cachedVariable.Value;
            }
            set
            {
                if (_cachedVariable == null)
                    _cachedVariable = _runtimeEntityVariablesContainer?.GetVariable<AnimationPlayableClipConfigEntityVariable>(_variableID);
                
                if (_cachedVariable != null)
                    _cachedVariable.Value = value;
            }
        }

        #endregion

        #region Clone

        public override PolymorphicValue<AnimationPlayableClipConfig> Clone()
        {
            return new AnimationPlayableClipConfigRuntimeVariableValue
            {
                _variableID = _variableID,
                _runtimeEntityVariablesContainer = _runtimeEntityVariablesContainer
            };
        }
        #endregion
    }
}