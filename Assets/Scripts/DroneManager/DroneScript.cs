using System.Collections.Generic;
using UnityEngine;

public class DroneScript : MonoBehaviour
{
    public List<Vector3> positions;
    private TrajectoryGenerator trajectoryGenerator;
    private int currentPositionIndex = 0;

    public float speed;

    void Start()
    {
        trajectoryGenerator = gameObject.GetComponent<TrajectoryGenerator>();
        if (trajectoryGenerator == null) {
            trajectoryGenerator = gameObject.AddComponent<TrajectoryGenerator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // if (positions.Count > 0 && currentPositionIndex < positions.Count)
        // {
        //     //Debug.Log(positions[currentPositionIndex].x + " " + positions[currentPositionIndex].y + " " + positions[currentPositionIndex].z);
        //     Debug.Log(positions[currentPositionIndex]);
        //     // Move the object towards the next position
        //     transform.position = Vector3.MoveTowards(transform.position, positions[currentPositionIndex], speed * Time.deltaTime);

        //     // Check if the object has reached the current target position
        //     if (Vector3.Distance(transform.position, positions[currentPositionIndex]) < 0.01f)
        //     {
        //         currentPositionIndex++;
        //     }
        // }
        transform.position = trajectoryGenerator.GetPositionAtTime(Time.time);
    }
}
