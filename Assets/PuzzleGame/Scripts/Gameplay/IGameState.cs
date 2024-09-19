namespace PuzzleGame.Gameplay
{
    public interface IGameState<T> where T : GameState
    {
        T GameState { get; }
    }
}
