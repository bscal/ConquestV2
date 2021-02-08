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

    }

}
