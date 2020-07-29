using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conquest {

    public static class HexUtils
    {

        public static bool ArrayContainsPlate(Dictionary<string, TileObject> hexdata, Hex[] hexes, int plateId, bool wrap)
        {
            foreach (Hex h in hexes)
            {
                if (!hexdata.ContainsKey(h.GetKey()))
                {
                    if (wrap && !HexUtils.HexOutOfBounds(GameManager.Singleton.World.size, h))
                    {
                        string key = HexUtils.WrapOffset(h, WorldSettings.Singleton.width).GetKey();
                        if (hexdata.ContainsKey(key) && hexdata[key].plateId == plateId)
                            return true;
                    }
                }
                else if(hexdata[h.GetKey()].plateId == plateId)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ArrayCountContains(Dictionary<string, TileObject> hexdata, Hex[] hexes, int plateId)
        {
            int contains = 0;
            foreach (Hex h in hexes)
            {
                if (!hexdata.ContainsKey(h.GetKey())) continue;
                if (hexdata[h.GetKey()].plateId == plateId) contains++;
            }
            return hexes.Length == contains;
        }

        public static Hex WrapOffset(Hex h, int gridWidth)
        {
            OffsetCoord coord = OffsetCoord.RoffsetFromCube(OffsetCoord.EVEN, h);
            int newQ;
            if (coord.col > gridWidth / 2)
                newQ = h.q - gridWidth - 1;
            else
                newQ = h.q + gridWidth + 1;

            return new Hex(newQ, h.r, -newQ - h.r);
        }

        public static bool HexOutOfBounds(Vector2Int size, Hex hex)
        {
            OffsetCoord coord = OffsetCoord.RoffsetFromCube(WorldSettings.Singleton.offset, hex);
            return (coord.row < 0 || coord.row > size.y ) || !WorldSettings.Singleton.wrapWorld && coord.col < 0 || !WorldSettings.Singleton.wrapWorld && coord.col > size.x;
        }

    }
}
