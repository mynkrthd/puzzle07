using System;
using PuzzleGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Ads
{
    public class MonetizeButton : MonoBehaviour
    {
        const int AdsReward = 25;

        [SerializeField]
        Button buy;
        [SerializeField]
        Button watchAds;
        [SerializeField]
        Button getCoins;
        [SerializeField]
        GameObject loading;

        PriceLabel priceLabel;

        string Item { get; set; }
        Price Price { get; set; }

        bool waitForAdsLoaded;

        public event Action PurchaseComplete = delegate { };

        int PurchaseProgress
        {
            get => UserProgress.Current.GetItemPurchaseProgress(Item);
            set => UserProgress.Current.SetItemPurchaseProgress(Item, value);
        }

        public void SetPrice(string item, Price price)
        {
            Item = item;
            Price = price;

            UpdateButtons();

            if (price.value <= UserProgress.Current.GetItemPurchaseProgress(item))
                PurchaseComplete.Invoke();
        }

        void UpdateButtons()
        {
            buy.gameObject.SetActive(false);
            getCoins.gameObject.SetActive(false);
            watchAds.gameObject.SetActive(false);
            loading.SetActive(false);

            switch (Price.type)
            {
                case PriceType.Coins when UserProgress.Current.Coins >= Price.value:
                    buy.gameObject.SetActive(true);
                    break;
                case PriceType.Coins when UnityAdsController.Instance.IsRewardedVideoLoaded:
                    getCoins.gameObject.SetActive(true);
                    break;
                case PriceType.Ads when UnityAdsController.Instance.IsRewardedVideoLoaded:
                    watchAds.gameObject.SetActive(true);
                    break;
                default:
                    loading.SetActive(true);
                    waitForAdsLoaded = true;
                    break;
            }
        }

        void OnBuyClick()
        {
            UserProgress.Current.Coins -= Price.value;
            PurchaseComplete.Invoke();
        }

        void OnWatchAdsClick()
        {
            UnityAdsController.Instance.ShowRewardedVideo(OnWatchAdsComplete);
        }

        void OnGetCoinsClick()
        {
            UnityAdsController.Instance.ShowRewardedVideo(OnGetCoinsComplete);
        }

        void Awake()
        {
            buy.onClick.AddListener(OnBuyClick);
            watchAds.onClick.AddListener(OnWatchAdsClick);
            getCoins.onClick.AddListener(OnGetCoinsClick);
        }

        void OnEnable()
        {
            UpdateButtons();
        }

        void Update()
        {
            if (waitForAdsLoaded && UnityAdsController.Instance.IsRewardedVideoLoaded)
            {
                waitForAdsLoaded = false;
                UpdateButtons();
            }
        }

        void OnWatchAdsComplete(bool success)
        {
            if (!success)
            {
                Debug.LogError("Unity Ads Show Failure");
                return;
            }

            PurchaseProgress++;

            if (Price.value <= PurchaseProgress)
                PurchaseComplete.Invoke();

            UpdateButtons();
        }
    
        void OnGetCoinsComplete(bool success)
        {
            if (!success)
            {
                Debug.LogError("Unity Ads Show Failure");
                return;
            }

            UserProgress.Current.Coins += AdsReward;
            UpdateButtons();
        }
    }
}
