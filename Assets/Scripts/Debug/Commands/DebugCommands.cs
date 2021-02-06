using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCommands
{

    public DebugCommands()
    {

        DebugCommand testBreathFirstSearch = new DebugArgsCommand("testbfs", "Tests BreathFirstSearch pathfinder", "testbfs", args => {
            Debug.Log(Hex.ParseHex(args[0]) + " -> " + Hex.ParseHex(args[1]));
        }, 2);
        DebugController.Singleton.AddCommand(testBreathFirstSearch);

    }

}
