using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame.Gameplay.PowerPuzzle
{
    public class GameControllerHexagonal : GameController
    {
        void Awake()
        {
            destroyVfx = "DestroyVFX_Hex";
        }

        protected override List<Vector2Int> GetAdjacentCoords(Vector2Int coords)
        {
            List<Vector2Int> adjacent = new List<Vector2Int>(base.GetAdjacentCoords(coords));

            if (coords.x % 2 == 0)
            {
                Vector2Int c0 = new Vector2Int(coords.x - 1, coords.y - 1);
                if (c0.x >= 0 && c0.y >= 0)
                    adjacent.Add(c0);

                Vector2Int c1 = new Vector2Int(coords.x + 1, coords.y - 1);
                if (c1.x < field.GetLength(0) && c1.y >= 0)
                    adjacent.Add(c1);
            }
            else
            {
                Vector2Int c0 = new Vector2Int(coords.x - 1, coords.y + 1);
                if (c0.x >= 0 && c0.y < field.GetLength(1))
                    adjacent.Add(c0);

                Vector2Int c1 = new Vector2Int(coords.x + 1, coords.y + 1);
                if (c1.x < field.GetLength(0) && c1.y < field.GetLength(1))
                    adjacent.Add(c1);
            }

            return adjacent;
        }

        protected override Vector2 GetBrickPosition(Vector2 coords)
        {
            Rect rect = fieldTransform.rect;
            Vector2 brickSize = new Vector2
            {
                x = rect.width / bricksCount.x,
                y = rect.height / (bricksCount.y + 0.5f)
            };

            RectTransform brickTransform = brickPrefab.GetComponent<RectTransform>();

            if (Mathf.RoundToInt(coords.x) % 2 == 1)
                coords.y += 0.5f;

            Vector2 brickPosition = Vector2.Scale(coords, brickSize);
            brickPosition += Vector2.Scale(brickSize, brickTransform.pivot);

            return brickPosition;
        }
    }
}
