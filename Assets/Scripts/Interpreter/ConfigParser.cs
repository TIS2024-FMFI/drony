using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interpreter
{
    public class ConfigParser
    {
        private List<string> _configLines;
        private int _currentLine;

        public ConfigParser(List<string> configLines)
        {
            _configLines = configLines;
            _currentLine = 0;
        }
    
        private List<string>? NextLine()
        {
            if (_currentLine >= _configLines.Count)
                return null;

            var line = _configLines[_currentLine];

            while (line.Trim() == "")
            {
                _currentLine++;
                line = _configLines[_currentLine];
            }
            _currentLine++;

            var split = line.Trim().Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries).ToList();
            var commentIndex = split.FindIndex(elem => elem.Contains('#'));
            return commentIndex < 0 ? split : split.Take(commentIndex).ToList();
        }
    }
}