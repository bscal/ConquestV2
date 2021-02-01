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

    public float FinalTempChange => (tempChange + tempEvent.Value) * changeTempMultiplier;
    public bool IsEventHappening => changeType != WorldTempChangeType.NONE;

    public WorldTempChangeType changeType = WorldTempChangeType.NONE;
    public float tempChange;
    public float changeTempMultiplier = 1.0f;

    public TemperatureEvent tempEvent;
    public int cdDuration;
    public bool onCd;

    private readonly MapGenerator m_gen;

    public WorldTemp(float normalTemp, MapGenerator gen)
    {
        NormalTemp = normalTemp;
        tempEvent = new TemperatureEvent();
        m_gen = gen;
        gen.IterationEvent += OnIteration;
    }

    public void StartTemperatureEvent(WorldTempChangeType type, TemperatureEvent evt, int cooldown)
    {
        changeType = type;
        tempEvent = evt;

        if (cooldown > 0)
        {
            cdDuration = cooldown;
            onCd = true;
        }

        Debug.Log("starting");
    }

    public void StopEvent()
    {
        changeType = WorldTempChangeType.NONE;
        Debug.Log("ending");
    }

    private void OnIteration(int iter, World world)
    {
        if (iter % 1 == 0)
        {
            if (!tempEvent.hasEnded && tempEvent.UpdateDuration())
                StopEvent();

            if (onCd && cdDuration-- < 1)
                onCd = false;
        }
    }

    internal void StartIceAge(TemperatureEvent evt, int cooldown)
    {
        if (!onCd)
            StartTemperatureEvent(WorldTempChangeType.ICE_AGE, evt, cooldown);
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
