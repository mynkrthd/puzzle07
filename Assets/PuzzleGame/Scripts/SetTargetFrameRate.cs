using UnityEngine;

namespace PuzzleGame
{
    public static class SetTargetFrameRate
    {
        [RuntimeInitializeOnLoadMethod]
        static void Initialize()
        {
            Application.targetFrameRate = 60;
        }
    }
}
