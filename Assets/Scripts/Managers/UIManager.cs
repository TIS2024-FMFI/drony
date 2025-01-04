using System;
using System.Collections.Generic; 
using Drony.Entities;
using UnityEngine;
using System.IO;
using Interpreter;
using Utility;

/// <summary>
/// Singleton class <c>UIManager</c> manages logic for UI
/// </summary>
public class UIManager 
{
    private static UIManager _instance;
    private TrajectoryManager trajectoryManager;
    private string commandFileName;
    private UIManager() { 
        trajectoryManager = TrajectoryManager.Instance;
    }
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new UIManager(); 
            }
            return _instance;
        }
    }

    public void UpdatePlaybackSpeed()
    {
        throw new NotImplementedException("UpdatePlaybackSpeed is not implemented yet.");
    }

    public void ProcessCommandFile(string path)
    {
        DroneSpawner.DestroyAllDrones();
        trajectoryManager = TrajectoryManager.Reinstanciate();

        string content = Utilities.ReadTextFile(path);
        CommandFileSplitter commandFileSplitter = new CommandFileSplitter(content);
        List<string> commandSection = commandFileSplitter.GetCommandSection();
        trajectoryManager.LoadTrajectories(commandSection);
        List<string> droneIds = trajectoryManager.GetDroneIds();
        foreach (var id in droneIds)
        {
            DroneSpawner.Instance.SpawnDrone(id);
        }
        commandFileName = $"Loaded: {Path.GetFileName(path)}";
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
        throw new NotImplementedException("ProcessMusicFile is not implemented yet.");
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
    public void SetPlaybackSpeed(int playbackSpeed)
    {
        trajectoryManager.SetPlaybackSpeed(playbackSpeed);
    }
    public void ResetCurrentTime()
    {
        trajectoryManager.ResetCurrentTime();
    }
}

