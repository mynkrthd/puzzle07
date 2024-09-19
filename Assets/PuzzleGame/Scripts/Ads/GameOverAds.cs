using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Ads
{
    public class GameOverAds : MonoBehaviour
    {
        public float lastChanceSeconds = 3f;

        [SerializeField]
        GameObject restartGameObject;

        [SerializeField]
        GameObject lastChanceGameObject;

        [SerializeField]
        Button lastChanceButton;

        [SerializeField]
        RectTransform maskImage;

        float maskSize;
        bool isWatching;
        public event Action LastChanceComplete = delegate { };

        void Awake()
        {
            maskSize = maskImage.rect.width;
            lastChanceButton.onClick.AddListener(OnWatchAdsClick);
        }

        void OnEnable()
        {
            lastChanceGameObject.gameObject.SetActive(false);
            restartGameObject.SetActive(true);
        }

        void OnWatchAdsClick()
        {
            isWatching = true;
            StopAllCoroutines();

            UnityAdsController.Instance.ShowRewardedVideo(OnAdsComplete);
        }

        public void LastChance()
        {
            if (!UnityAdsController.Instance.IsRewardedVideoLoaded)
                return;

            restartGameObject.SetActive(false);
            lastChanceGameObject.gameObject.SetActive(true);

            lastChanceButton.interactable = false;
            maskImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maskSize);

            StartTimer();
        }

        public void Skip()
        {
            OnTimerOver();
        }

        void StartTimer()
        {
            lastChanceButton.interactable = true;
            StartCoroutine(CountDownAnimation(lastChanceSeconds));
        }

        IEnumerator CountDownAnimation(float time)
        {
            float animationTime = time;

            while (animationTime > 0)
            {
                animationTime -= Time.deltaTime;
                float width = animationTime / time * maskSize;
                maskImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);

            OnTimerOver();
        }

        void OnTimerOver()
        {
            lastChanceGameObject.gameObject.SetActive(false);
            restartGameObject.SetActive(true);
        }

        void OnAdsComplete(bool success)
        {
            if (!isWatching) return;

            OnTimerOver();

            if (success)
                LastChanceComplete.Invoke();

            isWatching = false;
        }
    }
}
