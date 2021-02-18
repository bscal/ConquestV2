using Conquest;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIGeneratorDebugger : MonoBehaviour
{
    private const string PANEL_TITLE = "Selected Hex: ";

    public GameObject hexPanel;
    public GameObject hexDataPanel;
    public GameObject hexDataLeftPanel;
    public GameObject hexDataRightPanel;
    public Text hexText;

    public GameObject textPrefab;

    public Text counterText;
    public UnityEngine.UI.Button pauseButton;
    public UnityEngine.UI.Button stepButton;
    public Text hexInfoText;
    public Text tileObjText;
    public Text movementText;
    public Text plateDataText;
    public Text plateData2Text;

    private Hex m_watchedHex;
    private bool m_show;

    private Dictionary<string, Text> m_hexUIValues = new Dictionary<string, Text>();

    private void Start()
    {
        pauseButton.onClick.AddListener(OnPauseClicked);
        stepButton.onClick.AddListener(OnStepClicked);
    }

    void Update()
    {
        counterText.text = $"{GameManager.Singleton.Generator.GetCurrentIteration()}/{GameManager.Singleton.WorldSettings.numberOfIterations}";

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            var p = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y));
            Hex hex = GameManager.Singleton.World.layout.PixelToHex(new Point(p.x, p.y)).HexRound();
            OffsetCoord coord = OffsetCoord.RoffsetFromCube(WorldSettings.OFFFSET_COORD, hex);
            Hex wrap = HexUtils.WrapOffset(hex, GameManager.Singleton.World.size.x);
            OffsetCoord wcoord = OffsetCoord.RoffsetFromCube(WorldSettings.OFFFSET_COORD, wrap);
            print(string.Format("WrappedHex: {0}, {1} | WrappedCoords: {0}, {1}", hex.q, hex.r, coord.col, coord.row));

            var obj = GameManager.Singleton.World.tileData[hex];
            var hData = obj.hexData;
            var pl = GameManager.Singleton.World.GetPlateByID(hData.plateId);
            hexInfoText.text = $"Hex={hex.q}/{hex.r}/{hex.s} | {coord.col}/{coord.row}";
            tileObjText.text = $"h={Math.Round(hData.height, 2)}|tmp={Math.Round(hData.temp, 1)}|tid={obj.TileInfo()}|a={hData.age}";
            movementText.text = $"mv={hData.lastMoved}|" +
                $"em={hData.lastEmpty}|" +
                $"dir={pl.direction}|" +
                $"fm={hData.formingMoutain}|" +
                $"hs={hData.isHotSpot}";
            plateDataText.text = $"plate={hData.plateId},spd={Math.Round(pl.movementSpeed, 1)},el={Math.Round(pl.elevation, 0)}";
            plateData2Text.text = $"ocn={hData.isOcean},cst={hData.isCoast},cell={hData.cellid}";

            SetTitleStr(hex.ToString());
            SetValueOrCreate("height", $"h={hData.height.ToString("0.#####")}");
            SetValueOrCreate("rivers", $"river={obj.RiversToString()}");
        }

        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            ShowHexData(!hexPanel.activeSelf);
        }

    }

    public void SetTitleStr(string value)
    {
        hexText.text = PANEL_TITLE + value;
    }
    
    public string CreateValue(string key)
    {
        if (m_hexUIValues.ContainsKey(key))
            return null;

        Transform trans = (hexDataLeftPanel.transform.childCount < hexDataRightPanel.transform.childCount) 
            ? hexDataLeftPanel.transform : hexDataRightPanel.transform;

        GameObject newText = Instantiate(textPrefab, trans);
        newText.name = key;
        m_hexUIValues.Add(key, newText.GetComponent<Text>()); 

        return key;
    }

    public void SetValue(string key, string value)
    {
        if (!m_hexUIValues.ContainsKey(key))
            return;

        m_hexUIValues[key].text = value;
    }

    public void SetValueOrCreate(string key, string value)
    {
        if (!m_hexUIValues.ContainsKey(key))
            CreateValue(key);
        
        SetValue(key, value);
    }

    public void ShowHexData(bool enabled)
    {
        hexPanel.SetActive(enabled);
    }

    public LineRenderer CreateLineRender()
    {
        return gameObject.AddComponent<LineRenderer>();
    }

    private Vector2 m_scroll;
    private const float HEIGHT = 282;
    private const float WIDTH = 200;
    private void OnGUI()
    {
        //if (!m_show) return;

        float y = Screen.height - HEIGHT;


        GUI.Box(new Rect(0, y, WIDTH - 16, HEIGHT), "");

        Rect viewport = new Rect(0, 0, WIDTH, 16 * 32);

        m_scroll = GUI.BeginScrollView(new Rect(0, y, WIDTH - 16, HEIGHT - 32), m_scroll, viewport);

        World w = GameManager.Singleton.World;
        int realPlateCount = 0;
        for (int i = 0; i < w.plates.Count; i++)
        {
            Rect labelRect = new Rect(2, 32 * i, viewport.width, 32);
            float perc = (float)w.plates[i].hexes.Count / (float)GameManager.Singleton.World.numOfHexes;
            realPlateCount += w.plates[i].hexes.Count;
            String txt = string.Format("{0} = {1} ({2}), {3}", w.plates[i].id, w.plates[i].hexes.Count.ToString(), perc.ToString("0.00"), w.plates[i].movementSpeed);
            GUI.Label(labelRect, txt);
        }

        GUI.EndScrollView();
        Rect total = new Rect(2, Screen.height - 32, viewport.width, 32);
        String totalTxt = string.Format("#of: {0} | Total: {1} | Real: {2}", w.plates.Count, GameManager.Singleton.World.tileData.Count, realPlateCount);
        GUI.Label(total, totalTxt);

    }

    private void OnPauseClicked()
    {
        GameManager.Singleton.Generator.paused = !GameManager.Singleton.Generator.paused;
    }

    private void OnStepClicked()
    {
        GameManager.Singleton.Generator.paused = false;
        GameManager.Singleton.Generator.step = true;
    }
}
