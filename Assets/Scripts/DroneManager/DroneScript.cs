using System.Collections.Generic;
using Drony.Entities;
using UnityEngine;
using UnityEngine.UIElements;
using Utility;


// allows to draw text labels in editor
// TODO: bring this feature to production as well
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DroneScript : MonoBehaviour
{
    private TrajectoryManager trajectoryManager;
    private string id;
    private int currentTime;
    private float startTime;
    private List<GizmosState> markerPositions = new List<GizmosState>();
    private Color color;
    private GUIStyle style;
    private Vector3 previousBezierPoint;
    private bool previousBezierPointSet = false;
    private DroneLightController lightController;

    public float speed;

    public void Initialize(string droneId)
    {
        Debug.Log($"Initializing drone with id: {droneId}");
        trajectoryManager = TrajectoryManager.Instance;
        id = droneId;
        lightController = GetComponentInChildren<DroneLightController>();
    }


    void Start()
    {
        currentTime = 0;
        startTime = Time.time;
        int idHash = id.GetHashCode();
        color = Utilities.GetContrastColor(idHash);
        style = new GUIStyle();

        #if UNITY_EDITOR
        style.fontSize = 52;
        style.normal.textColor = color;
        #endif
    }

    // Update is called once per frame
    void FixedUpdate() // 100fps should be
    {
        DroneState currentState = trajectoryManager.GetCurrentDroneState(id);
        currentTime = trajectoryManager.GetCurrentTime();
        transform.position = currentState.Position;
        transform.rotation = currentState.YawAngle;
        currentTime = currentState.Time;
        markerPositions.Add(new GizmosState(transform.position, color, "", currentTime, style));
        if (currentState.Color != color)
        {
            Debug.Log($"Drone {id}: Checking light activation. Current: {color}, New: {currentState.Color}");
            ActivateLight(currentState.Color);
        }
        color = currentState.Color;
    }

    public void ActivateLight(Color newColor)
    {
        if (lightController != null)
        {
            lightController.ChangeLightColor(newColor);
            Debug.Log($"Drone {id}: Light activated with color {newColor}");
        }
    }

    private void OnDrawGizmos()
    {
        // foreach (DroneState droneState in trajectoryManager.GetKeyStates(id)) 
        // {
        //     if (currentTime < droneState.Time) {
        //         break;
        //     }
        //     Vector3 position = droneState.Position;
        //     string text = $"({position.x:F2}, {position.y:F2}, {position.z:F2})";
        //     // Handles.Label(position, text, style);
        //     Gizmos.DrawSphere(position, 0.1f);
        // }
        // foreach(Vector3 position in trajectoryManager.GetBezierPoints())
        // {
        //     string text = $"({position.x:F2}, {position.y:F2}, {position.z:F2})";
        //     GUIStyle styleBezier = new GUIStyle(style);
        //     styleBezier.normal.textColor = Color.red;
        //     // Handles.Label(position, text, styleBezier);
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawSphere(position, 0.05f);

        //     if (!previousBezierPointSet)
        //     {
        //         previousBezierPoint = position;
        //         previousBezierPointSet = true;
        //     } 
        //     else 
        //     {
        //         Gizmos.DrawLine(previousBezierPoint, position);
        //         previousBezierPointSet = false;
        //     }
        // }
        foreach (GizmosState marker in markerPositions)
        {
            if (currentTime < marker.Time) {
                break;
            }
            Gizmos.color = marker.Color;
            Gizmos.DrawSphere(marker.Position, 0.05f); // Draw small red spheres
        }
    }
}
