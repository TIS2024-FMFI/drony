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
    private int currentStateIndex;
    private TrajectoryGenerator trajectoryGenerator;
    private FlightProgramParser _flightProgramParser;
    private int TAKEOFF_SPEED = 1; // FIXME: add an option to set it in config file or in ui
    private float SMOOTHNESS = 0.5f; // in m
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
        currentStateIndex = 0;
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
                    SetPosCommand(
                        droneId,
                        timestampMilliseconds,
                        cmdArguments);
                    break;
                case Command.TakeOff:
                    TakeOffCommand(
                        droneId,
                        timestampMilliseconds,
                        cmdArguments);
                    break;
                case Command.FlyTo:
                    FlyToCommand(
                        droneId,
                        timestampMilliseconds,
                        cmdArguments);
                    break;
                case Command.FlyTrajectory:
                    FlyTrajectoryCommand(
                        droneId,
                        timestampMilliseconds,
                        cmdArguments);
                    break;
                case Command.FlySpiral:
                    drones[droneId] = trajectoryGenerator.GenerateSpiralTrajectory(
                        droneTrajectory,
                        timestampMilliseconds,
                        cmdArguments);
                    break;
                
            }  
            drones[droneId].setLastAsKeyState();
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
    private void TakeOffCommand(string droneId, int timestamp, CmdArgumentsDTO cmdArguments)
    {
        Debug.Log("--TakeOffCommand");
        int height = cmdArguments.DestinationHeight;

        DroneState lastState = drones[droneId].getLastAdded();
        Vector3 lastPosition = lastState.Position;
        int timeLastStateEndPlusOne = lastState.Time + 1;
        int timeLinearTrajectoryStart = timestamp;

        Vector3 destinationPosition = new Vector3(lastPosition.x, lastPosition.y + height, lastPosition.z);
        int defaultYaw = (int) lastState.YawAngle.eulerAngles.y; // FIXME: velmi blbe riesenie, treba prerobit

        CmdArgumentsDTO cmdArgumentsForLinear = new CmdArgumentsDTO(); 
        cmdArgumentsForLinear.DestinationPosition = destinationPosition;
        cmdArgumentsForLinear.DestinationYaw = defaultYaw;
        cmdArgumentsForLinear.Speed = TAKEOFF_SPEED;
        
        if (timeLastStateEndPlusOne < timeLinearTrajectoryStart) 
        {
            drones[droneId] = trajectoryGenerator.GenerateHoverTrajectory(
                                new DroneTrajectory(drones[droneId]), 
                                timeLastStateEndPlusOne, 
                                timeLinearTrajectoryStart, 
                                lastState
                            );
        }

        drones[droneId] = trajectoryGenerator.GenerateLinearTrajectory(drones[droneId], timestamp, cmdArgumentsForLinear);
    }
    private void FlyToCommand(string droneId, int timestamp, CmdArgumentsDTO cmdArguments)
    {
        Debug.Log("--FlyToCommand");
        DroneState lastState = drones[droneId].getLastAdded();
        int timeLastStateEndPlusOne = lastState.Time + 1;
        int timeLinearTrajectoryStart = timestamp;

        if (timeLastStateEndPlusOne > timeLinearTrajectoryStart) 
        {
            // Generate bezier trajectory
            DroneState bezierTrajectoryStartState = drones[droneId][timeLinearTrajectoryStart];

            Vector3 pointA = bezierTrajectoryStartState.Position;
            Vector3 pointB = lastState.Position;
            Vector3 pointC = cmdArguments.DestinationPosition;

            CmdArgumentsDTO cmdArgumentsForBezier = new CmdArgumentsDTO(); 
            cmdArgumentsForBezier.PointA = pointA;
            cmdArgumentsForBezier.PointB = pointB;
            cmdArgumentsForBezier.PointC = pointC;
            cmdArgumentsForBezier.Speed = cmdArguments.Speed;
            cmdArgumentsForBezier.StartYaw = (int) bezierTrajectoryStartState.YawAngle.eulerAngles.y;
            cmdArgumentsForBezier.DestinationYaw = cmdArguments.DestinationYaw;
            
            drones[droneId] = trajectoryGenerator.GenerateQuadraticBezierTrajectory(drones[droneId], timestamp, cmdArgumentsForBezier);
        }
        else if (timeLastStateEndPlusOne < timeLinearTrajectoryStart) 
        {
            drones[droneId] = trajectoryGenerator.GenerateHoverTrajectory(
                                new DroneTrajectory(drones[droneId]), 
                                timeLastStateEndPlusOne, 
                                timeLinearTrajectoryStart, 
                                lastState
                            );
            drones[droneId] = trajectoryGenerator.GenerateLinearTrajectory(drones[droneId], timestamp, cmdArguments);
        }
    }
    private void FlyTrajectoryCommand(string droneId, int timestamp, CmdArgumentsDTO cmdArguments)
    {
        Debug.Log("--FlyTrajectoryCommand");
        // TODO: insert hover or bezier between last and new
        DroneState lastState = drones[droneId].getLastAdded();
        int timeLastStateEndPlusOne = lastState.Time + 1;
        int timeTrajectoryStart = timestamp;

        List<PointDTO> points = cmdArguments.Points;
        if (points == null) {
            throw new InvalidOperationException("List is null");
        }
        if (points.Count < 2) {
            // generate linear
        }
        PointDTO A_DTO = new PointDTO();
        PointDTO B_DTO = points[0];
        PointDTO C_DTO = points[1];
        points.RemoveRange(0, 2);

        Vector3 A = lastState.Position;
        Vector3 B = B_DTO.Point;
        Vector3 C = C_DTO.Point;

        (Vector3 T1, Vector3 T2) = GetSmoothPointsForBezier(A, B, C);

        CmdArgumentsDTO cmdArgumentsForStartBezier = new CmdArgumentsDTO(); 
        cmdArgumentsForStartBezier.PointA = A;
        cmdArgumentsForStartBezier.PointB = T1;
        cmdArgumentsForStartBezier.PointC = B;
        cmdArgumentsForStartBezier.Speed = B_DTO.Speed;
        cmdArgumentsForStartBezier.StartYaw = (int) lastState.YawAngle.eulerAngles.y;
        cmdArgumentsForStartBezier.DestinationYaw = B_DTO.DestinationYaw;

        drones[droneId] = trajectoryGenerator.GenerateQuadraticBezierTrajectory(drones[droneId], timeLastStateEndPlusOne, cmdArgumentsForStartBezier);
        timeLastStateEndPlusOne = drones[droneId].getLastAdded().Time + 1; 
        drones[droneId].setLastAsKeyState();
        
        Vector3 S1 = T2;
        A_DTO.copy(B_DTO);
        B_DTO.copy(C_DTO);

        while (true)
        {
            if (points.Count == 0)
            {
                break;
            }
            C_DTO = points[0];
            points.RemoveRange(0, 1);

            A = A_DTO.Point;
            B = B_DTO.Point;
            C = C_DTO.Point;

            (T1, T2) = GetSmoothPointsForBezier(A, B, C);

            CmdArgumentsDTO cmdArgumentsForCubicBezier = new CmdArgumentsDTO(); 
            cmdArgumentsForCubicBezier.PointA = A;
            cmdArgumentsForCubicBezier.PointB = S1;
            cmdArgumentsForCubicBezier.PointC = T1;
            cmdArgumentsForCubicBezier.PointD = B;
            cmdArgumentsForCubicBezier.Speed = B_DTO.Speed;
            cmdArgumentsForCubicBezier.StartYaw = A_DTO.DestinationYaw;
            cmdArgumentsForCubicBezier.DestinationYaw = B_DTO.DestinationYaw;

            drones[droneId] = trajectoryGenerator.GenerateCubicBezierTrajectory(drones[droneId], timeLastStateEndPlusOne, cmdArgumentsForCubicBezier);
            timeLastStateEndPlusOne = drones[droneId].getLastAdded().Time + 1;
            drones[droneId].setLastAsKeyState();
            
            S1 = T2;
            A_DTO.copy(B_DTO);
            B_DTO.copy(C_DTO);
        }

        A = A_DTO.Point;
        B = B_DTO.Point;

        cmdArgumentsForStartBezier.PointA = A;
        cmdArgumentsForStartBezier.PointB = S1;
        cmdArgumentsForStartBezier.PointC = B;
        cmdArgumentsForStartBezier.Speed = B_DTO.Speed;
        cmdArgumentsForStartBezier.StartYaw = A_DTO.DestinationYaw;
        cmdArgumentsForStartBezier.DestinationYaw = B_DTO.DestinationYaw;

        drones[droneId] = trajectoryGenerator.GenerateQuadraticBezierTrajectory(drones[droneId], timeLastStateEndPlusOne, cmdArgumentsForStartBezier);
    }

    private (Vector3 T1, Vector3 T2) GetSmoothPointsForBezier(Vector3 A, Vector3 B, Vector3 C)
    {
        float angleABC = CalculateAngleABC(A, B, C);
        float angleBeta = (180 - angleABC) / 2;
        Vector3 T1 = CalculatePointA(B, A, C, SMOOTHNESS, angleBeta);
        Vector3 T2 = CalculatePointA(B, C, A, SMOOTHNESS, angleBeta);
        //Debug.Log($"T1: {T1}, T2: {T2}");
        bezierPoints.Add(T1);
        bezierPoints.Add(T2);
        return (T1, T2);
    }

    private float CalculateAngleABC(Vector3 A, Vector3 B, Vector3 C)
    {
        Vector3 BA = A - B;
        Vector3 BC = C - B;

        float dotProduct = Vector3.Dot(BA, BC);

        float magnitudeBA = BA.magnitude;
        float magnitudeBC = BC.magnitude;

        float cosTheta = dotProduct / (magnitudeBA * magnitudeBC);

        float angleRadians = Mathf.Acos(cosTheta);

        float angleDegrees = Mathf.Rad2Deg * angleRadians;

        return angleDegrees;
    }

    private Vector3 CalculatePointA(Vector3 B, Vector3 C, Vector3 D, float ABLength, float angleABC)
    {
        // Вектор BC
        Vector3 BC = C - B;
        Vector3 BD = D - B;

        // Нормаль к плоскости BCD
        Vector3 normal = Vector3.Cross(BD, BC).normalized;

        // Единичный вектор вдоль BC
        Vector3 unitBC = BC.normalized;

        // Вектор в плоскости, перпендикулярный BC
        Vector3 perpendicular = Vector3.Cross(normal, unitBC).normalized;

        // Разложение AB по BC и перпендикулярному вектору
        float u = ABLength * Mathf.Cos(angleABC * Mathf.Deg2Rad); // Компонента вдоль BC
        float v = ABLength * Mathf.Sin(angleABC * Mathf.Deg2Rad); // Компонента перпендикулярная BC

        // Вектор AB
        Vector3 AB = u * unitBC + v * perpendicular;

        // Координаты точки A
        Vector3 A = B + AB;
        return A;
    }


    private DroneState GetNextDroneState(int playbackSpeed, string droneId) {
            int gapInMillis = Utilities.ConvertFromPlaybackSpeedToMillisGap(playbackSpeed);
            DroneState currentDroneState = drones[droneId].getNext(playbackSpeed, currentStateIndex);
            currentStateIndex += gapInMillis;
            return currentDroneState;
    }
    public List<string> GetDroneIds() {
        List<string> ids = new List<string>();
        ids.AddRange(drones.Keys);
        return ids;
    }
    public DroneState GetStateAtTime(int playbackSpeed, string droneId)
    {
        try {
            return GetNextDroneState(playbackSpeed, droneId);
        } catch(ArgumentOutOfRangeException) {
            // FIXME: ONLY for testing, i am reseting index, so the animation will loop
            currentStateIndex = 0;
            return GetNextDroneState(playbackSpeed, droneId);
        }
    }
    public List<DroneState> GetKeyStates(string droneId)
    {
        return drones[droneId].KeyStates;
    }
    public List<Vector3> GetBezierPoints()
    {
        return bezierPoints;
    }
}
