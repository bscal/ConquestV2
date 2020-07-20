using Conquest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World
{
    public readonly Layout layout;
    public readonly Dictionary<string, TileObject> tileData;

    public readonly List<Plate> plates;

    public World()
    {
        layout = new Layout(Layout.pointy, new Point(8, 8), new Point(0, 0));
        tileData = new Dictionary<string, TileObject>();
        plates = new List<Plate>(WorldSettings.Singleton.plates);
    }

}
