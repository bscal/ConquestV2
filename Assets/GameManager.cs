﻿using Conquest;
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

            var obj = World.tileData[hex.GetKey()];
            var hData = obj.hexData;
            var pl = World.GetPlateByID(hData.plateId);
            m_UIGenDebug.hexInfoText.text = $"Hex={hex.q}/{hex.r}/{hex.s} | {coord.col}/{coord.row}";
            m_UIGenDebug.tileObjText.text = $"h={Math.Round(hData.height, 2)},t={hData.temp},tid={obj.tileId}";
            m_UIGenDebug.movementText.text = $"m={hData.moved},e={hData.empty},d={pl.direction}";
            m_UIGenDebug.plateDataText.text = $"id={hData.plateId},s={Math.Round(pl.movementSpeed, 1)},e={Math.Round(pl.elevation, 0)}";
            m_UIGenDebug.plateData2Text.text = $"o={hData.isOcean},c={hData.isCoast},";
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
