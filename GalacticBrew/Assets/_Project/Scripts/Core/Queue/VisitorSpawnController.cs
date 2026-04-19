using _Project.Scripts.Core.OrderSystem;
using Cysharp.Threading.Tasks;
using D_Dev.PolymorphicValueSystem;
using D_Dev.RuntimeEntityVariables;
using D_Dev.TimerSystem;
using UnityEngine;

namespace _Project.Scripts.Core.Queue
{
    public class VisitorSpawnController : MonoBehaviour
    {
        #region Fields

        [SerializeField] private VisitorQueueContainer _queueContainer;
        [SerializeField] private VisitorContainer _visitorContainer;
        [SerializeField] private OrdersContainer _ordersContainer;
        [SerializeReference] private PolymorphicValue<Transform> _visitorSpawnPoint;
        [SerializeField] private float _spawnInterval = 5f;
        [SerializeField] private bool _useRandomQueue;

        private CountdownTimer _spawnTimer;
        private bool _initialized;
        private bool _spawningStarted;

        #endregion

        #region Monobehaviour

        private async void Start()
        {
            if (_visitorContainer != null)
            {
                foreach (var settings in _visitorContainer.Visitors)
                {
                    if (settings != null)
                        await settings.Init();
                }
            }

            _spawnTimer = new CountdownTimer(_spawnInterval);
            _spawnTimer.OnTimerEnd += OnSpawnTimerEnd;
            _initialized = true;

            if (_ordersContainer == null || _ordersContainer.Count > 0)
                BeginSpawning();
            else
                _ordersContainer.OnOrderAdded += HandleFirstOrderAdded;
        }

        private void Update()
        {
            if (!_spawningStarted)
                return;

            _spawnTimer.Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            if (_spawnTimer != null)
                _spawnTimer.OnTimerEnd -= OnSpawnTimerEnd;

            if (_ordersContainer != null)
                _ordersContainer.OnOrderAdded -= HandleFirstOrderAdded;

            if (_visitorContainer != null)
            {
                foreach (var settings in _visitorContainer.Visitors)
                    settings?.DisposePool();
            }
        }

        #endregion

        #region Private

        private void HandleFirstOrderAdded(OrderInfo order)
        {
            if (_ordersContainer != null)
                _ordersContainer.OnOrderAdded -= HandleFirstOrderAdded;

            BeginSpawning();
        }

        private void BeginSpawning()
        {
            if (_spawningStarted || !_initialized)
                return;

            _spawningStarted = true;
            _spawnTimer.Start();
            SpawnVisitor().Forget();
        }

        private void OnSpawnTimerEnd()
        {
            if (!TryGetAvailableQueue(out _))
            {
                _spawnTimer.Start();
                return;
            }

            SpawnVisitor().Forget();
        }

        private async UniTaskVoid SpawnVisitor()
        {
            if (!TryGetAvailableQueue(out var targetQueue))
            {
                _spawnTimer.Start();
                return;
            }

            var spawnSettings = _visitorContainer != null ? _visitorContainer.GetRandom() : null;
            if (spawnSettings == null)
            {
                _spawnTimer.Start();
                return;
            }

            var visitorObj = await spawnSettings.Get();
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

            visitorObj.transform.position = _visitorSpawnPoint.Value.position;
            targetQueue.TryEnqueue(container);
            _spawnTimer.Start();
        }

        private bool TryGetAvailableQueue(out VisitorQueue queue)
        {
            queue = null;
            if (_queueContainer == null || _queueContainer.Count == 0)
                return false;

            var queues = _queueContainer.Queues;

            if (_useRandomQueue)
            {
                int startIndex = Random.Range(0, queues.Count);
                for (int i = 0; i < queues.Count; i++)
                {
                    var q = queues[(startIndex + i) % queues.Count];
                    if (q != null && !q.IsFull)
                    {
                        queue = q;
                        return true;
                    }
                }

                return false;
            }

            for (int i = 0; i < queues.Count; i++)
            {
                var q = queues[i];
                if (q != null && !q.IsFull)
                {
                    queue = q;
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
