using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        private bool IsList(string input)
        {
            return input.StartsWith('[') && input.EndsWith(']');
        }

        private bool ContanisList(string input)
        {
            return input.Contains('[') && input.Contains(']');
        }

        private string RemoveBrackets(string input)
        {
            return input.Substring(1, input.Length - 2);
        }

        private List<string> ParseList(string input)
        {
            input = input.Trim();
            if (IsList(input))
            {
                input = RemoveBrackets(input);
            }
            string[] elements = input.Split(',');
            var result = elements.Select(e => e.Trim().Trim('\'', '"')).ToList();

            return result;
        }

        private List<string> ParseListForTrajectory(string input)
        {
            input = input.Trim();
            string pattern = @"\[(.*?)\]";
            var matches = Regex.Matches(input, pattern);
            var result = new List<string>();
            foreach (Match match in matches)
            {
                result.Add(match.ToString());
            }
            return result;
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

            string arguments = string.Join(" ", line.Skip(3).Take(line.Count - 3));
            List<string> argumentsList = new List<string>();

            if (IsList(arguments))
            {
                string withoutBrackets = RemoveBrackets(arguments);

                if (ContanisList(withoutBrackets))
                {
                    argumentsList = ParseListForTrajectory(withoutBrackets);
                } 
                else 
                {
                    argumentsList = ParseList(arguments);
                }
            }
            else 
            {
                argumentsList = line.Skip(3).ToList();
            }

            var cmdArgumentsDTO = command switch
            {
                Command.TakeOff => ParseTakeOffArgs(argumentsList),
                Command.SetPos => ParseSetPositionArgs(argumentsList),
                Command.FlyTo => ParseLinearTrajectoryArgs(argumentsList),
                Command.FlyTrajectory => ParseTrajectoryArgs(argumentsList),
                Command.FlySpiral => ParseSpiralTrajectoryArgs(argumentsList),
                Command.DroneMode => ParseDroneModeArgs(argumentsList),
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
            cmdArgumentsDTO.DestinationYaw = Quaternion.Euler(0, destinationYaw, 0);
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

        private CmdArgumentsDTO ParseTrajectoryArgs(List<string> args)
        {
            CmdArgumentsDTO cmdArgumentsDTO = new CmdArgumentsDTO();
            cmdArgumentsDTO.Points = new List<PointDTO>();
            foreach (String arg in args)
            {
                List<string> pointData = ParseList(arg);
                int.TryParse(pointData[0], out var x);
                int.TryParse(pointData[1], out var y);
                int.TryParse(pointData[2], out var z);
                int.TryParse(pointData[3], out var destinationYaw);
                int.TryParse(pointData[4], out var speed);
                PointDTO pointDTO = new PointDTO();
                pointDTO.Point = new Vector3(x, y, z);
                pointDTO.DestinationYaw = Quaternion.Euler(0, destinationYaw, 0);
                pointDTO.Speed = speed;
                cmdArgumentsDTO.Points.Add(pointDTO);
            }
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
        private CmdArgumentsDTO ParseDroneModeArgs(List<string> args)
        {
            DroneMode command = args[0] switch
            {
                "exact" => DroneMode.Exact,
                "exactly" => DroneMode.Exact,
                "e" => DroneMode.Exact,
                "approx" => DroneMode.Approx,
                "approximately" => DroneMode.Approx,
                "a" => DroneMode.Approx,
                _ => DroneMode.Error
            };

            CmdArgumentsDTO cmdArgumentsDTO = new CmdArgumentsDTO();
            cmdArgumentsDTO.DroneMode = command;

            return cmdArgumentsDTO;
        }
    }
}
