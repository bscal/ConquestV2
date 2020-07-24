using Conquest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conquest {
    public class World
    {
        public readonly Layout layout;
        public readonly Vector2Int size;
        public readonly Dictionary<string, TileObject> tileData;

        public readonly List<Plate> plates;

        public World(int w, int h)
        {
            layout = new Layout(Layout.pointy, new Point(8, 8), new Point(0, 0));
            size = new Vector2Int(w, h);
            tileData = new Dictionary<string, TileObject>();
            plates = new List<Plate>(WorldSettings.Singleton.plates);
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
                if (wrapIfNull && !HexUtils.HexYBounds(size.y, hex.r))
                {
                    key = HexUtils.WrapOffset(hex, size.x).GetKey();
                    return tileData[key];
                }
                return null;
            }
            return tileData[key];
        }

        public bool TryGetHexData(Hex hex, bool wrapIfNull, out TileObject data)
        {
            string key = hex.GetKey();

            if (wrapIfNull && !tileData.ContainsKey(key) && !HexUtils.HexYBounds(size.y, hex.r))
                key = HexUtils.WrapOffset(hex, size.x).GetKey();

            return tileData.TryGetValue(key, out data);
        }

        public TileObject GetWrappedHex(Hex hex)
        {
            return tileData[HexUtils.WrapOffset(hex, size.x).GetKey()];
        }

    }
}
