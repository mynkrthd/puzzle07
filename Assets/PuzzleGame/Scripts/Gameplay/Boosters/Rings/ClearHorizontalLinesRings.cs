using PuzzleGame.Gameplay.Rings;

namespace PuzzleGame.Gameplay.Boosters.Rings
{
    public static class ClearHorizontalLinesRings
    {
        public static void Execute(RingsController[,] rings, int coordY)
        {
            int fieldSizeX = rings.GetLength(0);

            for (int x = 0; x < fieldSizeX; x++)
                rings[x, coordY].Clear();
        }
    }
}