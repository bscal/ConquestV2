using Conquest;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTemp
{
    public const float EARTH_TEMP = 13.73f;

    public float NormalTemp { get; set; }
    public float AvgTemp { get; set; }

    public float FinalTempChange => (tempChange + tempToChangeValue) * changeTempMultiplier;
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

    public void StartTemperatureEvent(WorldTempChangeType type, float duration, float totalValue, float cooldown)
    {
        changeType = type;
        tempToChangeValue = -totalValue / duration;
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

    internal void StartIceAge(float duration, float totalValue, float cooldown)
    {
        if (!evtOnCd)
            StartTemperatureEvent(WorldTempChangeType.ICE_AGE, duration, totalValue, cooldown);
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
