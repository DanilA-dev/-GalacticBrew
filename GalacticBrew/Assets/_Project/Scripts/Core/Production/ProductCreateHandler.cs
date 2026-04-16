using System.Collections;
using Cysharp.Threading.Tasks;
using D_Dev.CoroutineManagerSystem;
using D_Dev.EntitySpawner;
using D_Dev.PolymorphicValueSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Core.Production
{
    public class ProductCreateHandler : MonoBehaviour
    {
        #region Fields

        [Title("Production")]
        [SerializeField] private EntitySpawnSettings _spawnSettings;
        [SerializeField] private ProductContainer _outputContainer;
        [SerializeReference] private PolymorphicValue<float> _productTimeToMakeValue = new FloatConstantValue();
        [SerializeReference] private PolymorphicValue<int> _productMakeCount = new IntConstantValue();

        [Title("Settings")]
        [SerializeField] private bool _createOnInit;

        [FoldoutGroup("Events")]
        public UnityEvent OnProductionStarted;
        [FoldoutGroup("Events")]
        public UnityEvent OnProductionStopped;
        [FoldoutGroup("Events")]
        public UnityEvent OnProductionPaused;
        [FoldoutGroup("Events")]
        public UnityEvent<GameObject> OnProductCreated;
        [FoldoutGroup("Events")]
        public UnityEvent<float> OnProductCreateProgress;
        [FoldoutGroup("Events")]
        public UnityEvent OnOutputFull;

        private Coroutine _productionCoroutine;
        private bool _isRunning;
        private bool _isPaused;
        private bool _initialized;

        #endregion

        #region Properties

        public bool IsRunning => _isRunning;
        public bool IsPaused => _isPaused;
        public bool IsOutputFull => _outputContainer.IsStackFull;

        #endregion

        #region Monobehaviour

        private async void Start()
        {
            await InitAsync();
        }

        private void OnDestroy()
        {
            StopInternal();
            _spawnSettings?.DisposePool();
        }

        #endregion

        #region Public

        public async UniTask InitAsync()
        {
            if (_initialized || _spawnSettings == null)
                return;

            await _spawnSettings.Init();
            _initialized = true;

            if (_createOnInit)
                StartProduction();
        }

        public void StartProduction()
        {
            if (_isRunning && !_isPaused)
                return;

            if (IsOutputFull)
            {
                OnOutputFull?.Invoke();
                return;
            }

            if (_isPaused)
            {
                _isPaused = false;
                return;
            }

            _isRunning = true;
            _isPaused = false;
            _productionCoroutine = CoroutineManager.Run(ProductionRoutine());
            OnProductionStarted?.Invoke();
        }

        public void StopProduction()
        {
            if (!_isRunning)
                return;

            StopInternal();
            OnProductionStopped?.Invoke();
        }

        public void PauseProduction()
        {
            if (!_isRunning || _isPaused)
                return;

            _isPaused = true;
            OnProductionPaused?.Invoke();
        }

        public void ResumeProduction()
        {
            if (!_isRunning || !_isPaused)
                return;

            _isPaused = false;
        }

        #endregion

        #region Private

        private void StopInternal()
        {
            _isRunning = false;
            _isPaused = false;

            if (_productionCoroutine != null)
            {
                CoroutineManager.Stop(_productionCoroutine);
                _productionCoroutine = null;
            }
        }

        private IEnumerator ProductionRoutine()
        {
            if (!_initialized)
                yield return InitAsync().ToCoroutine();

            while (_isRunning)
            {
                float waitTime = Mathf.Max(0.01f, _productTimeToMakeValue?.Value ?? 0f);
                float elapsed = 0f;
                OnProductCreateProgress?.Invoke(0f);

                while (elapsed < waitTime)
                {
                    yield return null;

                    if (!_isRunning)
                        yield break;

                    if (_isPaused)
                        continue;

                    elapsed += Time.deltaTime;
                    OnProductCreateProgress?.Invoke(Mathf.Clamp01(elapsed / waitTime));
                }

                OnProductCreateProgress?.Invoke(1f);

                if (!_isRunning)
                    yield break;

                int batchCount = Mathf.Max(1, _productMakeCount?.Value ?? 1);
                for (int i = 0; i < batchCount; i++)
                {
                    if (IsOutputFull)
                    {
                        OnOutputFull?.Invoke();
                        StopInternal();
                        OnProductionStopped?.Invoke();
                        yield break;
                    }

                    yield return SpawnProduct().ToCoroutine();
                }
            }
        }

        private async UniTask SpawnProduct()
        {
            var product = await _spawnSettings.Get();
            if (product == null)
                return;

            bool accepted = await _outputContainer.PutIn(product);
            if (!accepted)
                return;

            OnProductCreated?.Invoke(product);
        }

        #endregion
    }
}
