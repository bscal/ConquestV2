﻿using Conquest;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton { get; private set; }

    [Header("Generator")]
    public MapGenerator generator;

    public World World { get; private set; }

    public HexFilter currentFilter = HexFilter.NONE;

    void Start()
    {
        Singleton = this;
        World = generator.CreateWorld();
    }

    void Update()
    {

        if (Input.GetMouseButtonDown((int)MouseButton.LeftMouse))
        {
            var p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Hex hex = World.layout.PixelToHex(new Point(p.x, p.y)).HexRound();
            foreach (Hex h in hex.Ring(3))
            {
                World.tileData[h.GetKey()].render.sprite = generator.tiles[3];
            }
        }
        if (Input.GetMouseButtonDown((int)MouseButton.RightMouse))
        {
            var p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Hex hex = World.layout.PixelToHex(new Point(p.x, p.y)).HexRound();
            print(string.Format("PixelToHex: {0}, {1}, ", hex.q, hex.r));
            print(World.tileData[hex.GetKey()].height);
            print(World.tileData[hex.GetKey()].plateId);
            print(World.tileData[hex.GetKey()].isPlateEdge);
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
