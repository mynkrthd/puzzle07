using PuzzleGame.Gameplay.Rings;

namespace PuzzleGame.Gameplay.Boosters.Rings
{
    public static class ClearCrossLinesRings
    {
        public static void Execute(RingsController[,] rings)
        {
            int fieldSizeX = rings.GetLength(0);
            int fieldSizeY = rings.GetLength(1);
        
            for (int x = 0; x < fieldSizeX; x++)
                rings[x, 1].Clear();

            for (int y = 0; y < fieldSizeY; y++)
                rings[1, y].Clear();
        }
    }
}