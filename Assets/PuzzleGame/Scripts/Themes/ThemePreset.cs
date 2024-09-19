using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame.Themes
{
    [CreateAssetMenu(fileName = "ThemePreset", menuName = "Theme Preset")]
    public class ThemePreset : ScriptableObject
    {
        public Price price;

        [Space] [SerializeField] Label brickLabel;
        [SerializeField] Color[] brickLabelColors;
        [SerializeField] Color[] brickSpriteColors;
        [SerializeField] Color[] buttonColors;
        [SerializeField] Color[] buttonTextColors;
        [SerializeField] Color[] fieldColors;
        [SerializeField] Color textColor;
        [SerializeField] Color backgroundColor;
        [SerializeField] Sprite backgroundSprite;
        [SerializeField] Color fieldHighlightColor;
        [SerializeField] Color inactiveColor;

        public LabelType LabelType => brickLabel.labelType;

        public Sprite BackgroundSprite => backgroundSprite;

        public Color GetColor(ColorType colorType, int index = 0) =>
            colorType switch
            {
                ColorType.Text => textColor,
                ColorType.Background => backgroundColor,
                ColorType.Field => GetColor(fieldColors, index),
                ColorType.BrickLabel => GetColor(brickLabelColors, index),
                ColorType.BrickSprite => GetColor(brickSpriteColors, index),
                ColorType.Button => GetColor(buttonColors, index),
                ColorType.ButtonText => GetColor(buttonTextColors, index),
                ColorType.FieldHighlight => fieldHighlightColor,
                ColorType.Inactive => inactiveColor,
                _ => throw new ArgumentOutOfRangeException()
            };

        static Color GetColor(IReadOnlyList<Color> colors, int index) =>
            colors[Mathf.Clamp(index, 0, colors.Count - 1)];

        public Sprite GetSprite(int index) =>
            brickLabel.collection && brickLabel.collection.sprites.Length > 0
                ? brickLabel.collection.sprites[Mathf.Clamp(index, 0, brickLabel.collection.sprites.Length - 1)]
                : null;
    }
}
