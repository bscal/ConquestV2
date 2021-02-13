using Conquest;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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
    private DebugCommands m_cmds;

    private Controls m_controls;

    void Start()
    {
        Singleton = this;
        GameState = new GameState();
        generator.CreateWorld();
        m_UIGenDebug = GameObject.Find("GeneratorUI").GetComponent<UIGeneratorDebugger>();
        m_controls = new Controls();
        m_controls.Enable();
        m_cmds = new DebugCommands();
    }

    void Update()
    {

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            var p = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y));
            Hex hex = World.layout.PixelToHex(new Point(p.x, p.y)).HexRound();
            if (!World.ContainsHex(hex)) return;
            foreach (Hex h in hex.Ring(2))
            {
                bool res = World.TryGetHexData(h, out TileObject data);
                if (!res) continue;
                data.render.sprite = TileMap.Singleton.GetTileByName("desert").sprite;
            }
        }


        if (Keyboard.current.kKey.wasPressedThisFrame)
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

    public UIGeneratorDebugger GetDebugger()
    {
        return m_UIGenDebug;
    }

}
