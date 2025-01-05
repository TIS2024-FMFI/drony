using System;
using System.Collections.Generic; 
using Drony.Entities;
using UnityEngine;
using Interpreter;
using Utility;
using Drony.dto;

/// <summary>
/// Singleton class <c>TrajectoryManager</c> manages logic for parser and generator 
/// to create trajectories for each drone.
/// </summary>
public class TrajectoryManager 
{
    private Dictionary<string, DroneTrajectory> drones;
    private int currentTime = 0;
    private TrajectoryGenerator trajectoryGenerator;
    private FlightProgramParser _flightProgramParser;
    private int TAKEOFF_SPEED = 1; // FIXME: add an option to set it in config file or in ui
    private float SMOOTHNESS = 0.5f; // in m
    public int totalTime = 0;
    private float PlaybackSpeed = 1;
    private List<Vector3> bezierPoints = new List<Vector3>();
    private static TrajectoryManager _instance;
    public static TrajectoryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new TrajectoryManager();
            }
            return _instance;
        }
    }

    private TrajectoryManager()
    {
        drones = new Dictionary<string, DroneTrajectory>();
        trajectoryGenerator = new TrajectoryGenerator();
    }

    public static TrajectoryManager Reinstanciate()
    {
        _instance = new TrajectoryManager();
        return _instance;
    }

    public void LoadTrajectories(List<string> droneCommands)
    {   
        _flightProgramParser = new FlightProgramParser(droneCommands);


        while (true) {
            (TimeSpan timestamp, string droneId, Command cmd, CmdArgumentsDTO cmdArguments) = _flightProgramParser.NextCommand();

            if (droneId == "") {
                break;
            } if (!drones.ContainsKey(droneId)) {
                drones[droneId] = trajectoryGenerator.initDroneTrajectory(droneId);
            }
            DroneTrajectory droneTrajectory = drones[droneId]; 
            int timestampMilliseconds = (int)timestamp.TotalMilliseconds;

            switch (cmd)
            {
                case Command.SetPos:
                    SetPosCommand(droneId, timestampMilliseconds, cmdArguments);
                    break;
                case Command.TakeOff:
                    TakeOffCommand(droneId, timestampMilliseconds, cmdArguments);
                    break;
                case Command.FlyTo:
                    FlyToCommand(droneId, timestampMilliseconds, cmdArguments);
                    break;
                case Command.DroneMode:
                    DroneModeCommand(droneId, timestampMilliseconds, cmdArguments);
                    break;
                case Command.FlyTrajectory:
                    FlyTrajectoryCommand(droneId, timestampMilliseconds, cmdArguments);
                    break;
                case Command.FlySpiral:
                    FlySpiralTrajectoryCommand(droneId, timestampMilliseconds, cmdArguments);
                    break;
                case Command.FlyCircle:
                    FlyCircleTrajectoryCommand(droneId, timestampMilliseconds, cmdArguments);
                    break;
                case Command.SetColor:
                    SetColorCommand(droneId, timestampMilliseconds, cmdArguments);
                    break;
                
            }  
            drones[droneId].setLastAsKeyState();
            UpdateTotalTime(droneId);
        }
    }
    private void SetPosCommand(string droneId, int timestamp, CmdArgumentsDTO cmdArguments)
    {
        Debug.Log("--SetPosCommand");
        drones[droneId] = trajectoryGenerator.SetInitialDronePosition(
                        drones[droneId],
                        timestamp,
                        cmdArguments);
    }
    private void SetColorCommand(string droneId, int timestamp, CmdArgumentsDTO cmdArguments)
    {
        Debug.Log("--SetColorCommand");
        throw new NotImplementedException("SetColorCommand is not implemented yet.");
        
    }
    private void TakeOffCommand(string droneId, int timestamp, CmdArgumentsDTO cmdArguments)
    {
        Debug.Log("--TakeOffCommand");

        DroneState lastState = drones[droneId].getLastAdded();
        Vector3 lastPosition = lastState.Position;
        int timeLastStateEndPlusOne = lastState.Time + 1;
        int timeTakeOffTrajectoryStart = timestamp;

        GenerateHoverTrajectoryIfNeeded(
            droneId: droneId, 
            lastState: lastState, 
            timeLastStateEndPlusOne: timeLastStateEndPlusOne,
            timeNewTrajectoryStart: timeTakeOffTrajectoryStart);

        int height = cmdArguments.DestinationHeight;
        Vector3 destinationPosition = new Vector3(lastPosition.x, lastPosition.y + height, lastPosition.z);

        drones[droneId] = trajectoryGenerator.GenerateLinearTrajectory(
            droneTrajectory: drones[droneId], 
            timestamp: timestamp, 
            startPosition: lastPosition,
            destinationPosition: destinationPosition,
            startRotation: lastState.YawAngle,
            targetRotation: lastState.YawAngle,
            speed: TAKEOFF_SPEED
        );
    }
    private void FlyToCommand(string droneId, int timestamp, CmdArgumentsDTO cmdArguments)
    {
        Debug.Log("--FlyToCommand");
        DroneState lastState = drones[droneId].getLastAdded();
        int timeLastStateEndPlusOne = lastState.Time + 1;
        int timeLinearTrajectoryStart = timestamp;

        if (NewTrajectoryStartsBeforePreviousEnds(timeLastStateEndPlusOne, timeLinearTrajectoryStart)) 
        {
            DroneState bezierTrajectoryStartState = drones[droneId][timeLinearTrajectoryStart];
            drones[droneId] = trajectoryGenerator.GenerateQuadraticBezierTrajectory(
                droneTrajectory: drones[droneId], 
                timestamp: timestamp, 
                pointA: bezierTrajectoryStartState.Position,
                pointB: lastState.Position,
                pointC: cmdArguments.DestinationPosition,
                startRotation: bezierTrajectoryStartState.YawAngle,
                targetRotation: cmdArguments.DestinationYaw,
                speed: cmdArguments.Speed
            );
            return;
        }
        GenerateHoverTrajectoryIfNeeded(
            droneId: droneId, 
            lastState: lastState, 
            timeLastStateEndPlusOne: timeLastStateEndPlusOne,
            timeNewTrajectoryStart: timeLinearTrajectoryStart
        );
        
        drones[droneId] = trajectoryGenerator.GenerateLinearTrajectory(
            droneTrajectory: drones[droneId], 
            timestamp: timestamp, 
            startPosition: lastState.Position,
            destinationPosition: cmdArguments.DestinationPosition,
            startRotation: lastState.YawAngle,
            targetRotation: cmdArguments.DestinationYaw,
            speed: cmdArguments.Speed
        );
    
    }
    private void HoverCommand(string droneId, int timestamp, CmdArgumentsDTO cmdArguments)
    {
        Debug.Log("--HoverCommand");
        throw new NotImplementedException("HoverCommand is not implemented yet.");
    }

    private void FlyTrajectoryCommand(string droneId, int timestamp, CmdArgumentsDTO cmdArguments)
    {
        if (DroneModeIsExact(droneId)) 
        {
            FlyExactTrajectory(droneId, timestamp, cmdArguments);
        } 
        else if (DroneModeIsApprox(droneId))
        {
            FlyApproxTrajectory(droneId, timestamp, cmdArguments, SMOOTHNESS);
        }
    }
    private void FlyExactTrajectory(string droneId, int timestamp, CmdArgumentsDTO cmdArguments)
    {
        Debug.Log("--FlyExactTrajectoryCommand");
        DroneState lastState = drones[droneId].getLastAdded();
        int timeLastStateEndPlusOne = lastState.Time + 1;
        int timeTrajectoryStart = timestamp;

        List<PointDTO> points = cmdArguments.Points;
        if (points == null || points.Count == 0) 
        {
            throw new InvalidOperationException("Trajectory is empty, but would be much more better if there was an UI which will show this exception to the user");
        }

        while (points.Count != 0)
        {
            PointDTO pointDTO = points[0];
            points.RemoveRange(0, 1);
            cmdArguments.DestinationPosition = pointDTO.Point;
            cmdArguments.DestinationYaw = pointDTO.DestinationYaw;
            cmdArguments.Speed = pointDTO.Speed;
            FlyToCommand(droneId, timeTrajectoryStart, cmdArguments);

            timeTrajectoryStart = drones[droneId].getLastAdded().Time + 1; 
        }
    }

    private void FlyApproxTrajectory(string droneId, int timestamp, CmdArgumentsDTO cmdArguments, float smoothness)
    {
        Debug.Log("--FlyApproxTrajectoryCommand");
        DroneState lastState = drones[droneId].getLastAdded();
        int timeLastStateEndPlusOne = lastState.Time + 1;
        int timeTrajectoryStart = timestamp;

        GenerateHoverTrajectoryIfNeeded(
            droneId: droneId, 
            lastState: lastState, 
            timeLastStateEndPlusOne: timeLastStateEndPlusOne,
            timeNewTrajectoryStart: timeTrajectoryStart
        );

        List<PointDTO> points = cmdArguments.Points;
        if (points == null || points.Count == 0) {
            throw new InvalidOperationException("Trajectory is empty");
        }
        if (points.Count == 1) {
            // generate linear in case there is only one point
            PointDTO pointDTO = points[0];
            cmdArguments.StartPosition = lastState.Position;
            cmdArguments.StartYaw = lastState.YawAngle;
            cmdArguments.DestinationPosition = pointDTO.Point;
            cmdArguments.DestinationYaw = pointDTO.DestinationYaw;
            cmdArguments.Speed = pointDTO.Speed;

            FlyToCommand(droneId, timeTrajectoryStart, cmdArguments);
            return;
        }

        PointDTO A_DTO = new PointDTO();
        PointDTO B_DTO = points[0];
        PointDTO C_DTO = points[1];
        points.RemoveRange(0, 2);

        Vector3 A = lastState.Position;
        Vector3 B = B_DTO.Point;
        Vector3 C = C_DTO.Point;

        (Vector3 T1, Vector3 T2) = MathOperations.GetSmoothPointsForBezier(A, B, C, smoothness);
        bezierPoints.Add(T1);
        bezierPoints.Add(T2);

        if (NewTrajectoryStartsBeforePreviousEnds(timeLastStateEndPlusOne, timeTrajectoryStart)) 
        {
            DroneState bezierTrajectoryStartState = drones[droneId][timeTrajectoryStart];
            drones[droneId] = trajectoryGenerator.GenerateCubicBezierTrajectory(
                droneTrajectory: drones[droneId], 
                timestamp: timeTrajectoryStart, 
                pointA: bezierTrajectoryStartState.Position,
                pointB: A,
                pointC: T1,
                pointD: B,
                startRotation: bezierTrajectoryStartState.YawAngle,
                targetRotation: B_DTO.DestinationYaw,
                speed: B_DTO.Speed
            );
        } else {
            drones[droneId] = trajectoryGenerator.GenerateQuadraticBezierTrajectory(
                droneTrajectory: drones[droneId], 
                timestamp: timeTrajectoryStart, 
                pointA: A,
                pointB: T1,
                pointC: B,
                startRotation: lastState.YawAngle,
                targetRotation: B_DTO.DestinationYaw,
                speed: B_DTO.Speed
            );
        }

        timeLastStateEndPlusOne = drones[droneId].getLastAdded().Time + 1; 
        drones[droneId].setLastAsKeyState();
        
        Vector3 S1 = T2;
        A_DTO.copy(B_DTO);
        B_DTO.copy(C_DTO);

        while (points.Count != 0)
        {
            C_DTO = points[0];
            points.RemoveRange(0, 1);

            A = A_DTO.Point;
            B = B_DTO.Point;
            C = C_DTO.Point;

            (T1, T2) = MathOperations.GetSmoothPointsForBezier(A, B, C, smoothness);
            bezierPoints.Add(T1);
            bezierPoints.Add(T2);

            drones[droneId] = trajectoryGenerator.GenerateCubicBezierTrajectory(
                droneTrajectory: drones[droneId], 
                timestamp: timeLastStateEndPlusOne, 
                pointA: A,
                pointB: S1,
                pointC: T1,
                pointD: B,
                startRotation: A_DTO.DestinationYaw,
                targetRotation: B_DTO.DestinationYaw,
                speed: B_DTO.Speed
            );

            timeLastStateEndPlusOne = drones[droneId].getLastAdded().Time + 1;
            drones[droneId].setLastAsKeyState();
            
            S1 = T2;
            A_DTO.copy(B_DTO);
            B_DTO.copy(C_DTO);
        }

        A = A_DTO.Point;
        B = B_DTO.Point;

        drones[droneId] = trajectoryGenerator.GenerateQuadraticBezierTrajectory(
            droneTrajectory: drones[droneId], 
            timestamp: timeLastStateEndPlusOne, 
            pointA: A,
            pointB: S1,
            pointC: B,
            startRotation: A_DTO.DestinationYaw,
            targetRotation: B_DTO.DestinationYaw,
            speed: B_DTO.Speed
        );
    }

    private void FlyCircleTrajectoryCommand(string droneId, int timestamp, CmdArgumentsDTO cmdArguments)
    {
        Debug.Log("--FlyCircleTrajectoryCommand");
        DroneState lastState = drones[droneId].getLastAdded();

        Vector3 StartPosition = lastState.Position;
        Vector3 O = cmdArguments.PointO;
        Vector3 DestinationPosition = cmdArguments.DestinationPosition;
        int numberOfRevolutions = cmdArguments.NumberOfRevolutions;
        bool isClockwise = cmdArguments.IsClockwise;

        List<int> angles;
        if (isClockwise)
        {
            angles = new List<int> {90, 180, 270, 360};
        } 
        else 
        {
            angles = new List<int> {270, 180, 90, 0};
        }
        
        List<PointDTO> points = new List<PointDTO>();
        PointDTO startPoint = new PointDTO();
        startPoint.Point = StartPosition;
        startPoint.Speed = cmdArguments.Speed;
        //points.Add(startPoint);
        for (int i = 0; i < numberOfRevolutions; i++)
        {
            foreach (int angle in angles)
            {
                PointDTO point = new PointDTO();
                point.Point = MathOperations.FindPointOnCircle(O, StartPosition, DestinationPosition, angle);
                point.Speed = cmdArguments.Speed;
                points.Add(point);
            }
        }
        PointDTO DestinationPoint = new PointDTO();
        DestinationPoint.Point = DestinationPosition;
        DestinationPoint.Speed = cmdArguments.Speed;
        points.Add(DestinationPoint);

        cmdArguments.Points = points;
        FlyApproxTrajectory(droneId, timestamp, cmdArguments, Vector3.Distance(StartPosition, O) / 2);
    }

    private void FlySpiralTrajectoryCommand(string droneId, int timestamp, CmdArgumentsDTO cmdArguments)
    {
        Debug.Log("--FlySpiralTrajectoryCommand");
        DroneState lastState = drones[droneId].getLastAdded();

        Vector3 StartPosition = lastState.Position;
        Vector3 A = cmdArguments.PointA;
        Vector3 B = cmdArguments.PointB;
        Vector3 DestinationPosition = cmdArguments.DestinationPosition;
        int numberOfRevolutions = cmdArguments.NumberOfRevolutions;
        bool isClockwise = cmdArguments.IsClockwise;
        List<PointDTO> points = new List<PointDTO>();
        List<Vector3> generatedPoints = MathOperations.FindPointsForSpiral(A, B, StartPosition, DestinationPosition, numberOfRevolutions, isClockwise);
        foreach (Vector3 point in generatedPoints)
        {
            PointDTO pointDto = new PointDTO();
            pointDto.Point = point;
            pointDto.Speed = cmdArguments.Speed;
            points.Add(pointDto);
        }
        PointDTO DestinationPoint = new PointDTO();
        DestinationPoint.Point = DestinationPosition;
        DestinationPoint.Speed = cmdArguments.Speed;
        points.Add(DestinationPoint);

        cmdArguments.Points = points;
        FlyApproxTrajectory(droneId, timestamp, cmdArguments,  Vector3.Distance(StartPosition, MathOperations.FindPerpendicularPoint(A, B, StartPosition)) / 2);
    }

    private void DroneModeCommand(string droneId, int timestamp, CmdArgumentsDTO cmdArguments)
    {
        Debug.Log($"--SetDroneMode: {cmdArguments.DroneMode}");
        drones[droneId].DroneMode = cmdArguments.DroneMode;
    }

    public DroneState GetCurrentDroneState(string droneId) {
            DroneState currentDroneState = drones[droneId].getNext(currentTime);
            return currentDroneState;
    }
    public List<string> GetDroneIds() {
        List<string> ids = new List<string>();
        ids.AddRange(drones.Keys);
        return ids;
    }
    public List<DroneState> GetKeyStates(string droneId)
    {
        return drones[droneId].KeyStates;
    }
    public List<Vector3> GetBezierPoints()
    {
        return bezierPoints;
    }
    private void UpdateTotalTime(string droneId)
    {
        int droneLastTime = drones[droneId].getLastAdded().Time;
        if (droneLastTime > totalTime)
        {
            totalTime = droneLastTime;
        }
    }
    public void SetPlaybackSpeed(float playbackSpeed)
    {
        PlaybackSpeed = playbackSpeed;
    }
    public void UpdateCurrentTime()
    {
        int gapInMillis = Utilities.ConvertFromPlaybackSpeedToMillisGap(PlaybackSpeed);
        currentTime += gapInMillis;
    }
    public void SetCurrentTime(int time)
    {
        currentTime = time;
    }
    public int GetCurrentTime()
    {
        return currentTime;
    }
    public void ResetCurrentTime()
    {
        currentTime = 0;
    }

    private void GenerateHoverTrajectoryIfNeeded(string droneId, DroneState lastState, int timeLastStateEndPlusOne, int timeNewTrajectoryStart)
    {
        if (ThereIsANeedToHoverAndWait(timeLastStateEndPlusOne, timeNewTrajectoryStart)) 
        {
            drones[droneId] = trajectoryGenerator.GenerateHoverTrajectory(
                                new DroneTrajectory(drones[droneId]), 
                                timeLastStateEndPlusOne, 
                                timeNewTrajectoryStart, 
                                lastState
                            );
        }
    }

    private bool NewTrajectoryStartsBeforePreviousEnds(int timeLastStateEndPlusOne, int timeLinearTrajectoryStart)
    {
        return timeLastStateEndPlusOne > timeLinearTrajectoryStart;
    }

    private bool ThereIsANeedToHoverAndWait(int timeLastStateEndPlusOne, int timeTrajectoryStart)
    {
        return timeLastStateEndPlusOne < timeTrajectoryStart;
    }

    private bool NewTrajectoryStartsRightAfterPreviousEnds(int timeLastStateEndPlusOne, int timeLinearTrajectoryStart)
    {
        return timeLastStateEndPlusOne == timeLinearTrajectoryStart;
    }
    private bool DroneModeIsExact(string droneId)
    {
        return drones[droneId].DroneMode == DroneMode.Exact;
    }
    private bool DroneModeIsApprox(string droneId)
    {
        return drones[droneId].DroneMode == DroneMode.Approx;
    }

}
