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
        List<string> fileLines = Utilities.GetLinesFromString(content);
        trajectoryManager.LoadTrajectories(fileLines);
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
}

