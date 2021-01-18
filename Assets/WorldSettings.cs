using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Conquest
{
    public class WorldSettings : MonoBehaviour
    {
        public static WorldSettings Singleton { get; private set; }

        public Vector2Int size;
        public int numOfHexes;
        public readonly int offset = OffsetCoord.EVEN;

        [Header("World Settings")]
        public uint seed;
        public bool wrapWorld;

        [Header("Size")]
        public int width;
        public int height;

        [NonSerialized]
        public int pixelW;
        [NonSerialized]
        public int pixelH;

        [Header("Generation")]
        public int numberOfIterations;
        public int plates;
        public int worldType;
        public int worldAge;

        [Header("Environmental")]
        public float waterFactor;
        public float mountainFactor;
        public float heatFactor;

        public float poleTemp = -30;
        public float equatorTemp = 20;
        public float iceAgeChance = .025f;

        private void Start()
        {
            Singleton = this;

            size = new Vector2Int(width, height);
            numOfHexes = width * height;
            pixelW = (int)(width * (Mathf.Sqrt(3) * 8));
            pixelH = (int)(height * (16.0f * .75f));
        }
    }
}