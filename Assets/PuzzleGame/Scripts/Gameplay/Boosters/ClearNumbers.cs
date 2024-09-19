using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame.Gameplay.Boosters
{
    public static class ClearNumbers
    {
        public static void Execute(NumberedBrick[,] field, int maxValue, Action onComplete)
        {
            int fieldSizeX = field.GetLength(0);
            int fieldSizeY = field.GetLength(1);
        
            List<Vector2Int> numberedBricks = new List<Vector2Int>();

            for (int x = 0; x < fieldSizeX; x++)
            {
                for (int y = 0; y < fieldSizeY; y++)
                {
                    if (field[x, y] != null && field[x, y].Number <= maxValue)
                        numberedBricks.Add(new Vector2Int(x, y));
                }
            }
        
            for (int i = 0; i < numberedBricks.Count; i++)
                field.DestroyBrick(numberedBricks[i], i < numberedBricks.Count - 1 ? null : onComplete);
        }
    }
}