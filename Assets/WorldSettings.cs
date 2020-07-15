using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WorldSettings : MonoBehaviour
{

    public static WorldSettings Singleton { get; private set; }

    public int plates;

    public int worldType;
    public int worldAge;

    public float waterFactor;
    public float mountainFactor;
    public float heatFactor;

    public float poleTemp;
    public float equatorTemp;

    private void Start()
    {
        Singleton = this;
    }
}