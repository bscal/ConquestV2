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
    public MapGenerator Generator => m_generator;
    public World World => m_generator.GetWorld();
    public WorldSettings WorldSettings => m_generator.GetWorld().settings;
    public SpriteManager SpriteManager => m_spriteManager;

    [Header("Generator")]
    [SerializeField]
    private MapGenerator m_generator;
    [Header("Sprites")]
    [SerializeField]
    private SpriteManager m_spriteManager;
    [Header("Debug")]
    [SerializeField]
    private DebugCommands m_cmds;

    private UIGeneratorDebugger m_UIGenDebug;
    private Controls m_controls;
    private HexFilter m_currentFilter = HexFilter.NONE;

    private void Awake()
    {
        Singleton = this;
        GameState = new GameState();
        m_UIGenDebug = GameObject.Find("GeneratorUI").GetComponent<UIGeneratorDebugger>();
        m_controls = new Controls();
        m_controls.Enable();
    }

    void Start()
    {
        m_generator.CreateWorld();
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
                obj.SetFilter(m_currentFilter);
            }
        }

       
    }

    public void ChangeFilter()
    {
        int id = (int)m_currentFilter;
        if (id == Enum.GetNames(typeof(HexFilter)).Length - 1) id = 0;
        else id++;
        m_currentFilter = (HexFilter)id;
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

    public DebugCommands GetCommands()
    {
        return m_cmds;
    }
}
