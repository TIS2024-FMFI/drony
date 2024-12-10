using System.Collections.Generic;
using Drony.Entities;
using UnityEngine;

public class DroneScript : MonoBehaviour
{
    private TrajectoryManager trajectoryManager;
    private string id;
    private int currentPositionIndex = 0;
    private float startTime;

    public float speed;

    public void Initialize(string droneId)
    {
        Debug.Log($"Initializing drone with id: {droneId}");
        trajectoryManager = TrajectoryManager.Instance;
        id = droneId;
    }


    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void FixedUpdate() // 100fps should be
    {
        DroneState currentState = trajectoryManager.GetStateAtTime(1, id);
        transform.position = currentState.Position;
        transform.rotation = currentState.YawAngle;
        // if (currentState.Time % 10000 == 0) {
        //     float elapsedTime = Time.time - startTime;
        //     Debug.Log($"Elapsed time: {elapsedTime} seconds");
        // }
        currentPositionIndex++;
        

    }
}
