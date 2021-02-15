using Conquest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCommands : MonoBehaviour
{

    private void Start()
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

            GameObject riverPath = Resources.Load("Prefabs/River/River") as GameObject;
            GameObject riverPart = Resources.Load("Prefabs/River/RiverPart") as GameObject;

            GameObject newRiver = Instantiate(riverPath, this.transform);
            RiverSprite river = newRiver.GetComponent<RiverSprite>();
            river.Init(arg, riverPart);
            river.GenerateRiverPath(GameManager.Singleton.World);

        }, 1);
        DebugController.Singleton.AddCommand(createRiver);
    }

}
