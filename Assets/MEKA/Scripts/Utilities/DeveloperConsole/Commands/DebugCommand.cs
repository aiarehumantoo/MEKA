using UnityEngine;
using System;

namespace Utilities.DeveloperConsole.Commands
{
    [CreateAssetMenu(fileName = "New Debug Command", menuName = "Utilities/DeveloperConsole/Commands/Debug Command")]
    public class DebugCommand : ConsoleCommand
    {
        public override bool Process(string[] args)
        {
            if(args.Length != 1)
            {
                return false;
            }

            if (args[0].Equals("toggleui", StringComparison.OrdinalIgnoreCase)) {

                // TEST
                //var go = GameObject.FindGameObjectWithTag("Player");
                //go.GetComponent<Debugger>().ToggleUI();

                //FindObjectOfType<DebugGUI>().ToggleDebuiUI();
                Debugger.ToggleDebugger();
                //DebugGUI.ToggleDebuiUI();
                return true;
            }
            return true;
        }
    }
}
