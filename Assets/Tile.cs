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
    [CreateAssetMenu(fileName = "Tile", menuName = "Conquest/Tile")]
    public class Tile : ScriptableObject
    {
        public string localName;

        public Sprite sprite;
        public Sprite coldSprite;
        public Sprite hotSprite;
        public Sprite veryColdSprite;

        public bool isSpriteInFront = false;

        public float minHeight;
        public float maxHeight;

        [NonSerialized]
        public int id;
    }
}

