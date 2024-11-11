using System;
using System.Collections.Generic;
using UnityEngine;
using Entities;

public class TrajectoryGenerator : MonoBehaviour
{
    int resolution = 240; // Would be enough to slowdown animation 4 times (0.25x)
    int index = 0;

    private DroneTrajectory trajectoryPoints;    // List of points representing the trajectory

    void Start()
    {
        trajectoryPoints = new DroneTrajectory(new DroneState(transform.position));
        trajectoryPoints.addTrajectory(GenerateLinearTrajectory(trajectoryPoints[trajectoryPoints.Count - 1].Position, new Vector3(3,3,3), 50, 10, new TimeSpan(0, 0, 30, 0, 500)));
        trajectoryPoints.addTrajectory(GenerateLinearTrajectory(trajectoryPoints[trajectoryPoints.Count - 1].Position, new Vector3(-3,3,3), 50, 10, new TimeSpan(0, 0, 30, 0, 500)));
    }

    private List<Vector3> GenerateLinearTrajectory(Vector3 startPosition, Vector3 destinationPosition, int destinationYaw, int speed, TimeSpan time)
    {
        List<Vector3> trajectory = new List<Vector3>();
        float distance = Vector3.Distance(startPosition, destinationPosition);
        float totalTime = distance / speed;
        int totalFrames = Mathf.CeilToInt(totalTime * resolution);

        // interpolation between start and destination vectors
        for (int i = 0; i <= totalFrames; i++)
        {
            float t = i / (float)totalFrames;
            Vector3 point = Vector3.Lerp(startPosition, destinationPosition, t);
            trajectory.Add(point);
        }
        return trajectory;
    }

    // private void TriggerUnity() {
    //     return;
    // }


    public List<Vector3> GenerateCircularTrajectory(float radius, int numPoints)
    {
        List<Vector3> points = new List<Vector3>();
        
        // Calculate points along the circle
        for (int i = 0; i < numPoints; i++)
        {
            float angle = i * Mathf.PI * 2 / numPoints;  // Divide circle into equal segments
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            points.Add(new Vector3(x, 0, z));            // Add point on XZ plane
        }
        
        return points;
    }


    public Vector3 GetPositionAtTime(int playbackSpeed)
    {
        index += playbackSpeed;
        if (index >= trajectoryPoints.Count) {
            return trajectoryPoints[trajectoryPoints.Count-1].Position;
        }
        return trajectoryPoints[index].Position;
    }

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
