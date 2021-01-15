using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Conquest
{

    public enum HexFilter
    {
        NONE,
        PLATE
    }

    public class TileObject : MonoBehaviour
    {

        public Hex hex;
        public HexData hexData;

        public SpriteRenderer render;
        public SpriteRenderer topRender;

        private Tile m_tile;
        private Tile m_topTile;

        private HexFilter m_filter;

        void Awake()
        {
            hexData = new HexData();
        }

        private void Update()
        {
            if (m_filter == HexFilter.PLATE)
            {
                render.color = GameManager.Singleton.World.GetPlateByID(hexData.plateId).color;
                topRender.color = GameManager.Singleton.World.GetPlateByID(hexData.plateId).color;
            }
        }

        public Tile GetBotTile() => m_tile;

        public Tile GetTopTile() => m_topTile;

        public void SetTile(Tile tile)
        {
            if (tile.isSpriteInFront)
            {
                SetTopTile(tile);
                SetBotTile(FindCorrectBaseTile());
            }
            else
            {
                SetTopTile(null);
                SetBotTile(tile);
            }
        }

        public void SetTopTile(Tile tile)
        {
            m_topTile = tile;

            // TODO have a Tile the represents a NULL tile. 
            if (tile == null)
                topRender.sprite = null;
            else
                topRender.sprite = tile.sprite;
        }

        public void SetBotTile(Tile tile)
        {
            m_tile = tile;
            render.sprite = tile.sprite;
        }

        public void SetFilter(HexFilter filter)
        {
            m_filter = filter;
            if (filter == HexFilter.NONE)
            {
                render.color = Color.white;
                topRender.color = Color.white;
            }
            else if (filter == HexFilter.PLATE)
            {
                render.color = GameManager.Singleton.World.GetPlateByID(hexData.plateId).color;
                topRender.color = GameManager.Singleton.World.GetPlateByID(hexData.plateId).color;
            }
        }

        public string TileInfo()
        {
            return string.Format("top: {0} | bot: {1}", (m_topTile != null) ? m_topTile.name : "NULL", (m_tile != null) ? m_tile.name : "NULL");
        }

        public Tile FindCorrectBaseTile()
        {
            return TileMap.Singleton.GetTileByName("grassland");
        }

        public Tile FindCorrectTile()
        {
            // new TODO Figure a way to do temp and wetness... Possibly
            // loop through all tiles and compare or compare in the return statement?
            
            TileMap map = TileMap.Singleton;
            float h = hexData.height;


            if (h > map.mountainPeaklvl)
                return map.GetTileByName("mountain_peak");
            else if (h > map.mountainLvl)
                return map.GetTileByName("mountain");
            else
            {
                if (h < map.GetTileByName("ocean").minHeight)
                    return map.GetTileByName("ocean");
                else if (hexData.height < map.GetTileByName("coast").minHeight)
                    return map.GetTileByName("coast");
                else if (hexData.height < map.GetTileByName("grassland").minHeight)
                    return map.GetTileByName("grassland");
                else if (hexData.height < map.GetTileByName("hill").minHeight)
                    return map.GetTileByName("hill");
                else
                    return FindCorrectBaseTile();
            }



        }
    }
}
