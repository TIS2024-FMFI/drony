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

    public float speed;

    public void Initialize(string droneId)
    {
        Debug.Log($"Initializing drone with id: {droneId}");
        trajectoryManager = TrajectoryManager.Instance;
        id = droneId;
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
        DroneState currentState = trajectoryManager.GetStateAtTime(1, id);
        transform.position = currentState.Position;
        transform.rotation = currentState.YawAngle;
        currentTime = currentState.Time;
        AddMarker(transform.position, currentState.IsKeyState);
        // for debugging:
        // if (currentState.IsKeyState) {
        //     float elapsedTime = Time.time - startTime;
        //     Debug.Log($"Estimated: {currentState.Time}. Elapsed: {elapsedTime}");
        // }
    }

    private void AddMarker(Vector3 position, bool isKeyState)
    {
        markerPositions.Add(new GizmosState(position, color, "", style));
    }

    private void OnDrawGizmos()
    {
        foreach (DroneState droneState in trajectoryManager.GetKeyStates(id)) 
        {
            if (currentTime < droneState.Time) {
                break;
            }
            Vector3 position = droneState.Position;
            string text = $"({position.x:F2}, {position.y:F2}, {position.z:F2})";
            // Handles.Label(position, text, style);
            //Gizmos.DrawSphere(position, 0.1f);
        }
        foreach(Vector3 position in trajectoryManager.GetBezierPoints())
        {
            string text = $"({position.x:F2}, {position.y:F2}, {position.z:F2})";
            GUIStyle styleBezier = new GUIStyle(style);
            styleBezier.normal.textColor = Color.red;
            // Handles.Label(position, text, styleBezier);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(position, 0.05f);

            if (!previousBezierPointSet)
            {
                previousBezierPoint = position;
                previousBezierPointSet = true;
            } 
            else 
            {
                Gizmos.DrawLine(previousBezierPoint, position);
                previousBezierPointSet = false;
            }
        }
        foreach (GizmosState marker in markerPositions)
        {
            Gizmos.color = marker.Color;
            Gizmos.DrawSphere(marker.Position, 0.05f); // Draw small red spheres
        }
    }
}
