using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class <c>TrajectoryManager</c> provides functions for generating different trajectory
/// </summary>
public class TrajectoryGenerator
{
    private int resolution;

    public TrajectoryGenerator(int resolution = 240) {
        this.resolution = resolution;
    }

    public List<Vector3> GenerateLinearTrajectory(Vector3 startPosition, Vector3 destinationPosition, int destinationYaw, int speed)
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

    public List<Vector3> GenerateCircularTrajectory(float radius, int numPoints)
    {
        List<Vector3> trajectory = new List<Vector3>();
        
        for (int i = 0; i < numPoints; i++)
        {
            float angle = i * Mathf.PI * 2 / numPoints;  
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            trajectory.Add(new Vector3(x, 0, z));            
        }
        
        return trajectory;
    }
}
