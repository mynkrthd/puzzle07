using UnityEngine;

namespace PuzzleGame.Gameplay.Rings
{
    public class Probabilities
    {
        public static T[] GetRandomValues<T> (T[] values, float[] probabilities, int count) {
            T[] randomValues = new T[count];
            for (int i = 0; i < count; i++)
                randomValues [i] = GetRandomValue (values, probabilities);
            return randomValues;
        }

        public static T GetRandomValue<T> (T[] values, float[] probabilities) {
            float probSum = 0;
            foreach (float prob in probabilities)
                probSum += prob;
            float rand = Random.Range (0f, probSum);
            float currSum = 0;
            for (int i = 0; i < probabilities.Length - 1; i++) {
                currSum += probabilities [i];
                if (rand <= currSum)
                    return values [i];
            }
            return values [values.Length - 1];
        }
    }
}
