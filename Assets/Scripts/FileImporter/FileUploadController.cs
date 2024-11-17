using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using SFB;
#endif

public class FileUploadController : MonoBehaviour
{
    private Button uploadButton;
    private Label fileNameLabel;
    private string filePath;

    void OnEnable()
    {
        // root element z UIDocument
        var root = GetComponent<UIDocument>().rootVisualElement;

        uploadButton = root.Q<Button>("upload-button");
        fileNameLabel = root.Q<Label>("file-name-label");

        // listener na tlacidlo
        uploadButton.clicked += OnUploadButtonClick;
    }

    private void OnUploadButtonClick()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        var extensions = new[] { new ExtensionFilter("Text Files", "txt") };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select a file", "", extensions, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            filePath = paths[0];
            fileNameLabel.text = Path.GetFileName(filePath);
            ProcessFile(filePath);
        }
#endif
    }

    private void ProcessFile(string path)
    {
        // tu ten file procesneme, config cast spracuje nejaky configcontroller a ostatok spracuje parser a interpreter pre commandy
        string fileContent = File.ReadAllText(path);
        Debug.Log("Loaded File Content: " + fileContent);

    }
}
