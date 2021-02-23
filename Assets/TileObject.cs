using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System;

namespace Conquest
{

    public enum HexFilter
    {
        NONE,
        PLATE,
        TEMP,
        CELL
    }

    [Serializable]
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
        public Wind wind;

        public bool IsVeryHot { get { return hexData.temp < VERY_HOT; } }
        public bool IsHot { get { return hexData.temp > HOT; } }
        public bool IsCold { get { return hexData.temp < COLD; } } 
        public bool IsVeryCold { get { return hexData.temp < VERY_COLD; } }
        public bool IsWater { get { return hexData.height <= m_world.settings.seaLvl; } }

        public SpriteRenderer render;
        public SpriteRenderer topRender;
        public SpriteRenderer overlayRenderer;

        private Sprite m_topSprite;

        private Tile m_tile;
        private Tile m_topTile;
        private bool m_covered;

        private bool[] rivers = new bool[6];

        private TileMap m_tileMap;
        private World m_world;
        private HexFilter m_filter;

        void Awake()
        {
            m_tileMap = GameManager.Singleton.TileMap;
            m_world = GameManager.Singleton.World;
            hexData = new HexData();
            wind = new Wind();
        }

        private void Update()
        {
            if (m_filter == HexFilter.PLATE)
            {
                render.color = GameManager.Singleton.World.GetPlateByID(hexData.plateId).color;
                topRender.color = GameManager.Singleton.World.GetPlateByID(hexData.plateId).color;
            }

            if (m_covered)
                topRender.sprite = m_tileMap.GetBlankSprite();
            else if (topRender.sprite != m_topSprite)
                topRender.sprite = m_topSprite;
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
                m_topSprite = null;
            else
                m_topSprite = FindSpriteForClimate(tile);
        }

        public void SetBotTile(Tile tile)
        {
            m_tile = tile;

            render.sprite = FindSpriteForClimate(tile);
        }

        public void SetFilter(HexFilter filter)
        {
            m_filter = filter;

            SetBlankFill(false);
            SetColor(Color.white);
            overlayRenderer.sprite = null;
            overlayRenderer.transform.localScale = new Vector3(1, 1);
            overlayRenderer.transform.rotation = Quaternion.identity;
            topRender.sprite = m_topSprite;

            if (filter == HexFilter.PLATE)
            {
                render.color = GameManager.Singleton.World.GetPlateByID(hexData.plateId).color;
                topRender.color = GameManager.Singleton.World.GetPlateByID(hexData.plateId).color;
            }
            else if (filter == HexFilter.TEMP)
            {
                float tempScale = (hexData.temp - VERY_COLD) / (VERY_HOT - VERY_COLD);
                Color hot = new Color(1, 0, 0, 1);
                Color cold = new Color(0, 0, 1, 1);
                Color c = Color.Lerp(cold, hot, tempScale);

                render.color = c;
                topRender.color = c;
            }
            else if (filter == HexFilter.CELL)
            {
                topRender.color = new Color(255f / ((hexData.cellid+1)*2) / 255f, 255f / (hexData.cellid + 1) / 255f, 0);
                SetBlankFill(true);

                if (hexData.cellid == 0)
                    overlayRenderer.sprite = GameManager.Singleton.SpriteManager.neutral;
                else
                {
                    overlayRenderer.sprite = GameManager.Singleton.SpriteManager.arrow;
                    overlayRenderer.transform.rotation = GameManager.Singleton.World.windManager.GetRotationFromWind(wind.direction);
                }
                overlayRenderer.transform.localScale = new Vector3(.25f, .25f);

            }
        }

        public string TileInfo()
        {
            return string.Format("top: {0} | bot: {1}", (m_topTile != null) ? m_topTile.name : "NULL", (m_tile != null) ? m_tile.name : "NULL");
        }

        public Tile FindCorrectBaseTile()
        {
            return m_tileMap.GetTileByName("Grassland");
        }

        public Tile FindCorrectTile()
        {
            float h = hexData.height;

            if (h > m_world.settings.mountainPeakLvl)
                return m_tileMap.GetTileByName("Snowy_Mountains");
            else if (h > m_world.settings.mountainLvl)
                return m_tileMap.GetTileByName("Mountains");
            else
            {
                if (h > m_tileMap.GetTileByName("Hills").minHeight)
                    return (m_tileMap.GetTileByName("Hills"));
                else if (hexData.height > m_tileMap.GetTileByName("Grassland").minHeight)
                    return m_tileMap.GetTileByName("Grassland");
                else if (hexData.height > m_tileMap.GetTileByName("Coast").minHeight)
                    return m_tileMap.GetTileByName("Coast");
                else if (hexData.height > m_tileMap.GetTileByName("Ocean").minHeight)
                    return m_tileMap.GetTileByName("Ocean");
                else
                    return FindCorrectBaseTile();
            }
        }

        private Sprite FindSpriteForClimate(Tile tile)
        {
            if (IsVeryCold && tile.veryColdSprite != null)
                return tile.veryColdSprite;
            if (IsCold && tile.coldSprite != null)
                return tile.coldSprite;
            if (IsHot && tile.hotSprite != null)
                return tile.hotSprite;
            return tile.sprite;
        }

        /**
         *  HexData Functions
         */

        protected bool Range(float val, float v1, float v2)
        {
            return val > v1 && val < v2;
        }

        public bool HeightRange(float v1, float v2)
        {
            return Range(hexData.height, v1, v2);
        }

        public bool TempRange(float v1, float v2)
        {
            return Range(hexData.temp, v1, v2);
        }

        public void SetBlankFill(bool fill)
        {
            m_covered = fill;
        }

        public void SetColor(Color color)
        {
            render.color = color;
            topRender.color = color;
        }

        public void SetRiver(int direction, bool hasRiver)
        {
            rivers[direction] = hasRiver;
        }

        public string RiversToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < rivers.Length; i++)
            {
                if (!rivers[i])
                    continue;
                sb.Append(i);
                sb.Append(", ");
            }
            return sb.ToString();
        }

    }
}
