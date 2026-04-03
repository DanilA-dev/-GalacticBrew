using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace D_Dev.AnimatorView.AnimationPlayableHandler
{
    public class AnimationClipPlayableMixer : BaseAnimationPlayableMixer
    {
        #region Fields

        [PropertyOrder(-1)]
        [SerializeField] private bool _connectToSeperateMixer;
        [PropertySpace(10)]
        [SerializeField] private bool _hasStartAnimations;
        [ShowIf(nameof(_hasStartAnimations))]
        [SerializeField] private AnimationPlayableClipConfig[] _onStartAnimations;

        private AnimationPlayableClipConfig _lastPlayedConfig;
        private AnimationLayerMixerPlayable _targetLayerMixerPlayable;

        private Dictionary<int, AnimationPlayablePairConfig> _playablesPair = new();
        private Dictionary<int, Stack<AnimationPlayablePairConfig>> _animationStack = new();
        private Dictionary<int, int> _blendInIds = new();
        private Dictionary<int, int> _blendOutIds = new();

        #endregion

        #region Monobehaviour

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_playableGraph?.PlayableGraph.IsValid() != true)
                return;

            _targetLayerMixerPlayable = _connectToSeperateMixer
                ? AnimationLayerMixerPlayable.Create(_playableGraph.PlayableGraph, 1)
                : _playableGraph.RootLayerMixer;

            if (_connectToSeperateMixer && _targetLayerMixerPlayable.IsValid())
            {
                _playableGraph.RootLayerMixer.ConnectInput(_layer, _targetLayerMixerPlayable, 0);
                _playableGraph.RootLayerMixer.SetInputWeight(_layer, 1f);
            }
        }

        private void Start()
        {
            if (_hasStartAnimations && _onStartAnimations.Length > 0)
                foreach (var animation in _onStartAnimations)
                    Play(animation);
        }

        private void OnDisable()
        {
            CancelAllBlends();

            if (_playableGraph?.PlayableGraph.IsValid() != true)
            {
                _playablesPair.Clear();
                _animationStack.Clear();
                return;
            }

            if (_targetLayerMixerPlayable.IsValid())
                foreach (var pair in _playablesPair)
                    _targetLayerMixerPlayable.DisconnectInput(pair.Key);

            foreach (var pair in _playablesPair)
                if (pair.Value.Playable.IsValid())
                    _playableGraph.PlayableGraph.DestroyPlayable(pair.Value.Playable);

            _playablesPair.Clear();
            _animationStack.Clear();

            if (_connectToSeperateMixer && _playableGraph.RootLayerMixer.IsValid())
                _playableGraph.RootLayerMixer.DisconnectInput(_layer);

            if (_connectToSeperateMixer && _targetLayerMixerPlayable.IsValid())
                _playableGraph.PlayableGraph.DestroyPlayable(_targetLayerMixerPlayable);
        }

        #endregion

        #region Public

        public void Play(AnimationPlayableClipConfig newPlayableConfig)
        {
            if (newPlayableConfig == null)
                return;

            if (_playableGraph?.PlayableGraph.IsValid() != true)
                return;

            if (!_targetLayerMixerPlayable.IsValid())
                return;

            if (_playablesPair.Count == 1 && _lastPlayedConfig == newPlayableConfig)
                return;

            if (_playablesPair.TryGetValue(newPlayableConfig.Layer + 1, out var oldPlayable))
            {
                if (oldPlayable.Playable.IsValid() &&
                    newPlayableConfig.GetAnimationClip() == oldPlayable.Playable.GetAnimationClip() &&
                    !newPlayableConfig.CanPlayIfSameClip)
                    return;

                if (oldPlayable.Playable.IsValid())
                    Stop(oldPlayable.ClipConfig);
            }

            int newAnimationLayer = newPlayableConfig.Layer + 1;
            var newAnimationPlayable = CreatePlayableClip(newPlayableConfig);

            if (newAnimationLayer >= _targetLayerMixerPlayable.GetInputCount())
                _targetLayerMixerPlayable.SetInputCount(newAnimationLayer + 1);

            _targetLayerMixerPlayable.DisconnectInput(newAnimationLayer);
            _targetLayerMixerPlayable.ConnectInput(newAnimationLayer, newAnimationPlayable, 0);

            if (newPlayableConfig.Mask != null)
                _targetLayerMixerPlayable.SetLayerMaskFromAvatarMask((uint)newAnimationLayer, newPlayableConfig.Mask);

            _playableGraph.RootLayerMixer.SetInputWeight(_layer, 1);
            _playableGraph.Animator.applyRootMotion = newPlayableConfig.ApplyRootMotion;
            _targetLayerMixerPlayable.SetLayerAdditive((uint)newAnimationLayer, newPlayableConfig.IsAdditive);

            var pairConfig = new AnimationPlayablePairConfig
                { ClipConfig = newPlayableConfig, Playable = newAnimationPlayable };

            _playablesPair[newAnimationLayer] = pairConfig;

            if (!_animationStack.ContainsKey(newAnimationLayer))
                _animationStack[newAnimationLayer] = new Stack<AnimationPlayablePairConfig>();
            _animationStack[newAnimationLayer].Push(pairConfig);

            ScheduleBlendIn(newPlayableConfig, newAnimationPlayable);
            if (!newPlayableConfig.IsLooping)
                ScheduleBlendOut(newPlayableConfig, newAnimationPlayable);

            _lastPlayedConfig = newPlayableConfig;
        }

        public void Stop(AnimationPlayableClipConfig config)
        {
            if (_playableGraph?.PlayableGraph.IsValid() != true)
                return;

            if (!_targetLayerMixerPlayable.IsValid())
                return;

            int layer = config.Layer + 1;
            CancelBlendsForLayer(layer);
            RestorePreviousLayerState(layer);
            DisconnectOneShot(config);
        }

        public IReadOnlyDictionary<int, AnimationPlayablePairConfig> GetActivePlayables() => _playablesPair;

        public bool TryGetActivePlayable(int layer, out AnimationPlayablePairConfig playablePair)
            => _playablesPair.TryGetValue(layer, out playablePair);

        public float GetLayerWeight(int layer)
        {
            if (!_targetLayerMixerPlayable.IsValid())
                return 0f;

            return _targetLayerMixerPlayable.GetInputWeight(layer);
        }

        public int GetActiveLayerCount() => _playablesPair.Count;
        
        #endregion

        #region Private

        private void ScheduleBlendIn(AnimationPlayableClipConfig config, AnimationClipPlayable playable)
        {
            int layer = config.Layer + 1;
            float crossFade = GetCrossFadeTime(config, playable.GetAnimationClip());
            float current = _targetLayerMixerPlayable.IsValid()
                ? _targetLayerMixerPlayable.GetInputWeight(layer)
                : 0f;

            CancelBlendIn(layer);

            if (AnimationPlayableBlendManager.Instance == null)
            {
                Debug.LogError("[AnimationPlayableBlendManager] = null");
                return;
            }

            _blendInIds[layer] = AnimationPlayableBlendManager.Instance.Schedule(
                _targetLayerMixerPlayable, layer, current, config.TargetWeight, crossFade);
        }

        private void ScheduleBlendOut(AnimationPlayableClipConfig config, AnimationClipPlayable playable)
        {
            int layer = config.Layer + 1;
            var clip = playable.GetAnimationClip();
            float crossFade = GetCrossFadeTime(config, clip);
            float actualDuration = clip.length / config.AnimationSpeed;
            float delay = config.UseAutoFadeTimeBasedOnClipLength
                ? actualDuration - crossFade
                : config.FadeDelay;

            CancelBlendOut(layer);

            if (AnimationPlayableBlendManager.Instance == null)
            {
                Debug.LogError("[AnimationPlayableBlendManager] = null");
                return;
            }

            _blendOutIds[layer] = AnimationPlayableBlendManager.Instance.Schedule(
                _targetLayerMixerPlayable, layer, config.TargetWeight, 0f, crossFade, delay,
                onComplete: () =>
                {
                    if (!config.IsLooping)
                    {
                        RestorePreviousLayerState(layer);
                        DisconnectOneShot(config);
                    }
                });
        }

        private void CancelBlendIn(int layer)
        {
            if (!_blendInIds.TryGetValue(layer, out int id))
                return;

            if (AnimationPlayableBlendManager.Instance == null)
            {
                Debug.LogError("[AnimationPlayableBlendManager] = null");
                return;
            }

            AnimationPlayableBlendManager.Instance.Cancel(id);
            _blendInIds.Remove(layer);
        }

        private void CancelBlendOut(int layer)
        {
            if (!_blendOutIds.TryGetValue(layer, out int id))
                return;

            if (AnimationPlayableBlendManager.Instance == null)
            {
                Debug.LogError("[AnimationPlayableBlendManager] = null");
                return;
            }

            AnimationPlayableBlendManager.Instance.Cancel(id);
            _blendOutIds.Remove(layer);
        }

        private void CancelBlendsForLayer(int layer)
        {
            CancelBlendIn(layer);
            CancelBlendOut(layer);
        }

        private void CancelAllBlends()
        {
            if (AnimationPlayableBlendManager.Instance == null)
            {
                Debug.LogError("[AnimationPlayableBlendManager] = null");
                return;
            }

            foreach (var id in _blendInIds.Values)
                AnimationPlayableBlendManager.Instance.Cancel(id);

            foreach (var id in _blendOutIds.Values)
                AnimationPlayableBlendManager.Instance.Cancel(id);

            _blendInIds.Clear();
            _blendOutIds.Clear();
        }

        private void RestorePreviousLayerState(int layer)
        {
            if (!_animationStack.TryGetValue(layer, out var stack) || stack.Count <= 1)
            {
                _targetLayerMixerPlayable.SetInputWeight(layer, 0);
                _playableGraph.RootLayerMixer.SetInputWeight(_layer, 1);
                return;
            }

            stack.Pop();
            if (stack.Count > 0)
            {
                var previous = stack.Peek();
                if (previous.Playable.IsValid() && previous.ClipConfig != null)
                {
                    _targetLayerMixerPlayable.SetInputWeight(layer, previous.ClipConfig.TargetWeight);
                    _playableGraph.RootLayerMixer.SetInputWeight(_layer, 1);
                }
            }
            else
            {
                _targetLayerMixerPlayable.SetInputWeight(layer, 0);
                _playableGraph.RootLayerMixer.SetInputWeight(_layer, 1);
            }
        }

        private void DisconnectOneShot(AnimationPlayableClipConfig config)
        {
            if (_playableGraph?.PlayableGraph.IsValid() != true)
                return;

            if (!_targetLayerMixerPlayable.IsValid())
                return;

            int layer = config.Layer + 1;

            if (!_playablesPair.TryGetValue(layer, out var playableConfig))
                return;

            if (!playableConfig.Playable.IsValid())
                return;

            if (_lastPlayedConfig == config)
                _lastPlayedConfig = null;

            _targetLayerMixerPlayable.DisconnectInput(layer);
            _playableGraph.PlayableGraph.DestroyPlayable(playableConfig.Playable);
            _playablesPair.Remove(layer);
        }

        private float GetCrossFadeTime(AnimationPlayableClipConfig config, AnimationClip clip) =>
            config.UseAutoFadeTimeBasedOnClipLength
                ? Mathf.Clamp(clip.length * 0.1f, 0.1f, clip.length * 0.5f)
                : config.CrossFadeTime;

        #endregion
    }
}
