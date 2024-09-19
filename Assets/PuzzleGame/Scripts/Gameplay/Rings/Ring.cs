using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Gameplay.Rings
{
    public class Ring : MonoBehaviour
    {
        Image image;
        Animator animator;
    
        readonly int fadeTrigger = Animator.StringToHash("Fade");
        readonly int colorTrigger = Animator.StringToHash("SetColor");

        void Awake()
        {
            image = GetComponent<Image>();
            animator = GetComponent<Animator>();
        }

        public void SetColor(Color color)
        {
            image.color = color;
            animator.SetTrigger(colorTrigger);
        }

        public void Clear()
        {
            image.color = Color.clear;
        }

        public void Fade()
        {
            animator.SetTrigger(fadeTrigger);
        }
    }
}