using System;
using System.Collections.Generic;
using System.Linq;

namespace CG
{
    public enum Direction
    {
        N = 0,
        NE = 1,
        E = 2,
        SE = 3,
        S = 4,
        SW = 5,
        W = 6,
        NW = 7
    }

    public static class Directions
    {
        public static readonly Direction[] All8 =
        {
            Direction.N, Direction.NE, Direction.E, Direction.SE, Direction.S, Direction.SW, Direction.W, Direction.NW
        };

        public static bool IsNear8To(this Vec a, Vec b)
        {
            return Math.Abs(a.X - b.X) <= 1 && Math.Abs(a.Y - b.Y) <= 1;
        }

        public static IEnumerable<Direction> PushDirections(this Direction targetDir)
        {
            for (var d = (int) targetDir - 1; d <= (int) targetDir + 1; d++)
            {
                var pushDir = (Direction) ((d + 8) % 8);
                yield return pushDir;
            }
        }
    }
}
