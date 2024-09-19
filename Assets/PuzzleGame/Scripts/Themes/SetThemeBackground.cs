using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Themes
{
    public class SetThemeBackground : MonoBehaviour
    {
        Image image;

        void Start()
        {
            image = GetComponent<Image>();
        
            UpdateColor(ThemeController.Instance.CurrentTheme);
            ThemeController.Instance.ThemeChanged += UpdateColor;
        }
    
        void OnDestroy()
        {
            ThemeController.Instance.ThemeChanged -= UpdateColor;
        }
    
        void UpdateColor(ThemePreset theme)
        {
            image.sprite = theme.BackgroundSprite;
            image.color = theme.GetColor(ColorType.Background);
        }
    }
}
