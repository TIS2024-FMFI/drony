using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneLightController : MonoBehaviour
{
    private List<Material> diodMaterials;
    private Renderer diodRenderer;

    private void Start()
    {
        diodMaterials = new List<Material>();
        
        diodRenderer = GetComponent<Renderer>();

        if (diodRenderer != null)
        {
            foreach (Material mat in diodRenderer.materials)
            {
                diodMaterials.Add(mat);
            }
        }
        

        foreach (Material mat in diodMaterials)
        {
            mat.color = Color.white;
            mat.SetColor("_EmissionColor", Color.white);
            mat.EnableKeyword("_EMISSION");
        }
    }

    public void ChangeLightColor(Color newColor)
    {
        foreach (Material mat in diodMaterials)
        {
            mat.color = newColor;
            mat.SetColor("_EmissionColor", newColor);
        }
    }
}
