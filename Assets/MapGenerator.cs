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
            GameObject line = new GameObject();
            var lr = line.AddComponent<LineRenderer>();
            lr.positionCount = 5;
            lr.SetPosition(0, new Vector3(0, 0, -1));
            lr.SetPosition(1, new Vector3(m_pixelW, 0, -1));
            lr.SetPosition(2, new Vector3(m_pixelW, m_pixelH, -1));
            lr.SetPosition(3, new Vector3(0, m_pixelH, -1));
            lr.SetPosition(4, new Vector3(0, 0, -1));
            lr.startWidth = 2;
            lr.endWidth = 2;

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
                m_world.AddPlate(p);
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
                    m_world.tileData[mapKey].hexData.plateId = m_world.GetPlates()[closestId].id;
                    m_world.GetPlates()[closestId].AddHex(hex);
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
            

            if (iters != 0 && iters % 10 == 0)
            {
                for (int i = m_world.plates.Count - 1; i > -1 ; i--)
                {
                    Plate p = m_world.plates[i];
                    p.direction = (HexDirection)Random.Range(0, HexConstants.MAX_DIR);
                    p.TrySplit();
                }
                OnDirectionChange();
                Debug.LogWarning("changing directions");
            }

            CalcCollisions();


            Dictionary<Hex, HexData> tempHeights = new Dictionary<Hex, HexData>(m_world.tileData.Count);
            for (int r = 0; r <= m_height; r++) // height
            {
                int r_offset = Mathf.FloorToInt(r / 2);
                for (int q = -r_offset; q <= m_width - r_offset; q++) // width with offset
                {
                    Hex hex = new Hex(q, r, -q - r);
                    // This is a var because testing different types
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

                    // Move Direction Hex.
                    Hex dirHex = hex.Neighbor((int)dir);

                    // Adds hex into temp map
                    if (!tempHeights.ContainsKey(mapKey))
                        tempHeights[mapKey] = new HexData(hData);

                    tempHeights[mapKey].isOcean = isLTESealvl;

                    // Hex is NOT out of bounds and dirHex is
                    // This does not apply is world wrapping is on
                    if (!HexUtils.HexOutOfBounds(m_world.size, hex) && HexUtils.HexOutOfBounds(m_world.size, dirHex))
                    {
                        //if (isLTESealvl)
                            //tempPlates[hData.plateId] -= 1f;
                        //else
                            //tempPlates[hData.plateId] -= 25f;
                        hData.empty = false;
                        hData.moved = false;
                        continue;
                    }

                    bool dirNotNull = m_world.TryGetHexData(dirHex, out TileObject dirObj);
                    if (!dirNotNull) continue;
                    HexData dirData = dirObj.hexData;
                    var dirKey = dirObj.hex.GetKey();

                    Plate dirPlate = m_world.GetPlateByID(dirData.plateId);
                    bool dirDiffPlate = hData.plateId != dirData.plateId;
                    bool dirInto = dir == Hex.ReverseDirection(m_world.GetPlateByID(dirData.plateId).direction);
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
                        tempHeights[dirKey] = new HexData(dirData);

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
                                //tempHeights[mapKey].height = height + ((height * .25f) + 10) * speedModifier;
                                tempHeights[mapKey].height += 20;
                                tempHeights[mapKey].formingMoutain = true;
                            }
                            move = false;
                            hData.empty = false;
                            //tempPlates[hData.plateId] -= 25f;
                        }
                    }

                    // dirHex is on different plate and diff plate is not moving.
                    // This is handled the similar to a plate collision but technically is not real one.
                    if (dirDiffPlate && dirHigher)
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
                                tempHeights[mapKey].height += 15;
                                tempHeights[mapKey].formingMoutain = true;
                            }
                            move = false;
                            hData.empty = false;
                            //tempPlates[hData.plateId] -= 25f;
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
                            mod += 25f + m_rand.NextFloat(0f, 10f);
                        if (hData.height < SEA_LVL - 55)
                            mod += 1;//3f;
                        if (hData.age < 10) // New created land gains more height
                            mod += m_rand.NextFloat(6f, 9f);
                        if (hData.age < 50)
                            mod += 1f;
                        //if (hData.age < 200)
                            //mod += 1f;
                        //if (height > 155) // Erosion
                           //mod -= 1f;
                        if (height > HILL_LVL)
                            mod -= 2.5f;
                        if (height > HILL_LVL + 55 && !hData.formingMoutain)
                            mod -= 2.5f;
                        if (!dirDiffPlate && dirData.formingMoutain) // hex moving into hex that forming mountain
                            mod -= 5f;
                        if (height < dirData.height - 35 && !dirData.isCoast && !dirData.isOcean)
                            mod += 2f;
                        if (dirData.height > HILL_LVL && height < dirData.height - 10 && !dirData.isCoast && !dirData.isOcean) // hex that are moving into a higher hex that is not coast increase height
                            mod += 5f;

                        //mod *= speedModifier;

                        tempHeights[dirKey].height = height + mod;
                        tempHeights[dirKey].isHotSpot = false;
                        dirData.empty = false;
                        if (plate.center != null && plate.center.Equals(hex))
                        {
                            plate.center = dirObj.hex;
                            hData.moveCenter = true;
                        }

                        if (isLTESealvl)
                            tempHeights[dirKey].isOcean = false;

                        tempHeights[mapKey].oldPlateId = plate.id;
                        tempHeights[dirKey].plateId = plate.id;
                        tempHeights[dirKey].oldPlateId = dirPlate.id;
                        tempHeights[mapKey].movedToHex = dirHex;
                    }

                    foreach (Hex ringHex in hexObj.hex.Ring(1))
                    {
                        if (m_world.TryGetHexData(ringHex, out TileObject ringObj))
                        {
                            HexData data = ringObj.hexData;
                            if (data.isOcean)
                            {
                                if (hData.height < SEA_LVL)
                                    tempHeights[mapKey].isOcean = true;
                            }
                        }
                    }
                    hData.isHotSpot = false;
                }
            }

            /*
             * Moves plate dot if hex was moved
             */
            //for (int i = 0; i < tempPlates.Length; i++)
           // {
            //    Plate p = m_world.plates[i];
             //   p.movementSpeed = tempPlates[i];
            //}
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
                if (!tempHeights.ContainsKey(pair.Key))
                    continue;
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

                    int closestId = GetClosestRingPlate(pair.Value.hex, ring);
                    if (closestId < 0)
                        closestId = hData.plateId;

                    hData.oldPlateId = hData.plateId;
                    hData.plateId = closestId;
                }

                m_world.GetPlateByID(hData.oldPlateId).RemoveHex(pair.Value.hex);
                m_world.GetPlateByID(hData.plateId).AddHex(pair.Value.hex);

                Tile tile = pair.Value.FindCorrectTile();
                pair.Value.SetTile(tile);

                int notOceanCount = 0;
                foreach (Hex ringHex in ring)
                {
                    if (m_world.TryGetHexData(ringHex, out TileObject ringObj))
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
                            notOceanCount++;
                    }
                }
                if (notOceanCount >= HexConstants.DIRECTIONS)
                    hData.isCoast = false;
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
                    hData.UpdateValues(tempHeights[key]);

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
                pair.Value.hexData.formingMoutain = false;
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

        private Dictionary<int, int> CountHexList(in List<Hex> ring)
        {
            Dictionary<int, int> counts = new Dictionary<int, int>(m_world.GetPlates().Count);
            for (int i = 0; i < ring.Count; i++)
            {
                if (!m_world.TryGetHexData(ring[i], out TileObject obj))
                    continue;

                if (!counts.ContainsKey(obj.hexData.plateId))
                    counts[obj.hexData.plateId] = 1;
                else 
                    counts[obj.hexData.plateId] += 1;
            }
            return counts;
        }

        private int GetClosestRingPlate(Hex hex, in List<Hex> ring)
        {
            Dictionary<int, int> ids = CountHexList(ring);
            return GetClosestPlate(hex, ids);
        }

        public int GetClosestPlate(Hex hex, Dictionary<int, int> platesIds)
        {
            int closestId = -1;
            int closest = int.MaxValue;
            foreach (var pair in platesIds)
            {
                if (pair.Value < 2) continue;
                if (pair.Value > 4) return pair.Key; // If a hex (6 sides) has 5 or 6 hexes around it we add it to that plate.

                int count = m_world.GetPlateByID(pair.Key).hexes.Count;
                if (count < closest)
                {
                    closestId = pair.Key;
                    closest = count;
                }
            }

            return closestId;
        }

        private float Normalize(float value, float min, float max)
        {
            return (value - min) / (max - min);
        }
    }
}