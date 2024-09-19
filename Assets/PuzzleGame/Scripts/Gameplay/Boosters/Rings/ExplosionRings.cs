using PuzzleGame.Gameplay.Rings;

namespace PuzzleGame.Gameplay.Boosters.Rings
{
    public static class ExplosionRings
    {
        public static void Execute(RingsController[,] rings)
        {
            rings[1, 1].Clear();
        } 
    }
}