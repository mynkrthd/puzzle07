using UnityEngine;

namespace PuzzleGame
{
    public class SetCameraToCanvas : MonoBehaviour
    {
        Canvas canvas;

        Canvas Canvas => canvas == null ? canvas = GetComponentInChildren<Canvas>() : canvas;
    
        void Start()
        {
            Canvas.worldCamera = Camera.main;
        }
    }
}