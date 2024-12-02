using System;
using System.Collections.Generic; 
using Drony.Entities;
using UnityEngine;
using Interpreter;
using Utility;

/// <summary>
/// Class <c>TrajectoryManager</c> manages logic for parser and generator 
/// to create trajectories for each drone. It inherits from MonoBehaviour for drawing gizmos.
/// </summary>
public class TrajectoryManager
{
    private Dictionary<string, DroneTrajectory> drones;
    private int currentStateIndex;
    private TrajectoryGenerator trajectoryGenerator;
    private FlightProgramParser _flightProgramParser;

    public TrajectoryManager()
    {
        drones = new Dictionary<string, DroneTrajectory>();
        trajectoryGenerator = new TrajectoryGenerator(1000);
        currentStateIndex = 0;
        // trajectoryPoints = new DroneTrajectory(new DroneState(transform.position));
        // trajectoryPoints.addTrajectory(GenerateLinearTrajectory(trajectoryPoints[trajectoryPoints.Count - 1].Position, new Vector3(3,3,3), 50, 10, new TimeSpan(0, 0, 30, 0, 500)));
        // trajectoryPoints.addTrajectory(GenerateLinearTrajectory(trajectoryPoints[trajectoryPoints.Count - 1].Position, new Vector3(-3,3,3), 50, 10, new TimeSpan(0, 0, 30, 0, 500)));
    }

    public void LoadTrajectories(string fileDate)
    {
        IEnumerable<string> lines = new List<string>
        {
            "00:00:00 1 set-position 0 0 0",
            "00:00:01 1 take-off 2",
            "00:00:10 1 fly-to 3 3 3 120 10",
            "00:00:20 1 fly-to -3 -3 3 240 10",
            "00:00:30 1 fly-to 2 2 3 360 10"
        };
        _flightProgramParser = new FlightProgramParser(lines);


        while (true) {
            (TimeSpan timestamp, string droneId, Command cmd, List<object> cmdArguments) = _flightProgramParser.NextCommand();

            if (droneId == "-1") {
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
