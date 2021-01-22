using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conquest {

    public static class HexUtils
    {

        public static bool ArrayContainsPlate(in Dictionary<Hex, TileObject> hexdata, Hex[] hexes, int plateId, bool wrap, int width)
        {
            foreach (Hex h in hexes)
            {
                var key = h.GetKey();
                if (!hexdata.ContainsKey(key))
                {
                    if (wrap && !HexUtils.HexOutOfBounds(GameManager.Singleton.World.size, h, wrap))
                    {
                        key = HexUtils.WrapOffset(h, width).GetKey();
                        if (hexdata.ContainsKey(key) && hexdata[key].hexData.plateId == plateId)
                            return true;
                    }
                }
                else if(hexdata[h.GetKey()].hexData.plateId == plateId)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ArrayCountContains(Dictionary<Hex, TileObject> hexdata, Hex[] hexes, int plateId)
        {
            int contains = 0;
            foreach (Hex h in hexes)
            {
                var key = h.GetKey();
                if (!hexdata.ContainsKey(key)) continue;
                if (hexdata[key].hexData.plateId == plateId) contains++;
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

        public static Hex WrapHexIfNull(Hex h, int gridWidth, in Dictionary<Hex, TileObject> data)
        {
            bool contains = data.ContainsKey(h.GetKey());
            if (!contains)
            {
                return null;
            }
            else if (!contains)
            {
                return WrapOffset(h, gridWidth);
            }
            return h;
        }

        public static bool HexOutOfBounds(Vector2Int size, Hex hex, bool wrap)
        {
            OffsetCoord coord = OffsetCoord.RoffsetFromCube(WorldSettings.OFFFSET_COORD, hex);
            if (coord.row < 0 || coord.row > size.y) return true;
            if (!wrap) return coord.col < 0 || coord.col > size.x;
            return false;
        }

        /// <summary>
        /// Detects if movDir is moving in an "away" direction from curDir.
        /// An away direction would be if curDir is moving NE then away directions would be E, NE, NW.
        /// (HexDirection enum is for pointy hex directions. This would still work same for flat top hexes just not same cardinal direction name)
        /// </summary>
        /// <param name="curDir"></param>
        /// <param name="movDir"></param>
        /// <returns></returns>
        public static bool HexMovingTowards(int curDir, int movDir)
        {
            return curDir == movDir
                || ClampWrap(curDir + 1, HexConstants.MIN_DIR, HexConstants.MAX_DIR) == movDir
                || ClampWrap(curDir - 1, HexConstants.MIN_DIR, HexConstants.MAX_DIR) == movDir;
        }

        /// <summary>
        /// Works similar to Clamp function but min wraps to max and max wraps to min.
        /// </summary>
        public static int ClampWrap(int value, int min, int max)
        {
            if (value < min)
                return max;
            else if (value > max)
                return min;
            else
                return value;
        }

        public static bool IsHexLeftOfHex(Hex a, Hex b)
        {
            Point pA = GameManager.Singleton.World.layout.HexToPixel(a);
            Point pB = GameManager.Singleton.World.layout.HexToPixel(b);

            return Mathf.Abs((float)pA.x - (float)pB.x) > 0;
        }

        public static bool IsHexAboveHex(Hex a, Hex b)
        {
            Point pA = GameManager.Singleton.World.layout.HexToPixel(a);
            Point pB = GameManager.Singleton.World.layout.HexToPixel(b);

            return Mathf.Abs((float)pA.y - (float)pB.y) > 0;
        }

    }
}
