using System;

namespace Conquest
{
    public class HexData : ICloneable
    {
        public int plateId;
        public int oldPlateId;
        public int age;
        public float height;
        public float temp;
        public float wetness;

        public bool empty;
        public bool moved;
        public bool formingMoutain;
        public bool isOcean;
        public bool isCoast;
        public bool isHotSpot;

        // Useful for debugging
        public bool lastEmpty;
        public bool lastMoved;

        public HexData() { }

        public HexData(HexData other)
        {
            CopyValues(other);
        }

        public object Clone()
        {
            HexData data = new HexData();
            data.CopyValues(this);
            return data;
        }

        /// <summary>
        /// Copies all values from other to current hex.
        /// </summary>
        /// <param name="other"></param>
        public void CopyValues(HexData other)
        {
            plateId         = other.plateId;
            oldPlateId      = other.oldPlateId;
            age             = other.age;
            height          = other.height;
            temp            = other.temp;
            wetness         = other.wetness;
            empty           = other.empty;
            moved           = other.moved;
            formingMoutain  = other.formingMoutain;
            isOcean         = other.isOcean;
            isCoast         = other.isCoast;
            isHotSpot       = other.isHotSpot;
            lastEmpty       = other.lastEmpty;
            lastMoved       = other.lastMoved;
        }
    }
}