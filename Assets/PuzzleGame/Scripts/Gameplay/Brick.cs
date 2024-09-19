using System;
using PuzzleGame.Themes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleGame.Gameplay
{
    public class Brick : MonoBehaviour, IPointerClickHandler
    {
        public event Action<Brick> PointerClick;

        [SerializeField]
        protected Image sprite;

        [SerializeField] GameObject highlight;

        int colorIndex;

        public RectTransform RectTransform { get; private set; }

        public int ColorIndex
        {
            get => colorIndex;
            set
            {
                colorIndex = value;
                UpdateColors(CurrentTheme);
            }
        }

        protected static ThemePreset CurrentTheme => ThemeController.Instance.CurrentTheme;

        protected virtual void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            Highlight(false);
        }

        protected virtual void Start()
        {
            UpdateColors(CurrentTheme);
            ThemeController.Instance.ThemeChanged += UpdateColors;
        }

        protected virtual void OnDestroy()
        {
            ThemeController.Instance.ThemeChanged -= UpdateColors;
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            PointerClick?.Invoke(this);
        }

        public void Highlight(bool value)
        {
            if (highlight && highlight.activeSelf != value)
                highlight.SetActive(value);
        }

        protected virtual void UpdateColors(ThemePreset theme)
        {
            sprite.color = theme.GetColor(ColorType.Field, ColorIndex);
        }
    }
}