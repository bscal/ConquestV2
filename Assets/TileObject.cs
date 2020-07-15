using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum HexFilter
{
    NONE,
    PLATE
}

public class TileObject : TileBase {

    public GameObject gameobject;
    public SpriteRenderer render;
    public Hex hex;

    public int plateId = 0;

    public int n = -2;
    public float height = 0;
    public float temp = 0;
    public float wetness = 0;

    public void SetFilter(HexFilter filter)
    {

        if (filter == HexFilter.NONE)
        {
            render.color = Color.white;
        }
        else if (filter == HexFilter.PLATE)
        {
            render.color = GameManager.Singleton.Plates[plateId].color;
        }
    }
}
