using Conquest;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton { get; private set; }

    [Header("Generator")]
    public MapGenerator generator;

    public World World { get; private set; }

    public HexFilter currentFilter = HexFilter.NONE;

    private UIGeneratorDebugger m_UIGenDebug;

    void Start()
    {
        Singleton = this;
        World = generator.CreateWorld();
        m_UIGenDebug = GameObject.Find("GeneratorUI").GetComponent<UIGeneratorDebugger>();
    }

    void Update()
    {

        if (Input.GetMouseButtonDown((int)MouseButton.LeftMouse))
        {
            var p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Hex hex = World.layout.PixelToHex(new Point(p.x, p.y)).HexRound();
            if (!World.ContainsHex(hex)) return;
            foreach (Hex h in hex.Ring(2))
            {
                bool res = World.TryGetHexData(h, WorldSettings.Singleton.wrapWorld, out TileObject data);
                if (!res) continue;
                data.render.sprite = generator.tiles[3];
            }
        }
        if (Input.GetMouseButtonDown((int)MouseButton.RightMouse))
        {
            var p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Hex hex = World.layout.PixelToHex(new Point(p.x, p.y)).HexRound();
            OffsetCoord coord = OffsetCoord.RoffsetFromCube(WorldSettings.Singleton.offset, hex);
            Hex wrap = HexUtils.WrapOffset(hex, World.size.x);
            OffsetCoord wcoord = OffsetCoord.RoffsetFromCube(WorldSettings.Singleton.offset, wrap);
            print(string.Format("WrappedHex: {0}, {1}, ", wrap.q, wrap.r));
            print(string.Format("WrappedCoords: {0}, {1}, ", wcoord.col, wcoord.row));

            var data = World.tileData[hex.GetKey()];
            var pl = World.GetPlateByID(data.plateId);
            m_UIGenDebug.hexInfoText.text = $"Hex={hex.q}/{hex.r}/{hex.s} | {coord.col}/{coord.row}";
            m_UIGenDebug.tileObjText.text = $"h={Math.Round(data.height, 2)},t={data.temp},tid={data.tileId}";
            m_UIGenDebug.movementText.text = $"mv={data.moved},emp={data.empty},d={pl.direction}";
            m_UIGenDebug.plateDataText.text = $"p={data.plateId},ms={Math.Round(pl.movementSpeed, 1)},el={Math.Round(pl.elevation, 0)}";
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            GameManager.Singleton.ChangeFilter();
            foreach (var obj in World.tileData.Values)
            {
                obj.SetFilter(GameManager.Singleton.currentFilter);
            }
        }

    }

    public void ChangeFilter()
    {
        int id = (int)currentFilter;
        if (id == Enum.GetNames(typeof(HexFilter)).Length - 1) id = 0;
        else id++;
        currentFilter = (HexFilter)id;
    }

}
