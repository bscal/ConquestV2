using Conquest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindManager
{

    public const int NO_SPIN = 0;
    public const int PROGRADE_SPIN = 1;
    public const int RETROGRADE_SPIN = 2;

    public const int NO_WIND = 0;
    public const int LEFT_WIND = 1;
    public const int RIGHT_WIND = 2;

    public static readonly Quaternion LEFT_ROTATION = Quaternion.Euler(new Vector3(0f, 0f, 45f));
    public static readonly Quaternion RIGHT_ROTATION = Quaternion.Euler(new Vector3(0f, 0f, 225f));

    public static readonly Dictionary<int, float> DIR_TO_ROT = new Dictionary<int, float>(9) {
        { -1, 0f },     // NONE
        { 0, 180f },    // E
        { 1, 135f },    // SE
        { 2, 45f },     // SW
        { 3, 0f },      // W
        { 4, 315f },    // NW
        { 5, 225f },    // NE
        { 6, 90f },     // S - The current hex sprite does not have a South/North side
        { 7, 270f }     // N - The current hex sprite does not have a South/North side
    };

    // Spin speed = # of cells;
    public const int EARTHS_CELL_COUNT = 3;

    private int m_rotation;
    private int m_cells;
    private CellLayout m_cellLayout;
    private World m_world;

    public WindManager(int rotation, CellLayout layout, World world)
    {
        m_rotation = rotation;
        m_cellLayout = layout;
        m_cells = layout.cellCount;
        m_world = world;
    }

    public void Init()
    {
        int h = m_world.settings.height;
        int equator = m_world.Equator;

        float counter = 0;
        for (int i = 0; i < m_cells; i++)
        {
            float yN = (h - equator) * (m_cellLayout.cellSize[1, i] - (m_cellLayout.cellSize[0, i])) / 90f;
            float yS = (equator - 0) * (m_cellLayout.cellSize[1, i] - (m_cellLayout.cellSize[0, i])) / 90f;
            Debug.Log(yN);
            foreach (var pair in m_world.tileData)
            {
                if (pair.Key.r > equator && pair.Key.r <= h - equator + Mathf.CeilToInt(yN) + counter && pair.Key.r > h - equator + counter)
                    pair.Value.hexData.cellid = i + 1;
                if (pair.Key.r < equator && pair.Key.r >= h - equator - Mathf.CeilToInt(yS) - counter && pair.Key.r < equator - 0 - counter)
                    pair.Value.hexData.cellid = i + 1;
            }
            counter += Mathf.CeilToInt(yN);
        }

        foreach (var pair in m_world.tileData)
        {
            pair.Value.wind.direction = GetWindDirection(pair.Value.hexData.cellid, pair.Key.r);
        }
    }

    public void SimulateWind(int iteration, SimulatedLoopHex loopedHex)
    {
        bool contains = m_world.TryGetHexData(loopedHex.hex.Neighbor(loopedHex.obj.wind.direction), out TileObject windDirObj);

        if (!contains) return;

        if (windDirObj.hexData.height >= TileMap.Singleton.mountainLvl)
        {
            windDirObj.wind.power = 0;
            windDirObj.wind.waterContent -= .5f;
        }
        else if (loopedHex.obj.hexData.height <= TileMap.Singleton.seaLvl)
        {
            windDirObj.wind.power = 2;
            windDirObj.wind.waterContent += 1;
        }
        else
        {
            windDirObj.wind.power = 1;
            windDirObj.wind.waterContent -= .5f;
        }
    }

    public int GetWindDirection(int cellid, int r)
    {
        if (m_rotation == PROGRADE_SPIN)
            if (r > m_world.Equator)
                return (cellid % 2 == 0) ? (int)HexDirection.NE : (int)HexDirection.SW;
            else if (r < m_world.Equator)
                return (cellid % 2 == 0) ? (int)HexDirection.SE : (int)HexDirection.NW;
        else if (m_rotation == RETROGRADE_SPIN)
            if (r > m_world.Equator)
                return (cellid % 2 == 0) ? (int)HexDirection.SE : (int)HexDirection.NW;
            else if (r < m_world.Equator)
                return (cellid % 2 == 0) ? (int)HexDirection.NE : (int)HexDirection.SW;
        return NO_WIND;
    }

    public Quaternion GetRotationFromWind(int direction)
    {
        if (direction < 0 || direction > DIR_TO_ROT.Count)
            return Quaternion.identity;
        return Quaternion.Euler(new Vector3(0f, 0f, DIR_TO_ROT[direction]));
    }

    public class CellLayout
    {
        public static readonly CellLayout SLOWEST = new CellLayout(
            1,
            12,
            new int[,] { { 0 }, { 90 } }
        );

        public static readonly CellLayout EARTH = new CellLayout(
            3,
            24,
            new int[,] { { 0, 30, 60 }, { 30, 60, 90 } }
        );

        public static readonly CellLayout FAST = new CellLayout(
            7,
            24 * 4,
            new int[,] { { 0, 24, 27, 31, 41, 58, 71 }, { 24, 27, 31, 41, 58, 71, 90 } }
        );

        public static readonly CellLayout FASTEST = new CellLayout(
            5,
            24 * 8,
            new int[,] { { 0, 23, 30, 47, 56 }, { 23, 30, 47, 56, 90 } }
        );

        public readonly int cellCount;
        public readonly int estHourPerDay;
        public readonly int[,] cellSize;

        public CellLayout(int cellCount, int estHourPerDay, int[,] cellSize)
        {
            this.cellCount = cellCount;
            this.estHourPerDay = estHourPerDay;
            this.cellSize = cellSize;
        }
    }
}
