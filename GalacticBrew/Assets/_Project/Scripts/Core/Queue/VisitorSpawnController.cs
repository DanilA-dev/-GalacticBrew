using Cysharp.Threading.Tasks;
using D_Dev.EntitySpawner;
using D_Dev.RuntimeEntityVariables;
using D_Dev.TimerSystem;
using UnityEngine;

namespace _Project.Scripts.Core.Queue
{
    public class VisitorSpawnController : MonoBehaviour
    {
        #region Fields

        [SerializeField] private VisitorQueueSystem _visitorQueueSystem;
        [SerializeField] private EntitySpawnSettings _spawnSettings;
        [SerializeField] private float _spawnInterval = 5f;

        private CountdownTimer _spawnTimer;
        private bool _initialized;

        #endregion

        #region Monobehaviour

        private async void Start()
        {
            await _spawnSettings.Init();

            _spawnTimer = new CountdownTimer(_spawnInterval);
            _spawnTimer.OnTimerEnd += OnSpawnTimerEnd;
            _spawnTimer.Start();

            SpawnVisitor().Forget();
            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized)
                return;

            _spawnTimer.Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            if (_spawnTimer != null)
                _spawnTimer.OnTimerEnd -= OnSpawnTimerEnd;

            _spawnSettings.DisposePool();
        }

        #endregion

        #region Private

        private void OnSpawnTimerEnd()
        {
            if (_visitorQueueSystem.IsFull)
            {
                _spawnTimer.Start();
                return;
            }

            SpawnVisitor().Forget();
        }

        private async UniTaskVoid SpawnVisitor()
        {
            var visitorObj = await _spawnSettings.Get();
            if (visitorObj == null)
            {
                _spawnTimer.Start();
                return;
            }

            if (!visitorObj.TryGetComponent(out RuntimeEntityVariablesContainer container))
            {
                _spawnTimer.Start();
                return;
            }

            _visitorQueueSystem.TryEnqueue(container);
            _spawnTimer.Start();
        }

        #endregion
    }
}
