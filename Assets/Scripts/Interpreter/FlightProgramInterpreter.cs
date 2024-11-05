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
    EOF,
    ERROR
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
        _constants = [];
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

        return split[..commentIndex];
    }

    public (Command, List<string>) NextCommand()
    {
        List<string>? line = NextLine();

        while (line is not null && line.FirstOrDefault()?.ToLower() == "def")
        {
            var name = line[1];
            var value = string.Join(" ", line[2..]);
            _constants[name] = value;
            line = NextLine();
        }

        if (line is null)
            return (Command.EOF, []);

        var timestamp = line[0];
        var droneId = line[1];
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
            _ => Command.ERROR
        };

        List<string> args = [];
        if (line.Count >= 3)
        {
            args = line[3..];
        }

        return (command, args);
    }
}
