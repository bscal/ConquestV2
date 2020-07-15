using Conquest;
using SimplexNoise;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Setup")]
    public int w;
    public int h;
    public Sprite[] tiles;

    [Header("Prefabs")]
    public GameObject prefab;
    public GameObject dot;

    private int pixelW = 0;
    private int pixelH = 0;
    private Layout m_layout;
    private Dictionary<string, TileObject> m_tileData;

    protected Vector3Int m_selected;

    void Start()
    {
        m_layout = new Layout(Layout.pointy, new Point(8, 8), new Point(0, 0));
        m_tileData = new Dictionary<string, TileObject>();

        pixelW = (int)(w * (Mathf.Sqrt(3) * 8));
        pixelH = (int)(h * (16.0f * .75f));

        SimplexNoise.Noise.Seed = 209323094;

        for (int r = 0; r <= h; r++) // height
        {
            int r_offset = Mathf.FloorToInt(r / 2);
            for (int q = -r_offset; q <= w - r_offset; q++) // width with offset
            {
                Hex hex = new Hex(q, r, -q - r);
                string mapKey = Hex.ToKey(q, r);
                Point pixel = m_layout.HexToPixel(hex);

                if (!m_tileData.ContainsKey(mapKey))
                {
                    TileObject tObj = ScriptableObject.CreateInstance<TileObject>();
                    m_tileData.Add(mapKey, tObj);

                    GameObject gameobject = Instantiate(prefab, new Vector3((float)pixel.x, (float)pixel.y, 0), Quaternion.identity);
                    SpriteRenderer render = gameobject.GetComponent<SpriteRenderer>();

                    int n = UnityEngine.Random.Range(0, 2);

                    float d = Noise.CalcPixel2D(q, r, .10f);
                    if (d > 0)
                        n = 2;
                    if (d > 100)
                        n = 0;
                    if (d > 200)
                        n = 1;

                    render.sprite = tiles[n];

                    tObj.hex = hex;
                    tObj.gameobject = gameobject;
                    tObj.n = n;
                    tObj.render = render;

                    tObj.height = d;
                }
            }
        }

        Generate();
    }


    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown((int)MouseButton.LeftMouse))
        {
            var p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Hex hex = m_layout.PixelToHex(new Point(p.x, p.y)).HexRound();
            foreach (Hex h in hex.Ring(3))
            {
                m_tileData[h.GetKey()].render.sprite = tiles[3];
            }
        }
        if (Input.GetMouseButtonDown((int)MouseButton.RightMouse))
        {
            var p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Hex hex = m_layout.PixelToHex(new Point(p.x, p.y)).HexRound();
            print(string.Format("PixelToHex: {0}, {1}, ", hex.q, hex.r));
            print(m_tileData[hex.GetKey()].height);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            GameManager.Singleton.ChangeFilter();
            foreach (var obj in m_tileData.Values) {
                obj.SetFilter(GameManager.Singleton.currentFilter);
            }
        }
    }

    private void Generate()
    {
        GameObject test = new GameObject();
        var l1 = test.AddComponent<LineRenderer>();
        l1.SetPosition(0, new Vector3(0, 0, -1));
        l1.SetPosition(1, new Vector3(pixelW, 0, -1));
        l1.startWidth = 2;
        l1.endWidth = 2;

        GameObject test1 = new GameObject();
        var l2 = test1.AddComponent<LineRenderer>();
        l2.SetPosition(0, new Vector3(pixelW, 0, -1));
        l2.SetPosition(1, new Vector3(pixelW, pixelH, -1));
        l2.startWidth = 2;
        l2.endWidth = 2;

        GameObject test2 = new GameObject();
        var l3 = test2.AddComponent<LineRenderer>();
        l3.SetPosition(0, new Vector3(pixelW, pixelH, -1));
        l3.SetPosition(1, new Vector3(0, pixelH, -1));
        l3.startWidth = 2;
        l3.endWidth = 2;

        GameObject test3 = new GameObject();
        var l4 = test3.AddComponent<LineRenderer>();
        l4.SetPosition(0, new Vector3(0, pixelH, -1));
        l4.SetPosition(1, new Vector3(0, 0, -1));
        l4.startWidth = 2;
        l4.endWidth = 2;

        uint seed = 123;
        Unity.Mathematics.Random rand = new Unity.Mathematics.Random(seed);
        for (int i = 0; i < WorldSettings.Singleton.plates; i++)
        {
            int x = rand.NextInt(0, pixelW);
            int y = rand.NextInt(0, pixelH);

            Hex hex = m_layout.PixelToHex(new Point(x, y)).HexRound();
            Point pt = m_layout.HexToPixel(hex);

            Instantiate(dot, new Vector3((float)pt.x, (float)pt.y, 0), Quaternion.identity);

            Plate p = new Plate(hex, UnityEngine.Random.ColorHSV());
            GameManager.Singleton.Plates.Add(p);
        }

        for (int r = 0; r <= h; r++) // height
        {
            int r_offset = Mathf.FloorToInt(r / 2);
            for (int q = -r_offset; q <= w - r_offset; q++) // width with offset
            {
                Hex hex = new Hex(q, r, -q - r);
                string mapKey = Hex.ToKey(q, r);
                Point pixel = m_layout.HexToPixel(hex);

                int closestId = 0;
                int closest = int.MaxValue;
                for (int i = 0; i < WorldSettings.Singleton.plates; i++)
                {
                    int dist = hex.Distance(GameManager.Singleton.Plates[i].center);

                    if (closest > dist)
                    {
                        closest = dist;
                        closestId = i;
                    }
                }
                m_tileData[mapKey].plateId = closestId;
                GameManager.Singleton.Plates[closestId].AddHex(hex);

            }
        }
    }
}
