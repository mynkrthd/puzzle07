using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    [RequireComponent(typeof(Text))]
    public class CoinsCounter : MonoBehaviour
    {
        Text label;

        void OnProgressUpdate()
        {
            label.text = UserProgress.Current.Coins.ToString();
        }

        void Start()
        {
            label = GetComponent<Text>();

            OnProgressUpdate();
            UserProgress.Current.ProgressUpdate += OnProgressUpdate;
        }

        void OnDestroy()
        {
            UserProgress.Current.ProgressUpdate -= OnProgressUpdate;
        }
    }
}
