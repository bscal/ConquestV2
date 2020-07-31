﻿using Conquest;
using SimplexNoise;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public bool paused = false;
        public int iterations = 0;

        private GenState m_state;

        public World CreateWorld()
        {
            m_width = WorldSettings.Singleton.width;
            m_height = WorldSettings.Singleton.height;
            m_pixelW = WorldSettings.Singleton.pixelW;
            m_pixelH = WorldSettings.Singleton.pixelH;
            m_world = new World(m_width, m_height);
            m_layout = m_world.layout;

            SimplexNoise.Noise.Seed = 209323094;

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

                        int n = UnityEngine.Random.Range(0, 2);

                        float d = Noise.CalcPixel2D(q, r, .1f);
                        //float d = 150;
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

            uint seed = 123;
            Unity.Mathematics.Random rand = new Unity.Mathematics.Random(seed);
            for (int i = 0; i < WorldSettings.Singleton.plates; i++)
            {
                int x = rand.NextInt(0, m_pixelW);
                int y = rand.NextInt(0, m_pixelH);

                Hex hex = m_layout.PixelToHex(new Point(x, y)).HexRound();
                Point pt = m_layout.HexToPixel(hex);

                

                Plate p = new Plate(hex, UnityEngine.Random.ColorHSV()) {
                    elevation = Random.Range(0f, 255f),
                    direction = (HexDirection)Random.Range(0, Hex.DIRECTION_COUNT - 1),
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
                    for (int i = 0; i < WorldSettings.Singleton.plates; i++)
                    {
                        int dist = hex.Distance(HexUtils.WrapOffset(m_world.plates[i].center, m_world.size.x));

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

//             for (int r = 0; r <= m_height; r++) // height
//             {
//                 int r_offset = Mathf.FloorToInt(r / 2);
//                 for (int q = -r_offset; q <= m_width - r_offset; q++) // width with offset
//                 {
//                     Hex hex = new Hex(q, r, -q - r);
//                     string mapKey = Hex.ToKey(q, r);
// 
//                     TileObject hData = m_world.GetHexData(hex, false);
//                     if (hData.height < 100) continue;
// 
//                     int closestId = -1;
//                     int closestDist = int.MaxValue;
// 
//                     foreach (Hex h in hex.Ring(1))
//                     {
//                         if (!m_world.TryGetHexData(h, WorldSettings.Singleton.wrapWorld, out TileObject rData))
//                             continue;
// 
//                         if (rData.height < 100) continue;
// 
//                         closestId = rData.plateId;
//                     }
// 
//                     if (closestId != -1)
//                     {
//                         m_world.tileData[mapKey].plateId = closestId;
//                         m_world.plates[closestId].RemoveHex(hex);
//                         m_world.plates[closestId].AddHex(hex);
//                     }
//                 }
//           }


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
            const int iterations = 0;
            for (int i = 0; i < iterations; i++)
            {


                for (int r = 0; r <= m_height; r++) // height
                {
                    int r_offset = Mathf.FloorToInt(r / 2);
                    for (int q = -r_offset; q <= m_width - r_offset; q++) // width with offset
                    {
//                         Hex hex = new Hex(q, r, -q - r);
//                         string mapKey = Hex.ToKey(q, r);
//                         TileObject hData = m_world.tileData[mapKey];
//                         /*
//                          * Moves hexes values in direction of its plate.
//                          * Hexes do not actually move but there values influences the direction
//                          * 
//                          * When a hex moves, its value is taken and move to the hex in the plate direction.
//                          * The current hex will have its values reduced.
//                          * If current hex is an edge tile and moving away increased height reduction.
//                          * If moving to hex is an edge tile it will gain height increase.
//                          */
// 
//                         // Current hex values
//                         Plate plate = m_world.plates[hData.plateId];
//                         HexDirection dir = plate.direction;
//                         HexDirection rev = Hex.ReverseDirection(dir);
//                         // Temp current hex data
//                         TileObject data = tempData[mapKey];
//                         float height = hData.height;
//                         bool isEdge = hData.isPlateEdge;
// 
//                         // Move to hex data
//                         Hex dirHex = hex.Neighbor((int)plate.direction);
//                         if (!tempData.ContainsKey(dirHex.GetKey())) continue;
//                         TileObject dirData = tempData[dirHex.GetKey()];
// 
//                         bool plateCollision = dir == Hex.ReverseDirection(m_world.plates[dirData.plateId].direction);
//                         bool isDifferentPlate = dirData.plateId != data.plateId;
//                         bool isDifferentPlateMovingIntoCur = dir == Hex.ReverseDirection(m_world.plates[dirData.plateId].direction);
//                         bool isCurPlateSmaller = m_world.plates[dirData.plateId].elevation < m_world.plates[dirData.plateId].elevation;
// 
//                         float val = height * .025f;
// 
// 
//                         if (isDifferentPlate && isDifferentPlateMovingIntoCur)
//                         {
//                             if (isCurPlateSmaller)
//                             {
//                                 data.height -= val;
//                             }
//                             else
//                             {
//                                 data.height += val;
//                             }
//                         }
//                         else if (!isDifferentPlate)
//                         {
//                             data.height -= val;
//                             dirData.height += val;
//                         }



                        //                     if (isEdge && !HexUtils.ArrayCountContains(m_world.tileData, hex.Front(dir), data.plateId))
                        //                     {
                        //                         foreach (Hex h in hex.Ring(1))
                        //                         {
                        //                             if (!tempData.ContainsKey(h.GetKey())) continue;
                        //                             var obj = tempData[h.GetKey()].height += 1.0f;
                        //                         }
                        //                         height += 1.5f;
                        //                     }
                        //                     else if (isEdge && !HexUtils.ArrayCountContains(m_world.tileData, hex.Front(rev), data.plateId))
                        //                     {
                        //                         foreach (Hex h in hex.Ring(1))
                        //                         {
                        //                             if (!tempData.ContainsKey(h.GetKey())) continue;
                        //                             var obj = tempData[h.GetKey()].height -= 1.0f;
                        //                         }
                        //                         height -= 1.5f;
                        //                     }
                        //                     else
                        //                     {
                        //                         height += .5f;
                        //                     }










                        //                     Hex revHex = hex.Neighbor((int)Hex.ReverseDirection(dir));
                        //                     TileObject reverseData = null;
                        //                     if (tempData.ContainsKey(revHex.GetKey()))
                        //                         reverseData = tempData[revHex.GetKey()];
                        // 
                        // 
                        // 
                        //                     float dirHeight = dirData.height;
                        //                     bool dirEdge = dirData.isPlateEdge;
                        // 
                        //                     const float MOVE_AMP = 3f;
                        //                     const float AWAY_AMP = -3f;
                        //                     const float BASE_REDUCTION = -1f;
                        //                     const float BASE_INCREASE = 1f;
                        // 
                        //                     // Same plate
                        //                     if (dirEdge && dirData.plateId == data.plateId)
                        //                     {
                        //                         height += BASE_INCREASE;
                        //                     }
                        // 
                        //                     if (dirEdge && plateCollision)
                        //                     {
                        //                         dirHeight += MOVE_AMP;
                        //                         height += MOVE_AMP;
                        //                     }
                        // 
                        //                     if (reverseData != null)
                        //                     {
                        //                         bool awayFromEdge = reverseData.plateId != hData.plateId;
                        //                         bool plateAway = dir == m_world.plates[reverseData.plateId].direction;
                        // 
                        //                         if (awayFromEdge && plateAway)
                        //                         {
                        //                             height += AWAY_AMP;
                        //                             reverseData.height += AWAY_AMP;
                        //                         }
                        //                     }
                        // 
                        //                     dirData.height = dirHeight;
                        //                     data.height = height;
                    }
                }
            }
        }

        public const int numOfIters = 500;
        float timer = 0f;
        int iters = 0;
        void Update()
        {
            if (paused) return;
            iterations = iters;
            if (m_state == GenState.DONE) return;
            if (m_state != GenState.ITERATING) m_state = GenState.ITERATING;
            timer += Time.deltaTime;
            if (timer < 0.25f) return;
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
                    p.direction = (HexDirection)Random.Range(0, Hex.DIRECTION_COUNT - 1);
                    p.movementSpeed = 1f;
                    SetCollisions(false);
                }
                
                Debug.LogWarning("changing directions");
            }
            CalcCollisions();
            Plate[] tempPlates = m_world.plates.ToArray();
            Dictionary<string, TileObject> tempData = m_world.tileData.ToDictionary(entry => entry.Key, entry => entry.Value);

            for (int r = 0; r <= m_height; r++) // height
            {
                int r_offset = Mathf.FloorToInt(r / 2);
                for (int q = -r_offset; q <= m_width - r_offset; q++) // width with offset
                {
                    Hex hex = new Hex(q, r, -q - r);
                    string mapKey = Hex.ToKey(q, r);

                    // Current Hex. This hex moves to directional hex.
                    TileObject hData = m_world.tileData[mapKey];
                    Plate plate = m_world.GetPlateByID(hData.plateId);
                    HexDirection dir = plate.direction;
                    TileObject data = tempData[mapKey];
                    float height = hData.height;
                    bool isEdge = hData.isPlateEdge;

                    // Move Direction Hex.
                    Hex dirHex = hex.Neighbor((int)dir);
                    bool dirNotNull = m_world.TryGetHexData(dirHex, WorldSettings.Singleton.wrapWorld, out TileObject dirData);
                    Plate dirPlate = dirNotNull ? m_world.plates[dirData.plateId] : null;
                    bool dirDiffPlate = dirNotNull && hData.plateId != dirData.plateId;
                    bool dirInto = dirNotNull && dir == Hex.ReverseDirection(m_world.plates[dirData.plateId].direction);
                    bool dirAway = dirNotNull && dir == m_world.plates[dirData.plateId].direction;
                    bool dirHigher = dirNotNull && plate.elevation < dirPlate.elevation;

                    //bools to track more complicated issues
                    //bool plateCollision = dir == Hex.ReverseDirection(m_world.plates[dirData.plateId].direction);
                    //bool isDifferentPlate = dirData.plateId != data.plateId;
                    //bool isDifferentPlateMovingIntoCur = dir == Hex.ReverseDirection(m_world.plates[dirData.plateId].direction);
                    bool isFrontCollision = dirNotNull && HexUtils.ArrayContainsPlate(m_world.tileData, hex.Front(dir), dirData.plateId, WorldSettings.Singleton.wrapWorld);


                    //                     float baseVal = height * .015f;
                    // 
                    //                     data.height -= baseVal;
                    //                     dirData.height += baseVal;
                    
                    if (HexUtils.HexOutOfBounds(m_world.size, dirHex))
                    {
                        tempPlates[hData.plateId].movementSpeed = -1f;
                        tempData[mapKey].empty = false;
                        continue;
                    }

                    var tmpData = tempData[dirData.hex.GetKey()];
                    //hData.height > 100.0f && dirData.height > 100.0f && 
                    if (dirDiffPlate && dirInto)
                    {
                        tempPlates[hData.plateId].movementSpeed -= 1f;
                        tempData[mapKey].empty = false;
                        continue;
                    }

                    if (dirDiffPlate)
                    {
                        //tempPlates[hData.plateId].movementSpeed -= .02f;
                        if (dirHigher)
                        {
                            continue;
                        }
                    }

                    if (plate.movementSpeed < 0f)
                    {
                        tempData[mapKey].empty = false;
                        continue;
                    }


                    float mod = 0f;
                    if (hData.height < 150f)
                    {
                        mod = hData.height * .1f + 5;
                    }

                    tmpData.height = hData.height + mod;
                    tempData[mapKey].empty = true;
                    tmpData.empty = false;
                    tmpData.moved = true;
                    plate.AddHex(dirHex);
                    tmpData.plateId = hData.plateId;
                }
            }

            for (int i = 0; i < tempPlates.Length; i++)
            {
                m_world.plates[i].movementSpeed = tempPlates[i].movementSpeed;

                bool res = m_world.TryGetHexData(m_world.plates[i].center, true, out TileObject data);
                if (res && data.moved)
                {
                    Hex newH = data.hex.Neighbor((int)m_world.plates[i].direction);
                    if (HexUtils.HexOutOfBounds(m_world.size, newH)) continue;
                    m_world.plates[i].center = newH;
                    Point pt = m_layout.HexToPixel(newH);
                    m_world.plates[i].obj.transform.position = new Vector3((float)pt.x, (float)pt.y, -1);
                }
            }

            ApplyTiles(tempData);
        }

        private IEnumerator GenerateRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f);
                if (m_state == GenState.GENERATE)
                    print("State_Generate");
                else if (m_state == GenState.ITERATING)
                    print("State_Iterating");
            }
        }

        private void ApplyTiles(in Dictionary<string, TileObject> tempData)
        {
            foreach (var pair in tempData)
            {
                float h = pair.Value.height;

                if (pair.Value.empty)
                {
                    h = 10f;

                    m_world.plates[pair.Value.plateId].RemoveHex(pair.Value.hex);
                    int closestId = 0;
                    int closest = int.MaxValue;
                    for (int i = 0; i < WorldSettings.Singleton.plates; i++)
                    {
                        int dist = pair.Value.hex.Distance(m_world.plates[i].center);

                        if (closest > dist)
                        {
                            closest = dist;
                            closestId = i;
                        }
                    }
                    for (int i = 0; i < WorldSettings.Singleton.plates; i++)
                    {
                        int dist = pair.Value.hex.Distance(HexUtils.WrapOffset(m_world.plates[i].center, m_world.size.x));

                        if (closest > dist)
                        {
                            closest = dist;
                            closestId = i;
                        }
                    }
                    m_world.tileData[pair.Key].plateId = closestId;
                    m_world.plates[closestId].AddHex(pair.Value.hex);
                }

                TileObject toTile = m_world.tileData[pair.Key];
                toTile.height = h;
                int n = 0;
                if (h < 100)
                    n = 2;
                if (h > 100)
                    n = 0;
                if (h > 200)
                    n = 1;
                toTile.tileId = n;
                toTile.render.sprite = tiles[n];
                toTile.collision = pair.Value.collision;

            }
        }

        private void Smooth()
        {
            Dictionary<string, TileObject> tempData = m_world.tileData.ToDictionary(entry => entry.Key, entry => entry.Value);
            foreach (var pair in m_world.tileData)
            {
                string key = pair.Key;
                TileObject tile = pair.Value;
                float val = tile.height;
                float avg = 0;
                int count = 0;
                foreach (Hex hex in tile.hex.Ring(1))
                {
                    var obj = m_world.GetHexData(hex, WorldSettings.Singleton.wrapWorld);
                    if (obj == null) continue;
                    avg += (obj.height / 455) * 100;
                    count++;
                }
                tempData[key].height += (avg / count) * .1f;
            }
            ApplyTiles(tempData);
        }

        private void SetCollisions(bool val)
        {
            foreach (var pair in m_world.tileData)
            {
                pair.Value.collision = val;
            }
        }

        private void CalcCollisions()
        {
            foreach (var pair in m_world.tileData)
            {
                Hex h = pair.Value.hex.Neighbor((int)m_world.GetPlateByID(pair.Value.plateId).direction);
                if (!m_world.TryGetHexData(h, WorldSettings.Singleton.wrapWorld, out TileObject dirData))
                    continue;
                bool dirDiffPlate = pair.Value.plateId != dirData.plateId;

                pair.Value.moved = false;

                if (HexUtils.HexOutOfBounds(m_world.size, dirData.hex) || dirData.collision)
                {
                    pair.Value.collision = true;

                }
                    
            }
        }
    }
}