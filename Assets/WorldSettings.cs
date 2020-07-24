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
        public int plates;
        public int worldType;
        public int worldAge;

        [Header("Environmental")]
        public float waterFactor;
        public float mountainFactor;
        public float heatFactor;

        public float poleTemp;
        public float equatorTemp;

        private void Start()
        {
            Singleton = this;

            pixelW = (int)(width * (Mathf.Sqrt(3) * 8));
            pixelH = (int)(height * (16.0f * .75f));
        }
    }
}