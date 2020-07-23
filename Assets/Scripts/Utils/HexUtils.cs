using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conquest {

    public static class HexUtils
    {

        public static bool ArrayContainsPlate(Dictionary<string, TileObject> hexdata, Hex[] hexes, int plateId)
        {
            foreach (Hex h in hexes)
            {
                if (hexdata[h.GetKey()].plateId == plateId) return true;
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
            int newQ;
            if (h.s < -gridWidth)
                newQ = gridWidth + 1 + h.s;
            else
                newQ = gridWidth - 1 + h.s;

            return new Hex(newQ, h.r, -newQ - h.r);
        }

    }
}
