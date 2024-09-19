using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PuzzleGame.Themes;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Gameplay.Rings
{
    public class RingsController : NumberedBrick
    {
        public event Action<int> MergeColor = delegate {  }; 
    
        public Ring bigRing;
        public Ring middleRing;
        public Ring smallRing;

        public Image animatedImage;
    
        const int emptyColor = -1;
        readonly int clearTrigger = Animator.StringToHash("Clear");
    
        public int[] ringStates = {emptyColor, emptyColor, emptyColor};

        List<Ring> Rings => new List<Ring>{bigRing, middleRing, smallRing};
        public Vector3 Position => transform.position;
        public bool IsFull => ringStates.All(s => s != emptyColor);
    
        public bool IsEmpty => ringStates.All(s => s == emptyColor);
        public int FreeStates => ringStates.Count(s => s == emptyColor);

        public List<int> GetColors()
        {
            List<int> colors = new List<int>(ringStates);
            colors.RemoveAll(c => c == emptyColor);
            return colors;
        }

        public bool CanMerge(int[] values)
        {
            return !values.Where((t, i) => t != emptyColor && ringStates[i] != emptyColor).Any();
        }
    
        public void Merge(int[] values)
        {
            for (int i = 0; i < values.Length; i++)
                ringStates[i] = values[i] == emptyColor ? ringStates[i] : values[i];
        
            UpdateColors(ThemeController.Instance.CurrentTheme);
            CheckForOneColor();
        }

        public void Merge(int index, int value)
        {
            if(ringStates[index] != emptyColor) return;
        
            ringStates[index] = value;
            UpdateColors(ThemeController.Instance.CurrentTheme);
        }

        public void Clear()
        {
            foreach (int state in ringStates)
            {
                if(state == emptyColor) continue;

                Clear(state);
            }
        }
    
        public void Clear(int color)
        {
            StartCoroutine(ClearColors(color));
        }

        IEnumerator ClearColors(int color)
        {
            List<Ring> toClear = new List<Ring>();
            for (int i = 0; i < ringStates.Length; i++)
            {
                if (ringStates[i] != color) continue;
            
                ringStates[i] = emptyColor;
                toClear.Add(Rings[i]);
            }
        
            yield return new WaitForSeconds(0.25f);

            Animate(color);
            toClear.ForEach(i => i.Fade());
        }

        void CheckForOneColor()
        {
            if (ringStates.Any(s => s == emptyColor)) return;
        
            int color = ringStates.First(s => s != emptyColor);
            if (ringStates.Any(s => s != color)) return;
        
            MergeColor.Invoke(color);
            Animate(color);
        }

        void Animate(int color)
        {
            animatedImage.color = ThemeController.Instance.CurrentTheme.GetColor(ColorType.BrickSprite, color);
            animator.SetTrigger(clearTrigger);
        }

        protected override void UpdateColors(ThemePreset theme)
        {
            Rings.ForEach(r => r.Clear());

            for (int i = 0; i < ringStates.Length; i++)
            {
                if (ringStates[i] < 0) continue;

                Rings[i].SetColor(theme.GetColor(ColorType.BrickSprite, ringStates[i]));
            }
        }
    }
}