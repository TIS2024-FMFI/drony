using System;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter
{
    public class CommandFileSplitter
    {
        private const string ConfigSectionHeader = ".config:";
        private const string CommandSectionHeader = ".command:";
        private readonly List<string> _configSection;
        private readonly List<string> _commandSection;
        
        public CommandFileSplitter(string fileContent)
        {
            var fileLines = fileContent.Split("\n")
                .Select(it => it.Trim())
                .Where(it => it.Length != 0)
                .ToList();
            
            var configHeaderIndex = fileLines.IndexOf(ConfigSectionHeader);
            if (configHeaderIndex == -1)
            {
                throw new Exception($"Invalid file format: config header '{ConfigSectionHeader}' was not found in file.");
            }
                
            var commandHeaderIndex = fileLines.IndexOf(CommandSectionHeader);
            if (commandHeaderIndex == -1)
            {
                throw new Exception($"Invalid file format: command header '{CommandSectionHeader}' was not found in file.");
            }
            if (configHeaderIndex > commandHeaderIndex)
            {
                throw new Exception($"Invalid file format: config definition expected before command definition");
            }
            
            _configSection = fileLines.Skip(1).Take(commandHeaderIndex).ToList();
            _commandSection = fileLines.Skip(commandHeaderIndex + 1).ToList();
        }

        public List<string> GetConfigSection() => _configSection;
        
        public List<string> GetCommandSection() => _commandSection;
    }
}