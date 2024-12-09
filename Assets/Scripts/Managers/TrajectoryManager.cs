using System;
using System.Collections.Generic; 
using Drony.Entities;
using UnityEngine;
using Interpreter;
using Utility;

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

    public void LoadTrajectories(string fileDate)
    {
        IEnumerable<string> lines = new List<string>
        {
            "00:00:00 1 set-position 0 0 0",
            "00:00:00 1 take-off 2",
            "00:00:05 1 fly-to 3 3 3 90 2",
            "00:00:10 1 fly-to -3 3 3 180 2",
            "00:00:15 1 fly-to -3 3 0 270 2",
            "00:00:20 1 fly-to 0 0 0 360 2",

            "00:00:00 2 set-position 0 0 5",
            "00:00:00 2 take-off 4",
            "00:00:10 2 fly-to 0 3 -10 180 2",
            "00:00:20 2 fly-to 0 3 3 270 2",
            "00:00:30 2 fly-to 0 10 5 0 2",

            "00:00:00 3 set-position 1 0 1",
            "00:00:00 3 take-off 1",
            "00:00:10 3 fly-to 3 1 3 0 4",
            "00:00:15 3 fly-to -3 1 3 0 4",
            "00:00:20 3 fly-to -3 1 -3 0 4",
            "00:00:25 3 fly-to 3 1 -3 0 4",
        };
        _flightProgramParser = new FlightProgramParser(lines);


        while (true) {
            (TimeSpan timestamp, string droneId, Command cmd, List<object> cmdArguments) = _flightProgramParser.NextCommand();

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
                    drones[droneId] = trajectoryGenerator.SetInitialDronePosition(
                        droneTrajectory,
                        timestampMilliseconds,
                        cmdArguments);
                    break;
                case Command.TakeOff:
                    drones[droneId] = trajectoryGenerator.GenerateTakeOffTrajectory(
                        droneTrajectory,
                        timestampMilliseconds,
                        cmdArguments);
                    break;
                case Command.FlyTo:
                    drones[droneId] = trajectoryGenerator.GenerateLinearTrajectory(
                        droneTrajectory, 
                        timestampMilliseconds,
                        cmdArguments);
                    break;
                
            }     
        }
    }
    private DroneState GetNextDroneState(int playbackSpeed, string droneId) {
            int gapInMillis = Utilities.ConvertFromPlaybackSpeedToMillisGap(playbackSpeed);
            DroneState currentDroneState = drones[droneId].getNext(playbackSpeed, currentStateIndex);
            currentStateIndex += gapInMillis;
            return currentDroneState;
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
}
