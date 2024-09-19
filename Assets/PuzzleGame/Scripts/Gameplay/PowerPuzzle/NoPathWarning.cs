using UnityEngine;

namespace PuzzleGame.Gameplay.PowerPuzzle
{
    public class NoPathWarning : MonoBehaviour
    {
        public GameController gameController;

        void Start()
        {
            gameController.NoPath += OnNoPath;
        }

        void OnNoPath()
        {
            GetComponent<Animator>().SetTrigger("Play");
        }
    }
}