using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Gameplay.Make10
{
    public class NumberCounter : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private Text highlightedLabel;
        [SerializeField] private Animator animator;

        private int value;
        private static readonly int On = Animator.StringToHash("On");

        public void UpdateValue(int value, int maxValue)
        {
            if(this.value == value) return;

            this.value = value;
            label.text = value.ToString();
            highlightedLabel.text = value.ToString();

            animator.SetTrigger(On);

            highlightedLabel.gameObject.SetActive(value > maxValue);
            label.gameObject.SetActive(value <= maxValue);
        }

        private void Awake()
        {
            label.text = "0";
            highlightedLabel.gameObject.SetActive(false);
        }
    }
}
