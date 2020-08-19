using Conquest;
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
        public readonly Dictionary<string, TileObject> tileData;

        public readonly List<Plate> plates;

        public World(int w, int h)
        {
            layout = new Layout(Layout.pointy, new Point(8, 8), new Point(0, 0));
            size = new Vector2Int(w, h);
            tileData = new Dictionary<string, TileObject>();
            plates = new List<Plate>(WorldSettings.Singleton.plates + 1);
        }

        public bool ContainsHex(Hex hex)
        {
            return tileData.ContainsKey(hex.GetKey());
        }

        public TileObject GetHexData(Hex hex, bool wrapIfNull)
        {
            string key = hex.GetKey();

            if (!tileData.ContainsKey(key))
            {
                if (wrapIfNull && !HexUtils.HexOutOfBounds(size, hex))
                {
                    key = HexUtils.WrapOffset(hex, size.x).GetKey();
                    return tileData[key];
                }
                return null;
            }
            return tileData[key];
        }

        public bool TryGetHexData(Hex hex, bool wrapIfNull, out TileObject obj)
        {
            string key = hex.GetKey();

            if (wrapIfNull && !tileData.ContainsKey(key) && !HexUtils.HexOutOfBounds(size, hex))
                key = HexUtils.WrapOffset(hex, size.x).GetKey();

            return tileData.TryGetValue(key, out obj);
        }

        public TileObject GetWrappedHex(Hex hex)
        {
            return tileData[HexUtils.WrapOffset(hex, size.x).GetKey()];
        }

        public Plate GetPlateByID(int id)
        {
            return plates[id];
        }

    }
}
