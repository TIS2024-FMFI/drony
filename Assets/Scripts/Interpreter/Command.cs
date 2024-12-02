namespace Interpreter
{
    public enum Command
    {
        Constant,
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
}