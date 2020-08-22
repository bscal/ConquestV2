﻿using Conquest;
using SimplexNoise;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;

namespace Conquest
{

    enum GenState
    {
        PRE_GEN,
        GENERATE,
        ITERATING,
        DONE
    }

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

        public bool paused = true;
        public int iterations = 0;

        [SerializeField]
        private TileMap tileMap;

        private GenState m_state;

        private uint m_seed;
        private Unity.Mathematics.Random m_rand;

        public World CreateWorld()
        {
            m_width = WorldSettings.Singleton.width;
            m_height = WorldSettings.Singleton.height;
            m_pixelW = WorldSettings.Singleton.pixelW;
            m_pixelH = WorldSettings.Singleton.pixelH;
            m_world = new World(m_width, m_height);
            m_layout = m_world.layout;

            m_seed = 123;
            SimplexNoise.Noise.Seed = 209323094;
            m_rand = new Unity.Mathematics.Random(m_seed);

            m_state = GenState.PRE_GEN;
            StartCoroutine(GenerateRoutine());

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
                        GameObject gameobject = Instantiate(prefab, new Vector3((float)pixel.x, (float)pixel.y, 0), Quaternion.identity);
                        TileObject tObj = gameobject.AddComponent<TileObject>();
                        m_world.tileData.Add(mapKey, tObj);
                        SpriteRenderer render = gameobject.GetComponent<SpriteRenderer>();
                        HexData hData = tObj.hexData;
                        int n = UnityEngine.Random.Range(0, 2);

                        float d = Noise.CalcPixel2D(q, r, .1f);
                        //float d = 150;
                        if (d > 0)
                            n = 2;
                        if (d > 100)
                            n = 0;
                        if (d > 200)
                            n = 1;

                        if (d < 100)
                            hData.isOcean = true;

                        render.sprite = tileMap.GetTile(n).sprite;

                        tObj.hex = hex;
                        tObj.gameobject = gameobject;
                        tObj.render = render;
                        tObj.tileId = n;

                        hData.height = d;
                    }
                }
            }

            m_state = GenState.GENERATE;
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

            for (int i = 0; i < WorldSettings.Singleton.plates; i++)
            {
                int x = m_rand.NextInt(0, m_pixelW);
                int y = m_rand.NextInt(0, m_pixelH);

                Hex hex = m_layout.PixelToHex(new Point(x, y)).HexRound();
                Point pt = m_layout.HexToPixel(hex);

                Plate p = new Plate(hex, UnityEngine.Random.ColorHSV()) {
                    elevation = Random.Range(0f, 255f),
                    movementSpeed = m_rand.NextFloat(10.0f, 16.0f),
                    direction = (HexDirection)Random.Range(0, HexConstants.DIRECTIONS - 1),
                    obj = Instantiate(dot, new Vector3((float)pt.x, (float)pt.y, -1), Quaternion.identity)
            };
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
                    for (int i = 0; i < WorldSettings.Singleton.plates; i++)
                    {
                        int dist = hex.Distance(HexUtils.WrapOffset(m_world.plates[i].center, m_world.size.x));

                        if (closest > dist)
                        {
                            closest = dist;
                            closestId = i;
                        }
                    }
                    m_world.tileData[mapKey].hexData.plateId = closestId;
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

                    foreach (Hex h in hex.Ring(1))
                    {
                        if (!m_world.tileData.ContainsKey(h.GetKey())) continue;
                        if (m_world.tileData[h.GetKey()].hexData.plateId != m_world.tileData[mapKey].hexData.plateId)
                        {
                            m_world.tileData[mapKey].hexData.isPlateEdge = true;
                            break;
                        }
                    }
                }
            }
        }
        public const float seaLevel = 100;
        public const float hillLevel = 200;

        public const int numOfIters = 250;
        float timer = 0f;
        int iters = 0;
        void Update()
        {
            if (paused) return;
            iterations = iters;
            if (m_state == GenState.DONE) return;
            if (m_state != GenState.ITERATING) m_state = GenState.ITERATING;
            timer += Time.deltaTime;
            if (timer < 0.05f) return;
            timer = 0;
            iters++;
            if (iters > numOfIters)
            {
                m_state = GenState.DONE;
                print("Done simulation!");
                StopCoroutine(GenerateRoutine());
                Smooth();
            }


            if (iters != 0 && iters % 50 == 0)
            {
                for (int i = 0; i < m_world.plates.Count; i++)
                {
                    Plate p = m_world.plates[i];
                    p.direction = (HexDirection)Random.Range(0, HexConstants.MAX_DIR);
                }
                OnDirectionChange();
                Debug.LogWarning("changing directions");
            }
            if (iters != 0 && iters % 5 == 0)
            {
                for (int i = 0; i < m_world.plates.Count; i++)
                {
                    Plate p = m_world.plates[i];
                    p.movementSpeed = Random.Range(10.0f, 16.0f);
                }
            }

            CalcCollisions();
            float[] tempPlates = new float[m_world.plates.Count];
            for (int i = 0; i < tempPlates.Length; i++)
            {
                tempPlates[i] = m_world.plates[i].movementSpeed;
            }
            Dictionary<string, HexData> tempHeights = new Dictionary<string, HexData>(m_world.tileData.Count);

            for (int r = 0; r <= m_height; r++) // height
            {
                int r_offset = Mathf.FloorToInt(r / 2);
                for (int q = -r_offset; q <= m_width - r_offset; q++) // width with offset
                {
                    Hex hex = new Hex(q, r, -q - r);
                    string mapKey = Hex.ToKey(q, r);

                    // Current Hex. This hex moves to directional hex.
                    TileObject hexObj = m_world.tileData[mapKey];
                    HexData hData = hexObj.hexData;
                    Plate plate = m_world.GetPlateByID(hData.plateId);
                    HexDirection dir = plate.direction;
                    float height = hData.height;

                    bool move = true;
                    bool destroy = false;

                    // Move Direction Hex.
                    Hex dirHex = hex.Neighbor((int)dir);
                    bool dirNotNull = m_world.TryGetHexData(dirHex, WorldSettings.Singleton.wrapWorld, out TileObject dirObj);
                    if (!dirNotNull) continue;
                    HexData dirData = dirObj.hexData;
                    string dirKey = dirObj.hex.GetKey();

                    Plate dirPlate = m_world.plates[dirData.plateId];
                    bool dirDiffPlate = hData.plateId != dirData.plateId;
                    bool dirInto = dir == Hex.ReverseDirection(m_world.plates[dirData.plateId].direction);
                    bool dirHigher = plate.elevation < dirPlate.elevation;
                    bool dirIntoFront = hex.Front(plate.direction).Contains(dirHex);
                    bool dirMovingAway = HexUtils.HexMovingTowards((int)plate.direction, (int)dirPlate.direction);

                    // old way of movement
                    // float baseVal = height * .015f;
                    // 
                    // data.height -= baseVal;
                    // dirData.height += baseVal;  

                    /**
                     * Adds HexData to tempData array
                     */
                    if (!tempHeights.ContainsKey(mapKey))
                    {
                        tempHeights[mapKey] = new HexData(hData);
                    }
                    if (!tempHeights.ContainsKey(dirKey))
                    {
                        tempHeights[dirKey] = new HexData(dirData);
                    }

                    if (height >= seaLevel)
                    {
                        tempHeights[mapKey].isOcean = false;
                    }

                    // Do not move hex if plate is not moving
                    if (plate.movementSpeed < 0.0f)
                    {
                        hData.empty = false;
                        move = false;
                    }

                    // Checks if dirHex is out of grid bounds. 
                    if (HexUtils.HexOutOfBounds(m_world.size, dirObj.hex))
                    {
                        tempPlates[hData.plateId] -= 5f;
                        hData.empty = false;
                        move = false;
                    }

                    // Convergent boundary
                    // Plate collision. current hex plate and moving direction plate colliding
                    if (dirDiffPlate && dirInto)
                    {
                        tempPlates[hData.plateId] -= 5f;
                        if (!dirHigher)
                        {
                            tempHeights[mapKey].height = height + (height * .75f) + 15;
                            tempHeights[mapKey].formingMoutain = true;
                        }
                        else
                        {
                            destroy = true;
                        }
                        hData.empty = false;
                        move = false;
                    }

                    // dirHex is on different plate and diff plate is not moving.
                    // This is handled the similar to a plate collision but technically is not real one.
                    if (dirDiffPlate && dirPlate.movementSpeed < 0f)
                    {
                        tempPlates[hData.plateId] -= 2f;
                        if (!dirHigher)
                        {
                            tempHeights[mapKey].height = height + (height * .5f) + 10;
                            tempHeights[mapKey].formingMoutain = true;
                        }
                        else
                        {
                            destroy = this;
                        }
                        hData.empty = false;
                        move = false;
                    }

                    /*
                     * Divergent plate boundaries exist. Every sim iteration plates are set to empty.
                     * When a plate moves, the dirHex is set to NOT empty. After the sim any empty
                     * hexes are set to a default height (to simulate crust being created). These naturally
                     * happen in areas of diverging plates.
                     */
                    hData.moved = move;
                    hData.age++;
                    if (move)
                    {
                        /*
                         * Moves hex from current iterated hex -> neighboring hex using the current plates direction
                         */
                        float mod = 1f;
                        if (hData.isHotSpot) // Hot spots
                            mod += 10f;
                        if (height < seaLevel && hData.age < 20) // New created land gains more height
                            mod += height * .25f + 5; 
                        if (height > 150) // Erosion
                            mod += -3f;
                        if (height > hillLevel) // Erosion
                            mod -= m_rand.NextInt(5, 8);
                        if (dirDiffPlate && !dirMovingAway) // plates moving away
                            mod += height * .1f + 10;
                        if (!dirDiffPlate && dirData.formingMoutain) // hex moving into hex that forming mountain
                            mod += height * .3f;
                        if (dirData.height > hillLevel && height < dirData.height && !dirData.isCoast) // hex that are moving into a higher hex that is not coast increase height
                            mod += height * .1f + 5f;

                        tempHeights[dirKey].height = height + mod;
                        tempHeights[dirKey].isHotSpot = false;
                        dirData.empty = false;
                        if (plate.center.Equals(hex))
                        {
                            plate.center = dirObj.hex;
                            hData.moveCenter = true;
                        }

                        if (hData.height >= seaLevel)
                        {
                            tempHeights[dirKey].isOcean = false;
                        }

                        foreach (Hex ringHex in hexObj.hex.Ring(1))
                        {
                            if (m_world.TryGetHexData(ringHex, WorldSettings.Singleton.wrapWorld, out TileObject ringObj))
                            {
                                HexData data = ringObj.hexData;
                                if (data.isOcean)
                                {
                                    if (hData.height < seaLevel)
                                    {
                                        tempHeights[hexObj.hex.GetKey()].isOcean = true;
                                    }
                                }
                            }
                        }

                        dirPlate.RemoveHex(dirHex);
                        plate.AddHex(dirHex);
                        tempHeights[mapKey].movedToHex = dirObj.hex;
                        tempHeights[mapKey].oldPlateId = hData.plateId;
                        tempHeights[dirObj.hex.GetKey()].plateId = hData.plateId;

                        tempPlates[hData.plateId] -= .1f;
                    }
                }
            }

            /*
             * Moves plate dot if hex was moved
             */
            for (int i = 0; i < tempPlates.Length; i++)
            {
                m_world.plates[i].movementSpeed = tempPlates[i];
            }

            ApplyTiles(tempHeights);

            foreach (var pair in m_world.tileData)
            {
                var hData = pair.Value.hexData;

                int notOceanCount = 0;
                foreach (Hex h in pair.Value.hex.Ring(1))
                {
                    if (m_world.TryGetHexData(h, WorldSettings.Singleton.wrapWorld, out TileObject data))
                    {
                        if (data.hexData.isOcean && hData.height > seaLevel)
                        {
                            hData.isCoast = true;
                            continue;
                        }
                    }
                    notOceanCount++;
                }

                if (notOceanCount >= HexConstants.DIRECTIONS)
                {
                    hData.isCoast = false;
                }
            }
        }

        private IEnumerator GenerateRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(5.0f);
                if (m_state == GenState.GENERATE)
                    print("State_Generate");
                else if (m_state == GenState.ITERATING)
                    print("State_Iterating");
            }
        }

        private void ApplyTiles(in Dictionary<string, HexData> tempHeights)
        {
            foreach (var pair in m_world.tileData)
            {
                HexData hData = pair.Value.hexData;
                if (!tempHeights.ContainsKey(pair.Key)) {
                    continue;
                }
                hData.UpdateValues(tempHeights[pair.Key]);

                if (hData.empty)
                {
                    hData.height = 10f;
                    hData.moved = true;
                    hData.isOcean = true;
                    hData.age = 0;
                    if (m_rand.NextFloat() < .05f)
                        hData.isHotSpot = true;

                    m_world.plates[hData.plateId].RemoveHex(pair.Value.hex);
                    int closestId = tempHeights[pair.Key].oldPlateId;
                    //int closestId = GetHighestPlateInHexList(pair.Value.hex.Ring(1));
                    m_world.tileData[pair.Key].hexData.plateId = closestId;
                    m_world.plates[closestId].AddHex(pair.Value.hex);
                }

                if (hData.moveCenter)
                {
                    Point pt = m_layout.HexToPixel(tempHeights[pair.Key].movedToHex);
                    m_world.plates[tempHeights[pair.Key].plateId].obj.transform.position = new Vector3((float)pt.x, (float)pt.y, -1);
                }

                float h = hData.height;
                int n = 0;
                if (h < 100)
                    n = 2;
                if (h > 100)
                    n = 0;
                if (h > 200)
                    n = 1;
                pair.Value.tileId = n;
                pair.Value.render.sprite = tiles[n];
                
            }
        }

        private void Smooth()
        {
            Dictionary<string, HexData> tempHeights = new Dictionary<string, HexData>(m_world.tileData.Count);

            foreach (var pair in m_world.tileData)
            {
                string key = pair.Key;
                TileObject obj = pair.Value;
                HexData hData = pair.Value.hexData;

                if (tempHeights.ContainsKey(key))
                {
                    hData.UpdateValues(tempHeights[key]);
                }

                if (hData.isCoast)
                {
                    float randVal = m_rand.NextFloat();
                    if (randVal < 0.5f)
                    {
                        hData.height = 90f;
                        obj.tileId = 2;
                        obj.render.sprite = tiles[2];
                    }
                }

//                 float val = tile.height;
//                 float avg = 0;
//                 int count = 0;
//                 foreach (Hex hex in tile.hex.Ring(1))
//                 {
//                     var obj = m_world.GetHexData(hex, WorldSettings.Singleton.wrapWorld);
//                     if (obj == null) continue;
//                     avg += (obj.height / 455) * 100;
//                     count++;
//                 }
//                 tempHeights[key].height += (avg / count) * .1f;
            }
            ApplyTiles(tempHeights);
        }

        private void OnDirectionChange()
        {
            foreach (var pair in m_world.tileData)
            {
                pair.Value.hexData.formingMoutain = false;
                pair.Value.hexData.isHotSpot = false;
            }
        }

        private void CalcCollisions()
        {
            foreach (var pair in m_world.tileData)
            {
                HexData hData = pair.Value.hexData;
                Hex h = pair.Value.hex.Neighbor((int)m_world.GetPlateByID(hData.plateId).direction);
                if (!m_world.TryGetHexData(h, WorldSettings.Singleton.wrapWorld, out TileObject dirData))
                    continue;

                hData.moveCenter = false;
                hData.empty = true;

                if (HexUtils.HexOutOfBounds(m_world.size, dirData.hex) || hData.collision)
                {
                    hData.collision = true;

                }

            }
        }

        private int[] CountHexList(in List<Hex> ring)
        {
            int[] counts = new int[WorldSettings.Singleton.plates];
            for (int i = 0; i < ring.Count; i++)
            {
                if (!m_world.TryGetHexData(ring[i], WorldSettings.Singleton.wrapWorld, out TileObject obj))
                    continue;

                counts[obj.hexData.plateId] += 1;
            }
            return counts;
        }

        private int GetHighestPlateInHexList(in List<Hex> ring)
        {
            int top = 0;
            int[] counts = new int[WorldSettings.Singleton.plates];
            for (int i = 0; i < ring.Count; i++)
            {
                if (!m_world.TryGetHexData(ring[i], WorldSettings.Singleton.wrapWorld, out TileObject obj))
                    continue;
                HexData hData = obj.hexData;
                counts[hData.plateId] += 1;
                if (counts[hData.plateId] > top)
                    top = hData.plateId;
            }
            return top;
        }
    }
}