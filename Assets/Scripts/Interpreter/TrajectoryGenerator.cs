using System.Collections.Generic;
using UnityEngine;

public class TrajectoryGenerator : MonoBehaviour
{
    public float radius = 5f;                  // Radius of the circular path
    public float speed = 200f;                   // Speed of movement along the path
    public int numPoints = 1000;                // Number of points in the trajectory
    public Vector3 center = Vector3.zero;
    private List<Vector3> trajectoryPoints;    // List of points representing the trajectory

    void Start()
    {
        trajectoryPoints = GenerateCircularTrajectory(radius, numPoints);
    }

    /// <summary>
    /// Generates a circular trajectory with a specified radius and number of points.
    /// </summary>
    /// <param name="radius">Radius of the circular trajectory</param>
    /// <param name="numPoints">Number of points to generate along the trajectory</param>
    /// <returns>A list of Vector3 points representing the trajectory</returns>
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

    /// <summary>
    /// Get the position along the trajectory based on time.
    /// </summary>
    /// <param name="time">The current time or frame-based time for interpolation</param>
    /// <returns>The Vector3 position on the trajectory</returns>
    public Vector3 GetPositionAtTime(float time)
    {
        int index = Mathf.FloorToInt((time * speed) % trajectoryPoints.Count);
        return trajectoryPoints[index];
    }

    /// <summary>
    /// Visualize the trajectory in the editor for debugging purposes.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (trajectoryPoints == null) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < trajectoryPoints.Count; i++)
        {
            Vector3 currentPoint = transform.position + trajectoryPoints[i];
            Vector3 nextPoint = transform.position + trajectoryPoints[(i + 1) % trajectoryPoints.Count];
            Gizmos.DrawLine(currentPoint, nextPoint);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(center, 0.1f);
    }
}
