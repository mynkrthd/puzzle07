using UnityEngine;

namespace PuzzleGame.Themes
{
    [RequireComponent(typeof(Camera))]
    public class SetCameraColor : MonoBehaviour
    {
        void UpdateColor(ThemePreset theme)
        {
            GetComponent<Camera>().backgroundColor = theme.GetColor(ColorType.Background);
        }

        void Start()
        {
            UpdateColor(ThemeController.Instance.CurrentTheme);
            ThemeController.Instance.ThemeChanged += UpdateColor;
        }

        void OnDestroy()
        {
            ThemeController.Instance.ThemeChanged -= UpdateColor;
        }
    }
}
