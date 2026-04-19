using D_Dev.CurrencySystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Core.OrderSystem
{
    [CreateAssetMenu(menuName = "Game/Order/OrderInfo", fileName = "New_OrderInfo")]
    public class OrderInfo : ScriptableObject
    {
        #region Fields

        [SerializeField] private CurrencyInfo _priceCurrency;
        [SerializeField] private int _price;
        [PreviewField(75, ObjectFieldAlignment.Right)]
        [SerializeField] private Sprite _orderIcon;

        #endregion

        #region Properties

        public Sprite OrderIcon => _orderIcon;

        public CurrencyInfo PriceCurrency => _priceCurrency;

        public int Price => _price;

        #endregion
    }
}
