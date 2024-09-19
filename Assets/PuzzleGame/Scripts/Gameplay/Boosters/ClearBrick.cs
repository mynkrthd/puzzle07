using System;
using UnityEngine;

namespace PuzzleGame.Gameplay.Boosters
{
    public static class ClearBrick
    {
        public static void Execute(NumberedBrick[,] field, Vector2Int coords, Action onComplete)
        {
            field.DestroyBrick(coords, onComplete);
        }
    }
}