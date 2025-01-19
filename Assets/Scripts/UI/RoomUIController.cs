using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

public class RoomUIController : MonoBehaviour
{
    public RoomController roomController;
    public DroneSpawner droneSpawner;

    private FloatField widthInput;
    private FloatField depthInput;
    private FloatField heightInput;

    private DropdownField wallSelector;
    private Slider colorSliderRed;
    private Slider colorSliderGreen;
    private Slider colorSliderBlue;

    private FloatField droneWidthMultiplier;
    private FloatField droneHeightMultiplier;
    private FloatField droneDepthMultiplier;
    private VisualElement droneTogglesContainer;
    private Dictionary<string, Toggle> droneToggles;


    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        widthInput = root.Q<FloatField>("width-input");
        depthInput = root.Q<FloatField>("depth-input");
        heightInput = root.Q<FloatField>("height-input");

        widthInput.RegisterValueChangedCallback(evt => UpdateRoomSize());
        depthInput.RegisterValueChangedCallback(evt => UpdateRoomSize());
        heightInput.RegisterValueChangedCallback(evt => UpdateRoomSize());

        wallSelector = root.Q<DropdownField>("wall-selector");
        colorSliderRed = root.Q<Slider>("color-slider-red");
        colorSliderGreen = root.Q<Slider>("color-slider-green");
        colorSliderBlue = root.Q<Slider>("color-slider-blue");
        wallSelector.choices = new System.Collections.Generic.List<string> {
            "Wall1",
            "Wall2",
            "Wall3",
            "Wall4",
            "Floor",
            "Roof"
        };
        wallSelector.value = "Wall1";


        colorSliderRed.RegisterValueChangedCallback(evt => UpdateWallColor());
        colorSliderGreen.RegisterValueChangedCallback(evt => UpdateWallColor());
        colorSliderBlue.RegisterValueChangedCallback(evt => UpdateWallColor());

        droneWidthMultiplier = root.Q<FloatField>("drone-width");
        droneHeightMultiplier = root.Q<FloatField>("drone-height");
        droneDepthMultiplier = root.Q<FloatField>("drone-depth");
        droneWidthMultiplier.value = 1;
        droneHeightMultiplier.value = 1;
        droneDepthMultiplier.value = 1;

        droneWidthMultiplier.RegisterValueChangedCallback(evt => UpdateSelectedDronesScale());
        droneHeightMultiplier.RegisterValueChangedCallback(evt => UpdateSelectedDronesScale());
        droneDepthMultiplier.RegisterValueChangedCallback(evt => UpdateSelectedDronesScale());
        droneTogglesContainer = root.Q<VisualElement>("drone-toggles");
        droneToggles = new Dictionary<string, Toggle>();

        PopulateDroneToggles();

    }

    private void UpdateRoomSize()
    {
        float width = widthInput.value;
        float height = heightInput.value;
        float depth = depthInput.value;

        // Validácia hodnôt
        if (width <= 10 || width > 500)
        {
            width = 30;
        }
        if (height <= 10 || height > 500)
        {
            height = 12;
        }
        if (depth <= 10 || depth > 500)
        {
            depth = 30;
        }

        Debug.Log($"Width: {width}, Height: {height}, Depth: {depth}");

 
        roomController.ResizeRoom(width, depth, height);
    }

    private void UpdateWallColor()
    {
        float red = colorSliderRed.value;
        float green = colorSliderGreen.value;
        float blue = colorSliderBlue.value;


        Color newColor = new Color(red, green, blue);

        Debug.Log($"Updating wall color to: R={red}, G={green}, B={blue}");

        switch (wallSelector.value)
        {
            case "Wall1":
                roomController.ChangeWallColor(roomController.wall1, newColor);
                break;
            case "Wall2":
                roomController.ChangeWallColor(roomController.wall2, newColor);
                break;
            case "Wall3":
                roomController.ChangeWallColor(roomController.wall3, newColor);
                break;
            case "Wall4":
                roomController.ChangeWallColor(roomController.wall4, newColor);
                break;
            case "Floor":
                roomController.ChangeWallColor(roomController.floor, newColor);
                break;
            case "Roof":
                roomController.ChangeWallColor(roomController.roof, newColor);
                break;
        }
    }

    private void PopulateDroneToggles()
    {
        if (droneTogglesContainer == null)
        {
            Debug.LogError("droneTogglesContainer is null. Ensure the VisualElement with name 'drone-toggles' exists in the UXML and is properly referenced.");
            return;
        }

        droneTogglesContainer.Clear();
        droneToggles.Clear();
        Debug.Log("Cleared toggles...");
        foreach (var drone in DroneSpawner.drones)
        {
            Debug.Log("drones iterated...");
            if (drone != null)
            {
                string droneName = drone.name;
                Toggle toggle = new Toggle { label = droneName, value = true };
                droneTogglesContainer.Add(toggle);
                droneToggles[droneName] = toggle;
            }
        }
        Debug.Log("PopulateDroneToggles done...");
    }


    private void UpdateSelectedDronesScale()
    {
        float width = Mathf.Clamp(droneWidthMultiplier.value, 0.1f, 20f);
        float height = Mathf.Clamp(droneHeightMultiplier.value, 0.1f, 20f);
        float depth = Mathf.Clamp(droneDepthMultiplier.value, 0.1f, 20f);

        foreach (var drone in DroneSpawner.drones)
        {
            if (drone != null && droneToggles.TryGetValue(drone.name, out Toggle toggle) && toggle.value)
            {
                drone.transform.localScale = new Vector3(width, height, depth);
                Debug.Log($"Updated scale of {drone.name} to: Width={width}, Height={height}, Depth={depth}");
            }
        }
    }

    public void RefreshDroneToggles()
    {
        Debug.Log("Populating drone toggles...");
        PopulateDroneToggles();
    }


}
