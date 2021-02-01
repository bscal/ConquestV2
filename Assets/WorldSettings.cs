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

    [Header("Environmental")]
    public float waterFactor;
    public float mountainFactor;
    public float heatFactor;

    public float poleTemp = -30;
    public float equatorTemp = 20;
    public float iceAgeChance = .025f;
}
