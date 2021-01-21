using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTemp
{
    public const float EARTH_TEMP = 13.73f;

    public float NormalTemp { get; set; }
    public float AvgTemp { get; set; }

    public WorldTempChangeType changeType = WorldTempChangeType.NONE;
    public float tempChange = 0.0f;
    public float changeTempMultiplier = 1.0f;

    public float FinalTempChange => tempChange * changeTempMultiplier;

    public WorldTemp(float normalTemp)
    {
        NormalTemp = normalTemp;
    }

}

public enum WorldTempChangeType
{
    NONE,
    WARMING,
    COOLING,
    ICE_AGE,
    FAST_WARMING
}
