using Conquest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverSprite : MonoBehaviour
{
    [SerializeField]
    private GameObject m_riverPartPrefab;

    private River m_river;
    private RiverContainer m_generation;
    private List<SpriteRenderer> m_parts = new List<SpriteRenderer>();
    private List<TileObject> m_partsTileObjects = new List<TileObject>();

    public void Init(Hex start)
    {
        m_river = new River(start);
    }

    public void Init(Hex start, GameObject riverPartPrefab)
    {
        m_river = new River(start);
        m_riverPartPrefab = riverPartPrefab;
    }

    public void GenerateRiverPath(World world)
    {
        m_parts.Clear();
        m_generation = m_river.GeneratePath(world);
        CreateRiverSprites(world);
    }

    private void CreateRiverSprites(World world)
    {
        List<Line> paths = new List<Line>();

        for (int i = 0; i < m_generation.riverPath.Count; i++)
        {
            // The last point goes nowhere and in included in Count - 2
            if (i == m_generation.riverPath.Count - 1)
                break;

            RiverPath start = m_generation.riverPath[i];
            Point cs = world.layout.HexCornerOffset(start.corner);
            Point hs = world.layout.HexToPixel(start.from.hex);
            Point ptStart = new Point(cs.x + hs.x, cs.y + hs.y);

            RiverPath end = m_generation.riverPath[i + 1];
            Point ce = world.layout.HexCornerOffset(end.corner);
            Point he = world.layout.HexToPixel(end.from.hex);
            Point ptEnd = new Point(ce.x + he.x, ce.y + he.y);

            Line line = new Line(ptStart, ptEnd);
            paths.Add(line);

            // Sets sides of TileObject that contain rivers
            end.from.SetRiver(end.corner, true);
        }

        for (int i = 0; i < paths.Count; i++)
        {
            // Increases river's width
            m_river.riverWidth++;

            Line path = paths[i];
            //RiverPath riverPart = m_generation.riverPath[i];

            Vector3 posStart = new Vector3((float)path.pt0.x, (float)path.pt0.y);
            Vector3 posEnd = new Vector3((float)path.pt1.x, (float)path.pt1.y);
            posStart.z = posEnd.z; // ensure there is no 3D rotation by aligning Z position

            // vector from this object towards the target location
            Vector3 vectorToTarget = posStart - posEnd;
            // rotate that vector by 90 degrees around the Z axis
            Vector3 rotatedVectorToTarget = Quaternion.Euler(0, 0, 90) * vectorToTarget;

            // get the rotation that points the Z axis forward, and the Y axis 90 degrees away from the target
            // (resulting in the X axis facing the target)
            Quaternion targetRotation = Quaternion.LookRotation(forward: Vector3.forward, upwards: rotatedVectorToTarget);


            GameObject part = Instantiate(m_riverPartPrefab, posStart, targetRotation, this.transform);
            SpriteRenderer renderer = part.GetComponent<SpriteRenderer>();
            m_parts.Add(renderer);

            if (i == 0) // Is start
                HandleRiverStart(part, renderer);

            else if (i == paths.Count - 1) // Is end
                HandleRiverEnd(part, renderer);

            else
                HandleRiverPath(part, renderer);

        }
    }

    private void HandleRiverStart(GameObject part, SpriteRenderer renderer)
    {
    }

    private void HandleRiverEnd(GameObject part, SpriteRenderer renderer)
    {
       

        if (m_river.reachedWater)
        {
        }
        else
        {
        }


    }

    private void HandleRiverPath(GameObject part, SpriteRenderer renderer)
    {
    }
}
