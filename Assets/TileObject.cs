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

        public GameObject gameobject;
        public SpriteRenderer render;
        public Hex hex;

        public int plateId = 0;
        public int tileId = 0;

        public float height = 0;
        public float temp = 0;
        public float wetness = 0;

        public bool isPlateEdge = false;
        public bool collision = false;
        public bool empty = false;

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
        public void SetFilter(HexFilter filter)
        {

            if (filter == HexFilter.NONE)
            {
                render.color = Color.white;
            }
            else if (filter == HexFilter.PLATE)
            {
                render.color = GameManager.Singleton.World.plates[plateId].color;
            }
        }
    }
}
