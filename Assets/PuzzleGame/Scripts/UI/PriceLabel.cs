using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public class PriceLabel : MonoBehaviour
    {
        [SerializeField]
        Text priceText;
        [SerializeField]
        GameObject coinIcon;
        [SerializeField]
        GameObject adsIcon;

        string Item { get; set; }
        Price Price { get; set; }

        public void SetPrice(string item, Price price)
        {
            Item = item;
            Price = price;

            UpdatePrice();
        }

        void UpdatePrice()
        {
            priceText.text = (Price.value - UserProgress.Current.GetItemPurchaseProgress(Item)).ToString();

            coinIcon.SetActive(Price.type == PriceType.Coins);
            adsIcon.SetActive(Price.type == PriceType.Ads);
        }

        void Awake()
        {
            UserProgress.Current.ProgressUpdate += UpdatePrice;
        }

        void OnDestroy()
        {
            UserProgress.Current.ProgressUpdate -= UpdatePrice;
        }
    }
}
