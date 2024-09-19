using System.Collections.Generic;
using System.Linq;

namespace PuzzleGame.Gameplay
{
    public class BricksHighlighter
    {
        readonly List<Brick> highlightedBricks = new();
        readonly Dictionary<NumberedBrick, int> highlightedNumberedBricks = new();

        public void SetHighlight(Brick[] bricks)
        {
            var toUnhighlight = highlightedBricks.Where(b => !bricks.Contains(b)).ToArray();
            foreach (var brick in toUnhighlight)
            {
                Unhighlight(brick);
            }

            var toHighlight = bricks.Where(b => !highlightedBricks.Contains(b)).ToArray();
            foreach (var brick in toHighlight)
            {
                Highlight(brick);
            }
        }

        public void SetHighlight(NumberedBrick[] bricks, int colorIndex)
        {
            var toUnhighlight = highlightedNumberedBricks.Keys.Where(b => !bricks.Contains(b)).ToArray();
            foreach (var brick in toUnhighlight)
            {
                Unhighlight(brick);
            }

            var toHighlight = bricks.Where(b => !highlightedNumberedBricks.ContainsKey(b)).ToArray();
            foreach (var brick in toHighlight)
            {
                Highlight(brick, colorIndex);
            }
        }

        public void UnhighlightBricks()
        {
            foreach (var brick in highlightedBricks)
                brick.Highlight(false);

            highlightedBricks.Clear();
        }

        public void UnhighlightNumberedBricks()
        {
            foreach (var (brick, colorIndex) in highlightedNumberedBricks)
                brick.ColorIndex = colorIndex;

            highlightedNumberedBricks.Clear();
        }

        void Highlight(Brick brick)
        {
            if (highlightedBricks.Contains(brick))
                return;

            brick.Highlight(true);
            highlightedBricks.Add(brick);
        }

        void Highlight(NumberedBrick brick, int colorIndex)
        {
            highlightedNumberedBricks[brick] = brick.ColorIndex;
            brick.ColorIndex = colorIndex;
        }

        void Unhighlight(Brick brick)
        {
            brick.Highlight(false);
            highlightedBricks.RemoveAll(b => b == brick);
        }

        public void Unhighlight(NumberedBrick brick)
        {
            if (highlightedNumberedBricks.TryGetValue(brick, out var colorIndex))
                brick.ColorIndex = colorIndex;

            highlightedNumberedBricks.Remove(brick);
        }
    }
}
