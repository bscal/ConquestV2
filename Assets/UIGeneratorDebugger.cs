using Conquest;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIGeneratorDebugger : MonoBehaviour
{

    public Text counterText;
    public UnityEngine.UI.Button pauseButton;
    public Text hexInfoText;
    public Text tileObjText;
    public Text movementText;
    public Text plateDataText;
    public Text plateData2Text;

    private Hex m_watchedHex;
    private bool m_show;

    private void Start()
    {
        pauseButton.onClick.AddListener(OnPauseClicked);
    }

    void Update()
    {
        counterText.text = $"{GameManager.Singleton.generator.iterations}/{MapGenerator.numOfIters}";

        if (Input.GetMouseButtonDown((int)MouseButton.RightMouse))
        {
            var p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Hex hex = GameManager.Singleton.World.layout.PixelToHex(new Point(p.x, p.y)).HexRound();
            OffsetCoord coord = OffsetCoord.RoffsetFromCube(WorldSettings.Singleton.offset, hex);
            Hex wrap = HexUtils.WrapOffset(hex, GameManager.Singleton.World.size.x);
            OffsetCoord wcoord = OffsetCoord.RoffsetFromCube(WorldSettings.Singleton.offset, wrap);
            print(string.Format("WrappedHex: {0}, {1} | WrappedCoords: {0}, {1}", hex.q, hex.r, coord.col, coord.row));

            var obj = GameManager.Singleton.World.tileData[hex];
            var hData = obj.hexData;
            var pl = GameManager.Singleton.World.GetPlateByID(hData.plateId);
            hexInfoText.text = $"Hex={hex.q}/{hex.r}/{hex.s} | {coord.col}/{coord.row}";
            tileObjText.text = $"h={Math.Round(hData.height, 2)}|tmp={hData.temp}|tid={obj.TileInfo()}|a={hData.age}";
            movementText.text = $"mv={hData.lastMoved}|" +
                $"em={hData.lastEmpty}|" +
                $"dir={pl.direction}|" +
                $"fm={hData.formingMoutain}|" +
                $"hs={hData.isHotSpot}";
            plateDataText.text = $"plate={hData.plateId},spd={Math.Round(pl.movementSpeed, 1)},el={Math.Round(pl.elevation, 0)}";
            plateData2Text.text = $"ocn={hData.isOcean},cst={hData.isCoast},";
        }
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
            float perc = (float)w.plates[i].hexes.Count / (float)WorldSettings.Singleton.numOfHexes;
            realPlateCount += w.plates[i].hexes.Count;
            String txt = string.Format("{0} = {1} ({2}), {3}", w.plates[i].id, w.plates[i].hexes.Count.ToString(), perc.ToString("0.00"), w.plates[i].movementSpeed);
            GUI.Label(labelRect, txt);
        }

        GUI.EndScrollView();
        Rect total = new Rect(2, Screen.height - 32, viewport.width, 32);
        String totalTxt = string.Format("#of: {0} | Total: {1} | Real: {2}", w.plates.Count, WorldSettings.Singleton.numOfHexes, realPlateCount);
        GUI.Label(total, totalTxt);

    }

    private void OnPauseClicked()
    {
        GameManager.Singleton.generator.paused = !GameManager.Singleton.generator.paused;
    }
}
