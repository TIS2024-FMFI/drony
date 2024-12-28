using System;
using System.Collections.Generic;
using System.Linq;
using Drony.dto;
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

            while (line.Trim() == "" || line.Trim()[0] == '#')
            {
                _currentLine++;
                if (_currentLine >= _programLines.Count)
                    return null;
                line = _programLines[_currentLine];
            }
            _currentLine++;

            var split = line.Trim().Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries).ToList();
            var commentIndex = split.FindIndex(elem => elem.Contains('#'));
            if (commentIndex < 0)
                return split;

            return split.Take(commentIndex).ToList();
        }

        public (TimeSpan, string, Command, CmdArgumentsDTO) NextCommand()
        {
            var line = NextLine();

            if (line is null)
                return (TimeSpan.Zero, "", Command.Eof, new CmdArgumentsDTO());

            if (line[0].ToLower() == "def")
            {
                var name = line[1];
                var value = string.Join(" ", line.Skip(2));
                CmdArgumentsDTO nameValueDTO = new CmdArgumentsDTO();
                nameValueDTO.Name = name;
                nameValueDTO.Value = value;
                return (TimeSpan.Zero, "", Command.Constant, nameValueDTO);
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

            var cmdArgumentsDTO = command switch
            {
                Command.TakeOff => ParseTakeOffArgs(line.Skip(3).ToList()),
                Command.SetPos => ParseSetPositionArgs(line.Skip(3).ToList()),
                Command.FlyTo => ParseLinearTrajectoryArgs(line.Skip(3).ToList()),
                Command.FlySpiral => ParseSpiralTrajectoryArgs(line.Skip(3).ToList()),
                _ => new CmdArgumentsDTO()
            };
            
            return (timeStamp, droneId, command, cmdArgumentsDTO);
        }

        private CmdArgumentsDTO ParseLinearTrajectoryArgs(List<string> args)
        {
            int.TryParse(args[0], out var x);
            int.TryParse(args[1], out var y);
            int.TryParse(args[2], out var z);
            int.TryParse(args[3], out var destinationYaw);
            int.TryParse(args[4], out var speed); 

            CmdArgumentsDTO cmdArgumentsDTO = new CmdArgumentsDTO();
            cmdArgumentsDTO.DestinationPosition = new Vector3(x, y, z);
            cmdArgumentsDTO.DestinationYaw = destinationYaw;
            cmdArgumentsDTO.Speed = speed;

            return cmdArgumentsDTO;
        }

        private CmdArgumentsDTO ParseSetPositionArgs(List<string> args)
        {
            int.TryParse(args[0], out var x);
            int.TryParse(args[1], out var y);
            int.TryParse(args[2], out var z);

            CmdArgumentsDTO cmdArgumentsDTO = new CmdArgumentsDTO();
            cmdArgumentsDTO.StartPosition = new Vector3(x, y, z);

            return cmdArgumentsDTO;
        }

        private CmdArgumentsDTO ParseTakeOffArgs(List<string> args)
        {
            int.TryParse(args[0], out var height);

            CmdArgumentsDTO cmdArgumentsDTO = new CmdArgumentsDTO();
            cmdArgumentsDTO.DestinationHeight = height;

            return cmdArgumentsDTO;
        }
        
        private CmdArgumentsDTO ParseSpiralTrajectoryArgs(List<string> args)
        {
            int.TryParse(args[0], out var x);
            int.TryParse(args[1], out var y);
            int.TryParse(args[2], out var z);
            int.TryParse(args[3], out var xA);
            int.TryParse(args[4], out var yA);
            int.TryParse(args[5], out var zA);
            int.TryParse(args[6], out var xB);
            int.TryParse(args[7], out var yB);
            int.TryParse(args[8], out var zB);
            int.TryParse(args[9], out var clockwise);
            int.TryParse(args[10], out var numberOfRevolutions);
            int.TryParse(args[11], out var speed);

            CmdArgumentsDTO cmdArgumentsDTO = new CmdArgumentsDTO();
            cmdArgumentsDTO.DestinationPosition = new Vector3(x, y, z);
            cmdArgumentsDTO.PointA = new Vector3(xA, yA, zA);
            cmdArgumentsDTO.PointB = new Vector3(xB, yB, zB);
            cmdArgumentsDTO.IsClockwise = clockwise == 1;
            cmdArgumentsDTO.NumberOfRevolutions = numberOfRevolutions;
            cmdArgumentsDTO.Speed = speed;

            return cmdArgumentsDTO;
        }
    }
}
