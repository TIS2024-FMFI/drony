using System;
using System.Collections.Generic; 
using Drony.Entities;
using UnityEngine;
using Interpreter;

/// <summary>
/// Class <c>TrajectoryManager</c> manages logic for parser and generator 
/// to create trajectories for each drone. It inherits from MonoBehaviour for drawing gizmos.
/// </summary>
public class TrajectoryManager
{
    private Dictionary<string, DroneTrajectory> drones;
    private TrajectoryGenerator trajectoryGenerator;
    private FlightProgramParser _flightProgramParser;

    public TrajectoryManager()
    {
        drones = new Dictionary<string, DroneTrajectory>();
        drones.Add("1", new DroneTrajectory("1"));
        drones.Add("2", new DroneTrajectory("2"));
        trajectoryGenerator = new TrajectoryGenerator(1000);
        // trajectoryPoints = new DroneTrajectory(new DroneState(transform.position));
        // trajectoryPoints.addTrajectory(GenerateLinearTrajectory(trajectoryPoints[trajectoryPoints.Count - 1].Position, new Vector3(3,3,3), 50, 10, new TimeSpan(0, 0, 30, 0, 500)));
        // trajectoryPoints.addTrajectory(GenerateLinearTrajectory(trajectoryPoints[trajectoryPoints.Count - 1].Position, new Vector3(-3,3,3), 50, 10, new TimeSpan(0, 0, 30, 0, 500)));
    }

    public void LoadTrajectories(string fileDate)
    {
        // FIXME: just for testing, cause we doesn't have Flight Program Interpreter implemented yet:
        string droneId = "1";
        Vector3 startPosition1 = new Vector3(0,0,0);
        Vector3 destinationPosition1 = new Vector3(3,3,3);
        int startYaw1 = 0;
        int destinationYaw1 = 110;
        int speed1 = 2; 

        Vector3 destinationPosition2 = new Vector3(-3,3,3);
        int destinationYaw2 = 210;
        int speed2 = 2; 

        Vector3 destinationPosition3 = new Vector3(0,0,0);
        int destinationYaw3 = 0;
        int speed3 = 2; 
        
        DroneTrajectory firstDroneTrajectory = drones[droneId];
        firstDroneTrajectory.addTrajectory(trajectoryGenerator.GenerateLinearTrajectory(startPosition1, destinationPosition1, startYaw1, destinationYaw1, speed1, 0));
        firstDroneTrajectory.addTrajectory(trajectoryGenerator.GenerateLinearTrajectory(destinationPosition1, destinationPosition2, destinationYaw1, destinationYaw2, speed2, 2000));
        firstDroneTrajectory.addTrajectory(trajectoryGenerator.GenerateLinearTrajectory(destinationPosition2, destinationPosition3, destinationYaw2, destinationYaw3, speed3, 4000));
        
    
        DroneTrajectory secondDroneTrajectory = drones["2"];
        secondDroneTrajectory.addTrajectory(trajectoryGenerator.GenerateLinearTrajectory(new Vector3(1,1,1), new Vector3(2,2,2), 0, 110, 2, 0));
        secondDroneTrajectory.addTrajectory(trajectoryGenerator.GenerateLinearTrajectory(new Vector3(2,2,2), new Vector3(-2,2,2), 110, 210, 2, 2000));
        secondDroneTrajectory.addTrajectory(trajectoryGenerator.GenerateLinearTrajectory(new Vector3(-2,2,2), new Vector3(1,1,1), 210, 0, 2, 4000));
        

    }
    public DroneState GetStateAtTime(int playbackSpeed, string droneId)
    {
        // TODO: there probably would be better to set the position to last state, when the trajectory
        // has been iterated
        try {
            return drones[droneId].getNext(playbackSpeed);
        } catch(ArgumentOutOfRangeException) {
            // FIXME: ONLY for testing, i am reseting index, so the animation will loop
            drones[droneId].CurrentStateIndex = 0;
            return drones[droneId].getNext(playbackSpeed);
        }
        
    }
}
