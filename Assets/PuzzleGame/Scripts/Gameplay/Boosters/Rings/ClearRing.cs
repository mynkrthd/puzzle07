using PuzzleGame.Gameplay.Rings;
using UnityEngine;

namespace PuzzleGame.Gameplay.Boosters.Rings
{
    public static class ClearRing
    {
        public static void Execute(RingsController[,] rings, Vector2Int coords)
        {
            rings[coords.x, coords.y].Clear();
        }
    }
}
