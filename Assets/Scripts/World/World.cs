using Conquest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conquest {
    public class World
    {
        public readonly Layout layout;
        public readonly Vector2Int size;
        public readonly Dictionary<string, TileObject> tileData;

        public readonly List<Plate> plates;

        public World(int w, int h)
        {
            layout = new Layout(Layout.pointy, new Point(8, 8), new Point(0, 0));
            size = new Vector2Int(w, h);
            tileData = new Dictionary<string, TileObject>();
            plates = new List<Plate>(WorldSettings.Singleton.plates);
        }

    }
}
