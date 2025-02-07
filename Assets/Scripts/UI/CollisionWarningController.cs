using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class CollisionWarningController : MonoBehaviour
    {
        public static CollisionWarningController Instance { get; set; }

        private UIManager uiManager;
        private Button continueButton;
        private Label collisionInfoLabel;
        private VisualElement root;

        void Start()
        {
            Instance = this;
            
            uiManager = UIManager.Instance;

            root = GetComponent<UIDocument>().rootVisualElement;
            root.AddToClassList("hidden");
            
            continueButton = root.Q<Button>("ContinueButton");
            continueButton.clicked += Continue;
            
            
            collisionInfoLabel = root.Q<Label>("CollisionInfo");
        }

        private void Continue()
        {
            Time.timeScale = 1; // Resume application
            
            root.AddToClassList("hidden");
        }

        public void Show(string id1, string id2)
        {
            Time.timeScale = 0; // Pause application
            root.RemoveFromClassList("hidden");
            
            var currentTime = TrajectoryManager.Instance.GetCurrentTime();
            var timeInSeconds = currentTime / 1000f;
            var hours = Mathf.FloorToInt(timeInSeconds / 3600);
            var minutes = Mathf.FloorToInt((timeInSeconds % 3600) / 60);
            var seconds = Mathf.FloorToInt(timeInSeconds % 60);
            var milliseconds = Mathf.FloorToInt(currentTime % 1000);
            var time = $"{hours:D1}:{minutes:D2}:{seconds:D2}.{milliseconds:D3}";
            
            collisionInfoLabel.text = $"{id1} collided with {id2} at {time}";
        }
    }
}