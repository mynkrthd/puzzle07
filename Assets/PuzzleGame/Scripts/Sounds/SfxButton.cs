using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace PuzzleGame.Sounds
{
    public class SfxButton : MonoBehaviour
    {
        public Image icon;

        public State sfxOn;
        public State sfxOff;

        [Serializable]
        public class State
        {
            public AudioMixerSnapshot snapshot;
            public Sprite sprite;
        }

        static bool IsOn
        {
            get => PlayerPrefs.GetInt("Sfx", 1) == 1;
            set => PlayerPrefs.SetInt("Sfx", value ? 1 : 0);
        }

        void Start()
        {
            UpdateState();
        }

        public void ChangeState()
        {
            IsOn = !IsOn;
            UpdateState();
        }

        void UpdateState()
        {
            State state = IsOn ? sfxOn : sfxOff;
            state.snapshot.TransitionTo(0f);
            icon.sprite = state.sprite;
        }
    }
}