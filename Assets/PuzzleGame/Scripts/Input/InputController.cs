using System;
using UnityEngine;

namespace PuzzleGame.Input
{
    public abstract class InputController : MonoBehaviour
    {
        public static event Action Left;
        public static event Action Right;
        public static event Action Down;
        public static event Action<int> Move;

        protected static void OnLeft()
        {
            Left?.Invoke();
        }

        protected static void OnRight()
        {
            Right?.Invoke();
        }

        protected static void OnDown()
        {
            Down?.Invoke();
        }

        protected static void OnMove(int value)
        {
            Move?.Invoke(value);
        }
    }
}