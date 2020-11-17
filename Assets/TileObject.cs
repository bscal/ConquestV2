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

        //         public int plateId = 0;
        //         public int tileId = 0;
        // 
        //         public int age = 0;
        //         public float height = 0;
        //         public float temp = 0;
        //         public float wetness = 0;
        // 
        //         public bool isPlateEdge = false;
        //         public bool collision = false;
        //         public bool empty = false;
        //         public bool moved = false;
        //         public bool generated = false;
        //         public bool formingMoutain = false;
        //         public bool isOcean = false;
        //         public bool isCoast = false;

        void Awake()
        {
            hexData = new HexData();
        }

        private void Update()
        {
            if (m_filter == HexFilter.PLATE)
            {
                render.color = GameManager.Singleton.World.plates[hexData.plateId].color;
                topRender.color = GameManager.Singleton.World.plates[hexData.plateId].color;
            }
        }

        private void OnBecameVisible()
        {
            Point op = GameManager.Singleton.World.layout.HexToPixel(hex);
            Vector3 opos = new Vector3((float)op.x, (float)op.y);
            if (Camera.main != null && new Rect(0, 0, 1, 1).Contains(Camera.main.WorldToViewportPoint(opos)))
            {
                var curPos = GameManager.Singleton.World.layout.HexToPixel(hex);
                transform.position = new Vector3((float)curPos.x, (float)curPos.y);
                return;
            }
        }

        private void OnBecameInvisible()
        {
            if (Camera.main == null) return;
            Point op = GameManager.Singleton.World.layout.HexToPixel(hex);
            Vector3 opos = new Vector3((float)op.x, (float)op.y);
            if (new Rect(0, 0, 1, 1).Contains(Camera.main.WorldToViewportPoint(opos)))
            {
                transform.position = new Vector3((float)opos.x, (float)opos.y);
                return;
            }

            OffsetCoord coord = OffsetCoord.RoffsetFromCube(OffsetCoord.EVEN, hex);
            int newQ;
            if (coord.col > GameManager.Singleton.World.size.x / 2)
                newQ = hex.q - GameManager.Singleton.World.size.x - 1;
            else
                newQ = hex.q + GameManager.Singleton.World.size.x + 1;
            Hex h = new Hex(newQ, hex.r, -newQ - hex.r);
            Point p = GameManager.Singleton.World.layout.HexToPixel(h);
            Vector3 pos = new Vector3((float)p.x, (float)p.y);
            if (new Rect(0, 0, 1, 1).Contains(Camera.main.WorldToViewportPoint(pos)))
            {
                transform.position = pos;
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
                render.color = GameManager.Singleton.World.plates[hexData.plateId].color;
                topRender.color = GameManager.Singleton.World.plates[hexData.plateId].color;
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
            if (hexData.height < 50)
                return TileMap.Singleton.GetTileByName("ocean");
            else if (hexData.height < 100)
                return TileMap.Singleton.GetTileByName("coast");
            else if (hexData.height < 175)
                return TileMap.Singleton.GetTileByName("grassland");
            else if (hexData.height < 215)
                return TileMap.Singleton.GetTileByName("hill");
            else if (hexData.height >= 215)
                return TileMap.Singleton.GetTileByName("mountain");
            else
                return FindCorrectBaseTile();
        }
    }
}
