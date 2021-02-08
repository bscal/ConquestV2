using Conquest;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class BreathFirstSearch
{

    private Hex m_start;
    private Hex m_goal;
    private Queue<Hex> m_frontier;
    private Dictionary<Hex, TileObject> m_cameFrom;

    private Dictionary<Hex, TileObject> m_hexes;

    public BreathFirstSearch(Hex start, Hex goal, Dictionary<Hex, TileObject> hexes)
    {
        m_start = start;
        m_goal = goal;
        m_hexes = hexes;

        m_frontier = new Queue<Hex>();
        m_frontier.Enqueue(start);

        m_cameFrom = new Dictionary<Hex, TileObject>();
        m_cameFrom.Add(start, m_hexes[start]);
    }

    public void Search()
    {
        while (m_frontier.Count > 0)
        {
            Hex current = m_frontier.Dequeue();

            if (current.Equals(m_goal))
                break;

            foreach (Hex next in current.Neightbors())
            {
                if (!m_cameFrom.ContainsKey(next) && IsHexValid(next))
                {
                    m_frontier.Enqueue(next);
                    m_cameFrom.Add(next, m_hexes[current]);
                }
            }
        }
    }

    public Queue<Hex> Frontier => m_frontier;
    public Dictionary<Hex, TileObject> CameFrom => m_cameFrom;

    private bool IsHexValid(Hex hex)
    {
        bool contains = m_hexes.TryGetValue(hex, out TileObject obj);

        return contains && obj.hexData.isPassible;
    }

}
