﻿using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using SFB; // Na nahrávanie súborov cez natívny dialog

public class FileUploadController : MonoBehaviour
{
    private string commandFilePath;
    private string wallTexturePath;
    private string droneModelPath;

    private bool AUTO_MODE = false;

    private Button uploadCommandFileButton;
    private Label commandFileNameLabel;

    private Button uploadWallTextureButton;
    private Label wallTextureLabel;

    private Button uploadDroneModelButton;
    private Label droneModelLabel;
    private UIManager uimanager;

    private Button uploadMusicFileButton;
    private Label musicFileLabel;

    void OnEnable()
    {
        uimanager = UIManager.Instance;
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Command File
        uploadCommandFileButton = root.Q<Button>("upload-button");
        commandFileNameLabel = root.Q<Label>("file-name-label");
        uploadCommandFileButton.clicked += () => OnUploadFile("Command File", new[] { new ExtensionFilter("Text Files", "txt") }, ProcessCommandFile);

        // Wall Texture
        uploadWallTextureButton = root.Q<Button>("wall-texture-upload");
        wallTextureLabel = root.Q<Label>("wall-texture-label");
        uploadWallTextureButton.clicked += () => OnUploadFile("Wall Texture", new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") }, ProcessWallTexture);

        // Drone Model
        uploadDroneModelButton = root.Q<Button>("drone-model-upload");
        droneModelLabel = root.Q<Label>("drone-model-label");
        uploadDroneModelButton.clicked += () => OnUploadFile("Drone Model", new[] { new ExtensionFilter("Drone Model Files", "sdl") }, ProcessDroneModel);

        uploadMusicFileButton = root.Q<Button>("upload-music-button");
        musicFileLabel = root.Q<Label>("upload-music-label");
        uploadMusicFileButton.clicked += () => OnUploadFile("Music File", new[] { new ExtensionFilter("Audio Files", "mp3", "wav", "ogg") }, ProcessMusicFile);
    }

    void Start()
    {
        if (AUTO_MODE)
        {
            uimanager.ProcessCommandFile("", AUTO_MODE);
            commandFileNameLabel.text = uimanager.GetCommandFileName();
        }
    }

    private void OnUploadFile(string title, ExtensionFilter[] extensions, System.Action<string> processFileCallback)
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel($"Select {title}", "", extensions, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string filePath = paths[0];
            processFileCallback.Invoke(filePath);
        }
    }
    private void ProcessCommandFile(string path)
    {
        uimanager.ProcessCommandFile(path);
        commandFileNameLabel.text = uimanager.GetCommandFileName();
    }

    private void ProcessWallTexture(string path)
    {
        wallTexturePath = path;
        wallTextureLabel.text = $"Loaded Texture: {Path.GetFileName(path)}";
        Debug.Log($"Wall Texture Loaded: {path}");
        // spracovat obsah
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(File.ReadAllBytes(path));
        Debug.Log($"Texture Loaded with size: {texture.width}x{texture.height}");
    }

    private void ProcessDroneModel(string path)
    {
        droneModelPath = path;
        droneModelLabel.text = $"Loaded Model: {Path.GetFileName(path)}";
        Debug.Log($"Drone Model Loaded: {path}");
        // spracovat obsah
    }

    private void ProcessMusicFile(string path)
    {
        uimanager.ProcessMusicFile(path);
        Debug.Log($"Music File Loaded: {path}");
        //musicFileLabel.text = $"Loaded Music File: {Path.GetFileName(path)}";
    }
}
