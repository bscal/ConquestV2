using Conquest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrappingTilesScript : MonoBehaviour
{

    [SerializeField]
    private TileObject obj;

    private bool visible;
    private bool invisible;

    private void LateUpdate()
    {
        if (visible)
        {
            Point op = GameManager.Singleton.World.layout.HexToPixel(obj.hex);
            Vector3 opos = new Vector3((float)op.x, (float)op.y);
            if (Camera.main != null && new Rect(0, 0, 1, 1).Contains(Camera.main.WorldToViewportPoint(opos)))
            {
                var curPos = GameManager.Singleton.World.layout.HexToPixel(obj.hex);
                obj.transform.position = new Vector3((float)curPos.x, (float)curPos.y);
                visible = false;
            }
        }

        else if (invisible)
        {
            if (Camera.main == null) return;
            Point op = GameManager.Singleton.World.layout.HexToPixel(obj.hex);
            Vector3 opos = new Vector3((float)op.x, (float)op.y);
            if (new Rect(0, 0, 1, 1).Contains(Camera.main.WorldToViewportPoint(opos)))
            {
                obj.transform.position = new Vector3((float)opos.x, (float)opos.y);
                return;
            }

            //OffsetCoord coord = OffsetCoord.RoffsetFromCube(OffsetCoord.EVEN, obj.hex);
            Vector3 center = new Vector3(Screen.width / 2, Screen.height / 2);

            int newQ;
            if (center.x > Camera.main.transform.position.x)
                newQ = obj.hex.q - GameManager.Singleton.World.size.x - 1;
            else
                newQ = obj.hex.q + GameManager.Singleton.World.size.x + 1;
            Hex h = new Hex(newQ, obj.hex.r, -newQ - obj.hex.r);
            Point p = GameManager.Singleton.World.layout.HexToPixel(h);
            Vector3 pos = new Vector3((float)p.x, (float)p.y);
            if (new Rect(0, 0, 1, 1).Contains(Camera.main.WorldToViewportPoint(pos)))
            {
                obj.transform.position = pos;
            }
        }
    }

    private void OnBecameVisible()
    {
        visible = true;
    }

    private void OnBecameInvisible()
    {
        invisible = true;
    }
}
