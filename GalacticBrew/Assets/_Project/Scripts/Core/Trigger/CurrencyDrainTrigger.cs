using System;
using System.Collections;
using D_Dev.ColliderEvents;
using D_Dev.CoroutineManagerSystem;
using D_Dev.CurrencySystem;
using D_Dev.CurrencySystem.Extensions;
using D_Dev.PolymorphicValueSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Core.Trigger
{
    [RequireComponent(typeof(TriggerColliderObservable))]
    public class CurrencyDrainTrigger : MonoBehaviour
    {
        #region Fields

        [Title("Cost")]
        [SerializeReference] protected PolymorphicValue<int> _targetCost = new IntConstantValue();

        [Title("Payment Settings")]
        [SerializeField, Min(0f)] private float _startDelay = 0f;
        [SerializeField] private int _currencyPayRate = 5;
        [SerializeField, Min(0.01f)] private float _payInterval = 0.1f;
        [SerializeField, Min(0f)] private float _refundTimeout = 1f;

        [Title("Pay Rate Acceleration")]
        [SerializeField] private float _accelerationInterval = 2f;
        [SerializeField] private float _accelerationMultiplier = 2f;
        [SerializeField] private int _maxPayRate = 200;

        [Title("Dependencies")]
        [SerializeField] private CurrencyInfoSetter _currencySetter;
        [SerializeField] private CurrencyEntitySpawner _currencyEntitySpawner;
        [SerializeField] private TriggerColliderObservable _trigger;

        [FoldoutGroup("Events")]
        public UnityEvent OnDrainStart;
        [FoldoutGroup("Events")]
        public UnityEvent<float> OnDrainProgress;
        [FoldoutGroup("Events")]
        public UnityEvent<int> OnRemainingCost;
        [FoldoutGroup("Events")]
        public UnityEvent OnDrainComplete;
        [FoldoutGroup("Events")]
        public UnityEvent OnDrainFail;
        [FoldoutGroup("Events")]
        public UnityEvent<int> OnRefunded;

        private int _currentCurrencyAdded;
        private int _currentPayRate;
        private bool _isDraining;
        private bool _isInTrigger;
        private bool _isComplete;
        private bool _isPaused;

        private Coroutine _drainCoroutine;
        private Coroutine _refundCoroutine;

        #endregion

        #region Properties

        public int CurrentCurrencyAdded => _currentCurrencyAdded;
        public int TargetCost => _targetCost.Value;
        public bool IsComplete => _isComplete;
        public bool IsPaused => _isPaused;
        public float Progress => TargetCost <= 0 ? 1f : Mathf.Clamp01((float)_currentCurrencyAdded / TargetCost);

        #endregion

        #region Monobehaviour

        private void Awake()
        {
            if (_trigger == null)
                _trigger = GetComponent<TriggerColliderObservable>();

            _trigger?.OnEnter.AddListener(OnTriggerEnterHandler);
            _trigger?.OnExit.AddListener(OnTriggerExitHandler);
        }

        private void Start()
        {
            OnRemainingCost?.Invoke(TargetCost);
        }

        private void OnDestroy()
        {
            _trigger?.OnEnter.RemoveListener(OnTriggerEnterHandler);
            _trigger?.OnExit.RemoveListener(OnTriggerExitHandler);
            
            if (_drainCoroutine != null)
                CoroutineManager.Stop(_drainCoroutine);
            if (_refundCoroutine != null)
                CoroutineManager.Stop(_refundCoroutine);
        }

        #endregion

        #region Public

        public void ResetTrigger()
        {
            _currentCurrencyAdded = 0;
            _isComplete = false;
        }

        public void Pause() => _isPaused = true;

        public void Resume() => _isPaused = false;

        #endregion

        #region Listeners

        private void OnTriggerEnterHandler(Collider other)
        {
            if (_isComplete)
                return;

            _isInTrigger = true;
            StartDrain();
        }

        private void OnTriggerExitHandler(Collider other)
        {
            _isInTrigger = false;
            StopDrain();
        }

        #endregion

        #region Private

        private void StartDrain()
        {
            if (_isDraining || _isComplete)
                return;

            _isDraining = true;
            _currentPayRate = _currencyPayRate;

            if (_refundCoroutine != null)
            {
                CoroutineManager.Stop(_refundCoroutine);
                _refundCoroutine = null;
            }

            OnDrainStart?.Invoke();
            _drainCoroutine = CoroutineManager.Run(DrainRoutine());
        }

        private void StopDrain()
        {
            if (!_isDraining)
                return;

            _isDraining = false;
            _currentPayRate = _currencyPayRate;

            if (_drainCoroutine != null)
            {
                CoroutineManager.Stop(_drainCoroutine);
                _drainCoroutine = null;
            }

            if (_isComplete)
                return;

            if (_refundCoroutine != null)
                CoroutineManager.Stop(_refundCoroutine);
            _refundCoroutine = CoroutineManager.Run(RefundRoutine());
        }

        private IEnumerator DrainRoutine()
        {
            if (_startDelay > 0f)
            {
                float delayElapsed = 0f;
                while (delayElapsed < _startDelay)
                {
                    yield return null;
                    if (!_isDraining)
                        yield break;
                    if (_isPaused)
                        continue;
                    delayElapsed += Time.deltaTime;
                }
            }

            float timeDraining = 0f;
            float nextAccelerationAt = _accelerationInterval;

            while (_currentCurrencyAdded < TargetCost)
            {
                yield return CoroutineManager.Wait(_payInterval);

                if (!_isDraining)
                    yield break;

                while (_isPaused)
                {
                    yield return null;
                    if (!_isDraining)
                        yield break;
                }

                timeDraining += _payInterval;

                if (timeDraining >= nextAccelerationAt)
                {
                    _currentPayRate = Mathf.Min(
                        Mathf.RoundToInt(_currentPayRate * _accelerationMultiplier),
                        _maxPayRate);
                    nextAccelerationAt += _accelerationInterval;
                }

                int remainingCost = TargetCost - _currentCurrencyAdded;
                OnRemainingCost?.Invoke(remainingCost);
                int payAmount = Mathf.Min(_currentPayRate, remainingCost);
                if (payAmount <= 0)
                    continue;

                if (_currencySetter.TryWithdrawValue(payAmount))
                {
                    _currentCurrencyAdded += payAmount;
                    OnDrainProgress?.Invoke(Progress);
                }
                else
                {
                    _isDraining = false;
                    OnDrainFail?.Invoke();
                    yield break;
                }
            }

            _isDraining = false;
            _isComplete = true;
            OnRemainingCost?.Invoke(0);
            OnDrainComplete?.Invoke();
        }

        private IEnumerator RefundRoutine()
        {
            int refundAmount = _currentCurrencyAdded;
            if (refundAmount <= 0)
                yield break;

            yield return CoroutineManager.Wait(_refundTimeout);

            if (_isDraining || _isInTrigger || _currentCurrencyAdded != refundAmount)
                yield break;

            OnRefunded?.Invoke(refundAmount);
            OnRemainingCost?.Invoke(TargetCost);
            _currencyEntitySpawner?.SpawnCurrencies(refundAmount);
            _currentCurrencyAdded = 0;
        }

        #endregion
    }
}
