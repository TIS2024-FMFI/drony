using System;
using System.Collections.Generic; 
using Drony.Entities;
/// <summary>
/// Class <c>TrajectoryManager</c> manages logic for parser and generator 
/// to create trajectories for each drone
/// </summary>
public class TrajectoryManager  : MonoBehaviour
{
    private int resolution = 240;
    private Dictionary<string, DroneTrajectory> drones;

    void Start()
    {
        drones = new Dictionary<string, DroneTrajectory>();
        drones.Add("1", new DroneTrajectory("1"));
        // trajectoryPoints = new DroneTrajectory(new DroneState(transform.position));
        // trajectoryPoints.addTrajectory(GenerateLinearTrajectory(trajectoryPoints[trajectoryPoints.Count - 1].Position, new Vector3(3,3,3), 50, 10, new TimeSpan(0, 0, 30, 0, 500)));
        // trajectoryPoints.addTrajectory(GenerateLinearTrajectory(trajectoryPoints[trajectoryPoints.Count - 1].Position, new Vector3(-3,3,3), 50, 10, new TimeSpan(0, 0, 30, 0, 500)));
    }

    public void LoadTrajectories(string filename)
    {
        while (true) 
        {
            
        }
    }
    public Vector3 GetPositionAtTime(int playbackSpeed, string droneId)
    {
        // index += playbackSpeed * 4;
        // if (index >= trajectoryPoints.Count) {
        //     return trajectoryPoints[trajectoryPoints.Count-1].Position;
        // }
        // return trajectoryPoints[index].Position;
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