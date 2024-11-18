using System;
using System.Collections.Generic; 
using Drony.Entities;
using UnityEngine;
/// <summary>
/// Class <c>TrajectoryManager</c> manages logic for parser and generator 
/// to create trajectories for each drone. It inherits from MonoBehaviour for drawing gizmos.
/// </summary>
public class TrajectoryManager
{
    private Dictionary<string, DroneTrajectory> drones;
    private TrajectoryGenerator trajectoryGenerator;

    public TrajectoryManager()
    {
        drones = new Dictionary<string, DroneTrajectory>();
        drones.Add("1", new DroneTrajectory("1"));
        trajectoryGenerator = new TrajectoryGenerator(240);
        // trajectoryPoints = new DroneTrajectory(new DroneState(transform.position));
        // trajectoryPoints.addTrajectory(GenerateLinearTrajectory(trajectoryPoints[trajectoryPoints.Count - 1].Position, new Vector3(3,3,3), 50, 10, new TimeSpan(0, 0, 30, 0, 500)));
        // trajectoryPoints.addTrajectory(GenerateLinearTrajectory(trajectoryPoints[trajectoryPoints.Count - 1].Position, new Vector3(-3,3,3), 50, 10, new TimeSpan(0, 0, 30, 0, 500)));
    }

    public void LoadTrajectories(string filename)
    {
        // FIXME: just for testing, cause we doesn't have Flight Program Interpreter implemented yet:
        string droneId = "1";
        Vector3 startPosition1 = new Vector3(0,0,0);
        Vector3 destinationPosition1 = new Vector3(3,3,3);
        int destinationYaw1 = 90; 
        int speed1 = 2; 

        Vector3 destinationPosition2 = new Vector3(-3,3,3);
        int destinationYaw2 = 90; 
        int speed2 = 2; 

        Vector3 destinationPosition3 = new Vector3(0,0,0);
        int destinationYaw3 = 90; 
        int speed3 = 2; 
        
        DroneTrajectory firstDroneTrajectory = drones[droneId];
        firstDroneTrajectory.addTrajectory(trajectoryGenerator.GenerateLinearTrajectory(startPosition1, destinationPosition1, destinationYaw1, speed1));
        firstDroneTrajectory.addTrajectory(trajectoryGenerator.GenerateLinearTrajectory(destinationPosition1, destinationPosition2, destinationYaw2, speed2));
        firstDroneTrajectory.addTrajectory(trajectoryGenerator.GenerateLinearTrajectory(destinationPosition2, destinationPosition3, destinationYaw3, speed3));


    }
    public Vector3 GetPositionAtTime(int playbackSpeed, string droneId)
    {
        // TODO: there probably would be better to set the position to last state, when the trajectory
        // has been iterated
        try {
            return drones[droneId].getNext(playbackSpeed).Position;
        } catch(ArgumentOutOfRangeException) {
            // FIXME: ONLY for testing, i am reseting index, so the animation will loop
            drones[droneId].CurrentStateIndex = 0;
            return drones[droneId].getNext(playbackSpeed).Position;
        }
        
    }

    // TODO: add gizmos
    // private void OnDrawGizmos()
    // {
    //     if (trajectoryPoints == null) return;

    //     Gizmos.color = Color.red;
    //     for (int i = 0; i < trajectoryPoints.Count; i++)
    //     {
    //         Vector3 currentPoint = transform.position + trajectoryPoints[i];
    //         Vector3 nextPoint = transform.position + trajectoryPoints[(i + 1) % trajectoryPoints.Count];
    //         Gizmos.DrawLine(currentPoint, nextPoint);
    //     }
    // }
}