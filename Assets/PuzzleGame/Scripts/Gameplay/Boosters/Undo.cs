using System;

namespace PuzzleGame.Gameplay.Boosters
{
    public static class Undo
    {
        public static void ClearGame<T>(T[,] field) where T : Brick
        {
            int fieldSizeX = field.GetLength(0);
            int fieldSizeY = field.GetLength(1);
        
            for (int x = 0; x < fieldSizeX; x++)
            {
                for (int y = 0; y < fieldSizeY; y++)
                {
                    if (field[x, y] == null) continue;
                
                    field[x,y].Destroy();
                }
            }
        }

        public static bool Execute(GameState gameState, Action onComplete)
        {
            bool result = gameState.UndoGameState();
        
            onComplete?.Invoke();

            UserProgress.Current.SaveGameState(UserProgress.Current.CurrentGameId);
            UserProgress.Current.Save();

            return result;
        }
    }
}