using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using SFB; // Na nahrávanie súborov cez natívny dialog

public class FileUploadController : MonoBehaviour
{
    private string commandFilePath;
    private string wallTexturePath;
    private string droneModelPath;

    private Button uploadCommandFileButton;
    private Label commandFileNameLabel;

    private Button uploadWallTextureButton;
    private Label wallTextureLabel;

    private Button uploadDroneModelButton;
    private Label droneModelLabel;

    void OnEnable()
    {
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
    }

    private void OnUploadFile(string title, ExtensionFilter[] extensions, System.Action<string> processFileCallback)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        string[] paths = StandaloneFileBrowser.OpenFilePanel($"Select {title}", "", extensions, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string filePath = paths[0];
            processFileCallback.Invoke(filePath);
        }
#endif
    }

    private void ProcessCommandFile(string path)
    {
        commandFilePath = path;
        commandFileNameLabel.text = $"Loaded: {Path.GetFileName(path)}";
        Debug.Log($"Command File Loaded: {path}");
        // spracovat obsah
        string content = File.ReadAllText(path);
        Debug.Log($"Command File Content: {content}");
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
}
