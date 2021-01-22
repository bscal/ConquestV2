using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conquest
{
    public class Plate
    {

        public const float MIN_SPLIT_PERCENTAGE = 0.45f;
        public const float MAX_SPLIT_PERCENTAGE = 0.9f;

        public int id;
        public Hex center;
        public Color color;
        public List<Hex> hexes;

        public int age;
        public float elevation;
        public HexDirection direction;
        public float movementSpeed;

        private readonly MapGenerator gen;

        public Plate(MapGenerator gen, Color color) : this(gen, null, color)
        {
        }

        public Plate(MapGenerator gen, Hex center, Color color)
        {
            this.gen = gen;
            this.center = center;
            this.color = color;
            this.hexes = new List<Hex>();
            this.age = 0;
            this.elevation = 0f;
            this.direction = HexDirection.NONE;
            this.movementSpeed = 1.0f;
        }

        public bool ContainsHex(Hex hex)
        {
            return hexes.Contains(hex);
        }

        public void AddHex(Hex hex)
        {
            hexes.Add(hex);
        }

        public void RemoveHex(Hex hex)
        {
            hexes.RemoveAll(h => h.Equals(hex));
        }

        public bool TrySplit()
        {
            if (hexes.Count < 1)
            {
                GameManager.Singleton.World.GetPlates().Remove(this);
                return false;
            }

            float percentageOfWorld = (float)hexes.Count/(float)gen.GetWorld().numOfHexes;

            if (percentageOfWorld > MAX_SPLIT_PERCENTAGE)
            {
                Split(SplitType.NORMAL);
            }
            
            else if (percentageOfWorld > MIN_SPLIT_PERCENTAGE)
            {
                float chance = ((percentageOfWorld - MIN_SPLIT_PERCENTAGE) / (MAX_SPLIT_PERCENTAGE - MIN_SPLIT_PERCENTAGE)) + .1f;
                float rand = UnityEngine.Random.value;
                Debug.Log(percentageOfWorld + " = " + chance + " | " + rand);
                if (chance >= rand)
                {
                    Split(SplitType.NORMAL);
                }
            }
            return true;
        }

        public void Split(SplitType type)
        {
            if (hexes.Count < 1)
            {
                GameManager.Singleton.World.GetPlates().Remove(this);
                return;
            }

            if (type == SplitType.NORMAL)
            {
                Plate newPlate = new Plate(gen, UnityEngine.Random.ColorHSV());
                newPlate.direction = (HexDirection)UnityEngine.Random.Range(0, HexConstants.MAX_DIR);
                int id = GameManager.Singleton.World.AddPlate(newPlate);
                Debug.Log("CREATING PLATE");
                int q = 0;
                int r = 0;
                foreach (Hex h in hexes)
                {
                    q += h.q;
                    r += h.r;
                }
                q /= hexes.Count;
                r /= hexes.Count;
                 
                FractionalHex fHexCenter = new FractionalHex(q, r, -q-r);
                Hex centerHex = fHexCenter.HexRound();

                for (int i = hexes.Count - 1; i > -1; i--)
                {
                    Hex cur = hexes[i];

                    if ((cur.q - centerHex.q) > 0)
                    {
                        int dist = cur.Distance(centerHex);
                        if (dist < 2 && UnityEngine.Random.value < .5)
                            continue;
                        hexes.RemoveAt(i);
                        newPlate.AddHex(cur);
                        if (GameManager.Singleton.World.TryGetHexData(cur, out TileObject obj))
                            obj.hexData.plateId = id;
                    }
                }
            }
        }

        public enum SplitType
        {
            NORMAL,
            SMALL_LARGE,
            TRIPLE,
        }
    }
}

