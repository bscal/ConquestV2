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
    public int riverWidth;
    public bool reachedWater;

    public River(Hex start)
    {
        this.start = start;
        this.path = new List<Line>();
        this.side = LEFT;
    }

    public RiverContainer GeneratePath(World world)
    {
        List<TileObject> path = new List<TileObject>();
        List<RiverPath> lines = new List<RiverPath>();
        Hex currentHex = start;
        Hex lastHex = null;
        int startCorner = Random.Range(0, 5);

        while (true)
        {
            TileObject[] tiles = world.HexArrayToTileObjectArray(currentHex.Neightbors());
            LowestPointContainer lowest = FindLowestNeightbor(tiles, world.TileData[currentHex].hexData.height, currentHex, lastHex);

            if (lowest.obj != null)
            {
                List<RiverPath> nextPath = LineToNextHex(this, world.TileData[currentHex], lowest.obj, startCorner, lowest.direction);
                lines.AddRange(nextPath);
                path.Add(world.TileData[currentHex]);
                lastHex = currentHex;
                currentHex = lowest.obj.hex;
                startCorner = Hex.MirrorCorner(lowest.direction, lowest.direction);

                if (lowest.obj.IsWater)
                {
                    reachedWater = true;
                    break;
                }
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

    private List<RiverPath> LineToNextHex(River river, TileObject from, TileObject to, int startCorner, int dir)
    {
        List<RiverPath> lines = new List<RiverPath>();

        List<int> sides = ClosestCorner(startCorner, dir, river.side);

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

    private List<int> ClosestCorner(int startCorner, int direction, int side)
    {
        List<int> directions = new List<int>();

        while (startCorner != direction)
        {
            directions.Add(startCorner);
            startCorner = (side == LEFT) ? HexConstants.Subtract(startCorner, 1) : HexConstants.Add(startCorner, 1);
            
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
