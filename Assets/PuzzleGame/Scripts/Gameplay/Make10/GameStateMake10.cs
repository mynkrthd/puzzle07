using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame.Gameplay.Make10
{
    [Serializable]
    public class GameStateMake10 : GameState
    {
        [SerializeField] 
        List<Make10Save> saves = new List<Make10Save>();
    
        [SerializeField]
        float[] figureRotations = new float[0];
    
        [SerializeField]
        int[] figures = new int[0];
    
        [SerializeField]
        FigureState[] figureStates = new FigureState[0];

        public void SetFigures(int[] value, FigureState[] indexes, float[] rotations)
        {
            figures = (int[]) value.Clone();
            figureStates = (FigureState[])indexes.Clone();
            figureRotations = (float[]) rotations.Clone();
        }

        public int[] GetFigures()
        {
            return (int[]) figures.Clone();
        }
    
        public FigureState[] GetFiguresIndexes()
        {
            return (FigureState[])figureStates.Clone();
        }

        public float[] GetFigureRotations()
        {
            return (float[]) figureRotations.Clone();
        }

        public override void SaveGameState()
        {
            base.SaveGameState();
        
            Make10Save save = new Make10Save
            {
                figures = GetFigures(),
                figureRotations = GetFigureRotations(),
                figureStates = GetFiguresIndexes()
            };

            saves.Add(save);
        }

        public override bool UndoGameState()
        {
            if (!base.UndoGameState())
                return false;
        
            Make10Save save = saves[saves.Count - 1];
            saves.RemoveAt(saves.Count - 1);

            figures = save.figures;
            figureRotations = save.figureRotations;
            figureStates = save.figureStates;

            return true;
        }

        public override void ClearSave()
        {
            base.ClearSave();
            saves.Clear();
        }
    }

    [Serializable]
    public class FigureState
    {
        [SerializeField]
        public int[] State = new int[0];
    }

    [Serializable]
    public class Make10Save
    {
        [SerializeField]
        public float[] figureRotations = new float[0];
    
        [SerializeField]
        public int[] figures = new int[0];
    
        [SerializeField]
        public FigureState[] figureStates = new FigureState[0];
    }
}