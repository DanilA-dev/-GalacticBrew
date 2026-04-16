using DG.Tweening;
using UnityEngine;

namespace Game.Core.Production
{
    [CreateAssetMenu(menuName = "Game/Production/Product Transfer Settings", fileName = "ProductTransferSettings")]
    public class ProductTransferSettings : ScriptableObject
    {
        [SerializeField] private float _duration = 0.4f;
        [SerializeField] private Ease _moveEase = Ease.OutQuad;
        [SerializeField] private bool _useJump = true;
        [SerializeField] private float _jumpPower = 1.5f;
        [SerializeField, Min(1)] private int _jumpNum = 1;
        [SerializeField] private Ease _rotateEase = Ease.OutQuad;
        [SerializeField, Min(0f)] private float _betweenItemsInterval = 0.08f;

        public float Duration => _duration;
        public Ease MoveEase => _moveEase;
        public bool UseJump => _useJump;
        public float JumpPower => _jumpPower;
        public int JumpNum => _jumpNum;
        public Ease RotateEase => _rotateEase;
        public float BetweenItemsInterval => _betweenItemsInterval;
    }
}
