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
                case Command.FlySpiral:
                    drones[droneId] = trajectoryGenerator.GenerateSpiralTrajectory(
                        droneTrajectory,
                        timestampMilliseconds,
                        cmdArguments);
                    break;
                
            }  
            //drones[droneId].setLastAsKeyState();
        }
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
}
