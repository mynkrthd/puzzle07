using System;
using System.Collections.Generic;
using PuzzleGame.Gameplay;
using PuzzleGame.Gameplay.Boosters;
using UnityEngine;

namespace PuzzleGame
{
    [CreateAssetMenu(fileName = "GamePreset", menuName = "Game Preset")]
    public class GamePreset : ScriptableObject
    {
        public Price price;
        public BaseGameController gamePrefab;
        public LastChance lastChance;
        public bool canBuyBoosters = true;

        [SerializeField] BoosterConfigList boosterConfigList;

        public List<BoosterConfig> BoosterConfigs => boosterConfigList.list;
    }

    [Serializable]
    public class BoosterConfig
    {
        public BoosterPreset booster;
        public int startCount;
    }

    [Serializable]
    public class BoosterConfigList
    {
        public List<BoosterConfig> list;
    }
}