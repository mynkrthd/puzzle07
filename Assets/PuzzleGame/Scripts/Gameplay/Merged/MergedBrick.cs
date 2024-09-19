using System;
using PuzzleGame.Themes;
using UnityEngine;

namespace PuzzleGame.Gameplay.Merged
{
    public class MergedBrick : NumberedBrick
    {
        public Sprite xBrick;
        public int maxNumber = 7;
    
        bool IsMaxNumber => Number == maxNumber;

        protected override void UpdateColors(ThemePreset theme)
        {
            if (!IsMaxNumber)
            {
                base.UpdateColors(theme);
                return;
            }

            label.gameObject.SetActive(false);
            labelImage.gameObject.SetActive(true);
            labelImage.sprite = xBrick;
            labelImage.color = theme.GetColor(ColorType.BrickLabel, int.MaxValue);
            sprite.color = theme.GetColor(ColorType.BrickSprite, int.MaxValue);
        }

        public void DoRotate(Quaternion targetRotation, Action onComplete)
        {
            StartCoroutine(transform.DoRotation(targetRotation, onComplete));
        }
    }
}