using System.Threading;
using _Project.Scripts.Core.Queue;
using Cysharp.Threading.Tasks;
using D_Dev.ColliderEvents;
using D_Dev.EntityVariable.Types;
using D_Dev.RuntimeEntityVariables;
using D_Dev.ScriptableVaiables;
using Game.Core.Production;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace _Project.Scripts.Core.OrderSystem
{
    public class OrderQueueHandler : MonoBehaviour
    {
        #region Fields

        [Title("Refs")]
        [SerializeField] private ProductContainer _orderContainer;
        [SerializeField] private VisitorQueue _queue;
        
        [Title("Triggers")]
        [SerializeField] private TriggerColliderObservable _cashierTrigger;
        [SerializeField] private TriggerColliderObservable _servedPointTrigger;

        [Title("Variables")]
        [SerializeField] private StringScriptableVariable _isServedID;

        [FoldoutGroup("Events")]
        public UnityEvent<VisitorOrderHandler> OnVisitorArrived;
        [FoldoutGroup("Events")]
        public UnityEvent<VisitorOrderHandler> OnOrderServed;
        [FoldoutGroup("Events")]
        public UnityEvent OnOrderFailed;
        [FoldoutGroup("Events")]
        public UnityEvent OnNoCurrentVisitor;

        private VisitorOrderHandler _current;
        private RuntimeEntityVariablesContainer _currentContainer;
        private bool _serving;
        private CancellationTokenSource _serveLoopCts;

        #endregion

        #region Properties

        public VisitorOrderHandler Current => _current;
        public bool IsServing { get; private set; }

        #endregion

        #region Monobehaviour

        private void OnEnable()
        {
            _servedPointTrigger?.OnEnter.AddListener(OnServedVisitorEnter);
            
            _cashierTrigger?.OnEnter.AddListener(HandleServingEnter);
            _cashierTrigger?.OnExit.AddListener(HandleServingExit);
        }

        private void OnDisable()
        {
            _servedPointTrigger?.OnEnter.RemoveListener(OnServedVisitorEnter);

            _cashierTrigger?.OnEnter.RemoveListener(HandleServingEnter);
            _cashierTrigger?.OnExit.RemoveListener(HandleServingExit);

            StopServeLoop();
        }

        #endregion

        #region Public

        public void ServeCurrent()
        {
            if (_current == null)
            {
                OnNoCurrentVisitor?.Invoke();
                return;
            }

            if (_serving)
                return;

            ServeAsync().Forget();
        }

        #endregion

        #region Private

        private async UniTask ServeAsync()
        {
            _serving = true;
            var visitor = _current;
            var container = _currentContainer;

            try
            {
                bool success = await visitor.TryFulfillOrderAsync();
                if (success)
                {
                    SetIsServed(container, true);
                    _current = null;
                    _currentContainer = null;
                    OnOrderServed?.Invoke(visitor);
                    _queue?.MoveQueue();
                }
                else
                {
                    OnOrderFailed?.Invoke();
                }
            }
            finally
            {
                _serving = false;
            }
        }

        private void StartServeLoop()
        {
            StopServeLoop();
            _serveLoopCts = new CancellationTokenSource();
            ServeLoopAsync(_serveLoopCts.Token).Forget();
        }

        private void StopServeLoop()
        {
            if (_serveLoopCts == null)
                return;

            _serveLoopCts.Cancel();
            _serveLoopCts.Dispose();
            _serveLoopCts = null;
        }

        private async UniTaskVoid ServeLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && IsServing)
            {
                if (_current != null && !_serving)
                    await ServeAsync();

                await UniTask.Yield(PlayerLoopTiming.Update, ct).SuppressCancellationThrow();
            }
        }

        private bool IsServed(RuntimeEntityVariablesContainer container)
        {
            if (_isServedID == null || container == null)
                return false;

            var variable = container.GetVariable<BoolEntityVariable>(_isServedID);
            return variable?.Value?.Value ?? false;
        }

        private void SetIsServed(RuntimeEntityVariablesContainer container, bool value)
        {
            if (_isServedID == null || container == null)
                return;

            var variable = container.GetVariable<BoolEntityVariable>(_isServedID);
            if (variable?.Value != null)
                variable.Value.Value = value;
        }

        #endregion

        #region Listeners
        
        private void OnServedVisitorEnter(Collider other)
        {
            if (_current != null)
                return;
            
            var container = other.GetComponentInParent<RuntimeEntityVariablesContainer>();
            if (container == null)
                return;

            if (IsServed(container))
                return;
            
            var visitor = container.GetComponent<VisitorOrderHandler>();
            if (visitor == null)
                return;
            
            _current = visitor;
            _currentContainer = container;
            visitor.SetSourceContainer(_orderContainer);
            visitor.GenerateOrder();
            OnVisitorArrived?.Invoke(visitor);
        }

        private void HandleServingEnter(Collider c)
        {
            IsServing = true;
            StartServeLoop();
        }

        private void HandleServingExit(Collider c)
        {
            IsServing = false;
            StopServeLoop();
        }

        #endregion
    }
}
