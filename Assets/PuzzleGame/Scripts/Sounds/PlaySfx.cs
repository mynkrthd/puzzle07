using UnityEngine;

namespace PuzzleGame.Sounds
{
    public class PlaySfx : MonoBehaviour
    {
        public AudioSource source;
        public float minPitch = 1f;
        public float maxPitch = 1f;
        public bool playOnAwake = true;

        void OnEnable()
        {
            if (playOnAwake)
                Play();
        }

        public void Play()
        {
            source.pitch = Random.Range(minPitch, maxPitch);
            source.Play();
        }
    }
}