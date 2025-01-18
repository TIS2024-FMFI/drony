using UnityEngine;

public class DroneLightController : MonoBehaviour
{
    public Light droneLight;

    private void Start()
    {
        if (droneLight == null)
        {
            droneLight = GetComponentInChildren<Light>();
        }

        if (droneLight != null)
        {
            droneLight.enabled = false;
        }
    }

    public void ChangeLightColor(Color newColor)
    {
        if (droneLight != null)
        {
            droneLight.color = newColor;
            droneLight.enabled = true;
        }
    }
}
