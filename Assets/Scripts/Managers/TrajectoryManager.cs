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
        int defaultYaw = (int)lastState.YawAngle.eulerAngles.y; // FIXME: velmi blbe riesenie, treba prerobit

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
            Vector3 pointA = drones[droneId][timeLinearTrajectoryStart].Position;
            Vector3 pointB = lastState.Position;
            Vector3 pointC = cmdArguments.DestinationPosition;

            CmdArgumentsDTO cmdArgumentsForBezier = new CmdArgumentsDTO(); 
            cmdArgumentsForBezier.PointA = pointA;
            cmdArgumentsForBezier.PointB = pointB;
            cmdArgumentsForBezier.PointC = pointC;
            cmdArgumentsForBezier.Speed = cmdArguments.Speed;
            
            drones[droneId] = trajectoryGenerator.GenerateQuadraticBezierTrajectory(drones[droneId], timestamp, cmdArgumentsForBezier);
            return;

        } 
        else if (timeLastStateEndPlusOne < timeLinearTrajectoryStart) 
        {
            drones[droneId] = trajectoryGenerator.GenerateHoverTrajectory(
                                new DroneTrajectory(drones[droneId]), 
                                timeLastStateEndPlusOne, 
                                timeLinearTrajectoryStart, 
                                lastState
                            );
        }
        drones[droneId] = trajectoryGenerator.GenerateLinearTrajectory(drones[droneId], timestamp, cmdArguments);

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
}
