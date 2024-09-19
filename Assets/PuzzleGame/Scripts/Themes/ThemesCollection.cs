using UnityEngine;

namespace PuzzleGame.Themes
{
    [CreateAssetMenu(fileName = "ThemesCollection", menuName = "Themes Collection")]
    public class ThemesCollection : ScriptableObject
    {
        public ThemePreset[] themes;
    }
}
