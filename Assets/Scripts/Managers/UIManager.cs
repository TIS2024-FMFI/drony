using System;
using System.Collections;
using System.Collections.Generic;
using Drony.Entities;
using UnityEngine;
using System.IO;
using Interpreter;
using Utility;
using System.Threading.Tasks;

/// <summary>
/// Singleton class <c>UIManager</c> manages logic for UI
/// </summary>
public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    private TrajectoryManager trajectoryManager;
    private string commandFileName;
    private RoomUIController roomUIController;

    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject uiManagerGameObject = new GameObject("UIManager");
                _instance = uiManagerGameObject.AddComponent<UIManager>();
                DontDestroyOnLoad(uiManagerGameObject);
            }
            return _instance;
        }
    }
    private void Awake()
    {
        trajectoryManager = TrajectoryManager.Instance;

        roomUIController = FindObjectOfType<RoomUIController>();
        if (roomUIController == null)
        {
            Debug.LogError("RoomUIController not found! Make sure it is in the scene.");
        }
    }

    public async void ProcessCommandFile(string path)
    {
        LoadingManager.Instance.ShowLoading();

        List<string> commandSection = null;
        List<string> droneIds = null;

        await Task.Run(() =>
        {
            trajectoryManager = TrajectoryManager.Reinstanciate();

            string content = Utilities.ReadTextFile(path);
            CommandFileSplitter commandFileSplitter = new CommandFileSplitter(content);
            commandSection = commandFileSplitter.GetCommandSection();

            trajectoryManager.LoadTrajectories(commandSection);

            droneIds = trajectoryManager.GetDroneIds();
        });
        DroneSpawner.DestroyAllDrones();

        foreach (var id in droneIds)
        {
            DroneSpawner.Instance.SpawnDrone(id);
            await Task.Yield();
        }
        // Refresh UI toggles with new drones
        if (roomUIController != null)
        {
            Debug.Log("Refreshing drone toggles...");
            roomUIController.RefreshDroneToggles();
        }

        commandFileName = $"Loaded: {Path.GetFileName(path)}";
        LoadingManager.Instance.HideLoading();
    }


    public void ProcessWallTexture(string path)
    {
        throw new NotImplementedException("ProcessWallTexture is not implemented yet.");
    }

    public void ProcessDroneModel(string path)
    {
        throw new NotImplementedException("ProcessDroneModel is not implemented yet.");
    }

    public void ProcessMusicFile(string path)
    {
        if (File.Exists(path))
        {
            Debug.Log($"Processing Music File: {path}");
            AudioManager.Instance.LoadAndPlayMusic(path);
        }
        else
        {
            Debug.LogError($"Music file not found: {path}");
        }
        //throw new NotImplementedException("ProcessMusicFile is not implemented yet.");
    }
    public string GetCommandFileName()
    {
        return commandFileName;
    }
    public int GetTotalTime()
    {
        return trajectoryManager.totalTime;
    }
    public int GetCurrentTime()
    {
        return trajectoryManager.GetCurrentTime();
    }
    public void UpdateCurrentTime()
    {
        trajectoryManager.UpdateCurrentTime();
    }
    public void SetCurrentTime(int time)
    {
        trajectoryManager.SetCurrentTime(time);
    }
    public void SetPlaybackSpeed(float playbackSpeed)
    {
        trajectoryManager.SetPlaybackSpeed(playbackSpeed);
    }
    public void ResetCurrentTime()
    {
        trajectoryManager.ResetCurrentTime();
    }

    public void CheckCollisions()
    {
        var collisions = trajectoryManager.GetCurrentCollisions();

        foreach (var (d1, d2) in collisions)
        {
            Debug.LogError($"Drone collision: {d1}, {d2}");
        }
    }
}

