using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame.Gameplay.Rings
{
    [Serializable]
    public class GameStateRings : GameState
    {
        [SerializeField] 
        public List<RingSave> saves = new List<RingSave>();
        [SerializeField]
        RingState[] rings = new RingState[0];
        [SerializeField]
        int[] nextRing = new int[0];
        [SerializeField]
        int colors;
    
        public int Colors
        {
            get => colors;
            set => colors = value;
        }

        public void SetRings(RingState[] value)
        {
            rings = (RingState[])value.Clone();
        }

        public RingState[] GetRings()
        {
            return (RingState[])rings.Clone();
        }

        public void SetNextRings(int[] value)
        {
            nextRing = (int[]) value.Clone();
        }

        public int[] GetNextRings()
        {
            return (int[]) nextRing.Clone();
        }

        public override void SaveGameState()
        {
            base.SaveGameState();
        
            var save = new RingSave
            {
                rings = GetRings(),
                nextRing = GetNextRings()
            };

            saves.Add(save);
        }

        public override bool UndoGameState()
        {
            if (saves.Count == 0 || !base.UndoGameState())
                return false;

            RingSave save = saves[saves.Count - 1];
            saves.RemoveAt(saves.Count - 1);

            rings = save.rings;
            nextRing = save.nextRing;

            return true;
        }

        public override void ClearSave()
        {
            base.ClearSave();
            saves.Clear();
        }
    }

    [Serializable]
    public class RingState
    {
        [SerializeField]
        public int[] State = new int[0];
    }

    [Serializable]
    public class RingSave
    {
        [SerializeField]
        public RingState[] rings = new RingState[0];
    
        [SerializeField]
        public int[] nextRing = new int[0];
    }
}