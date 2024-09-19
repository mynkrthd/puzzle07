using System.Collections.Generic;
using System.Linq;
using PuzzleGame.Gameplay.Puzzle1010;
using UnityEngine;

namespace PuzzleGame.Gameplay.Doku
{
    public class GameControllerDoku : GameController1010
    {
        protected override Brick SpawnEmptyBrick(Vector2Int coords)
        {
            var brick = base.SpawnEmptyBrick(coords);
            brick.ColorIndex = coords.x / 3 % 2 == coords.y / 3 % 2 ? 1 : 0;
            return brick;
        }

        protected override Vector2Int[] GetCompleteLines()
        {
            var linesBricks = new List<Vector2Int>(base.GetCompleteLines());

            for (var offsetX = 0; offsetX < 9; offsetX += 3)
            {
                for (var offsetY = 0; offsetY < 9; offsetY += 3)
                {
                    var coords = GetBlockCoords(new Vector2Int(offsetX, offsetY));

                    if (coords.All(c => field[c.x, c.y] != null))
                        linesBricks.AddRange(coords);
                }
            }

            return linesBricks.Distinct().ToArray();
        }

        Vector2Int[] GetBlockCoords(Vector2Int offset)
        {
            var coords = new Vector2Int[9];

            for (var i = 0; i < 9; i++)
                coords[i] = offset + new Vector2Int(i / 3, i % 3);

            return coords;
        }
    }
}