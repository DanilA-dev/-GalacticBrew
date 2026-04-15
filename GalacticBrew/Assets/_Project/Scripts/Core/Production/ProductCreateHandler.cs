using System.Collections;
using System.Collections.Generic;
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
        [SerializeReference] private PolymorphicValue<float> _productTimeToMakeValue = new FloatConstantValue();
        [SerializeReference] private PolymorphicValue<int> _productMakeCount = new IntConstantValue();
        [SerializeReference] private PolymorphicValue<int> _productLimit = new IntConstantValue();

        [Title("Spawn Points")]
        [SerializeField] private Transform[] _productSpawnPoints;
        [SerializeField] private Vector3 _stackOffset = new Vector3(0f, 0.2f, 0f);

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
        public UnityEvent OnProductLimitReached;

        private readonly List<GameObject> _createdProducts = new List<GameObject>();
        private Coroutine _productionCoroutine;
        private bool _isRunning;
        private bool _isPaused;
        private bool _initialized;

        #endregion

        #region Properties

        public int CreatedCount => _createdProducts.Count;
        public int ProductLimit => _productLimit?.Value ?? 0;
        
        public bool IsRunning => _isRunning;
        
        public bool IsPaused => _isPaused;
        public bool IsLimitReached => _createdProducts.Count >= ProductLimit;

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
            
            if(_createOnInit)
                StartProduction();
        }

        public void StartProduction()
        {
            if (_isRunning && !_isPaused)
                return;

            if (IsLimitReached)
            {
                OnProductLimitReached?.Invoke();
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

        public void ClearProducts()
        {
            _createdProducts.Clear();
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
                    if (IsLimitReached)
                    {
                        OnProductLimitReached?.Invoke();
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

            PlaceProduct(product);
            _createdProducts.Add(product);
            OnProductCreated?.Invoke(product);
        }

        private void PlaceProduct(GameObject product)
        {
            if (_productSpawnPoints == null || _productSpawnPoints.Length == 0)
                return;

            int count = _createdProducts.Count;
            int index = count % _productSpawnPoints.Length;
            int stackLevel = count / _productSpawnPoints.Length;
            var point = _productSpawnPoints[index];

            if (point == null)
                return;

            var t = product.transform;
            t.SetParent(point, worldPositionStays: false);
            t.localPosition = _stackOffset * stackLevel;
            t.localRotation = Quaternion.identity;
        }

        #endregion
    }
}
