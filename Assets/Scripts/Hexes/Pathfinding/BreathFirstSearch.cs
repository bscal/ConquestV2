using Conquest;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class BreathFirstSearch
{

    private Queue<Hex> m_frontier;
    private Dictionary<Hex, Hex> m_cameFrom;

    private Dictionary<Hex, TileObject> m_hexes;

    public BreathFirstSearch(Hex start, Dictionary<Hex, TileObject> hexes)
    {
        m_hexes = hexes;

        m_frontier = new Queue<Hex>();
        m_frontier.Enqueue(start);

        m_cameFrom = new Dictionary<Hex, Hex>();
        m_cameFrom.Add(start, Hex.NULL_HEX);
    }

    public void Search()
    {
        while (m_frontier.Count > 0)
        {
            Hex current = m_frontier.Dequeue();
            foreach (Hex next in current.Neightbors())
            {
                if (!m_cameFrom.ContainsKey(next) && IsHexValid(next))
                {
                    m_frontier.Enqueue(next);
                    m_cameFrom.Add(next, current);
                }
            }
        }
    }

    public Queue<Hex> Frontier => m_frontier;
    public Dictionary<Hex, Hex> CameFrom => m_cameFrom;

    private bool IsHexValid(Hex hex)
    {
        TileObject obj = m_hexes[hex];

        if (obj == null && obj.hexData.isPassible) return false;

        return true;
    }

}
