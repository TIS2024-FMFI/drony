using UnityEngine;
using UnityEngine.UIElements;

public class WallSettingsController : MonoBehaviour
{
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        var wallSelector = root.Q<DropdownField>("wall-selector");

        wallSelector.choices = new System.Collections.Generic.List<string>
        {
            "Wall1",
            "Wall2",
            "Wall3",
            "Wall4",
            "Floor",
            "Roof"
        };

        wallSelector.value = "Wall1";

        wallSelector.RegisterValueChangedCallback(evt =>
        {
            Debug.Log($"Selected Surface: {evt.newValue}");
        });
    }
}
