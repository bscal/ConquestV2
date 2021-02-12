using Conquest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class River
{
    public static readonly Point NULL_POINT = new Point(double.MinValue, double.MinValue);

    public Hex start;
    public Hex end;

    public List<Line> path;

    public int riverType;
    public int riverFlow;

    public River(Hex start)
    {
        this.start = start;
        this.path = new List<Line>();
    }

    private int FindLowestPoint(TileObject[] hexes, World world)
    {
        int corner = -1;
        float lowest = float.MaxValue;

        for (int i = 0; i < hexes.Length; i++)
        {
            if (hexes[i] == null)
                continue;

            float height = hexes[i].hexData.height;

            // Moves index + 1 getting the next tile. If 5 goes to 0 instead of 6. (Hexes are 6 sided)
            int nextIndex = i + ((i > 5) ? 0 : 1);
            if (hexes[nextIndex] != null)
            {
                height += hexes[nextIndex].hexData.height;
                height /= 2;
            }

            if (height < lowest)
            {
                lowest = height;
                corner = i;
            }
        }
        return corner;
    }

    public List<TileObject> GeneratePath(World world)
    {
        List<TileObject> path = new List<TileObject>();
        Hex currentHex = start;
        Hex lastHex = null;
        //int startCorner = FindLowestPoint(world.HexArrayToTileObjectArray(currentHex.Neightbors()), world);
        //Hex sideHex = currentHex.Neighbor(Random.Range(0);

        while (true)
        {
            TileObject[] tiles = world.HexArrayToTileObjectArray(currentHex.Neightbors());

            TileObject lowest = FindLowestNeightbor(tiles, world.tileData[currentHex].hexData.height, currentHex, lastHex);
            Debug.Log(currentHex);
            if (lowest != null)
            {
                path.Add(lowest);
                lastHex = currentHex;
                currentHex = lowest.hex;

                if (lowest.IsWater)
                    break;
            }
            else
                break;
        }

        return path;
    }

    private TileObject FindLowestNeightbor(TileObject[] hexes, float currentHeight, Hex current, Hex lastHex)
    {
        float lowestHeight = currentHeight;
        TileObject lowest = null;
        foreach (TileObject next in hexes)
        {
            if (!next.hex.Equals(current) && next.hexData.height < lowestHeight)
            {
                if (lastHex != null && next.hex.Equals(lastHex))
                    continue;

                lowestHeight = next.hexData.height;
                lowest = next;
            }
        }
        return lowest;
    }




}

public class HexPoint
{

    public const int SE         = 0;
    public const int NE         = 1;
    public const int N          = 2;
    public const int NW         = 3;
    public const int SW         = 4;
    public const int S          = 5;
    public const int CENTER     = 6;

}
