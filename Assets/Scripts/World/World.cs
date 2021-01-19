using Conquest;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conquest
{
    public class World
    {
        private static readonly Plate NO_PLATE = new Plate(new Hex(0, 0, 0), Color.white) {
            direction = HexDirection.NONE
        };

        public readonly Layout layout;
        public readonly Vector2Int size;
        public readonly Dictionary<Hex, TileObject> tileData;

        public readonly List<Plate> plates;
        public int plateCounter;

        public int cells = 4;
        public float worldTemp = 13.73f;
        public float worldTempChange = 0.0f;

        public int Equator { get { return size.y / 2; } }
        public int NorthPole { get { return size.y; } }
        public int SouthPole { get { return 0; } }

        public World(int w, int h)
        {
            layout = new Layout(Layout.pointy, new Point(8, 8), new Point(0, 0));
            size = new Vector2Int(w, h);
            tileData = new Dictionary<Hex, TileObject>();
            plates = new List<Plate>(WorldSettings.Singleton.plates + 1);
        }

        public bool ContainsHex(Hex hex)
        {
            return tileData.ContainsKey(hex.GetKey());
        }

        public bool TryGetHexData(Hex hex, out TileObject obj)
        {
            return GetHexData(hex, WorldSettings.Singleton.wrapWorld, out obj);
        }

        public bool GetHexData(Hex hex, bool wrapIfNull, out TileObject obj)
        {
            if (wrapIfNull && !tileData.ContainsKey(hex) && !HexUtils.HexOutOfBounds(size, hex))
                hex = HexUtils.WrapOffset(hex, size.x);

            return tileData.TryGetValue(hex, out obj);
        }

        public TileObject GetWrappedHex(Hex hex)
        {
            return tileData[HexUtils.WrapOffset(hex, size.x)];
        }

        public Plate GetPlateByID(int id)
        {
            return plates.Find(plate => plate.id == id); 
        }

        public List<Plate> GetPlates() => plates;

        public void SetPlate(Hex hex, int i)
        {
            if (!tileData.TryGetValue(hex, out TileObject hData)) return;
            Plate old = GetPlateByID(hData.hexData.plateId);
            if (old != null)
                old.RemoveHex(hex);

            hData.hexData.plateId = i;

            GetPlateByID(i).AddHex(hex);
        }

        public int AddPlate(Plate plate)
        {
            plates.Add(plate);
            plate.id = plateCounter++;
            return plate.id;
        }
    }
}
