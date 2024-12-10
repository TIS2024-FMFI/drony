using UnityEngine;
using UnityEngine.UIElements;

public class SidePanelToggle : MonoBehaviour
{
    private VisualElement sidePanel;
    private Button toggleButton;
    private bool isPanelVisible = true;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        sidePanel = root.Q<VisualElement>("side-panel");
        toggleButton = root.Q<Button>("toggle-panel-button");

        toggleButton.clicked += TogglePanel;

        UpdatePanelState();
    }

    void TogglePanel()
    {
        isPanelVisible = !isPanelVisible;
        UpdatePanelState();
    }

    void UpdatePanelState()
    {
        if (isPanelVisible)
        {
            sidePanel.RemoveFromClassList("hidden");
            toggleButton.style.left = 400;
            toggleButton.text = "<";
        }
        else
        {
            sidePanel.AddToClassList("hidden");
            toggleButton.style.left = 0;
            toggleButton.text = ">";
        }
    }
}
