using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PuzzleGame.Gameplay.Boosters
{
    public static class Explosion
    {
        public static void Execute(NumberedBrick[,] field, IEnumerable<Vector2Int> adjacentCoords, Action onComplete)
        {
            List<Vector2Int> coords = adjacentCoords.ToList();
        
            for (int i = 0; i < coords.Count; i++)
                field.DestroyBrick(coords[i], i < coords.Count - 1 ? null : onComplete);
        }
    }
}