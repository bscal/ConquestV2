using Conquest;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conquest
{
    [Serializable]
    public class World
    {
        public WorldSettings settings;

        public Layout layout;
        public Vector2Int size;
        private HexTileObjectDictionary m_tileData;
        public Dictionary<Hex, TileObject> TileData => m_tileData.dictionary;
        public List<Plate> plates;

        public int numOfHexes;
        public int pixelW;
        public int pixelH;

        public int plateCounter;
        public int cells = 4;
        public WorldTemp worldTemp;
        public WindManager windManager;

        public int Equator { get { return size.y / 2; } }
        public int NorthPole { get { return size.y; } }
        public int SouthPole { get { return 0; } }

        public World(WorldSettings settings)
        {
            this.settings = settings;
            // Size.x = 9.1 because it will be improperly aligned otherwise
            // This is because the Hex Sprites are not even hexagon on all sides and need to be stretched
            layout = new Layout(Layout.pointy, new Point(9.235, 8.0), new Point(0, 0));

            numOfHexes = settings.width * settings.height;
            pixelW = (int)(settings.width * (Mathf.Sqrt(3) * layout.size.x));
            pixelH = (int)(settings.height * (2 * layout.size.y) * 3/4);
            size = new Vector2Int(settings.width, settings.height);
            m_tileData = HexTileObjectDictionary.New<HexTileObjectDictionary>();
            plates = new List<Plate>(settings.plates + 1);
            worldTemp = new WorldTemp(WorldTemp.EARTH_TEMP, GameManager.Singleton.Generator);
            windManager = new WindManager(WindManager.PROGRADE_SPIN, WindManager.CellLayout.EARTH, this);
        }

        public void UpdateValues()
        {
            float avgWorldTemp = 0;
            foreach (var pair in TileData)
            {
                avgWorldTemp = pair.Value.hexData.temp;
            }
            worldTemp.AvgTemp = avgWorldTemp / TileData.Count;
        }

        public bool ContainsHex(Hex hex)
        {
            return TileData.ContainsKey(hex.GetKey());
        }

        public bool TryGetHexData(Hex hex, out TileObject obj)
        {
            return GetHexData(hex, settings.wrapWorld, out obj);
        }

        public bool GetHexData(Hex hex, bool wrapIfNull, out TileObject obj)
        {
            if (wrapIfNull && !TileData.ContainsKey(hex) && !HexUtils.HexOutOfBounds(size, hex, wrapIfNull))
                hex = HexUtils.WrapOffset(hex, size.x);

            return TileData.TryGetValue(hex, out obj);
        }

        public TileObject GetWrappedHex(Hex hex)
        {
            return TileData[HexUtils.WrapOffset(hex, size.x)];
        }

        public Plate GetPlateByID(int id)
        {
            return plates.Find(plate => plate.id == id); 
        }

        public List<Plate> GetPlates() => plates;

        public int AddPlate(Plate plate)
        {
            plates.Add(plate);
            plate.id = plateCounter++;
            return plate.id;
        }

        public TileObject[] HexArrayToTileObjectArray(Hex[] hexes)
        {
            TileObject[] objs = new TileObject[hexes.Length];
            for (int i = 0; i < hexes.Length; i++)
            {
                Hex h = hexes[i];
                bool contains = TryGetHexData(h, out TileObject obj);
                if (contains)
                    objs[i] = obj;
                else
                    objs[i] = null;
            }
            return objs;
        }
    }
}
