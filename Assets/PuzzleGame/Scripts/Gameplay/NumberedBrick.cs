using System;
using System.Collections;
using System.Collections.Generic;
using PuzzleGame.Themes;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Gameplay
{
    [RequireComponent(typeof(Animator))]
    public class NumberedBrick : Brick
    {
        public Text label;
        public Image labelImage;

        [SerializeField]
        float moveDuration;

        int number;

        protected Animator animator;
        ColorType? overrideColorType;

        static readonly int LandTrigger = Animator.StringToHash("Land");
        static readonly int MergeTrigger = Animator.StringToHash("Merge");
        static readonly int BlinkTrigger = Animator.StringToHash("Blink");
        static readonly int DefaultTrigger = Animator.StringToHash("Default");
        static readonly int DestroyTrigger = Animator.StringToHash("Destroy");

        public bool IsDestroyed { get; private set; }

        public int Number
        {
            get => number;
            set
            {
                number = value;
                if(label)
                    label.text = number.ToString();
            }
        }


        protected override void Awake()
        {
            base.Awake();
            animator = GetComponent<Animator>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            IsDestroyed = true;
        }

        public void SetOverrideColorType(ColorType? value)
        {
            overrideColorType = value;
            UpdateColors(CurrentTheme);
        }

        public void DoLocalMove(Vector2 position, Action onComplete)
        {
            StartCoroutine(LocalMove(position, onComplete));
        }

        public void DoLocalPath(List<Vector2> path, Action onComplete)
        {
            if (path.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            Vector2 position = path[0];
            path.RemoveAt(0);
            DoLocalMove(position, () => DoLocalPath(path, onComplete));
        }

        public void DoLandingAnimation(Action onComplete)
        {
            animator.SetTrigger(LandTrigger);
            this.DelayedCall(0.25f, onComplete);
        }

        public void DoMergingAnimation(Action onComplete)
        {
            animator.SetTrigger(MergeTrigger);
            this.DelayedCall(0.25f, onComplete);
        }

        public void DoBlinkingAnimation()
        {
            animator.SetTrigger(BlinkTrigger);
        }

        public void DoStopBlinking()
        {
            animator.SetTrigger(DefaultTrigger);
        }

        public void DoDestroyAnimation(Action onComplete)
        {
            animator.SetTrigger(DestroyTrigger);
            this.DelayedCall(0.25f, () => 
            {
                onComplete?.Invoke();
                Destroy(gameObject);
            });
        }

        public void DisableNumber()
        {
            label.gameObject.SetActive(false);
        }

        IEnumerator LocalMove(Vector2 position, Action onComplete)
        {
            Vector2 startPosition = RectTransform.anchoredPosition;
            float t = Time.deltaTime;
            while (t < moveDuration)
            {
                RectTransform.anchoredPosition = Vector2.Lerp(startPosition, position, t / moveDuration);
                yield return null;
                t += Time.deltaTime;
            }

            RectTransform.anchoredPosition = position;

            onComplete?.Invoke();
        }

        protected override void UpdateColors(ThemePreset theme)
        {
            base.UpdateColors(theme);

            if (label)
            {
                label.gameObject.SetActive(theme.LabelType == LabelType.Text);
                label.color = GetColor(ColorType.BrickLabel);
            }

            if (labelImage)
            {
                labelImage.gameObject.SetActive(theme.LabelType == LabelType.Sprite);

                labelImage.sprite = theme.GetSprite(ColorIndex);
                labelImage.color = GetColor(ColorType.BrickLabel);
            }

            sprite.color = GetColor(ColorType.BrickSprite);
        }

        Color GetColor(ColorType colorType)
        {
            var color = CurrentTheme.GetColor(colorType, ColorIndex);

            if (!overrideColorType.HasValue)
                return color;

            var overrideColor = CurrentTheme.GetColor(overrideColorType.Value, ColorIndex);
            overrideColor.a *= color.a;

            return overrideColor;
        }
    }
}