using UnityEngine;
using System;

namespace Utilities.DeveloperConsole.Commands
{
    [CreateAssetMenu(fileName = "New Settings Command", menuName = "Utilities/DeveloperConsole/Commands/Settings Command")]
    public class SettingsCommand : ConsoleCommand
    {
        public override bool Process(string[] args)
        {
            if (args[0].Equals("mouse", StringComparison.OrdinalIgnoreCase))
            {
                if (args[1].Equals("sensitivity", StringComparison.OrdinalIgnoreCase))
                {
                    if (!float.TryParse(args[2], out float sens))
                    {
                        return false;
                    }
                    GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().SetSensitivity(sens);
                }
            }
            return true;
        }
    }
}
