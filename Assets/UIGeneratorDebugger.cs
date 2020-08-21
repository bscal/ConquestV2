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
            //print(string.Format("WrappedHex: {0}, {1} | WrappedCoords: {0}, {1}", wrap.q, wrap.r, wcoord.col, wcoord.row));

            var obj = GameManager.Singleton.World.tileData[hex.GetKey()];
            var hData = obj.hexData;
            var pl = GameManager.Singleton.World.GetPlateByID(hData.plateId);
            hexInfoText.text = $"Hex={hex.q}/{hex.r}/{hex.s} | {coord.col}/{coord.row}";
            tileObjText.text = $"h={Math.Round(hData.height, 2)},t={hData.temp},tid={obj.tileId}";
            movementText.text = $"m={Convert.ToInt32(hData.moveCenter)}," +
                $"e={Convert.ToInt32(hData.empty)}," +
                $"d={Convert.ToInt32(pl.direction)}" +
                $"M={Convert.ToInt32(hData.formingMoutain)}" +
                $"I={Convert.ToInt32(hData.isHotSpot)}";
            plateDataText.text = $"id={hData.plateId},s={Math.Round(pl.movementSpeed, 1)},e={Math.Round(pl.elevation, 0)}";
            plateData2Text.text = $"o={hData.isOcean},c={hData.isCoast},";
        }
    }

    private void OnPauseClicked()
    {
        GameManager.Singleton.generator.paused = !GameManager.Singleton.generator.paused;
    }
}
