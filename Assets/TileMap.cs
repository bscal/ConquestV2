using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conquest
{
    /// <summary>
    /// ScriptableObject. An array of Tile objects. Use to get Tiles by index or name.
    /// TileMap contains an internal Dictionary pairing Tile names to index.
    /// </summary>
    [CreateAssetMenu(fileName = "TileMap", menuName = "Conquest/TileMap", order = 0)]
    public class TileMap : ScriptableObject, ISerializationCallbackReceiver
    {
        public static TileMap Singleton { get; private set; }

        public static readonly Tile NULL_TILE = new Tile() {

        };

        [NonSerialized]
        private Dictionary<string, int> m_nameToID;

        public Tile[] tiles;

        public TileMap()
        {
            Singleton = this;
        }

        public Tile GetTile(int index)
        {
            return tiles[index];
        }

        public Tile GetTileByName(string name)
        {
            if (m_nameToID.TryGetValue(name.ToUpper(), out int value))
            {
                return tiles[value];
            }
            return null;
        }

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
            m_nameToID = new Dictionary<string, int>();
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].id = i;
                m_nameToID.Add(tiles[i].name.ToUpper(), i);
            }
        }
    }
}

