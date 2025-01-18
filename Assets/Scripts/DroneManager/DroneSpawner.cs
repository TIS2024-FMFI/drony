using System.Collections.Generic;
using UnityEngine;

public class DroneSpawner : MonoBehaviour
{
    public static DroneSpawner Instance { get; private set; }
    public static List<GameObject> drones = new List<GameObject>();
    [SerializeField] private GameObject dronePrefab; // Drag the drone prefab in the Inspector
    [SerializeField] private Transform dronesParent; // Optional: Parent object to keep hierarchy organized

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }

    public void SpawnDrone(string droneId)
    {
        if (dronePrefab == null)
        {
            Debug.LogError("Drone prefab is not assigned!");
            return;
        }

        // Instantiate the drone
        GameObject newDrone = Instantiate(dronePrefab, dronesParent);
        newDrone.name = droneId; // Optional: Rename the drone for clarity

        // Initialize the DroneScript
        DroneScript droneScript = newDrone.GetComponentInChildren<DroneScript>();
        if (droneScript != null)
        {
            droneScript.Initialize(droneId);
        }
        else
        {
            Debug.LogError("DroneScript is missing on the prefab!");
        }
        Color randomColor = new Color(Random.value, Random.value, Random.value);
        droneScript?.ActivateLight(randomColor);
        Debug.Log($"Drone {droneId} has been spawned.");
        drones.Add(newDrone);
    }
    
    public static void DestroyAllDrones()
    {
        foreach (GameObject drone in drones)
        {
            Destroy(drone);
        }
    }

    public static void ActivateAllDroneLights()
    {
        foreach (GameObject drone in drones)
        {
            DroneScript droneScript = drone.GetComponentInChildren<DroneScript>();
            if (droneScript != null)
            {
                droneScript.ActivateLight(Color.blue); // Example: Turn on all lights as blue
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ActivateAllDroneLights();
        }
    }
}
