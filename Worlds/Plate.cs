using System;
using System.Collections.Generic;
using UnityEngine;

namespace Conquest
{
    public struct Plate
    {
        public Hex center;
        public Color color;
        public HashSet<string> hexes;

        public int age;
        public int elevation;
        public HexDirection movement;

        public Plate(Hex center, Color color)
        {
            this.center = center;
            this.color = color;
            this.hexes = new HashSet<string>();

            this.age = 0;
            this.elevation = 0;
            this.movement = HexDirection.NONE;
        }

        public bool ContainsHex(Hex h)
        {
            return hexes.Contains(h.GetKey());
        }

        public void AddHex(Hex h)
        {
            hexes.Add(h.GetKey());
        }
    }
}
