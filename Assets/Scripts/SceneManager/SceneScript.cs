using System.Collections.Generic;
using UnityEngine;

public class SceneScript : MonoBehaviour
{
    public TrajectoryManager trajectoryManager;

    private void Awake()
    {
        Debug.Log("I am awake!");
        trajectoryManager = new TrajectoryManager();
        trajectoryManager.LoadTrajectories("not fully implemented");
        var drones = Object.FindObjectsByType<DroneScript>(FindObjectsSortMode.None);

        // starting with id == 1, it initialize each drone with the corresponding id.
        int currentId = 1;
        foreach (var drone in drones)
        {
            drone.Initialize(trajectoryManager, currentId.ToString());
            currentId++;
        }
        
    }
}