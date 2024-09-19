using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame.Gameplay.Bricks2048
{
    [Serializable]
    public class GameState2048Bricks : GameState
    {
        [SerializeField] 
        List<Save2048Bricks> saves = new List<Save2048Bricks>();

        [SerializeField]
        Vector2Int currentBrickCoordsCoords;
        [SerializeField]
        int currentBrick;
        [SerializeField]
        int nextBrick;

        public Vector2Int CurrentBrickCoords
        {
            get => currentBrickCoordsCoords;
            set => currentBrickCoordsCoords = value;
        }

        public int NextBrick
        {
            get => nextBrick;
            set => nextBrick = value;
        }

        public int CurrentBrick
        {
            get => currentBrick;
            set => currentBrick = value;
        }
    
        public override void SaveGameState()
        {
            base.SaveGameState();
        
            Save2048Bricks save = new Save2048Bricks
            {
                currentBrick = CurrentBrick,
                nextBrick = NextBrick
            };

            saves.Add(save);
        }

        public override bool UndoGameState()
        {
            if (!base.UndoGameState())
                return false;
        
            Save2048Bricks save = saves[saves.Count - 1];
            saves.RemoveAt(saves.Count - 1);

            CurrentBrick = save.currentBrick;
            NextBrick = save.nextBrick;

            return true;
        }

        public override void ClearSave()
        {
            base.ClearSave();
            saves.Clear();
        }
    }

    [Serializable]
    public class Save2048Bricks
    {
        [SerializeField]
        public int currentBrick;
    
        [SerializeField]
        public int nextBrick;
    }
}