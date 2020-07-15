using Conquest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World
{

    public WorldSettings settings;

    public Dictionary<string, TileObject> tileData;

    public List<Plate> plates;

    public World(WorldSettings settings)
    {
        this.settings = settings;
        tileData = new Dictionary<string, TileObject>();
        plates = new List<Plate>(WorldSettings.Singleton.plates);
    }

}
