using System.Collections;
using System.Collections.Generic;

namespace Conquest
{
    public class HexLibUtils
    {

    }

    public struct Line
    {
        public readonly Point pt0;
        public readonly Point pt1;

        public Line(Point pt0, Point pt1)
        {
            this.pt0 = pt0;
            this.pt1 = pt1;
        }

        public Point GetCenter() => new Point(pt0.x - pt1.x, pt0.y - pt1.y);
    }
}
