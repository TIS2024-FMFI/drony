using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class RoomUIController : MonoBehaviour
{
    public RoomController roomController;

    private FloatField widthInput;
    private FloatField depthInput;
    private FloatField heightInput;

    private DropdownField wallSelector;
    private Slider colorSliderRed;
    private Slider colorSliderGreen;
    private Slider colorSliderBlue;


    void Start()
    {
        // Získanie root elementu (pripojený cez UIDocument)
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Načítanie FloatField prvkov podľa `name` z UXML
        widthInput = root.Q<FloatField>("width-input");
        depthInput = root.Q<FloatField>("depth-input");
        heightInput = root.Q<FloatField>("height-input");

        // Prepojenie zmien s callbackom
        widthInput.RegisterValueChangedCallback(evt => UpdateRoomSize());
        depthInput.RegisterValueChangedCallback(evt => UpdateRoomSize());
        heightInput.RegisterValueChangedCallback(evt => UpdateRoomSize());

        // Načítanie DropdownField a ColorField
        wallSelector = root.Q<DropdownField>("wall-selector");
        colorSliderRed = root.Q<Slider>("color-slider-red");
        colorSliderGreen = root.Q<Slider>("color-slider-green");
        colorSliderBlue = root.Q<Slider>("color-slider-blue");

        // Pridanie možností pre DropdownField
        wallSelector.choices = new System.Collections.Generic.List<string> {
            "Wall1", // Predná stena
            "Wall2", // Pravá stena
            "Wall3", // Zadná stena
            "Wall4", // Ľavá stena
            "Floor", // Podlaha
            "Roof"   // Strop
        };
        wallSelector.value = "Wall1"; // Predvolená hodnota

        // Prepojenie zmien farby s callbackom
        colorSliderRed.RegisterValueChangedCallback(evt => UpdateWallColor());
        colorSliderGreen.RegisterValueChangedCallback(evt => UpdateWallColor());
        colorSliderBlue.RegisterValueChangedCallback(evt => UpdateWallColor());
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

        // Zmena veľkosti miestnosti
        roomController.ResizeRoom(width, depth, height);
    }

    private void UpdateWallColor()
    {
        // Získanie RGB hodnôt zo sliderov
        float red = colorSliderRed.value;
        float green = colorSliderGreen.value;
        float blue = colorSliderBlue.value;

        // Vytvorenie novej farby
        Color newColor = new Color(red, green, blue);

        Debug.Log($"Updating wall color to: R={red}, G={green}, B={blue}");

        // Zmena farby na vybranom povrchu
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
}
