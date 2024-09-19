using System;
using UnityEngine;

namespace PuzzleGame.Gameplay.Boosters
{
    public static class BoostersHelper
    {
        public static void DestroyBrick(this NumberedBrick[,] field, Vector2Int coords, Action onComplete)
        {
            if (field[coords.x, coords.y] == null)
            {
                onComplete?.Invoke();
                return;
            }
        
            NumberedBrick brick = field[coords.x, coords.y];
            brick.DoDestroyAnimation(onComplete);
            field[coords.x, coords.y] = null;
        }
    }
}
