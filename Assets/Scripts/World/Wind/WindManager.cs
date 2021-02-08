using Conquest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindManager
{

    public const int NO_SPIN = 0;
    public const int PROGRADE_SPIN = 1;
    public const int RETROGRADE_SPIN = 2;

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
    }

    void Start()
    {
        
    }

    void Update()
    {
        
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
