using PuzzleGame.Gameplay.Merged;

namespace PuzzleGame.Gameplay.Boosters.Merged
{
    public static class RemoveFigureMerged
    {
        public static void Execute(MergedFigureController nextBrick)
        {
            foreach (Brick brick in nextBrick.bricks)
                brick.Destroy();

            nextBrick.bricks.Clear();
            nextBrick.ResetPosition();
        }
    }
}
