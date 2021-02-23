using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldSettings", menuName = "Conquest/World")]
public class WorldSettings : ScriptableObject
{
    public static readonly int OFFFSET_COORD = OffsetCoord.EVEN;

    [Header("World Settings")]
    public uint seed;
    public bool wrapWorld;

    [Header("Size")]
    public int width;
    public int height;

    [Header("Generation")]
    public int numberOfIterations;
    public int itersForUpdate = 5;
    public int plates;
    public int worldType;
    public int worldAge;

    [Header("World Generation")]
    public float waterFactor;
    public float mountainFactor;
    public float heatFactor;

    public float poleTemp = -30;
    public float equatorTemp = 20;
    public float iceAgeChance = .025f;

    public int seaLvl = 100;
    public int oceanLvl = 55;
    public int mountainLvl = 215;
    public int mountainPeakLvl = 235;

    [Header("Planet Settings")]
    public WorldSpinDirection spinDirection;
    public WorldSpinSpeed spinSpeed;
}

public enum WorldSpinDirection
{ 
    NONE,
    PROGRADE,
    RETROGRADE
}
public enum WorldSpinSpeed
{
    SLOW,
    NORMAL,
    FAST,
    VERY_FAST
}
