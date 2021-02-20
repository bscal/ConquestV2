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
    //[CreateAssetMenu(fileName = "TileMap", menuName = "Conquest/TileMap")]
    public class TileMap : MonoBehaviour
    {
        [SerializeField]
        private Sprite m_blankSprite;
        [SerializeField]
        private Tile m_defaultTile;
        [SerializeField]
        private Tile m_nullTile;

        [NonSerialized]
        private Dictionary<string, Tile> m_tilesMap;

        private void Awake()
        {
            m_tilesMap = new Dictionary<string, Tile>();
            Tile[] tiles = Resources.LoadAll<Tile>("Tiles");

            foreach (Tile tile in tiles)
            {
                RegisterTile(tile);
            }

            Debug.Log($"Registered {tiles.Length} tiles.");
        }

        public Sprite GetBlankSprite() => m_blankSprite;
        public Tile GetNullTile() => m_nullTile;
        public Tile GetDefaultTile() => m_defaultTile;

        public void RegisterTile(Tile tile)
        {
            m_tilesMap.Add(tile.name, tile);
        }

        public Tile GetTileByName(string name)
        {
            if (m_tilesMap.TryGetValue(name, out Tile tile))
                return tile;
            return null;
        }

        /*
        public void OnAfterDeserialize()
        {
            DEFAULT_TILE = tiles[0];
            m_nameToID = new Dictionary<string, int>();
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].id = i;
                m_nameToID.Add(tiles[i].name.ToUpper(), i);
            }
        }*/
    }
}

