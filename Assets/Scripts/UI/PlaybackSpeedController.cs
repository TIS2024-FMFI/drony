using UnityEngine;
using UnityEngine.UIElements;

public class PlaybackSpeedController : MonoBehaviour
{
    private readonly float[] speedValues = { 0.25f, 0.5f, 0.75f, 1f, 1.5f, 1.75f, 2f };
    private Label speedLabel;
    private SliderInt playbackSlider;
    private TimeBarController timeBarController;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        speedLabel = root.Q<Label>("speed-label");
        playbackSlider = root.Q<SliderInt>("playback-slider");

        timeBarController = FindAnyObjectByType<TimeBarController>();


        playbackSlider.lowValue = 0;
        playbackSlider.highValue = speedValues.Length - 1;

        UpdateSpeedLabel(playbackSlider.value);

        playbackSlider.RegisterValueChangedCallback(evt =>
        {
            UpdateSpeedLabel(evt.newValue);
        });
    }

    void UpdateSpeedLabel(int sliderValue)
    {
        float speed = speedValues[sliderValue];
        speedLabel.text = $"Speed: {speed}x";

        if (timeBarController != null)
        {
            timeBarController.SetTimeMultiplier(speed);
        }
    }
}
