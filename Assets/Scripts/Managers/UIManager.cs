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
}

