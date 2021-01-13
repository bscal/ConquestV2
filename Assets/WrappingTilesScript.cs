using Conquest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrappingTilesScript : MonoBehaviour
{

    [SerializeField]
    private TileObject obj;

    private bool visible = true;

    private void LateUpdate()
    {
        if (Camera.main == null && !visible) return;
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

    private void OnBecameVisible()
    {
        visible = true;
    }

    private void OnBecameInvisible()
    {
        visible = false;
    }
}
