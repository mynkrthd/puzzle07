using System;
using UnityEngine;

namespace PuzzleGame.Gameplay.Boosters
{
    public static class ClearCrossLines
    {
        public static void Execute(NumberedBrick[,] field, Action onComplete)
        {
            int fieldSizeX = field.GetLength(0);
            int fieldSizeY = field.GetLength(1);

            int xIndex = fieldSizeX / 2;
            int yIndex = fieldSizeY / 2;

            for (int x = 0; x < fieldSizeX; x++)
                field.DestroyBrick(new Vector2Int(x, yIndex), null);

            for (int y = 0; y < fieldSizeY; y++)
                field.DestroyBrick(new Vector2Int(xIndex, y), y < fieldSizeY - 1 ? null : onComplete);
        }
    }
}