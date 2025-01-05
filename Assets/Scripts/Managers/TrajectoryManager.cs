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
        int height = cmdArguments.DestinationHeight;

        DroneState lastState = drones[droneId].getLastAdded();
        Vector3 lastPosition = lastState.Position;
        int timeLastStateEndPlusOne = lastState.Time + 1;
        int timeLinearTrajectoryStart = timestamp;

        Vector3 destinationPosition = new Vector3(lastPosition.x, lastPosition.y + height, lastPosition.z);

        CmdArgumentsDTO cmdArgumentsForLinear = new CmdArgumentsDTO(); 
        cmdArgumentsForLinear.StartPosition = lastPosition;
        cmdArgumentsForLinear.StartYaw = lastState.YawAngle;
        cmdArgumentsForLinear.DestinationPosition = destinationPosition;
        cmdArgumentsForLinear.DestinationYaw = lastState.YawAngle;
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

        if (NewTrajectoryStartsBeforePreviousEnds(timeLastStateEndPlusOne, timeLinearTrajectoryStart)) 
        {
            DroneState bezierTrajectoryStartState = drones[droneId][timeLinearTrajectoryStart];
            FlyQuadraticBezier(
                A: bezierTrajectoryStartState.Position,
                B: lastState.Position,
                C: cmdArguments.DestinationPosition,
                droneId: droneId,
                timestamp: timestamp,
                speed: cmdArguments.Speed,
                startYaw: bezierTrajectoryStartState.YawAngle,
                destinationYaw: cmdArguments.DestinationYaw
            );
            return;
        }
        if (ThereIsANeedToHoverAndWait(timeLastStateEndPlusOne, timeLinearTrajectoryStart)) 
        {
            drones[droneId] = trajectoryGenerator.GenerateHoverTrajectory(
                                new DroneTrajectory(drones[droneId]), 
                                timeLastStateEndPlusOne, 
                                timeLinearTrajectoryStart, 
                                lastState
                            );
        }
        
        cmdArguments.StartPosition = lastState.Position;
        cmdArguments.StartYaw = lastState.YawAngle;
        drones[droneId] = trajectoryGenerator.GenerateLinearTrajectory(drones[droneId], timestamp, cmdArguments);
    
    }

    private bool NewTrajectoryStartsBeforePreviousEnds(int timeLastStateEndPlusOne, int timeLinearTrajectoryStart)
    {
        return timeLastStateEndPlusOne > timeLinearTrajectoryStart;
    }

    private bool ThereIsANeedToHoverAndWait(int timeLastStateEndPlusOne, int timeLinearTrajectoryStart)
    {
        return timeLastStateEndPlusOne < timeLinearTrajectoryStart;
    }

    private bool NewTrajectoryStartsRightAfterPreviousEnds(int timeLastStateEndPlusOne, int timeLinearTrajectoryStart)
    {
        return timeLastStateEndPlusOne == timeLinearTrajectoryStart;
    }
    

    private void FlyQuadraticBezier(string droneId, int timestamp, Vector3 A, Vector3 B, Vector3 C, int speed, Quaternion startYaw, Quaternion destinationYaw)
    {
        CmdArgumentsDTO cmdArgumentsForBezier = new CmdArgumentsDTO(); 
        cmdArgumentsForBezier.PointA = A;
        cmdArgumentsForBezier.PointB = B;
        cmdArgumentsForBezier.PointC = C;
        cmdArgumentsForBezier.Speed = speed;
        cmdArgumentsForBezier.StartYaw = startYaw;
        cmdArgumentsForBezier.DestinationYaw = destinationYaw;

        drones[droneId] = trajectoryGenerator.GenerateQuadraticBezierTrajectory(drones[droneId], timestamp, cmdArgumentsForBezier);
    }

    private void FlyCubicBezier(string droneId, int timestamp, Vector3 A, Vector3 B, Vector3 C, Vector3 D, int speed, Quaternion startYaw, Quaternion destinationYaw)
    {
        CmdArgumentsDTO cmdArgumentsForBezier = new CmdArgumentsDTO(); 
        cmdArgumentsForBezier.PointA = A;
        cmdArgumentsForBezier.PointB = B;
        cmdArgumentsForBezier.PointC = C;
        cmdArgumentsForBezier.PointD = D;
        cmdArgumentsForBezier.Speed = speed;
        cmdArgumentsForBezier.StartYaw = startYaw;
        cmdArgumentsForBezier.DestinationYaw = destinationYaw;

        drones[droneId] = trajectoryGenerator.GenerateCubicBezierTrajectory(drones[droneId], timestamp, cmdArgumentsForBezier);
    }

    private void FlyTrajectoryCommand(string droneId, int timestamp, CmdArgumentsDTO cmdArguments)
    {
        if (drones[droneId].DroneMode == DroneMode.Exact) 
        {
            FlyExactTrajectory(droneId, timestamp, cmdArguments);
        } 
        else if (drones[droneId].DroneMode == DroneMode.Approx)
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
        if (points == null || points.Count == 0) {
            throw new InvalidOperationException("Trajectory is empty");
        }

        while (true)
        {
            if (points.Count == 0)
            {
                break;
            }
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

        if (ThereIsANeedToHoverAndWait(timeLastStateEndPlusOne, timeTrajectoryStart)) 
        {
            drones[droneId] = trajectoryGenerator.GenerateHoverTrajectory(
                                new DroneTrajectory(drones[droneId]), 
                                timeLastStateEndPlusOne, 
                                timeTrajectoryStart, 
                                lastState
                            );
        }


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

        (Vector3 T1, Vector3 T2) = GetSmoothPointsForBezier(A, B, C, smoothness);

        if (NewTrajectoryStartsBeforePreviousEnds(timeLastStateEndPlusOne, timeTrajectoryStart)) 
        {
            DroneState bezierTrajectoryStartState = drones[droneId][timeTrajectoryStart];
            FlyCubicBezier(
                A: bezierTrajectoryStartState.Position,
                B: A,
                C: T1,
                D: B,
                droneId: droneId,
                timestamp: timeTrajectoryStart,
                speed: B_DTO.Speed,
                startYaw: bezierTrajectoryStartState.YawAngle,
                destinationYaw: B_DTO.DestinationYaw
            );
        } else {
            FlyQuadraticBezier(
                A: A,
                B: T1,
                C: B,
                droneId: droneId,
                timestamp: timeTrajectoryStart,
                speed: B_DTO.Speed,
                startYaw: lastState.YawAngle,
                destinationYaw: B_DTO.DestinationYaw
            );
        }

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

            (T1, T2) = GetSmoothPointsForBezier(A, B, C, smoothness);

            FlyCubicBezier(
                A: A,
                B: S1,
                C: T1,
                D: B,
                droneId: droneId,
                timestamp: timeLastStateEndPlusOne,
                speed: B_DTO.Speed,
                startYaw: A_DTO.DestinationYaw,
                destinationYaw: B_DTO.DestinationYaw
            );

            timeLastStateEndPlusOne = drones[droneId].getLastAdded().Time + 1;
            drones[droneId].setLastAsKeyState();
            
            S1 = T2;
            A_DTO.copy(B_DTO);
            B_DTO.copy(C_DTO);
        }

        A = A_DTO.Point;
        B = B_DTO.Point;

        FlyQuadraticBezier(
            A: A,
            B: S1,
            C: B,
            droneId: droneId,
            timestamp: timeLastStateEndPlusOne,
            speed: B_DTO.Speed,
            startYaw: A_DTO.DestinationYaw,
            destinationYaw: B_DTO.DestinationYaw
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
                point.Point = FindPointOnCircle(O, StartPosition, DestinationPosition, angle);
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
        List<Vector3> generatedPoints = FindPointsForSpiral(A, B, StartPosition, DestinationPosition, numberOfRevolutions, isClockwise);
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
        FlyApproxTrajectory(droneId, timestamp, cmdArguments,  Vector3.Distance(StartPosition, FindPerpendicularPoint(A, B, StartPosition)) / 2);
    }

    private void DroneModeCommand(string droneId, int timestamp, CmdArgumentsDTO cmdArguments)
    {
        Debug.Log($"--SetDroneMode: {cmdArguments.DroneMode}");
        drones[droneId].DroneMode = cmdArguments.DroneMode;
    }

    private (Vector3 T1, Vector3 T2) GetSmoothPointsForBezier(Vector3 A, Vector3 B, Vector3 C, float smoothness)
    {
        float angleABC = CalculateAngleABC(A, B, C);
        float angleBeta = (180 - angleABC) / 2;
        Vector3 T1 = CalculatePointA(B, A, C, smoothness, angleBeta);
        Vector3 T2 = CalculatePointA(B, C, A, smoothness, angleBeta);
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
        Vector3 BC = C - B;
        Vector3 BD = D - B;

        Vector3 normal = Vector3.Cross(BD, BC).normalized;

        Vector3 unitBC = BC.normalized;

        Vector3 perpendicular = Vector3.Cross(normal, unitBC).normalized;

        float u = ABLength * Mathf.Cos(angleABC * Mathf.Deg2Rad);
        float v = ABLength * Mathf.Sin(angleABC * Mathf.Deg2Rad);

        Vector3 AB = u * unitBC + v * perpendicular;

        Vector3 A = B + AB;
        return A;
    }

    private List<Vector3> FindPointsForSpiral(Vector3 A, Vector3 B, Vector3 S, Vector3 D, int numberOfRevolutions, bool isClockwise)
    {
        if (numberOfRevolutions <= 0)
        {
            return null;
        }
        Vector3 SO = FindPerpendicularPoint(A, B, S);
        Vector3 DO = FindPerpendicularPoint(A, B, D);
        float distanceSD = Vector3.Distance(SO, DO);
        int numberOfPointsInTurn = 4;
        int numberOfPoints = numberOfRevolutions * numberOfPointsInTurn;
        float Dx = distanceSD / numberOfPoints;
        List<Vector3> points = new List<Vector3>();

        for (int i = 0; i < numberOfPoints; i++)
        {
            float angle = isClockwise ? -90 : 90;
            Vector3 P1 = FindPointOnCircleByAB(SO, S, A, B, angle);
            Vector3 P1s = FindPointOnParallel(A, B, P1, Dx);
            points.Add(P1s);
            S = P1s;
            SO = FindPerpendicularPoint(A, B, S);
        }
        return points;
    }

    /// <summary>
    /// Finds the perpendicular projection of point S onto the line AB.
    /// </summary>
    /// <param name="A">Point A on the line.</param>
    /// <param name="B">Point B on the line.</param>
    /// <param name="S">Point S to project onto the line.</param>
    /// <returns>Perpendicular projection point on the line AB.</returns>
    private Vector3 FindPerpendicularPoint(Vector3 A, Vector3 B, Vector3 S)
    {
        Vector3 AB = B - A;
        Vector3 AS = S - A;
        float t = Vector3.Dot(AS, AB) / Vector3.Dot(AB, AB);
        return A + t * AB; // Point on AB
    }

    /// <summary>
    /// Finds a point P1' on a vector parallel to AB, at a distance Dx from P1.
    /// </summary>
    /// <param name="A">Point A on the line.</param>
    /// <param name="B">Point B on the line.</param>
    /// <param name="P1">Starting point P1.</param>
    /// <param name="Dx">Distance to move from P1 along the parallel vector.</param>
    /// <returns>Point P1' at distance Dx along the parallel vector.</returns>
    private Vector3 FindPointOnParallel(Vector3 A, Vector3 B, Vector3 P1, float Dx)
    {
        Vector3 AB = B - A; // Vector along the line AB
        Vector3 unitDirection = AB.normalized; // Unit vector in the direction of AB

        // Point P1' in the specified direction and distance Dx
        return P1 + unitDirection * Dx;
    }
    
    public static Vector3 FindPerpendicularRandomPoint(Vector3 A, Vector3 B, Vector3 O)
    {
        // Step 1: Calculate the normal vector of the plane (AB direction)
        Vector3 normal = (B - A).normalized;

        // Step 2: Calculate C (constant term in the plane equation)
        float C = Vector3.Dot(normal, O);

        // Step 3: Generate random x and y values
        float randomX = UnityEngine.Random.Range(-10f, 10f);
        float randomY = UnityEngine.Random.Range(-10f, 10f);

        // Step 4: Solve for z
        float z;
        if (Mathf.Abs(normal.z) > 1e-5) // Check if N_z is not zero
        {
            z = (C - normal.x * randomX - normal.y * randomY) / normal.z;
        }
        else
        {
            // N_z is zero, so set z arbitrarily and solve for x or y instead
            z = UnityEngine.Random.Range(-10f, 10f);
            randomX = (C - normal.y * randomY - normal.z * z) / normal.x;
        }

        // Step 5: Return the random point
        return new Vector3(randomX, randomY, z);
    }

    public Vector3 FindPointOnCircle(Vector3 O, Vector3 P, Vector3 D, float alphaDegrees)
    {
        // Calculate the normal of the plane defined by P, O, and D
        Vector3 planeNormal = Vector3.Cross(P - O, D - O).normalized;

        // Calculate the radius vector (OP) and normalize it
        Vector3 OP = P - O;
        float radius = OP.magnitude;
        Vector3 OPNormalized = OP.normalized;

        // Find a vector perpendicular to OP in the plane (perpendicular1)
        Vector3 perpendicular1 = Vector3.Cross(planeNormal, OPNormalized).normalized;

        // Convert alpha to radians
        float alphaRadians = Mathf.Deg2Rad * alphaDegrees;

        // Compute the new point P1 using the circle equation
        Vector3 P1 = O + Mathf.Cos(alphaRadians) * radius * OPNormalized +
                          Mathf.Sin(alphaRadians) * radius * perpendicular1;

        return P1;
    }

    public Vector3 FindPointOnCircleByAB(Vector3 O, Vector3 P, Vector3 A, Vector3 B, float alphaDegrees)
    {
        // Calculate the normal of the plane defined by A and B (AB direction)
        Vector3 planeNormal = (B - A).normalized;

        // Calculate the radius vector (OP) and normalize it
        Vector3 OP = P - O;
        float radius = OP.magnitude;
        Vector3 OPNormalized = OP.normalized;

        // Find a vector perpendicular to OP in the plane (perpendicular1)
        Vector3 perpendicular1 = Vector3.Cross(planeNormal, OPNormalized).normalized;

        // Convert alpha to radians
        float alphaRadians = Mathf.Deg2Rad * alphaDegrees;

        // Compute the new point P1 using the circle equation
        Vector3 P1 = O + Mathf.Cos(alphaRadians) * radius * OPNormalized +
                        Mathf.Sin(alphaRadians) * radius * perpendicular1;

        return P1;
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

}
