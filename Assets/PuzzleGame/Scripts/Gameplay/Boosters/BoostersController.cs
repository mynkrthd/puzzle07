using System;
using System.Collections.Generic;

namespace PuzzleGame.Gameplay.Boosters
{
    public class BoostersController
    {
        static BoostersController instance;
    
        BoosterPreset currentBooster;
        List<BoosterConfig> configs;
    
        string currentGameId = "";

        public event Action<BoosterPreset, bool> BoosterProceeded = delegate { };
        public event Action<BoosterPreset> BoosterPurchased = delegate { };
        public event Action<BoosterPreset> BoosterUpdated = delegate {  };

        public static BoostersController Instance => instance ?? (instance = new BoostersController());

        public BoosterPreset CurrentBooster
        {
            get => currentBooster;
            set => currentBooster = value;
        }

        BoostersController()
        {
            UserProgress.Current.ProgressUpdate += OnProgressUpdate;
            OnProgressUpdate();
        }

        public void SetBoostersConfig(List<BoosterConfig> configList)
        {
            configs = configList;
        }

        public void SetBoosters()
        {
            configs.ForEach(c => 
            {
                UserProgress.Current.RemoveAllItemPurchase(ItemName(c.booster));
                for (int i = 0; i < c.startCount; i++)
                    UserProgress.Current.OnItemPurchased(ItemName(c.booster));
            
                BoosterUpdated.Invoke(c.booster);
            });
        }

        private void OnProgressUpdate()
        {
            currentGameId = UserProgress.Current.CurrentGameId;
        }
    
        private string ItemName(BoosterPreset preset)
        {
            return currentGameId + preset.name;
        }

        private void RemoveBoosterPurchase(BoosterPreset preset)
        {
            UserProgress.Current.RemoveItemPurchase(ItemName(preset));
            UserProgress.Current.SetItemPurchaseProgress(ItemName(preset), 0);
        }

        public bool IsBoosterPurchased(BoosterPreset preset)
        {
            return UserProgress.Current.IsItemPurchased(ItemName(preset));
        }
    
        public int GetBoosterPurchaseCount(BoosterPreset preset)
        {
            return UserProgress.Current.GetItemsPurchasedCount(ItemName(preset));
        }
    
        public void OnBoosterPurchased(BoosterPreset preset)
        {
            for (int i = 0; i < preset.CountToBuy; i++)
                UserProgress.Current.OnItemPurchased(ItemName(preset));
        
            BoosterPurchased.Invoke(preset);
        }

        public void OnBoosterProceeded(bool succeed)
        {
            if(succeed)
                RemoveBoosterPurchase(currentBooster);
        
            BoosterProceeded.Invoke(currentBooster, succeed);
        }
    }
}