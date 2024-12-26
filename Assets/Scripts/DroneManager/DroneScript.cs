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
    private int currentPositionIndex = 0;
    private float startTime;
    private List<GizmosState> markerPositions = new List<GizmosState>();

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
        currentPositionIndex++;
        AddMarker(transform.position);

        // for debugging:
        // if (currentState.Time % 10000 == 0) {
        //     float elapsedTime = Time.time - startTime;
        //     Debug.Log($"Elapsed time: {elapsedTime} seconds");
        // }
    }

    private void AddMarker(Vector3 position)
    {
        int idHash = id.GetHashCode();
        Color color = Utilities.GetContrastColor(idHash);

        GUIStyle style = new GUIStyle();
        #if UNITY_EDITOR
        style.fontSize = 52;
        style.normal.textColor = color;
        #endif
        string text = $"({position.x:F2}, {position.y:F2}, {position.z:F2})";

        markerPositions.Add(new GizmosState(position, color, text, style));
    }

    private void OnDrawGizmos()
    {
        int i = 0;
        foreach (GizmosState marker in markerPositions)
        {
            if (i % 100 == 0) {
                Handles.Label(marker.Position, marker.Text, marker.Style);
            }
            Gizmos.color = marker.Color;
            Gizmos.DrawSphere(marker.Position, 0.05f); // Draw small red spheres
            i++;
        }
    }
}
