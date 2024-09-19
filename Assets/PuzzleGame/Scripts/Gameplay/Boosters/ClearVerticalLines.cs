using System;
using UnityEngine;

namespace PuzzleGame.Gameplay.Boosters
{
    public class ClearVerticalLines
    {
        public static void Execute(NumberedBrick[,] field, int coordX, int linesCount, Action onComplete)
        {
            int fieldSizeY = field.GetLength(1);

            for (int i = 0; i < linesCount; i++)
            {
                for (int y = 0; y < fieldSizeY; y++)
                    field.DestroyBrick(new Vector2Int(i + coordX, y), i == linesCount - 1 && y == fieldSizeY - 1 ? onComplete : null);
            }
        }
    }
}
