using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TemperatureEvent
{

    public float toValue;
    public float toDuration;
    public float returnValue;
    public float returnDuration;

    public bool hasEnded;

    public float Value => (hasEnded) ? 0 : (toDuration > 0) ? toValue : returnValue;

    public TemperatureEvent() {
        hasEnded = true;
    }

    public TemperatureEvent(float toValue, float toDuration, float returnValue, float returnDuration)
    {
        this.toValue = toValue / toDuration;
        this.toDuration = toDuration;
        this.returnValue = returnValue / returnDuration;
        this.returnDuration = returnDuration;
    }

    public bool UpdateDuration()
    {
        if (toDuration > 0)
            toDuration--;
        else if (returnDuration > 0)
            returnDuration--;

        return hasEnded = (toDuration < 1 && returnDuration < 1);
    }
}
