using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FreeMovement : MonoBehaviour
{
    public float moveSpeed = 5f;         // Speed for movement
    public float lookSpeed = 2f;        // Speed for looking around
    public float zoomSpeed = 2f;        // Speed for zooming in/out
    public float minZoomDistance = 2f;  // Minimum zoom distance
    public float maxZoomDistance = 20f; // Maximum zoom distance
    public float minFOV = 15f;          // Minimum FOV (narrow view, zoomed in)
    public float maxFOV = 60f;          // Maximum FOV (wide view, zoomed out)

    private float yaw = 0f;         // Current yaw of the camera
    private float pitch = 0f;       // Current pitch of the camera
    private float zoomLevel;      // Current zoom level (distance from origin)
    private Camera cameraComponent;

    private void Start()
    {
        // Cache the Camera component
        cameraComponent = GetComponent<Camera>();
        if (cameraComponent == null)
        {
            Debug.LogError("No Camera component found! Attach this script to a Camera object.");
        }
        float fovNormalized = (cameraComponent.fieldOfView - minFOV) / (maxFOV - minFOV);   // Initial FOV
        zoomLevel = Mathf.Lerp(minZoomDistance, maxZoomDistance, fovNormalized);        
    }

    private void Update()
    {

        // Only handle looking when RMB is pressed
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * lookSpeed;
            pitch -= Input.GetAxis("Mouse Y") * lookSpeed;
            pitch = Mathf.Clamp(pitch, -90f, 90f); // Prevent flipping

            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        }

        // Handle movement
        float moveForward = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        float moveRight = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveUp = 0f;

        if (Input.GetKey(KeyCode.Space)) moveUp = moveSpeed * Time.deltaTime; // Ascend
        if (Input.GetKey(KeyCode.LeftShift)) moveUp = -moveSpeed * Time.deltaTime; // Descend

        transform.Translate(moveRight, moveUp, moveForward);

        // Handle zoom
        Zoom();
    }

    private void Zoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0 && cameraComponent != null)
        {
            // Update zoom level and clamp it
            zoomLevel -= scrollInput * zoomSpeed;
            zoomLevel = Mathf.Clamp(zoomLevel, minZoomDistance, maxZoomDistance);

            // Adjust the FOV based on zoom level
            float t = (zoomLevel - minZoomDistance) / (maxZoomDistance - minZoomDistance); // Normalize
            cameraComponent.fieldOfView = Mathf.Lerp(minFOV, maxFOV, t);
        }
    }
}
