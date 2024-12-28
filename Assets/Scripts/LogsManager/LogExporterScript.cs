using UnityEngine;
using System.IO;

public class LogExporter : MonoBehaviour
{
    private string logFilePath;

    private void Awake()
    {
        // Define the log file path
        logFilePath = Path.Combine(Application.persistentDataPath, "GameLogs.txt");

        // Clear any existing log file
        File.WriteAllText(logFilePath, "Log File Created: " + System.DateTime.Now + "\n");

        // Subscribe to the logMessageReceived callback
        Application.logMessageReceived += HandleLog;
    }

    private void OnDestroy()
    {
        // Unsubscribe when the script is destroyed
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Format the log entry
        string logEntry = $"[{type}] {logString}\n";

        // Include stack trace for errors or exceptions
        if (type == LogType.Error || type == LogType.Exception)
        {
            logEntry += $"StackTrace: {stackTrace}\n";
        }

        // Append the log entry to the file
        File.AppendAllText(logFilePath, logEntry);
    }
}
