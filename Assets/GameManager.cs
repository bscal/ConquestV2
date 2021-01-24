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

    public GameState GameState { get; private set; }

    [Header("Generator")]
    public MapGenerator generator;

    public World World => generator.GetWorld();
    public WorldSettings WorldSettings => generator.GetWorld().settings;

    public HexFilter currentFilter = HexFilter.NONE;

    private UIGeneratorDebugger m_UIGenDebug;

    void Start()
    {
        Singleton = this;
        GameState = new GameState();
        generator.CreateWorld();
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
                bool res = World.TryGetHexData(h, out TileObject data);
                if (!res) continue;
                data.render.sprite = TileMap.Singleton.GetTileByName("desert").sprite;
            }
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

    public IEnumerable CalculateUpdateCoroutine()
    {
        const float WAIT = 1 / 20;
        while (true)
        {
            World?.UpdateValues();
            yield return new WaitForSeconds(WAIT);
        }
    }

}
