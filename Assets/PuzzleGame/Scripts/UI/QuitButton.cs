using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    [RequireComponent(typeof(Button))]
    public class QuitButton : MonoBehaviour
    {
        void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        static void OnClick()
        {
            Application.Quit();
        }
    }
}