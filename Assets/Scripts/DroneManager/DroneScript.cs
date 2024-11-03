using System.Collections.Generic;
using UnityEngine;

public class DroneScript : MonoBehaviour
{
    public List<Vector3> positions;
    private int currentPositionIndex = 0;

    public float speed;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (positions.Count > 0 && currentPositionIndex < positions.Count)
        {
            // Move the object towards the next position
            transform.position = Vector3.MoveTowards(transform.position, positions[currentPositionIndex], speed * Time.deltaTime);

            // Check if the object has reached the current target position
            if (Vector3.Distance(transform.position, positions[currentPositionIndex]) < 0.01f)
            {
                currentPositionIndex++;
            }
        }
    }
}
