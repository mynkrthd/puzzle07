using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PuzzleGame.Gameplay
{
    public static class WaveAlgorithm
    {
        class PathNode
        {
            public readonly Vector2Int coords;
            public readonly PathNode previous;

            public PathNode(Vector2Int coords, PathNode previous)
            {
                this.coords = coords;
                this.previous = previous;
            }
        }

        public static List<Vector2Int> GetArea<T>(T[,] field, Vector2Int start,
            Func<Vector2Int, IEnumerable<Vector2Int>> adjacent, Predicate<T> predicate)
        {
            var area = new List<Vector2Int> { start };

            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var coords = queue.Dequeue();

                foreach (var c in adjacent.Invoke(coords))
                    if (!area.Contains(c) && predicate(field[c.x, c.y]))
                    {
                        queue.Enqueue(c);
                        area.Add(c);
                    }
            }

            return area;
        }

        public static List<Vector2Int> GetPath<T>(T[,] field, Vector2Int start, Vector2Int end,
            Func<Vector2Int, IEnumerable<Vector2Int>> adjacent, Predicate<T> predicate)
        {
            var path = new List<Vector2Int>();

            var linkedPath = new List<PathNode> { new PathNode(start, null) };

            var queue = new Queue<PathNode>();
            queue.Enqueue(linkedPath[0]);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                if (node.coords == end)
                {
                    while (node.previous != null)
                    {
                        path.Add(node.coords);
                        node = node.previous;
                    }

                    path.Add(node.coords);
                    path.Reverse();

                    return path;
                }

                foreach (var c in adjacent.Invoke(node.coords))
                    if (linkedPath.All(n => n.coords != c) && predicate(field[c.x, c.y]))
                    {
                        linkedPath.Add(new PathNode(c, node));
                        queue.Enqueue(linkedPath[linkedPath.Count - 1]);
                    }
            }

            return path;
        }
    }
}