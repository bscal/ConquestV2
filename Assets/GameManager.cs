using Conquest;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton { get; private set; }

    public static World World { get; private set; }

    private List<Plate> m_plates;
    public List<Plate> Plates { get { return m_plates; } }



    public HexFilter currentFilter = HexFilter.NONE;

    void Start()
    {
        Singleton = this;
        World = new World();

        m_plates = new List<Plate>(WorldSettings.Singleton.plates);
    }

    public void ChangeFilter()
    {
        int id = (int)currentFilter;
        if (id == Enum.GetNames(typeof(HexFilter)).Length - 1) id = 0;
        else id++;
        currentFilter = (HexFilter)id;
    }

}
