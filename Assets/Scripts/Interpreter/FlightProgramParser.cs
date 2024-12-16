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

        public (TimeSpan, string, Command, List<object>) NextCommand()
        {
            var line = NextLine();

            if (line is null)
                return (TimeSpan.Zero, "", Command.Eof, new List<object>());

            if (line[0].ToLower() == "def")
            {
                var name = line[1];
                var value = string.Join(" ", line.Skip(2));
                return (TimeSpan.Zero, "", Command.Constant, new List<object> { name, value });
            }
            
            TimeSpan.TryParse(line[0], out var timeStamp);
            string droneId = line[1];
            
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
                Command.TakeOff => ParseTakeOffArgs(line.Skip(3).ToList()),
                Command.SetPos => ParseSetPositionArgs(line.Skip(3).ToList()),
                Command.FlyTo => ParseLinearTrajectoryArgs(line.Skip(3).ToList()),
                Command.FlySpiral => ParseSpiralTrajectoryArgs(line.Skip(3).ToList()),
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

        private List<object> ParseSetPositionArgs(List<string> args)
        {
            int.TryParse(args[0], out var x);
            int.TryParse(args[1], out var y);
            int.TryParse(args[2], out var z);
            var startPoint = new Vector3(x, y, z);
            return new List<object> {startPoint,};
        }

        private List<object> ParseTakeOffArgs(List<string> args)
        {
            int.TryParse(args[0], out var height);
            return new List<object> {height,};
        }
        
        private List<object> ParseSpiralTrajectoryArgs(List<string> args)
        {
            int.TryParse(args[0], out var x);
            int.TryParse(args[1], out var y);
            int.TryParse(args[2], out var z);
            var destinationPoint = new Vector3(x, y, z);
            int.TryParse(args[3], out var xA);
            int.TryParse(args[4], out var yA);
            int.TryParse(args[5], out var zA);
            var pointA = new Vector3(xA, yA, zA);
            int.TryParse(args[6], out var xB);
            int.TryParse(args[7], out var yB);
            int.TryParse(args[8], out var zB);
            var pointB = new Vector3(xB, yB, zB);
            int.TryParse(args[9], out var clockwise);
            bool isClockwise = clockwise == 1;
            int.TryParse(args[10], out var numberOfRevolutions);
            int.TryParse(args[11], out var speed);
            return new List<object> { destinationPoint, pointA, pointB, isClockwise, numberOfRevolutions, speed };
        }
    }
}
