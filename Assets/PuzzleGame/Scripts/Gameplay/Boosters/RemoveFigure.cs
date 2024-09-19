namespace PuzzleGame.Gameplay.Boosters
{
    public static class RemoveFigure
    {
        public static void Execute(FigureController figureController)
        {
            foreach (Brick brick in figureController.bricks)
                brick.Destroy();

            figureController.bricks.Clear();
            figureController.ResetPosition();
        }
    }
}