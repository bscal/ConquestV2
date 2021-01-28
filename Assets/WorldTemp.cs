using Conquest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTemp
{
    public const float EARTH_TEMP = 13.73f;

    public float NormalTemp { get; set; }
    public float AvgTemp { get; set; }

    public float FinalTempChange => tempChange * changeTempMultiplier;
    public bool IsEventHappening => changeType != WorldTempChangeType.NONE;

    public WorldTempChangeType changeType = WorldTempChangeType.NONE;
    public float tempChange;
    public float changeTempMultiplier = 1.0f;

    public float tempToChangeValue;
    public float tempToChangeDuration;
    private float evtCdDuration;
    private bool evtOnCd;

    private readonly MapGenerator m_gen;

    public WorldTemp(float normalTemp, MapGenerator gen)
    {
        NormalTemp = normalTemp;
        m_gen = gen;
        gen.IterationEvent += OnIteration;
    }

    public void StartTemperatureEvent(float duration, float totalValue, float cooldown)
    {
        tempToChangeValue = totalValue / duration;
        tempToChangeDuration = duration;
        evtCdDuration = m_gen.GetCurrentIteration() + cooldown;
        evtOnCd = true;
    }

    public void StopEvent()
    {
        evtOnCd = false;
        changeType = WorldTempChangeType.NONE;
    }

    private void OnIteration(int iter, World world)
    {
        if (evtOnCd && evtCdDuration >= iter)
            evtOnCd = false;
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
