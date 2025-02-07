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

    public async void ProcessCommandFile(string path, bool autoMode=false)
    {
        LoadingManager.Instance.ShowLoading();

        List<string> commandSection = null;
        List<string> droneIds = null;

        await Task.Run(() =>
        {
            string content;
            if (autoMode)
            {
                content = ".config:\nnot implemented yet\n.command:\n00:00:00 drone1 set-position [-3, 0, -3]\n00:00:00 drone2 set-position [-2, 0, -3]\n00:00:00 drone3 set-position [-1, 0, -3]\n00:00:00 drone4 set-position [0, 0, -3]\n00:00:00 drone5 set-position [1, 0, -3]\n00:00:00 drone6 set-position [2, 0, -3]\n00:00:00 drone1 set-color [200, 30, 30]\n00:00:00 drone2 set-color [200, 30, 30]\n00:00:00 drone3 set-color [200, 30, 30]\n00:00:00 drone4 set-color [200, 30, 30]\n00:00:00 drone5 set-color [200, 30, 30]\n00:00:00 drone6 set-color [200, 30, 30]\n00:00:01 drone1 take-off [1]\n00:00:01 drone2 take-off [2]\n00:00:01 drone3 take-off [3]\n00:00:01 drone4 take-off [4]\n00:00:01 drone5 take-off [5]\n00:00:01 drone6 take-off [6]\n00:00:01 drone1 drone-mode [approx]\n00:00:01 drone2 drone-mode [approx]\n00:00:01 drone3 drone-mode [approx]\n00:00:01 drone4 drone-mode [approx]\n00:00:01 drone5 drone-mode [approx]\n00:00:01 drone6 drone-mode [approx]\n00:00:05 drone1 fly-to [0,2,-5,180,2]\n00:00:07 drone2 fly-to [2,4,-5,180,2]\n00:00:09 drone3 fly-to [2,7,-5,180,2]\n00:00:11 drone4 fly-to [0,5,-5,180,2]\n00:00:13 drone5 fly-to [-2,7,-5,180,2]\n00:00:15 drone6 fly-to [-2,4,-5,180,2]\n00:00:20 drone1 fly-trajectory [[2, 4, -5, 180, 2], [2, 7, -5, 180, 2], [1, 7, -5, 180, 2], [0, 6, -5, 180, 2], [-1, 7, -5, 180, 2], [-2, 7, -5, 180, 2], [-2, 4, -5, 180, 2], [0, 2, -5, 180, 2], [2, 4, -5, 180, 2], [2, 7, -5, 180, 2], [1, 7, -5, 180, 2], [0, 6, -5, 180, 2], [-1, 7, -5, 180, 2], [-2, 7, -5, 180, 2], [-2, 4, -5, 180, 2], [0, 2, -5, 180, 2]]\n00:00:20 drone2 fly-trajectory [[2, 7, -5, 180, 2], [1, 7, -5, 180, 2], [0, 6, -5, 180, 2], [-1, 7, -5, 180, 2], [-2, 7, -5, 180, 2], [-2, 4, -5, 180, 2], [0, 2, -5, 180, 2], [2, 4, -5, 180, 2], [2, 7, -5, 180, 2], [1, 7, -5, 180, 2], [0, 6, -5, 180, 2], [-1, 7, -5, 180, 2], [-2, 7, -5, 180, 2], [-2, 4, -5, 180, 2], [0, 2, -5, 180, 2], [2, 4, -5, 180, 2]]\n00:00:20 drone3 fly-trajectory [[1, 7, -5, 180, 2], [0, 6, -5, 180, 2], [-1, 7, -5, 180, 2], [-2, 7, -5, 180, 2], [-2, 4, -5, 180, 2], [0, 2, -5, 180, 2], [2, 4, -5, 180, 2], [2, 7, -5, 180, 2], [1, 7, -5, 180, 2], [0, 6, -5, 180, 2], [-1, 7, -5, 180, 2], [-2, 7, -5, 180, 2], [-2, 4, -5, 180, 2], [0, 2, -5, 180, 2], [2, 4, -5, 180, 2], [2, 7, -5, 180, 2]]\n00:00:20 drone4 fly-trajectory [[-1, 7, -5, 180, 2], [-2, 7, -5, 180, 2], [-2, 4, -5, 180, 2], [0, 2, -5, 180, 2], [2, 4, -5, 180, 2], [2, 7, -5, 180, 2], [1, 7, -5, 180, 2], [0, 6, -5, 180, 2], [-1, 7, -5, 180, 2], [-2, 7, -5, 180, 2], [-2, 4, -5, 180, 2], [0, 2, -5, 180, 2], [2, 4, -5, 180, 2], [2, 7, -5, 180, 2], [1, 7, -5, 180, 2], [0, 6, -5, 180, 2]]\n00:00:20 drone5 fly-trajectory [[-2, 4, -5, 180, 2], [0, 2, -5, 180, 2], [2, 4, -5, 180, 2], [2, 7, -5, 180, 2], [1, 7, -5, 180, 2], [0, 6, -5, 180, 2], [-1, 7, -5, 180, 2], [-2, 7, -5, 180, 2], [-2, 4, -5, 180, 2], [0, 2, -5, 180, 2], [2, 4, -5, 180, 2], [2, 7, -5, 180, 2], [1, 7, -5, 180, 2], [0, 6, -5, 180, 2], [-1, 7, -5, 180, 2], [-2, 7, -5, 180, 2]]\n00:00:20 drone6 fly-trajectory [[0, 2, -5, 180, 2], [2, 4, -5, 180, 2], [2, 7, -5, 180, 2], [1, 7, -5, 180, 2], [0, 6, -5, 180, 2], [-1, 7, -5, 180, 2], [-2, 7, -5, 180, 2], [-2, 4, -5, 180, 2], [0, 2, -5, 180, 2], [2, 4, -5, 180, 2], [2, 7, -5, 180, 2], [1, 7, -5, 180, 2], [0, 6, -5, 180, 2], [-1, 7, -5, 180, 2], [-2, 7, -5, 180, 2], [-2, 4, -5, 180, 2]]\n00:00:40 drone1 fly-to [-3, 3, -3,180,2]\n00:00:41 drone2 fly-to [-2, 3, -3,180,2]\n00:00:42 drone3 fly-to [-1, 3, -3,180,2]\n00:00:43 drone4 fly-to [0, 3, -3,180,2]\n00:00:44 drone5 fly-to [1, 3, -3,180,2]\n00:00:45 drone6 fly-to [2, 3, -3,180,2]\n00:00:47 drone1 land\n00:00:47 drone2 land\n00:00:47 drone3 land\n00:00:47 drone4 land\n00:00:47 drone5 land\n00:00:47 drone6 land\n";
            } else
            {
                trajectoryManager = TrajectoryManager.Reinstanciate();

                content = Utilities.ReadTextFile(path);
                Debug.Log(content);
            }
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
}

