using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame.Gameplay.Puzzle1010
{
    [Serializable]
    public class GameState1010 : GameState
    {
        [SerializeField] 
        List<Save1010> saves = new List<Save1010>();
    
        [SerializeField]
        int[] figures = new int[0];
        [SerializeField]
        float[] figureRotations = new float[0];
        [SerializeField]
        int[] figureIndexes = new int[0];

        public void SetFigures(int[] value, int[] indexes, float[] rotations)
        {
            figures = (int[]) value.Clone();
            figureIndexes = (int[]) indexes.Clone();
            figureRotations = (float[]) rotations.Clone();
        }

        public int[] GetFigures()
        {
            return (int[]) figures.Clone();
        }

        public float[] GetFigureRotations()
        {
            return (float[]) figureRotations.Clone();
        }
    
        public int[] GetFigureIndexes()
        {
            return (int[]) figureIndexes.Clone();
        }

        public override void SaveGameState()
        {
            base.SaveGameState();
        
            Save1010 save = new Save1010
            {
                figures = GetFigures(),
                figureRotations = GetFigureRotations(),
                figureIndexes = GetFigureIndexes()
            };

            saves.Add(save);
        }

        public override bool UndoGameState()
        {
            if (!base.UndoGameState())
                return false;
        
            Save1010 save = saves[saves.Count - 1];
            saves.RemoveAt(saves.Count - 1);

            figures = save.figures;
            figureRotations = save.figureRotations;
            figureIndexes = save.figureIndexes;

            return true;
        }

        public override void ClearSave()
        {
            base.ClearSave();
            saves.Clear();
        }
    }

    [Serializable]
    public class Save1010
    {
        [SerializeField]
        public int[] figures = new int[0];
    
        [SerializeField]
        public float[] figureRotations = new float[0];
    
        [SerializeField]
        public int[] figureIndexes = new int[0];
    }
}