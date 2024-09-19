using System;
using UnityEngine;
using UnityEngine.Advertisements;

namespace PuzzleGame.Ads
{
    public class UnityAdsController : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        string GameId => 
#if UNITY_IOS
    AdsGameId.iOS;
#elif UNITY_ANDROID
            AdsGameId.android;
#else
    AdsGameId.other;
#endif

        Action<bool> rewardedVideoCompleteCallback;

        public static UnityAdsController Instance { get; private set; }
    
        public bool IsRewardedVideoLoaded { get; private set; }
        public bool IsVideoLoaded { get; private set; }

        void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (!Advertisement.isSupported)
                return;

            Advertisement.Initialize(GameId, true, this);
        }

        void LoadRewardedVideo() => Advertisement.Load(PlacementId.RewardedVideo, this);

        void LoadVideo() => Advertisement.Load(PlacementId.Video, this);
    
        public void ShowRewardedVideo(Action<bool> onComplete)
        {
            if (IsRewardedVideoLoaded)
            {
                rewardedVideoCompleteCallback = onComplete;

                Advertisement.Show(PlacementId.RewardedVideo, this);
                IsRewardedVideoLoaded = false;
            }
            else
            {
                onComplete?.Invoke(false);
            }
        }

        public void ShowVideo()
        {
            if (!IsVideoLoaded)
                return;
        
            IsVideoLoaded = false;
            Advertisement.Show(PlacementId.Video, this);
        }

        public void OnInitializationComplete()
        {
            Debug.Log("Unity Ads Initialized");

            Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
            Advertisement.Banner.Show(PlacementId.Banner);
        
            LoadVideo();
            LoadRewardedVideo();
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.LogError(message);
        }

        public void OnUnityAdsAdLoaded(string placementId)
        {
            switch (placementId)
            {
                case PlacementId.RewardedVideo:
                    IsRewardedVideoLoaded = true;
                    break;
                case PlacementId.Video:
                    IsVideoLoaded = true;
                    break;
            }
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            Debug.LogError(message);
        }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            switch (placementId)
            {
                case PlacementId.RewardedVideo:
                    rewardedVideoCompleteCallback?.Invoke(showCompletionState == UnityAdsShowCompletionState.COMPLETED);
                    rewardedVideoCompleteCallback = null;
                
                    LoadRewardedVideo();
                    break;
                case PlacementId.Video:
                    IsVideoLoaded = false;
                    LoadVideo();
                    break;
            }
        }

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            switch (placementId)
            {
                case PlacementId.RewardedVideo:
                    rewardedVideoCompleteCallback?.Invoke(false);
                    rewardedVideoCompleteCallback = null;
                
                    LoadRewardedVideo();
                    break;
                case PlacementId.Video:
                    IsVideoLoaded = false;
                    LoadVideo();
                    break;
            }
        }

        public void OnUnityAdsShowStart(string placementId)
        {
        }

        public void OnUnityAdsShowClick(string placementId)
        {
        }
    }
}
