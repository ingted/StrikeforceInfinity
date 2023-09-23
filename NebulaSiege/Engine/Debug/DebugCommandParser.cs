﻿using System;
using System.Collections.Generic;
using System.Linq;
using static NebulaSiege.Engine.Debug.DebugCommandParameterPrototype;

namespace NebulaSiege.Engine.Debug
{
    internal class DebugCommandParser
    {
        public List<DebugCommandPrototype> Commands { get; private set; } = new();

        public DebugCommandParser(string[] prototypes)
        {
            foreach (var prototype in prototypes)
            {
                var commandParts = prototype.Split('|');

                if (commandParts.Length != 2)
                {
                    throw new Exception("Malformed debug command prototype.");
                }

                var command = new DebugCommandPrototype(commandParts[0]);

                var commandParameters = commandParts[1].Split(",");

                foreach (var commandParameter in commandParameters)
                {
                    if (string.IsNullOrEmpty(commandParameter) == false)
                    {
                        var commandParameterParts = commandParameter.Split(":");

                        if (commandParameterParts.Length != 3)
                        {
                            throw new Exception("Malformed debug command prototype parameter.");
                        }

                        command.Parameters.Add(new DebugCommandParameterPrototype(
                            commandParameterParts[0],
                            (commandParameterParts[1].Trim().ToLower() != "optional"),
                            Enum.Parse<DebugCommandParameterType>(commandParameterParts[2], true)
                        ));
                    }
                }

                Commands.Add(command);
            }
        }

        public DebugCommand Parse(string commandText)
        {
            int paramStartIndex = commandText.IndexOf(' ');

            string commandName;

            if (paramStartIndex > 0)
            {
                commandName = commandText.Substring(0, paramStartIndex).Trim(); //We have parameters
                commandText = commandText.Substring(paramStartIndex).Trim();
            }
            else
            {
                commandName = commandText.Trim(); //We have no parameters.
                commandText = string.Empty;
            }

            var commandPrototype = Commands.Where(o => o.Name.ToLower() == commandName.ToLower()).FirstOrDefault();
            if (commandPrototype == null)
            {
                throw new Exception($"Unknown command '{commandName}'.");
            }

            var commandParameters = commandText.Split(',', StringSplitOptions.RemoveEmptyEntries);

            //If the supplied parameter count is more than we expect.
            if (commandParameters.Count() > commandPrototype.Parameters.Count)
            {
                throw new Exception($"Too many parameters supplied to '{commandName}'.");
            }

            var parsedCommand = new DebugCommand(commandName);

            int paramIndex = 0;

            //Loop though the supplied parameters:
            for (; paramIndex < commandParameters.Length; paramIndex++)
            {
                var paramPrototype = commandPrototype.Parameters[paramIndex];
                parsedCommand.Parameters.Add(new DebugCommandParameter(paramPrototype, commandParameters[paramIndex]));
            }

            //Loop through the not-supplied parameters:
            for (; paramIndex < commandPrototype.Parameters.Count; paramIndex++)
            {
                var paramPrototype = commandPrototype.Parameters[paramIndex];
                if (paramPrototype.IsRequired)
                {
                    throw new Exception($"Command '{parsedCommand.Name}' parameter '{paramPrototype.Name}' is not optional.");
                }

                parsedCommand.Parameters.Add(new DebugCommandParameter(paramPrototype, null));
            }

            return parsedCommand;
        }
    }
}
