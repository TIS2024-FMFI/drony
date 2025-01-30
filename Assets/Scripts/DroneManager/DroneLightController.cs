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
            droneLight.color = Color.white;
            droneLight.type = LightType.Point;
            droneLight.intensity = 0.1f; 
            droneLight.range = 0.1f;
            droneLight.shadows = LightShadows.Soft;
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
