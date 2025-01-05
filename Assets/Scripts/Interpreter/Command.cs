namespace Interpreter
{
    public enum Command
    {
        Constant,
        SetPos,
        TakeOff,
        FlyTo,
        FlySpiral,
        FlyCircle,
        DroneMode,
        FlyTrajectory,
        Land,
        Hover,
        SetColor,
        Eof,
        Error
    }
}