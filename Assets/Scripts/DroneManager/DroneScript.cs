using System.Collections.Generic;
using Drony.Entities;
using UnityEngine;

public class DroneScript : MonoBehaviour
{
    private TrajectoryManager trajectoryManager;
    private string id;
    private int currentPositionIndex = 0;

    public float speed;

    public void Initialize(TrajectoryManager manager, string droneId)
    {
        Debug.Log($"Initializing drone with id: {droneId}");
        trajectoryManager = manager;
        id = droneId;
    }


    void Start()
    {
        // trajectoryGenerator = gameObject.GetComponent<TrajectoryGenerator>();
        // if (trajectoryGenerator == null) {
        //     trajectoryGenerator = gameObject.AddComponent<TrajectoryGenerator>();
        // }
    }

    // Update is called once per frame
    void FixedUpdate() // 100fps should be
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
        DroneState currentState = trajectoryManager.GetStateAtTime(4, id);
        transform.position = currentState.Position;
        transform.rotation = currentState.YawAngle;
    }
}
