using UnityEngine;
using UnityEngine.UIElements;

public class TimeBarController : MonoBehaviour
{
    private Label currentTimeLabel;
    private Label totalTimeLabel;
    private ProgressBar timeProgressBar;
    private Button playPauseButton;
    private Button restartButton;
    private int totalTimeLength = 180000; //3 minutes
    private int currentTime = 0;
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

    void FixedUpdate()
    {
        
        if (isTiming)
        {
            currentTime = UIManager.Instance.GetCurrentTime();
            if (currentTime >= totalTimeLength)
            {
                UIManager.Instance.SetCurrentTime(totalTimeLength);
                currentTime = UIManager.Instance.GetCurrentTime();
                isTiming = false;
                playPauseButton.text = "Play";
                RestartTimer();
            }
            UIManager.Instance.UpdateCurrentTime();
            UpdateCurrentTime(currentTime);
            UpdateTimeProgressBar(currentTime, totalTimeLength);
        }
        totalTimeLength = UIManager.Instance.GetTotalTime();
        SetTotalTime(totalTimeLength);
    }

    private void TogglePlayPause()
    {
        isTiming = !isTiming;
        playPauseButton.text = isTiming ? "Pause" : "Play";
        UpdateAudioState();
    }

    private void UpdateAudioState()
    {

        if (isTiming)
        {
            AudioManager.Instance.PlayAudio();
        } 
        else 
        {
            AudioManager.Instance.PauseAudio();
        }
    }

    private void RestartTimer()
    {
        isTiming = false;
        currentTime = 0;
        UIManager.Instance.ResetCurrentTime();
        UpdateCurrentTime(currentTime);
        UpdateTimeProgressBar(currentTime, totalTimeLength);
        playPauseButton.text = "Play";

        // Reset audio time and stop
        AudioManager.Instance.StopAudio();
        AudioManager.Instance.SetAudioTime(0f);
    }

    private void SetTotalTime(float lengthInMilliseconds)
    {
        totalTimeLabel.text = FormatTime(lengthInMilliseconds);
    }

    private void UpdateCurrentTime(float lengthInMilliseconds)
    {
        currentTimeLabel.text = FormatTime(lengthInMilliseconds);
    }

    private void UpdateTimeProgressBar(float current, float total)
    {
        timeProgressBar.value = (current / total) * 100f;
    }

    public void SetTimeMultiplier(float multiplier)
    {
        timeMultiplier = multiplier;
        Debug.Log($"Timeline speed set to {multiplier}x");
        UIManager.Instance.SetPlaybackSpeed(timeMultiplier);
    }

    private string FormatTime(float timeInMilliseconds)
{
    float timeInSeconds = timeInMilliseconds / 1000f;

    int hours = Mathf.FloorToInt(timeInSeconds / 3600);
    int minutes = Mathf.FloorToInt((timeInSeconds % 3600) / 60);
    int seconds = Mathf.FloorToInt(timeInSeconds % 60);
    int milliseconds = Mathf.FloorToInt(timeInMilliseconds % 1000);
    return $"{hours:D1}:{minutes:D2}:{seconds:D2}.{milliseconds:D3}";
}
}
