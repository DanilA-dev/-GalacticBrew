using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Core.OrderSystem
{
    public class VisitorOrderView : MonoBehaviour
    {
        #region Fields

        [Title("Refs")]
        [SerializeField] private VisitorOrderHandler _handler;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _countText;

        #endregion

        #region Monobehaviour

        private void Reset()
        {
            _handler = GetComponent<VisitorOrderHandler>();
        }

        private void Awake()
        {
            if (_handler == null)
                _handler = GetComponent<VisitorOrderHandler>();
        }

        private void OnEnable()
        {
            if (_handler != null)
                _handler.OnOrderGenerated.AddListener(HandleOrderGenerated);
        }

        private void OnDisable()
        {
            if (_handler != null)
                _handler.OnOrderGenerated.RemoveListener(HandleOrderGenerated);
        }

        #endregion

        #region Private

        private void HandleOrderGenerated(OrderInfo order)
        {
            if (_iconImage != null)
            {
                _iconImage.sprite = order?.OrderIcon;
                _iconImage.enabled = order?.OrderIcon != null;
            }

            if (_countText != null)
                _countText.text = _handler.RequiredCount.ToString();
        }

        #endregion
    }
}
