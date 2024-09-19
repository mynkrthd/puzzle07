using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame.Gameplay
{
    [Serializable]
    public class GameState
    {
        public event Action StateUpdate;

        [SerializeField]
        int score;
        [SerializeField]
        int topScore;

        [SerializeField]
        int[] field = new int[0];

        [SerializeField]
        int[] nextBricks = new int[0];

        [SerializeField]
        public List<GameSave> fieldSaves = new List<GameSave>();
    
        [SerializeField]
        bool isGameOver;

        [SerializeField]
        string themeId;

        public int Score
        {
            get => score;
            set
            {
                score = value;

                if (score > topScore)
                    topScore = score;

                StateUpdate?.Invoke();
            }
        }

        public int TopScore => topScore;

        public bool IsGameOver
        {
            get => isGameOver;
            set => isGameOver = value;
        }

        public string ThemeId
        {
            get => themeId;
            set => themeId = value;
        }

        public int[] GetField()
        { 
            return (int[]) field.Clone();
        }

        public void SetField(int[] value)
        {
            field = (int[]) value.Clone();
        }

        public int[] GetNextBricks()
        {
            return (int[]) nextBricks.Clone();
        }

        public void SetNextBricks(int[] value)
        {
            nextBricks = (int[]) value.Clone();
        }

        public virtual void SaveGameState()
        {
            GameSave save = new GameSave
            {
                score = Score,
                field = GetField(),
                nextBricks = GetNextBricks()
            };

            fieldSaves.Add(save);
        }
    
        public virtual bool UndoGameState()
        {
            if (fieldSaves.Count == 0) return false;

            GameSave save = fieldSaves[fieldSaves.Count - 1];
            fieldSaves.RemoveAt(fieldSaves.Count - 1);

            score = save.score;
            field = save.field;
            nextBricks = save.nextBricks;
            isGameOver = false;
        
            return true;
        }
    
        public void Reset()
        {
            isGameOver = true;
            ClearSave();
        }

        public virtual void ClearSave()
        {
            fieldSaves.Clear();
        }
    }

    [Serializable]
    public class GameSave
    {
        [SerializeField]
        public int score;
    
        [SerializeField]
        public int[] field = new int[0];
    
        [SerializeField]
        public int[] nextBricks = new int[0];
    }
}