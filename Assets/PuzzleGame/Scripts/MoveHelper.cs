using System;
using System.Collections;
using UnityEngine;

namespace PuzzleGame
{
    public static class MoveHelper
    {
        public static IEnumerator DoLocalMove(this RectTransform rectTransform, Vector2 position)
        {
            Vector2 startPosition = rectTransform.anchoredPosition;
            float t = Time.deltaTime;
            while (t < 0.1f)
            {
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, position, t / 0.1f);
                yield return null;
                t += Time.deltaTime;
            }
        
            rectTransform.anchoredPosition = position;
        }

        public static IEnumerator DoLerp(Vector2 start, Vector2 target, Action<Vector2> onUpdate)
        {
            float t = Time.deltaTime;
            while (t < 0.1f)
            {
                onUpdate?.Invoke(Vector2.Lerp(start, target, t / 0.1f));
                yield return null;
                t += Time.deltaTime;
            }

            onUpdate?.Invoke(target);
        }

        public static IEnumerator DoLocalScale(this Transform transform, Vector3 scale)
        {
            Vector3 startScale = transform.localScale;
            float t = Time.deltaTime;
            while (t < 0.1f)
            {
                transform.localScale = Vector3.Lerp(startScale, scale, t / 0.1f);
                yield return null;
                t += Time.deltaTime;
            }

            transform.localScale = scale;
        }
    
        public static IEnumerator DoRotation(this Transform transform, Quaternion targetRotation, Action onComplete)
        {
            Quaternion startRotation = transform.localRotation;
            float t = Time.deltaTime;
            while (t < 0.15f)
            {
                transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, t / 0.15f);
                yield return null;
                t += Time.deltaTime;
            }

            transform.localRotation = targetRotation;
            onComplete?.Invoke();
        }

        public static void DelayedCall(this MonoBehaviour behaviour, float delay, Action onComplete)
        {
            behaviour.StartCoroutine(DoDelayedCall(delay, onComplete));
        }
    
        static IEnumerator DoDelayedCall(float delay, Action onComplete)
        {
            yield return new WaitForSeconds(delay);
            onComplete?.Invoke();
        }
    
    }
}
