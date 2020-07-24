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
                    if (wrap && !HexUtils.HexYBounds(WorldSettings.Singleton.height, h.r))
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

        public static bool HexYBounds(int height, int y)
        {
            return y < 0 || y > height - 1;
        }

    }
}
