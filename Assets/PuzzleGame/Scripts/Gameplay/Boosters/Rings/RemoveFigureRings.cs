using PuzzleGame.Gameplay.Rings;

namespace PuzzleGame.Gameplay.Boosters.Rings
{
    public static class RemoveFigureRings
    {
        public static void Execute(NextRingController nextRing)
        {
            nextRing.Clear();
        }
    }
}
