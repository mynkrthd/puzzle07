using System;
using System.Collections.Generic;
using PuzzleGame.Ads;
using PuzzleGame.Gameplay;
using PuzzleGame.Gameplay.Boosters;
using PuzzleGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame
{
    public class GameSelector : MonoBehaviour
    {
        [SerializeField] GamePresetsList gamePresetsList;
        [SerializeField] BoostersPanel boostersPanel;
        [SerializeField] BoosterButton boosterPrefab;
        [SerializeField] RectTransform boostersParent;
        [SerializeField] Button boostersBlocker;

        [SerializeField] GameObject navigation;
        [SerializeField] GameObject fieldBlocker;

        [SerializeField] Button next;
        [SerializeField] Button previous;
        [SerializeField] Toggle togglePrefab;
        [SerializeField] Transform togglesParent;
        [SerializeField] PriceLabel priceLabel;

        [SerializeField] MonetizeButton monetizeButton;

        [SerializeField] GameOverAds gameOver;

        [SerializeField] int highlightSortingOrder;

        int currentGameIndex;
        BaseGameController currentGame;
    
        Toggle[] toggles;
        readonly List<BoosterButton> boosters = new();

        static readonly int BigField = Animator.StringToHash("Big");
        static readonly int MiddleField = Animator.StringToHash("Middle");
        static readonly int SmallField = Animator.StringToHash("Small");

        public void MinimizeCurrentGame(bool value)
        {
            if (!value)
            {
                MaximizeCurrentGame();
                return;
            }

            Time.timeScale = 0;
            ResetTriggers();
            currentGame.fieldAnimator.SetTrigger(SmallField);
            navigation.SetActive(false);
        }

        void MaximizeCurrentGame()
        {
            bool isGameAvailable = gamePresetsList.presets[currentGameIndex].price.value <= 0 ||
                                   UserProgress.Current.IsItemPurchased(currentGame.name);

            if (isGameAvailable && !gameOver.gameObject.activeSelf)
            {
                Time.timeScale = 1;
                ResetTriggers();
                currentGame.fieldAnimator.SetTrigger(BigField);
                navigation.SetActive(true);
            }
            else
            {
                ResetTriggers();
                currentGame.fieldAnimator.SetTrigger(MiddleField);
                navigation.SetActive(true);
                fieldBlocker.SetActive(true);
            }
        }

        void ResetTriggers()
        {
            currentGame.fieldAnimator.ResetTrigger(BigField);
            currentGame.fieldAnimator.ResetTrigger(MiddleField);
            currentGame.fieldAnimator.ResetTrigger(SmallField);
        }

        void OnNextClick()
        {
            currentGameIndex++;
            currentGameIndex %= gamePresetsList.presets.Length;

            UpdateCurrentGame();
        }

        void OnPreviousClick()
        {
            currentGameIndex--;
            if (currentGameIndex < 0)
                currentGameIndex += gamePresetsList.presets.Length;

            UpdateCurrentGame();
        }

        void OnGamePurchased()
        {
            UserProgress.Current.OnItemPurchased(gamePresetsList.presets[currentGameIndex].name);
            UpdateCurrentGame();
        }

        void UpdateCurrentGame()
        {
            if (currentGame)
                DestroyImmediate(currentGame.gameObject);

            currentGame = Instantiate(gamePresetsList.presets[currentGameIndex].gamePrefab);
            currentGame.name = gamePresetsList.presets[currentGameIndex].name;
            UserProgress.Current.CurrentGameId = currentGame.name;
        
            currentGame.gameObject.AddComponent<SetCameraToCanvas>();
            currentGame.HighlightSortingOrder = highlightSortingOrder;

            if (toggles != null && toggles.Length > 0)
            {
                for (int i = 0; i < toggles.Length; i++)
                    toggles[i].isOn = i == currentGameIndex;
            }
        
            UpdateBoosters();
            gameOver.gameObject.SetActive(false);

            Price price = gamePresetsList.presets[currentGameIndex].price;

            bool isGameAvailable = price.value <= 0 ||
                                   UserProgress.Current.IsItemPurchased(currentGame.name);

            Time.timeScale = isGameAvailable ? 1 : 0;

            priceLabel.gameObject.SetActive(!isGameAvailable);
            monetizeButton.gameObject.SetActive(!isGameAvailable);

            fieldBlocker.SetActive(!isGameAvailable);

            if (isGameAvailable)
            {
                currentGame.GameOver += OnGameOver;
                return;
            }

            priceLabel.SetPrice(currentGame.name, price);

            ResetTriggers();
            currentGame.fieldAnimator.SetTrigger(MiddleField);

            monetizeButton.SetPrice(currentGame.name, price);
        }

        void UpdateBoosters()
        {
            foreach (var booster in boosters)
                DestroyImmediate(booster.gameObject);
            boosters.Clear();
        
            for (int i = 0; i < gamePresetsList.presets[currentGameIndex].BoosterConfigs.Count; i++)
            {
                BoosterButton booster = Instantiate(boosterPrefab, boostersParent);
                booster.Init(
                    gamePresetsList.presets[currentGameIndex].BoosterConfigs[i].booster,
                    highlightSortingOrder,
                    gamePresetsList.presets[currentGameIndex].canBuyBoosters);
            
                booster.Select += OnBoosterSelected;
                boosters.Add(booster);

                BoostersController.Instance.SetBoostersConfig(gamePresetsList.presets[currentGameIndex].BoosterConfigs);
            }
        }

        void OnBoosterSelected(BoosterPreset preset, bool isPurchased)
        {
            BoostersController.Instance.CurrentBooster = preset;
            Time.timeScale = 0;

            if (isPurchased)
            {
                boostersBlocker.gameObject.SetActive(true);
                currentGame.HighlightBoosterTarget(preset.Type, true);
            }
            else
            {
                boostersPanel.Init(preset);
                MinimizeCurrentGame(true);
            }
        }
    
        void OnBoosterDeselected(BoosterPreset preset)
        {
            boostersBlocker.gameObject.SetActive(false);
            boosters.Find(b => b.Preset == preset).Deselect();
            currentGame.HighlightBoosterTarget(preset.Type, false);
            Time.timeScale = 1;
        }

        void OnGameOver()
        {
            ResetTriggers();
            currentGame.fieldAnimator.SetTrigger(MiddleField);
            fieldBlocker.SetActive(true);

            foreach(BoosterButton button in boosters)
                button.SetRaycast(false);
        
            gameOver.gameObject.SetActive(true);
            gameOver.LastChance();
        }
    
        void OnLastChance()
        {
            gameOver.gameObject.SetActive(false);
            fieldBlocker.SetActive(false);
        
            foreach(BoosterButton button in boosters)
                button.SetRaycast(true);

            currentGame.LastChance(gamePresetsList.presets[currentGameIndex].lastChance);
            MinimizeCurrentGame(false);
        }
    
        void Awake()
        {
            currentGameIndex = Array.FindIndex(gamePresetsList.presets, g => g.name == UserProgress.Current.CurrentGameId);

            if (currentGameIndex < 0)
                currentGameIndex = 0;

            bool multipleModesAvailable = gamePresetsList.presets.Length > 1;

            if (multipleModesAvailable)
            {
                toggles = new Toggle[gamePresetsList.presets.Length];

                for (int i = 0; i < gamePresetsList.presets.Length; i++)
                    toggles[i] = Instantiate(togglePrefab, togglesParent);
            }
        
            UpdateCurrentGame();

            next.gameObject.SetActive(multipleModesAvailable);
            next.onClick.AddListener(OnNextClick);

            previous.gameObject.SetActive(multipleModesAvailable);
            previous.onClick.AddListener(OnPreviousClick);

            boostersBlocker.onClick.AddListener(() => OnBoosterDeselected(BoostersController.Instance.CurrentBooster));

            monetizeButton.PurchaseComplete += OnGamePurchased;
            gameOver.LastChanceComplete += OnLastChance;

            BoostersController.Instance.BoosterProceeded += OnBoosterProceeded;
            BoostersController.Instance.BoosterPurchased += OnBoosterPurchased;
        }

        private void OnDestroy()
        {
            BoostersController.Instance.BoosterProceeded -= OnBoosterProceeded;
            BoostersController.Instance.BoosterPurchased -= OnBoosterPurchased;
        }

        void OnBoosterProceeded(BoosterPreset preset, bool succeed)
        {
            OnBoosterDeselected(preset);
        }

        void OnBoosterPurchased(BoosterPreset preset)
        {
            MinimizeCurrentGame(false);
            OnBoosterSelected(preset, true);
        }
    }
}
