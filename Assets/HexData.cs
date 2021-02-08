using System;

namespace Conquest
{
    public class HexData
    {

        // Not Copied

        public int age;
        public float speedModifier;
        public bool isPassible = true;

        // Variables that are Copied for Simulation

        public bool formingMoutain;
        public bool isOcean;
        public bool isCoast;
        public bool isHotSpot;

        public int plateId;
        public int oldPlateId;
        public float height;
        public float temp;
        public float wetness;

        public bool empty;
        public bool moved;

        // Useful for debugging
        public bool lastEmpty;
        public bool lastMoved;

        public HexData() { }

        public HexData(HexData other)
        {
            CopyValues(other);
        }

        public void CopyValues(HexData other)
        {
            age = other.age;
            plateId = other.plateId;
            oldPlateId = other.oldPlateId;
            height = other.height;
            temp = other.temp;
            wetness = other.wetness;
            empty = other.empty;
            moved = other.moved;
            formingMoutain = other.formingMoutain;
            isOcean = other.isOcean;
            isCoast = other.isCoast;
            isHotSpot = other.isHotSpot;
            lastEmpty = other.lastEmpty;
            lastMoved = other.lastMoved;
        }

    }
}