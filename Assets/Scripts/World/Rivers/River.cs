using Conquest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class River
{
    public static readonly Point NULL_POINT = new Point(double.MinValue, double.MinValue);

    public const int STRAIGHT = 0;
    public const int LEFT = 1;
    public const int RIGHT = 2;

    public Hex start;
    public Hex end;
    public int side;

    public List<Line> path;

    public int riverType;
    public int riverFlow;

    public River(Hex start)
    {
        this.start = start;
        this.path = new List<Line>();
        this.side = LEFT;
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

    public RiverContainer GeneratePath(World world)
    {
        List<TileObject> path = new List<TileObject>();
        List<RiverPath> lines = new List<RiverPath>();
        Hex currentHex = start;
        Hex lastHex = null;
        int corner = Random.Range(0, 5);

        //path.Add(world.tileData[currentHex]);
        while (true)
        {
            TileObject[] tiles = world.HexArrayToTileObjectArray(currentHex.Neightbors());
            LowestPointContainer lowest = FindLowestNeightbor(tiles, world.tileData[currentHex].hexData.height, currentHex, lastHex);
            Debug.Log(currentHex);
            if (lowest.obj != null)
            {
                List<RiverPath> nextPath = LineToNextHex(this, world.tileData[currentHex], lowest.obj, corner, lowest.direction);
                lines.AddRange(nextPath);
                path.Add(world.tileData[currentHex]);
                lastHex = currentHex;
                currentHex = lowest.obj.hex;
                corner = Hex.MirrorCorner(lowest.direction, lowest.direction);
                if (lowest.obj.IsWater)
                    break;
            }
            else
                break;
        }

        return new RiverContainer() {
            tiles = path,
            riverPath = lines
        };
    }

    private LowestPointContainer FindLowestNeightbor(TileObject[] hexes, float currentHeight, Hex current, Hex lastHex)
    {
        float lowestHeight = currentHeight;
        TileObject lowest = null;
        int direction = -1;
        for (int i = 0; i < hexes.Length; i++)
        {
            TileObject next = hexes[i];
            if (!next.hex.Equals(current) && next.hexData.height < lowestHeight)
            {
                if (lastHex != null && next.hex.Equals(lastHex))
                    continue;

                lowestHeight = next.hexData.height;
                lowest = next;
                direction = i;
            }
        }
        return new LowestPointContainer() {
            obj = lowest,
            direction = direction,
        };
    }

    private List<RiverPath> LineToNextHex(River river, TileObject from, TileObject to, int startDir, int dir)
    {
        List<RiverPath> lines = new List<RiverPath>();

        List<int> sides = ClosestCorner(startDir, dir, river.side);

        foreach (int side in sides)
        {
            lines.Add(new RiverPath() {
                from = from,
                to = to,
                direction = dir,
                corner = side
            });
        }
        return lines;
    }

    private List<int> ClosestCorner(int fromDirection, int direction, int side)
    {
        List<int> directions = new List<int>();
        //directions.Add(fromDirection);

        while (fromDirection != direction)
        {
            Debug.Log(fromDirection);
            directions.Add(fromDirection);
            fromDirection = (side == LEFT) ? HexConstants.Subtract(fromDirection, 1) : HexConstants.Add(fromDirection, 1);
            
        }

        return directions;
    }

}

public class RiverContainer
{
    public List<TileObject> tiles;
    public List<RiverPath> riverPath;
}

public class RiverPath
{
    public TileObject from;
    public TileObject to;
    public int corner;
    public int direction;
}

public class LowestPointContainer
{
    public TileObject obj;
    public int direction;
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
