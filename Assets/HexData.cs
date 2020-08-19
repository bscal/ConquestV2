﻿namespace Conquest
{
    public class HexData
    {
        public int plateId = 0;
        public int age = 0;
        public float height = 0;
        public float temp = 0;
        public float wetness = 0;

        public bool isPlateEdge = false;
        public bool collision = false;
        public bool empty = false;
        public bool moved = false;
        public bool generated = false;
        public bool formingMoutain = false;
        public bool isOcean = false;
        public bool isCoast = false;

        public int oldPlateId;
        public Hex movedToHex;

        public HexData() { }

        public HexData(HexData other)
        {
            CopyValues(other);
        }

        /// <summary>
        /// Copies all values from other to current hex.
        /// </summary>
        /// <param name="other"></param>
        public void CopyValues(HexData other)
        {
            plateId         = other.plateId;
            age             = other.age;
            height          = other.height;
            temp            = other.temp;
            wetness         = other.wetness;
            isPlateEdge     = other.isPlateEdge;
            collision       = other.collision;
            empty           = other.empty;
            moved           = other.moved;
            generated       = other.generated;
            formingMoutain  = other.formingMoutain;
            isOcean         = other.isOcean;
            isCoast         = other.isCoast;
            oldPlateId      = other.oldPlateId;
            movedToHex      = other.movedToHex;
        }

        /// <summary>
        /// Used in updating some of a hexes values.
        /// </summary>
        /// <param name="other"></param>
        public void UpdateValues(HexData other)
        {
            height          = other.height;
            plateId         = other.plateId;
            formingMoutain  = other.formingMoutain;
            isOcean         = other.isOcean;
            isCoast         = other.isCoast;
        }
    }
}