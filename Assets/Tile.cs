using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conquest
{
    /// <summary>
    /// Container for a 2D Tile (Hex) sprite and name
    /// </summary>
    [Serializable]
    public class Tile
    {
        public string name;

        public Sprite sprite;
        public Sprite coldSprite;
        public Sprite hotSprite;
        public Sprite veryColdSprite;

        public bool isSpriteInFront = false;

        public float minHeight;

        [NonSerialized]
        public int id;
    }
}

