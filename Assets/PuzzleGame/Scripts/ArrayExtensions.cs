using UnityEngine;

namespace PuzzleGame
{
    public static class ArrayExtensions
    {
        public static T Get<T>(this T[,] array, Vector2Int coords) => array[coords.x, coords.y];

        public static void Set<T>(this T[,] array, Vector2Int coords, T value) => array[coords.x, coords.y] = value;
    }
}
