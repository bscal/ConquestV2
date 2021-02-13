using Conquest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCommands
{

    public DebugCommands()
    {

        DebugCommand testBreathFirstSearch = new DebugArgsCommand("testbfs", "Tests BreathFirstSearch pathfinder", "testbfs", args => {
            Debug.Log(Hex.ParseHex(args[0]) + " -> " + Hex.ParseHex(args[1]));

            BreathFirstSearch search = new BreathFirstSearch(Hex.ParseHex(args[0]), Hex.ParseHex(args[1]), GameManager.Singleton.World.tileData);
            search.Search();

            foreach (var pair in search.CameFrom)
            {
                pair.Value.SetBlankFill(true);
                pair.Value.SetColor(Color.red);
            }

        }, 2);
        DebugController.Singleton.AddCommand(testBreathFirstSearch);

        DebugCommand reset = new DebugCommand("reset", "reset all hexes", "reset", args => {

            foreach (var pair in GameManager.Singleton.World.tileData)
            {
                pair.Value.SetBlankFill(false);
                pair.Value.SetTile(pair.Value.FindCorrectTile());
            }

        });
        DebugController.Singleton.AddCommand(reset);


        DebugCommand createRiver = new DebugArgsCommand("create_river", "creates a river at hex", "create_river <hex>", args => {
            Hex arg = Hex.ParseHex(args[0]);

            River r = new River(arg);
            var list = r.GeneratePath(GameManager.Singleton.World);

            LineRenderer river = GameManager.Singleton.GetDebugger().CreateLineRender();
            river.enabled = true;
            river.positionCount = list.riverPath.Count;
            river.startWidth = 2;
            river.endWidth = 2;


            for (int i = 0; i < list.tiles.Count; i++)
            {
                TileObject obj = list.tiles[i];
                obj.SetColor(Color.blue);
            }

            World world = GameManager.Singleton.World;
            int counter = 0;
            foreach (RiverPath line in list.riverPath)
            {
                Point p = world.layout.HexCornerOffset(line.corner);
                Point c = world.layout.HexToPixel(line.from.hex);
                river.SetPosition(counter++, new Vector3((float)p.x + (float)c.x, (float)p.y + (float)c.y));
            }

        }, 1);
        DebugController.Singleton.AddCommand(createRiver);
    }

}
