using System;
using Cysharp.Threading.Tasks;
using D_Dev.ColliderEvents;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Core.Production
{
    [Flags]
    public enum ContainerMode
    {
        None = 0,
        CanAccept = 1 << 0,
        CanGive = 1 << 1,
    }

    public class ProductContainer : MonoBehaviour
    {
        #region Fields

        [Title("Refs")]
        [SerializeField] private ProductStack _stack;
        [SerializeField] private TriggerColliderObservable _productTriggerCollider;

        [Title("Settings")]
        [SerializeField] private ContainerMode _mode = ContainerMode.CanAccept | ContainerMode.CanGive;
        [SerializeField] private ProductTransferSettings _transferSettings;
        [SerializeField, Min(1)] private int _maxTransferPerEnter = 999;

        [FoldoutGroup("Events")]
        public UnityEvent<GameObject> OnProductEntered;
        [FoldoutGroup("Events")]
        public UnityEvent<GameObject> OnProductExited;
        [FoldoutGroup("Events")]
        public UnityEvent OnFull;
        [FoldoutGroup("Events")]
        public UnityEvent OnEmpty;

        private bool _transferring;

        #endregion

        #region Properties

        public ProductStack Stack => _stack;
        public ContainerMode Mode => _mode;
        public bool HasSpace => _stack != null && !_stack.IsFull;
        public bool HasProducts => _stack != null && !_stack.IsEmpty;
        public bool CanAccept => (_mode & ContainerMode.CanAccept) != 0 && HasSpace;
        public bool CanGive => (_mode & ContainerMode.CanGive) != 0 && HasProducts;
        public bool IsStackFull => _stack.IsFull;

        #endregion

        #region Monobehaviour

        private void OnEnable()
        {
            if (_productTriggerCollider != null)
                _productTriggerCollider.OnEnter.AddListener(HandleTriggerEnter);
        }

        private void OnDisable()
        {
            if (_productTriggerCollider != null)
                _productTriggerCollider.OnEnter.RemoveListener(HandleTriggerEnter);
        }

        #endregion

        #region Public

        public async UniTask<bool> PutIn(GameObject product)
        {
            if (product == null || _stack == null || _stack.IsFull)
                return false;

            if (!_stack.TryReserveSlot(out var parent, out var localTarget))
                return false;

            var t = product.transform;
            t.SetParent(parent, worldPositionStays: true);

            if (_transferSettings != null && _transferSettings.Duration > 0f)
            {
                var seq = DOTween.Sequence();

                if (_transferSettings.UseJump)
                {
                    seq.Join(t.DOLocalJump(localTarget, _transferSettings.JumpPower,
                            _transferSettings.JumpNum, _transferSettings.Duration)
                        .SetEase(_transferSettings.MoveEase));
                }
                else
                {
                    seq.Join(t.DOLocalMove(localTarget, _transferSettings.Duration)
                        .SetEase(_transferSettings.MoveEase));
                }

                seq.Join(t.DOLocalRotateQuaternion(Quaternion.identity, _transferSettings.Duration)
                    .SetEase(_transferSettings.RotateEase));

               await seq.AsyncWaitForCompletion().AsUniTask();
            }
            else
            {
                t.localPosition = localTarget;
                t.localRotation = Quaternion.identity;
            }

            if (product == null)
            {
                _stack.ReleaseReserved();
                return false;
            }

            _stack.CommitReserved(product);
            OnProductEntered?.Invoke(product);

            if (_stack.IsFull)
                OnFull?.Invoke();

            return true;
        }

        public bool TryTakeOut(out GameObject product)
        {
            product = null;
            if (_stack == null)
                return false;

            if (!_stack.TryPopTop(out product))
                return false;

            OnProductExited?.Invoke(product);

            if (_stack.IsEmpty)
                OnEmpty?.Invoke();

            return true;
        }

        public async UniTask TransferTo(ProductContainer target, int maxCount = int.MaxValue)
        {
            if (target == null || target == this || _transferring)
                return;

            _transferring = true;
            try
            {
                int moved = 0;
                float interval = _transferSettings != null ? _transferSettings.BetweenItemsInterval : 0f;

                while (moved < maxCount && CanGive && target.CanAccept)
                {
                    if (!TryTakeOut(out var product))
                        break;

                    bool accepted = await target.PutIn(product);
                    if (!accepted)
                    {
                        if (!await PutIn(product) && product != null)
                            Destroy(product);
                        break;
                    }

                    moved++;

                    if (interval > 0f && moved < maxCount && CanGive && target.CanAccept)
                        await UniTask.Delay(TimeSpan.FromSeconds(interval));
                }
            }
            finally
            {
                _transferring = false;
            }
        }

        #endregion

        #region Private

        private void HandleTriggerEnter(Collider other)
        {
            if (!CanGive)
                return;

            var target = other.GetComponentInParent<ProductContainer>();
            if (target == null || target == this || !target.CanAccept)
                return;

            TransferTo(target, _maxTransferPerEnter).Forget();
        }

        #endregion
    }
}
