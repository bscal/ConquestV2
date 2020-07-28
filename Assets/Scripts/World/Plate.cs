using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conquest
{
    public class Plate
    {

        public Hex center;
        public Color color;
        public List<string> hexes;

        public int age;
        public float elevation;
        public HexDirection direction;
        public float movementSpeed;

        public Plate(Hex center, Color color)
        {
            this.center = center;
            this.color = color;
            this.hexes = new List<string>();
            this.age = 0;
            this.elevation = 0f;
            this.direction = HexDirection.NONE;
            this.movementSpeed = 1.0f;
        }

        public bool ContainsHex(Hex h)
        {
            return hexes.Contains(h.GetKey());
        }

        public void AddHex(Hex h)
        {
            hexes.Add(h.GetKey());
        }

        public void RemoveHex(Hex hex)
        {
            hexes.Remove(hex.GetKey());
        }
    }
}

