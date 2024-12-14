using UnityEngine;
using UnityEngine.UIElements;

public class TimeBarController : MonoBehaviour
{
    private Label currentTimeLabel;
    private Label totalTimeLabel;
    private ProgressBar timeProgressBar;
    private Button playPauseButton;
    private Button restartButton;

    private float totalTimeLength = 180f; //3 min
    private float currentTime = 0f;
    private bool isTiming = false;
    private float timeMultiplier = 1f;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;


        currentTimeLabel = root.Q<Label>("current-time");
        totalTimeLabel = root.Q<Label>("total-time");
        timeProgressBar = root.Q<ProgressBar>("time-progress");

        playPauseButton = root.Q<Button>("play-pause-button");
        restartButton = root.Q<Button>("restart-button");

        playPauseButton.clicked += TogglePlayPause;
        restartButton.clicked += RestartTimer;

        SetTotalTime(totalTimeLength);
    }

    void Update()
    {
        if (isTiming)
        {
            currentTime += Time.deltaTime * timeMultiplier;
            Debug.Log(currentTime);
            if (currentTime >= totalTimeLength)
            {
                currentTime = totalTimeLength;
                isTiming = false;
                playPauseButton.text = "Play";
            }

            UpdateCurrentTime(currentTime);
            UpdateTimeProgressBar(currentTime, totalTimeLength);
        }
    }

    private void TogglePlayPause()
    {
        Debug.Log("toggle");
        isTiming = !isTiming;
        playPauseButton.text = isTiming ? "Pause" : "Play";
    }

    private void RestartTimer()
    {
        isTiming = false;
        currentTime = 0f;
        UpdateCurrentTime(currentTime);
        UpdateTimeProgressBar(currentTime, totalTimeLength);
        playPauseButton.text = "Play";
    }

    private void SetTotalTime(float lengthInSeconds)
    {
        totalTimeLabel.text = FormatTime(lengthInSeconds);
    }

    private void UpdateCurrentTime(float timeInSeconds)
    {
        currentTimeLabel.text = FormatTime(timeInSeconds);
    }

    private void UpdateTimeProgressBar(float current, float total)
    {
        timeProgressBar.value = (current / total) * 100f;
    }


    public void SetTimeMultiplier(float multiplier)
    {
        timeMultiplier = multiplier;
    }

    private string FormatTime(float timeInSeconds)
    {
        int hours = Mathf.FloorToInt(timeInSeconds / 3600);
        int minutes = Mathf.FloorToInt((timeInSeconds % 3600) / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return $"{hours:D1}:{minutes:D2}:{seconds:D2}";
    }
}
