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
                content = ".config:\nblablabla\n\n.command:\n00:00:00 droncik set-position [0, 0, 0]\n00:00:00 droncik take-off [5]\n00:00:00 droncik set-color [250, 10, 10]\n00:00:01.000 droncik set-color [255, 50, 0]\n00:00:01.100 droncik set-color [255, 80, 0]\n00:00:01.200 droncik set-color [255, 110, 0]\n00:00:01.300 droncik set-color [255, 140, 0]\n00:00:01.400 droncik set-color [255, 170, 0]\n00:00:01.500 droncik set-color [255, 200, 0]\n00:00:01.600 droncik set-color [255, 230, 0]\n00:00:01.700 droncik set-color [255, 255, 50]\n00:00:01.800 droncik set-color [255, 255, 80]\n00:00:01.900 droncik set-color [255, 255, 110]\n00:00:02.000 droncik set-color [255, 220, 140]\n00:00:02.100 droncik set-color [255, 200, 170]\n00:00:02.200 droncik set-color [255, 180, 200]\n00:00:02.300 droncik set-color [255, 160, 230]\n00:00:02.400 droncik set-color [255, 140, 255]\n00:00:02.500 droncik set-color [255, 110, 255]\n00:00:02.600 droncik set-color [255, 80, 255]\n00:00:02.700 droncik set-color [255, 50, 255]\n00:00:02.800 droncik set-color [255, 30, 200]\n00:00:02.900 droncik set-color [255, 10, 150]\n00:00:03.000 droncik set-color [255, 0, 100]\n00:00:03.100 droncik set-color [230, 0, 80]\n00:00:03.200 droncik set-color [200, 0, 60]\n00:00:03.300 droncik set-color [170, 0, 40]\n00:00:03.400 droncik set-color [140, 0, 20]\n00:00:03.500 droncik set-color [110, 0, 10]\n00:00:03.600 droncik set-color [140, 0, 20]\n00:00:03.700 droncik set-color [170, 0, 40]\n00:00:03.800 droncik set-color [200, 0, 60]\n00:00:03.900 droncik set-color [230, 0, 80]\n00:00:04.000 droncik set-color [255, 0, 100]\n00:00:04.100 droncik set-color [255, 10, 150]\n00:00:04.200 droncik set-color [255, 30, 200]\n00:00:04.300 droncik set-color [255, 50, 255]\n00:00:04.400 droncik set-color [255, 80, 255]\n00:00:04.500 droncik set-color [255, 110, 255]\n00:00:04.600 droncik set-color [255, 140, 255]\n00:00:04.700 droncik set-color [255, 160, 230]\n00:00:04.800 droncik set-color [255, 180, 200]\n00:00:04.900 droncik set-color [255, 200, 170]\n00:00:05.000 droncik set-color [255, 220, 140]\n00:00:05.100 droncik set-color [255, 255, 110]\n00:00:05.200 droncik set-color [255, 255, 80]\n00:00:05.300 droncik set-color [255, 255, 50]\n00:00:05.400 droncik set-color [255, 230, 0]\n00:00:05.500 droncik set-color [255, 200, 0]\n00:00:05.600 droncik set-color [255, 170, 0]\n00:00:05.700 droncik set-color [255, 140, 0]\n00:00:05.800 droncik set-color [255, 110, 0]\n00:00:05.900 droncik set-color [255, 80, 0]\n\n# 00:00:02 droncik fly-to 5 3 0 90 5s\n# 00:00:05 droncik fly-to 10 10 0 180 2\n# 00:00:07 droncik fly-to 10 10 -10 270 2\n# 00:00:10 droncik fly-to 0 1 10 360 1\n# 00:00:00 droncik drone-mode [approx]\n# 00:00:02 droncik fly-trajectory [[4,2,0,90,2],[6,4,0,180,2],[6,7,0,270,2],[5,7,0,0,2],[4,5,0,90,2],[3,7,0,180,2],[2,7,0,270,2],[2,4,0,0,2],[4,2,0,90,2]]\n\n\n# 00:00:05 droncik fly-circle [0,5,0,0,5,1,1,5,4]\n00:00:05 droncik fly-spiral [5, 5, 5, 1, 4, 1, 3, 3, 3, 1, 8, 1]\n#00:00:20 droncik fly-circle [3,3,-6,3,3,-3,1,3,3]\n\n# good spiral: [3, 5, 3, 3, 0, 0, 3, 3, 0, 1, 4, 2]\n\n# 00:00:05 1 fly-spiral 3 3 3 3 0 0 3 3 2 1 2 1\n\n\n# 00:00:01 1 fly-to 3 3 3 90 2\n# 00:00:03 1 fly-to -3 3 3 180 2\n# 00:00:05 1 fly-to -3 3 0 270 2\n# 00:00:06 1 fly-to 0 0 0 360 2\n\n\n# 00:00:00 drone set-position 0 0 0\n# 00:00:00 drone take-off [3]\n# 00:00:05 drone fly-circle [0,3,0,1,3,0,1,30,3]\n# 00:00:00 drone drone-mode [approx]\n# 00:00:03 drone fly-trajectory [[4,2,0,90,2],[6,4,0,180,2],[6,7,0,270,2],[5,7,0,0,2],[4,5,0,90,2],[3,7,0,180,2],[2,7,0,270,2],[2,4,0,0,2],[4,2,0,90,2]]\n\n\n# 00:00:00 2 take-off 4\n# 00:00:02 2 fly-to 0 3 -10 180 2\n# 00:00:04 2 fly-to 0 3 3 270 2\n# 00:00:06 2 fly-to 0 10 5 0 2\n\n# 00:00:00 3 set-position 1 0 1\n# 00:00:00 3 take-off 1\n# 00:00:02 3 fly-trajectory [[5, 5, 0, 0, 1], [6, 6, 1, 0, 1], [7, 5, 2, 0, 1], [6, 4, 3, 0, 1], [5, 3, 4, 0, 1], [4, 4, 5, 0, 1], [3, 5, 6, 0, 1], [4, 6, 7, 0, 1], [5, 7, 8, 0, 1], [6, 6, 9, 0, 1], [7, 5, 10, 0, 1], [6, 4, 9, 0, 1], [5, 3, 8, 0, 1]]\n\n# 00:00:02 3 fly-to 3 1 3 0 4\n# 00:00:04 3 fly-to -3 1 3 0 4\n# 00:00:06 3 fly-to -3 1 -3 0 4\n# 00:00:07 3 fly-to 3 1 -3 0 4";
            } else
            {
                trajectoryManager = TrajectoryManager.Reinstanciate();

                content = Utilities.ReadTextFile(path);
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

