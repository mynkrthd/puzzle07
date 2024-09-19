using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Themes
{
    [RequireComponent(typeof(Graphic))]
    public class SetThemeColor : MonoBehaviour
    {
        [SerializeField]
        ColorType colorType;
        [SerializeField]
        int index;
        [SerializeField]
        bool setAlpha = true;

        void Start()
        {
            UpdateColor(ThemeController.Instance.CurrentTheme);
            ThemeController.Instance.ThemeChanged += UpdateColor;
        }

        void OnDestroy()
        {
            ThemeController.Instance.ThemeChanged -= UpdateColor;
        }

        void UpdateColor(ThemePreset theme)
        {
            var graphic = GetComponent<Graphic>();
            var color = theme.GetColor(colorType, index);

            if (!setAlpha)
                color.a = graphic.color.a;

            graphic.color = color;
        }
    }
}
