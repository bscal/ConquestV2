﻿using System.Collections;
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

        // Temperature of tile. #s loosly based in degrees celsius
        public const float VERY_HOT = 18;
        public const float HOT = 10;
        public const float BASE_TEMP = 0f;
        public const float COLD = -10f;
        public const float VERY_COLD = -20f;

        // Overall wetness of a tile. Mostly precipitation on the tile and not bodies of water
        public const float VERY_DRY = 96;
        public const float DRY = 32;
        public const float BASE_WETNESS = 0f;
        public const float WET = -32f;
        public const float VERY_WET = -96f;

        public Hex hex;
        public HexData hexData;

        public bool IsVeryHot { get { return hexData.temp < VERY_HOT; } }
        public bool IsHot { get { return hexData.temp > HOT; } }
        public bool IsCold { get { return hexData.temp < COLD; } } 
        public bool IsVeryCold { get { return hexData.temp < VERY_COLD; } }
        

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
                topRender.sprite = FindSpriteForClimate(tile);
        }

        public void SetBotTile(Tile tile)
        {
            m_tile = tile;

            render.sprite = FindSpriteForClimate(tile);
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
            TileMap map = TileMap.Singleton;
            float h = hexData.height;

            if (h > map.mountainPeaklvl)
                return map.GetTileByName("mountain_peak");
            else if (h > map.mountainLvl)
                return map.GetTileByName("mountain");
            else
            {
                if (h > map.GetTileByName("hill").minHeight)
                    return (map.GetTileByName("hill"));
                else if (hexData.height > map.GetTileByName("grassland").minHeight)
                    return map.GetTileByName("grassland");
                else if (hexData.height > map.GetTileByName("coast").minHeight)
                    return map.GetTileByName("coast");
                else if (hexData.height > map.GetTileByName("ocean").minHeight)
                    return map.GetTileByName("ocean");
                else
                    return FindCorrectBaseTile();
            }
        }

        private Sprite FindSpriteForClimate(Tile tile)
        {
            if (IsCold && tile.coldSprite != null)
                return tile.coldSprite;
            else if (IsHot && tile.hotSprite != null)
                return tile.hotSprite;
            else if (IsVeryCold && tile.veryColdSprite != null)
                return tile.veryColdSprite;
            else
                return tile.sprite;
        }
    }
}
