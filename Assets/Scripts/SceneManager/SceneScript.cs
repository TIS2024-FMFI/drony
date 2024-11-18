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
        Debug.Log(drones);
        foreach (var drone in drones)
        {
            drone.Initialize(trajectoryManager);
        }
    }
}
