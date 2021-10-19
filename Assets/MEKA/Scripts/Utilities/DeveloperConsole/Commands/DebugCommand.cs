using UnityEngine;
using System;

using Utilities.DebugUI;

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

                Debugger.ToggleDebugger();
                return true;
            }
            return true;
        }
    }
}
