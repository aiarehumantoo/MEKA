using System;
using System.Collections.Generic;
using System.Linq;

using Utilities.DeveloperConsole.Commands;

namespace Utilities.DeveloperConsole
{
    public class DeveloperConsole
    {
        private readonly string prefix;
        //private readonly IEnumerable<IConsoleCommand> commands;
        private readonly IEnumerable<ConsoleCommand> commands;

        //public DeveloperConsole(string prefix, IEnumerable<IConsoleCommand> commands)
        public DeveloperConsole(string prefix, IEnumerable<ConsoleCommand> commands)
        {
            this.prefix = prefix;
            this.commands = commands;
        }

        public void ProcessCommand(string inputValue)
        {
            if (!inputValue.StartsWith(prefix))
            {
                return;
            }

            inputValue = inputValue.Remove(0, prefix.Length);

            string[] inputSplit = inputValue.Split(' '); // Split at spaces
            string commandInput = inputSplit[0]; // Commandword
            string[] args = inputSplit.Skip(1).ToArray(); // Arguments

            ProcessCommand(commandInput, args);
        }

        public void ProcessCommand(string commandInput, string[] args)
        {
            foreach (var command in commands)
            {
                if (!commandInput.Equals(command.CommandWord, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (command.Process(args))
                {
                    return;
                }
            }
        }

    }
}