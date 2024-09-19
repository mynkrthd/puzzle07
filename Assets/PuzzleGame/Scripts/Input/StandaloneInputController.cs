using UnityEngine;

namespace PuzzleGame.Input
{
    public class StandaloneInputController : InputController
    {
        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
                OnLeft();

            if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
                OnRight();

            if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
                OnDown();
        }
    }
}