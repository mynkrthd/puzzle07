using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace PuzzleGame.Themes
{
    public class ThemeController
    {
        static ThemeController instance;

        ThemePreset[] themes;

        ThemePreset currentTheme;

        string currentGameId = "";

        public event Action<ThemePreset> ThemeChanged = delegate { };
        public event Action<ThemePreset> ThemePurchased = delegate { };

        public static ThemeController Instance => instance ?? (instance = new ThemeController());

        public ThemePreset CurrentTheme
        {
            get => currentTheme;
            set
            {
                Assert.IsTrue(Array.IndexOf(themes, value) >= 0);

                currentTheme = value;

                ThemeChanged.Invoke(currentTheme);
            }
        }
    
        ThemeController()
        {
            LoadThemeCollection();
            UserProgress.Current.ProgressUpdate += OnProgressUpdate;
        }

        void OnProgressUpdate()
        {
            if (currentGameId == UserProgress.Current.CurrentGameId)
                return;

            LoadThemeCollection();
        }

        void LoadThemeCollection()
        {
            currentGameId = UserProgress.Current.CurrentGameId;
            themes = Resources.Load<ThemesCollection>($"Themes/ThemesCollection{currentGameId}").themes;
            CurrentTheme = Array.Find(themes, t => t.name == UserProgress.Current.CurrentThemeId) ?? themes[0];
        }

        public void SaveCurrentTheme()
        {
            UserProgress.Current.CurrentThemeId = currentTheme.name;
        }

        public ThemePreset[] GetThemes()
        {
            return (ThemePreset[]) themes.Clone();
        }

        public bool IsThemePurchased(ThemePreset theme)
        {
            return UserProgress.Current.IsItemPurchased(theme.name);
        }

        public void OnThemePurchased(ThemePreset theme)
        {
            UserProgress.Current.OnItemPurchased(theme.name);
            ThemePurchased.Invoke(theme);
        }
    }
}
