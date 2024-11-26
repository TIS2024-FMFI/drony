using UnityEngine;
using UnityEngine.UIElements;

public class PlaybackSpeedController : MonoBehaviour
{
    private readonly float[] speedValues = { 0.25f, 0.5f, 0.75f, 1f, 1.5f, 1.75f, 2f };
    private Label speedLabel;
    private SliderInt playbackSlider;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // refs na ui elementy
        speedLabel = root.Q<Label>("speed-label");
        playbackSlider = root.Q<SliderInt>("playback-slider");

        // Nastav slider rozsah
        playbackSlider.lowValue = 0;
        playbackSlider.highValue = speedValues.Length - 1;

        UpdateSpeedLabel(playbackSlider.value);

        // Pripoj sa k udalosti zmeny hodnoty slideru
        playbackSlider.RegisterValueChangedCallback(evt =>
        {
            UpdateSpeedLabel(evt.newValue);
        });
    }

    void UpdateSpeedLabel(int sliderValue)
    {
        float speed = speedValues[sliderValue];
        speedLabel.text = $"Speed: {speed}x";

        //zmeni speed v buducnosti
    }
}
