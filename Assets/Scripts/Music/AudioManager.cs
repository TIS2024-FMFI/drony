using UnityEngine;
using System.IO;
using UnityEngine.Networking;

public class AudioManager
{
    private static AudioManager _instance;
    private AudioSource audioSource;

    private AudioManager()
    {
        // Ensure there's an AudioSource in the scene
        GameObject audioSourceObject = new GameObject("AudioManager");
        audioSource = audioSourceObject.AddComponent<AudioSource>();
    }

    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AudioManager();
            }
            return _instance;
        }
    }

    // Method to load and play audio
    public void LoadAndPlayMusic(string path)
    {
        if (File.Exists(path))
        {
            LoadAudioClip(path, clip =>
            {
                audioSource.clip = clip;
                //audioSource.Play(); // Start playing the audio once it's loaded
                Debug.Log($"Now Playing: {Path.GetFileName(path)}");
            });
        }
        else
        {
            Debug.LogError($"File does not exist: {path}");
        }
    }

    // Method to load the audio clip
    private void LoadAudioClip(string path, System.Action<AudioClip> onComplete)
    {
        string url = "file://" + path; // Convert path to URI
        Debug.Log($"Loading Audio from: {url}");

        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
        www.SendWebRequest().completed += operation =>
        {
            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                onComplete?.Invoke(clip);
            }
            else
            {
                Debug.LogError($"Failed to load audio: {www.error}");
            }
        };
    }

    // Method to get the AudioSource (for external access)
    public AudioSource GetAudioSource()
    {
        return audioSource;
    }

    // Method to play the audio
    public void PlayAudio()
    {
        if (audioSource.clip != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("No audio clip assigned to AudioSource.");
        }
    }

    // Method to pause the audio
    public void PauseAudio()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    // Method to stop the audio
    public void StopAudio()
    {
        audioSource.Stop();
    }

    // Method to get the current playback time
    public float GetAudioTime()
    {
        return audioSource.time;
    }
    public void SetPlaybackSpeed(float speed)
    {
        // AudioSource.pitch ranges from 0 to 3, and 1 is normal speed
        audioSource.pitch = speed;
        Debug.Log($"Playback speed set to {speed}x");
    }

    // Method to set the playback time
    public void SetAudioTime(float time)
    {
        audioSource.time = time;
    }

    // Method to get the audio clip length
    public float GetAudioLength()
    {
        if (audioSource.clip == null)
        {
            return 0.0f;
        }
        return audioSource.clip.length;
    }
}
