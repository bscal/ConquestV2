using Conquest;
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

        public const float MIN_SPD = 155.0f;
        public const float MAX_SPD = 255.0f;

        public const float SOFT_MAX_HEIGHT = 255f;
        public const float MIN_HEIGHT = 0;
        public const float SEA_LVL = 100;
        public const float PLAIN_LVL = SOFT_MAX_HEIGHT / 2;
        public const float HILL_LVL = 175;
        public const float MTN_LVL = 225;
        public const float X_MTN_LVL = 255;

        private GenState m_state;

        private uint m_seed;
        private Unity.Mathematics.Random m_rand;
        private float m_plateSpdDecayModifier = 2f;   // Increase for larger plates, Decrease for smaller plates

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
                    var mapKey = hex.GetKey();
                    Point pixel = m_layout.HexToPixel(hex);

                    if (!m_world.tileData.ContainsKey(mapKey))
                    {
                        GameObject gameobject = Instantiate(prefab, new Vector3((float)pixel.x, (float)pixel.y, 0), Quaternion.identity);
                        TileObject tObj = gameobject.GetComponent<TileObject>();
                        m_world.tileData.Add(mapKey, tObj);
                        HexData hData = tObj.hexData;

                        int n = UnityEngine.Random.Range(0, 2);
                        float d = Noise.CalcPixel2D(q, r, .1f);

                        if (d < 100)
                            hData.isOcean = true;

                        tObj.hex = hex;

                        hData.age = Random.Range(0, 50);
                        hData.height = d;

                        Tile tile = tObj.FindCorrectTile();
                        tObj.SetTile(tile);
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
                    movementSpeed = m_rand.NextFloat(MIN_SPD, MAX_SPD) * m_plateSpdDecayModifier,
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
                    var mapKey = hex.GetKey();

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
                    var mapKey = hex.GetKey();

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


            if (iters != 0 && iters % 25 == 0)
            {
                for (int i = 0; i < m_world.plates.Count; i++)
                {
                    Plate p = m_world.plates[i];
                    p.direction = (HexDirection)Random.Range(0, HexConstants.MAX_DIR);
                }
                OnDirectionChange();
                Debug.LogWarning("changing directions");
            }
            if (iters != 0 && iters % 25 == 0)
            {
                for (int i = 0; i < m_world.plates.Count; i++)
                {
                    Plate p = m_world.plates[i];
                    p.movementSpeed = Random.Range(MIN_SPD, MAX_SPD) * m_plateSpdDecayModifier;
                }
            }

            CalcCollisions();
            float[] tempPlates = new float[m_world.plates.Count];
            for (int i = 0; i < tempPlates.Length; i++)
            {
                tempPlates[i] = m_world.plates[i].movementSpeed;
            }

            Dictionary<Hex, HexData> tempHeights = new Dictionary<Hex, HexData>(m_world.tileData.Count);
            for (int r = 0; r <= m_height; r++) // height
            {
                int r_offset = Mathf.FloorToInt(r / 2);
                for (int q = -r_offset; q <= m_width - r_offset; q++) // width with offset
                {
                    Hex hex = new Hex(q, r, -q - r);
                    var mapKey = hex.GetKey();

                    // Current Hex. This hex moves to directional hex.
                    TileObject hexObj = m_world.tileData[mapKey];
                    HexData hData = hexObj.hexData;
                    Plate plate = m_world.GetPlateByID(hData.plateId);
                    HexDirection dir = plate.direction;
                    float height = hData.height;
                    bool isLTESealvl = height <= SEA_LVL;

                    float speedModifier = Normalize(Mathf.Clamp(plate.movementSpeed, 0.05f, MAX_SPD), 0, MAX_SPD) * 2.0f;
                    bool move = true;
                    bool destroy = false;

                    // Move Direction Hex.
                    Hex dirHex = hex.Neighbor((int)dir);

                    if (!tempHeights.ContainsKey(mapKey))
                    {
                        tempHeights[mapKey] = new HexData(hData);
                    }

                    if (!isLTESealvl)
                    {
                        tempHeights[mapKey].isOcean = false;
                    }
                    else if (isLTESealvl)
                    {
                        tempHeights[mapKey].isCoast = false;
                    }

                    // Do not move hex if plate is not moving
                    if (plate.movementSpeed < 0.0f)
                    {
                        hData.empty = false;
                        move = false;
                    }

                    if (!HexUtils.HexOutOfBounds(m_world.size, hex) && HexUtils.HexOutOfBounds(m_world.size, dirHex))
                    {
                        if (isLTESealvl)
                            tempPlates[hData.plateId] -= 1f;
                        else
                            tempPlates[hData.plateId] -= 25f;
                        hData.empty = false;
                        hData.moved = false;
                        continue;
                    }

                    bool dirNotNull = m_world.TryGetHexData(dirHex, WorldSettings.Singleton.wrapWorld, out TileObject dirObj);
                    if (!dirNotNull) continue;
                    HexData dirData = dirObj.hexData;
                    var dirKey = dirObj.hex.GetKey();

                    Plate dirPlate = m_world.plates[dirData.plateId];
                    bool dirDiffPlate = hData.plateId != dirData.plateId;
                    bool dirInto = dir == Hex.ReverseDirection(m_world.plates[dirData.plateId].direction);
                    bool dirHigher = plate.elevation < dirPlate.elevation;
                    bool isDirLTESealvl = dirData.height <= SEA_LVL;
                    //bool dirMovingAway = HexUtils.HexMovingTowards((int)plate.direction, (int)dirPlate.direction);

                    // old way of movement
                    // float baseVal = height * .015f;
                    // 
                    // data.height -= baseVal;
                    // dirData.height += baseVal;  

                    /**
                     * Adds HexData to tempData array
                     */
                    if (!tempHeights.ContainsKey(dirKey))
                    {
                        tempHeights[dirKey] = new HexData(dirData);
                    }

                    // Convergent boundary
                    // Plate collision. current hex plate and moving direction plate colliding
                    if (dirDiffPlate && dirInto)
                    {
                        hData.empty = false;
                        bool collision = false;
                        if (!isLTESealvl && !isDirLTESealvl)
                            collision = true;
                        else if (isLTESealvl && !isDirLTESealvl)
                            move = false;

                        if (collision)
                        {
                            if (!dirHigher)
                            {
                                tempHeights[mapKey].height = height + ((height * .25f) + 10) * speedModifier;
                                tempHeights[mapKey].formingMoutain = true;
                            }
                            move = false;
                            tempPlates[hData.plateId] -= 25f;
                        }
                        else
                        {
                            destroy = true;
                        }


//                         if (!isLTESealvl && dirData.height > SEA_LVL)
//                             tempPlates[hData.plateId] -= 20f;
//                         if (!dirHigher && dirData.height > SEA_LVL)
//                         {
//                             tempHeights[mapKey].height = height + ((height * .1f) + 10) * speedModifier;
//                             tempHeights[mapKey].formingMoutain = true;
//                             hData.empty = false;
//                             move = false;
//                             tempPlates[hData.plateId] -= 20f;
//                         }
//                         else
//                         {
//                             hData.empty = false;
//                             destroy = true;
//                         }
                    }

                    // dirHex is on different plate and diff plate is not moving.
                    // This is handled the similar to a plate collision but technically is not real one.
                    if (dirDiffPlate && dirPlate.movementSpeed < 0f)
                    {
                        hData.empty = false;
                        bool collision = false;
                        if (!isLTESealvl && !isDirLTESealvl)
                            collision = true;
                        else if (isLTESealvl && !isDirLTESealvl)
                            move = false;

                        if (collision)
                        {
                            if (!dirHigher)
                            {
                                tempHeights[mapKey].height = height + ((height * .25f) + 10) * speedModifier;
                                tempHeights[mapKey].formingMoutain = true;
                            }
                            move = false;
                            tempPlates[hData.plateId] -= 25f;
                        }
                        else
                        {
                            destroy = true;
                        }
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
                        float mod = 0f;
                        if (hData.isHotSpot) // Hot spots
                            mod += 25f;
                        if (isLTESealvl)
                            mod += 0;//3f;
                        if (hData.age < 25)
                            mod += 1f;
                        if (hData.age < 10) // New created land gains more height
                            mod += 3f;
                        if (hData.age < 50)
                            mod += 2f;
                        if (hData.age < 200)
                            mod += 1f;
                        if (height > 150) // Erosion
                            mod += -.5f;
                        if (height > HILL_LVL) // Erosion of higher terrain
                            mod -= m_rand.NextInt(1, 3);
                        if (!dirDiffPlate && dirData.formingMoutain) // hex moving into hex that forming mountain
                            mod += 10f;
                        if (dirData.height > HILL_LVL && height < dirData.height && !dirData.isCoast) // hex that are moving into a higher hex that is not coast increase height
                            mod += m_rand.NextInt(3, 5);

                        mod *= speedModifier;
                        

                        tempHeights[dirKey].height = height + mod;
                        tempHeights[dirKey].isHotSpot = false;
                        dirData.empty = false;
                        if (plate.center.Equals(hex))
                        {
                            plate.center = dirObj.hex;
                            hData.moveCenter = true;
                        }

                        if (isLTESealvl)
                        {
                            tempHeights[dirKey].isOcean = false;
                        }

                        dirPlate.RemoveHex(dirHex);
                        plate.AddHex(dirHex);
                        tempHeights[mapKey].movedToHex = dirObj.hex;
                        tempHeights[mapKey].oldPlateId = hData.plateId;
                        tempHeights[dirKey].plateId = hData.plateId;

                        //tempPlates[hData.plateId] -= .01f;

                        //plate.movementSpeed -= .0001f;
                    }

                    foreach (Hex ringHex in hexObj.hex.Ring(1))
                    {
                        if (m_world.TryGetHexData(ringHex, out TileObject ringObj))
                        {
                            HexData data = ringObj.hexData;
                            if (data.isOcean)
                            {
                                if (hData.height < SEA_LVL)
                                {
                                    tempHeights[mapKey].isOcean = true;
                                }
                            }
                        }
                    }
                    hData.isHotSpot = false;
                }
            }

            /*
             * Moves plate dot if hex was moved
             */
            for (int i = 0; i < tempPlates.Length; i++)
            {
                Plate p = m_world.plates[i];
                p.movementSpeed = tempPlates[i];
//                 m_world.SetPlate(p.center, i);
// 
//                 foreach (Hex ringHex in p.center.Ring(1))
//                 {
//                     if (m_world.TryGetHexData(ringHex, out TileObject ringObj))
//                     {
//                         m_world.SetPlate(p.center, i);
//                     }
//                 }
            }
            ApplyTiles(tempHeights);
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

        private void ApplyTiles(in Dictionary<Hex, HexData> tempHeights)
        {
            foreach (var pair in m_world.tileData)
            {
                HexData hData = pair.Value.hexData;
                if (!tempHeights.ContainsKey(pair.Key)) {
                    continue;
                }
                hData.UpdateValues(tempHeights[pair.Key]);

                var ring = pair.Value.hex.Ring(1);

                if (hData.empty)
                {
                    hData.height = 10f;
                    hData.moved = true;
                    hData.isOcean = true;
                    hData.age = 0;
                    if (m_rand.NextFloat() < .025f)
                        hData.isHotSpot = true;

                    m_world.plates[hData.plateId].RemoveHex(pair.Value.hex);
                    //int closestId = tempHeights[pair.Key].oldPlateId;
                    //int closestId = GetHighestPlateInHexList(pair.Value.hex.Ring(1));
                    int closestId = GetClosestRingPlate(pair.Value.hex, ring);
                    m_world.tileData[pair.Key].hexData.plateId = closestId;
                    m_world.plates[closestId].AddHex(pair.Value.hex);
                }

                if (hData.moveCenter)
                {
                    Point pt = m_layout.HexToPixel(tempHeights[pair.Key].movedToHex);
                    m_world.plates[pair.Value.hexData.plateId].obj.transform.position = new Vector3((float)pt.x, (float)pt.y, -1);
                }

                float h = hData.height;

                Tile tile = pair.Value.FindCorrectTile();
                pair.Value.SetTile(tile);

                int notOceanCount = 0;
                foreach (Hex ringHex in ring)
                {
                    if (m_world.TryGetHexData(ringHex, WorldSettings.Singleton.wrapWorld, out TileObject ringObj))
                    {
                        HexData data = ringObj.hexData;
                        if (data.isOcean)
                        {
                            if (hData.height > SEA_LVL)
                            {
                                hData.isCoast = true;
                                break;
                            }
                        }
                        else
                        {
                            notOceanCount++;
                        }
                    }
                }
                if (notOceanCount >= HexConstants.DIRECTIONS)
                {
                    hData.isCoast = false;
                }
            }
        }

        private void Smooth()
        {
            Dictionary<Hex, HexData> tempHeights = new Dictionary<Hex, HexData>(m_world.tileData.Count);

            foreach (var pair in m_world.tileData)
            {
                var key = pair.Key;
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
                        obj.SetTile(tileMap.GetTileByName("grassland"));
                    }
                }
            }
            ApplyTiles(tempHeights);
        }

        private void OnDirectionChange()
        {
            foreach (var pair in m_world.tileData)
            {
                pair.Value.hexData.formingMoutain = false;
            }
        }

        private void CalcCollisions()
        {
            foreach (var pair in m_world.tileData)
            {
                HexData hData = pair.Value.hexData;
                hData.moveCenter = false;
                hData.empty = true;
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

        private int GetClosestRingPlate(Hex hex, in List<Hex> ring)
        {
            int[] ids = CountHexList(ring);
            return GetClosestPlate(hex, ids);
        }

        public int GetClosestPlate(Hex hex, int[] platesIds)
        {
            int closestId = -1;
            int closest = int.MaxValue;
            for (int i = 0; i < platesIds.Length; i++)
            {
                // TODO better names for these values?
                if (platesIds[i] < 2) continue;
                if (platesIds[i] > 4) return i; // If a hex (6 sides) has 5 or 6 hexes around it we add it to that plate.

                if (m_world.plates[i].hexes.Count < closest)
                {
                    closestId = i;
                    closest = m_world.plates[i].hexes.Count;
                }

                //int dist = hex.Distance(m_world.plates[i].center);
            }
            //             for (int i = 0; i < platesIds.Length; i++)
            //             {
            //                 if (platesIds[i] < 1) continue;
            //                 int dist = hex.Distance(HexUtils.WrapOffset(m_world.plates[i].center, m_world.size.x));
            // 
            //                 if (closestId != -1 && m_world.plates[closestId].hexes.Count < m_world.plates[i].hexes.Count / 2) continue;
            // 
            //                 if (closest > dist)
            //                 {
            //                     closest = dist;
            //                     closestId = i;
            //                 }
            //             }

            return closestId;
        }

        private float Normalize(float value, float min, float max)
        {
            return (value - min) / (max - min);
        }
    }
}