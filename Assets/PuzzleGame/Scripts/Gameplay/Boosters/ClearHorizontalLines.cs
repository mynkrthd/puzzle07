using System;
using UnityEngine;

namespace PuzzleGame.Gameplay.Boosters
{
    public static class ClearHorizontalLines
    {
        public static void Execute(NumberedBrick[,] field, int coordY, int linesCount, Action onComplete)
        {
            int fieldSizeX = field.GetLength(0);

            for (int x = 0; x < fieldSizeX; x++)
            {
                for (int i = 0; i < linesCount; i++)
                    field.DestroyBrick(new Vector2Int(x, coordY + i), x == fieldSizeX - 1 && i == linesCount - 1 ? onComplete : null);
            }
        }
    }
}