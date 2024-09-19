using PuzzleGame.Gameplay.Rings;

namespace PuzzleGame.Gameplay.Boosters.Rings
{
    public class ClearVerticalLinesRings
    {
        public static void Execute(RingsController[,] rings, int coordX)
        {
            int fieldSizeY = rings.GetLength(1);

            for (int y = 0; y < fieldSizeY; y++)
                rings[coordX, y].Clear();
        }
    }
}