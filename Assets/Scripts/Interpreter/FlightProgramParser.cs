using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interpreter
{
    public class FlightProgramParser
    {
        private readonly List<string> _programLines;
        private int _currentLine;

        public FlightProgramParser(IEnumerable<string> lines)
        {
            _programLines = lines.ToList();
            _currentLine = 0;
        }

        private List<string>? NextLine()
        {
            if (_currentLine >= _programLines.Count)
                return null;

            var line = _programLines[_currentLine];

            while (line.Trim() == "")
            {
                _currentLine++;
                line = _programLines[_currentLine];
            }
            _currentLine++;

            var split = line.Trim().Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries).ToList();
            var commentIndex = split.FindIndex(elem => elem.Contains('#'));
            if (commentIndex < 0)
                return split;

            return split.Take(commentIndex).ToList();
        }

        public (DateTime, int, Command, List<object>) NextCommand()
        {
            var line = NextLine();

            if (line is null)
                return (DateTime.Now, -1, Command.Eof, new List<object>());

            if (line[0].ToLower() == "def")
            {
                var name = line[1];
                var value = string.Join(" ", line.Skip(2));
                return (DateTime.Now, -1, Command.Constant, new List<object> { name, value });
            }
            
            DateTime.TryParse(line[1], out var timeStamp);
            int.TryParse(line[1], out var droneId);
            
            var command = line[2] switch
            {
                "set-position" => Command.SetPos,
                "take-off" => Command.TakeOff,
                "fly-to" => Command.FlyTo,
                "fly-spiral" => Command.FlySpiral,
                "drone-mode" => Command.DroneMode,
                "fly-trajectory" => Command.FlyTrajectory,
                "land" => Command.Land,
                "hover" => Command.Hover,
                _ => Command.Error
            };

            var args = command switch
            {
                Command.FlyTo => ParseLinearTrajectoryArgs(line.Skip(3).ToList()),
                _ => new List<object>()
            };
            
            return (timeStamp, droneId, command, args);
        }

        private List<object> ParseLinearTrajectoryArgs(List<string> args)
        {
            int.TryParse(args[0], out var x);
            int.TryParse(args[1], out var y);
            int.TryParse(args[2], out var z);
            var destinationPoint = new Vector3(x, y, z);
            int.TryParse(args[3], out var destinationYaw);
            int.TryParse(args[4], out var speed); 
            return new List<object> { destinationPoint, destinationYaw, speed };
        }
    }
}
