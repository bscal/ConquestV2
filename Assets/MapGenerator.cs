using Conquest;
using SimplexNoise;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Setup")]
    public Sprite[] tiles;

    [Header("Prefabs")]
    public GameObject prefab;
    public GameObject dot;

    private World m_world;
    private int m_width;
    private int m_height;
    private int m_pixelW;
    private int m_pixelH;
    private Layout m_layout;

    public World CreateWorld()
    {
        m_world = new World();
        m_width = WorldSettings.Singleton.width;
        m_height = WorldSettings.Singleton.height;
        m_pixelW = WorldSettings.Singleton.pixelW;
        m_pixelH = WorldSettings.Singleton.pixelH;
        m_layout = m_world.layout;

        SimplexNoise.Noise.Seed = 209323094;

        for (int r = 0; r <= m_height; r++) // height
        {
            int r_offset = Mathf.FloorToInt(r / 2);
            for (int q = -r_offset; q <= m_width - r_offset; q++) // width with offset
            {
                Hex hex = new Hex(q, r, -q - r);
                string mapKey = Hex.ToKey(q, r);
                Point pixel = m_layout.HexToPixel(hex);

                if (!m_world.tileData.ContainsKey(mapKey))
                {
                    TileObject tObj = ScriptableObject.CreateInstance<TileObject>();
                    m_world.tileData.Add(mapKey, tObj);

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
                    tObj.tileId = n;
                    tObj.render = render;

                    tObj.height = d;
                }
            }
        }

        Generate();
        return m_world;
    }

    private void Generate()
    {
        GameObject test = new GameObject();
        var l1 = test.AddComponent<LineRenderer>();
        l1.SetPosition(0, new Vector3(0, 0, -1));
        l1.SetPosition(1, new Vector3(m_pixelW, 0, -1));
        l1.startWidth = 2;
        l1.endWidth = 2;

        GameObject test1 = new GameObject();
        var l2 = test1.AddComponent<LineRenderer>();
        l2.SetPosition(0, new Vector3(m_pixelW, 0, -1));
        l2.SetPosition(1, new Vector3(m_pixelW, m_pixelH, -1));
        l2.startWidth = 2;
        l2.endWidth = 2;

        GameObject test2 = new GameObject();
        var l3 = test2.AddComponent<LineRenderer>();
        l3.SetPosition(0, new Vector3(m_pixelW, m_pixelH, -1));
        l3.SetPosition(1, new Vector3(0, m_pixelH, -1));
        l3.startWidth = 2;
        l3.endWidth = 2;

        GameObject test3 = new GameObject();
        var l4 = test3.AddComponent<LineRenderer>();
        l4.SetPosition(0, new Vector3(0, m_pixelH, -1));
        l4.SetPosition(1, new Vector3(0, 0, -1));
        l4.startWidth = 2;
        l4.endWidth = 2;

        uint seed = 123;
        Unity.Mathematics.Random rand = new Unity.Mathematics.Random(seed);
        for (int i = 0; i < WorldSettings.Singleton.plates; i++)
        {
            int x = rand.NextInt(0, m_pixelW);
            int y = rand.NextInt(0, m_pixelH);

            Hex hex = m_layout.PixelToHex(new Point(x, y)).HexRound();
            Point pt = m_layout.HexToPixel(hex);

            Instantiate(dot, new Vector3((float)pt.x, (float)pt.y, -1), Quaternion.identity);

            Plate p = new Plate(hex, UnityEngine.Random.ColorHSV());
            p.elevation = Random.Range(0, 4);
            p.direction = (HexDirection)Random.Range(0, Hex.DIRECTION_COUNT - 1);
            m_world.plates.Add(p);
        }

        /*  
         *  ------------------------------------------------------
         *      Setting hexes to plates
         *  ------------------------------------------------------
         */
        for (int r = 0; r <= m_height; r++) // height
        {
            int r_offset = Mathf.FloorToInt(r / 2);
            for (int q = -r_offset; q <= m_width - r_offset; q++) // width with offset
            {
                Hex hex = new Hex(q, r, -q - r);
                string mapKey = Hex.ToKey(q, r);
                Point pixel = m_layout.HexToPixel(hex);

                int closestId = 0;
                int closest = int.MaxValue;
                for (int i = 0; i < WorldSettings.Singleton.plates; i++)
                {
                    int dist = hex.Distance(m_world.plates[i].center);

                    if (closest > dist)
                    {
                        closest = dist;
                        closestId = i;
                    }
                }
                m_world.tileData[mapKey].plateId = closestId;
                m_world.plates[closestId].AddHex(hex);
            }
        }


        /*  
         *  ------------------------------------------------------
         *      Checking if hexes are on edge of plate
         *  ------------------------------------------------------
         */
        for (int r = 0; r <= m_height; r++) // height
        {
            int r_offset = Mathf.FloorToInt(r / 2);
            for (int q = -r_offset; q <= m_width - r_offset; q++) // width with offset
            {
                Hex hex = new Hex(q, r, -q - r);
                string mapKey = Hex.ToKey(q, r);
                Point pixel = m_layout.HexToPixel(hex);

                foreach (Hex h in hex.Ring(1))
                {
                    if (!m_world.tileData.ContainsKey(h.GetKey())) continue;
                    if (m_world.tileData[h.GetKey()].plateId != m_world.tileData[mapKey].plateId)
                    {
                        m_world.tileData[mapKey].isPlateEdge = true;
                        break;
                    }
                }
            }
        }

        /*  
         *  ------------------------------------------------------
         *      Simulate plates moving
         *  ------------------------------------------------------
         */
        Dictionary<string, TileObject> tempData = m_world.tileData.ToDictionary(entry => entry.Key, entry => entry.Value);
        const int iterations = 100;
        for (int i = 0; i < iterations; i++)
        {
            for (int r = 0; r <= m_height; r++) // height
            {
                int r_offset = Mathf.FloorToInt(r / 2);
                for (int q = -r_offset; q <= m_width - r_offset; q++) // width with offset
                {
                    // Current hex values
                    Hex hex = new Hex(q, r, -q - r);
                    string mapKey = Hex.ToKey(q, r);
                    TileObject hData = m_world.tileData[mapKey];
                    Plate plate = m_world.plates[hData.plateId];

                    TileObject data = tempData[mapKey];
                    float height = data.height;
                    bool isEdge = data.isPlateEdge;

                    /*
                     * Moves hexes values in direction of its plate.
                     * Hexes do not actually move but there values influences the direction
                     * 
                     * When a hex moves, its value is taken and move to the hex in the plate direction.
                     * The current hex will have its values reduced.
                     * If current hex is an edge tile and moving away increased height reduction.
                     * If moving to hex is an edge tile it will gain height increase.
                     */

                    // Other hex values
                    HexDirection dir = plate.direction;
                    Hex dirHex = hex.Neighbor((int)plate.direction);

                    if (!tempData.ContainsKey(dirHex.GetKey())) continue;
                    TileObject dirData = m_world.tileData[dirHex.GetKey()];

                    Hex revHex = hex.Neighbor((int)Hex.ReverseDirection(dir));
                    TileObject reverseData = null;
                    if (tempData.ContainsKey(revHex.GetKey()))
                        reverseData = m_world.tileData[revHex.GetKey()];



                    float dirHeight = dirData.height;
                    bool dirEdge = dirData.isPlateEdge;
                    bool awayFromEdge = reverseData == null || reverseData.isPlateEdge && reverseData.plateId != hData.plateId;

                    const float MOVE_AMP = 1.25f;
                    const float AWAY_AMP = 0.2f;
                    const float BASE_REDUCTION = 0.2f;

                    if (dirEdge)
                    {
                        dirHeight *= MOVE_AMP;
                        height /= BASE_REDUCTION;
                    }
                    else if (awayFromEdge)
                    {
                        height *= AWAY_AMP;
                    }
                    height *= BASE_REDUCTION;
                    dirData.height = dirHeight;
                    data.height = height;
                }
            }
        }
        foreach (var pair in tempData)
        {
            float h = pair.Value.height;
            m_world.tileData[pair.Key].height = h;
            int n = 0;
            if (h > -1)
                n = 2;
            if (h > 100)
                n = 0;
            if (h > 200)
                n = 1;
            m_world.tileData[pair.Key].tileId = n;
            m_world.tileData[pair.Key].render.sprite = tiles[n];
        }
    }
}
