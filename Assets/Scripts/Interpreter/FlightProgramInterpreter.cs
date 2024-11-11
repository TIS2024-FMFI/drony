using System;
using System.Collections.Generic;
using System.Linq;

public enum Command
{
    SetPos,
    TakeOff,
    FlyTo,
    FlySpiral,
    DroneMode,
    FlyTrajectory,
    Land,
    Hover,
    Eof,
    Error
}

public class FlightProgramInterpreter
{
    private List<string> _programLines;
    private int _currentLine;
    private Dictionary<string, string> _constants;

    public FlightProgramInterpreter(IEnumerable<string> lines)
    {
        _programLines = lines.ToList();
        _currentLine = 0;
        _constants = new Dictionary<string, string>();
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

        return split.Take(commandIndex).ToList();
    }

    public (DateTime, int, Command, List<string>) NextCommand()
    {
        var line = NextLine();

        while (line is not null && line.FirstOrDefault()?.ToLower() == "def")
        {
            var name = line[1];
            var value = string.Join(" ", line.Skip(2));
            _constants[name] = value;
            line = NextLine();
        }

        if (line is null)
            return (DateTime.Now, -1, Command.Eof, new List<string>());

        var timestamp = DateTime.Parse(line[0]);
        var droneId = int.Parse(line[1]);
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

        var args = new List<string>();
        if (line.Count >= 3)
        {
            args = line.Skip(3).ToList();
        }

        return (timestamp, droneId, command, args);
    }
}
