using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conquest {

    public static class HexUtils
    {

        public static bool ArrayContainsPlate(in Dictionary<string, TileObject> hexdata, Hex[] hexes, int plateId, bool wrap)
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

        public static Hex WrapHexIfNull(Hex h, int gridWidth, in Dictionary<string, TileObject> data)
        {
            bool contains = data.ContainsKey(h.GetKey());
            if (!WorldSettings.Singleton.wrapWorld && !contains)
            {
                return null;
            }
            else if (!contains)
            {
                return WrapOffset(h, gridWidth);
            }
            return h;
        }

        public static bool HexOutOfBounds(Vector2Int size, Hex hex)
        {
            OffsetCoord coord = OffsetCoord.RoffsetFromCube(WorldSettings.Singleton.offset, hex);
            return (coord.row < 0 || coord.row > size.y ) || !WorldSettings.Singleton.wrapWorld && coord.col < 0 || !WorldSettings.Singleton.wrapWorld && coord.col > size.x;
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

    }
}
