using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame.Sounds
{
    public class SoundCollection : MonoBehaviour
    {
        public List<SoundItem> collection;

        public PlaySfx GetSfx(SoundId id)
        {
            var result = collection.Find(c => c.soundId == id);
            return result?.source;
        }
    }

    [Serializable]
    public class SoundItem
    {
        public SoundId soundId;
        public PlaySfx source;
    }
}