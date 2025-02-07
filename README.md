# 🛸 Simulator for choreographies of swarm of drones 

## 📖 About the Project
The **Drone Simulator** is a Unity-based application designed to simulate choreographed drone movements in a 3D environment. Users can import flight plans, visualize drone trajectories, and detect collisions in real-time. This project was developed as part of an academic course and serves as a tool for testing and demonstrating drone choreography.

## ✨ Features
- Import and interpret flight programs to generate drone trajectories.
- 3D visualization of drones following predefined paths.
- Collision detection with highlighted impact points.
- Music synchronization with drone movement.
- Adjustable parameters (speed, number of drones, space dimensions, etc.).

## 🖥️ System Requirements
- **Unity Version:** 2022.3 LTS (or later recommended)
- **Supported Platforms:** Windows/macOS/Linux
- **Hardware Requirements:**
  - Minimum: 8GB RAM, Integrated Graphics
  - Recommended: 16GB RAM, Dedicated GPU

## 🚀 Getting Started
### 🛠️ How to Open the Project in Unity
1. **Clone the Repository:**
   ```sh
   git clone https://github.com/TIS2024-FMFI/drony.git
   ```
2. **Open Unity Hub:**
   - Click *"Open"* and select the cloned project folder.
3. **Install Dependencies:**
   - If prompted, install missing Unity packages.
4. **Run the Simulation:**
   - Open `Scenes/MainScene.unity`.
   - Click *"Play"* in the Unity Editor.

### ✅ How to Test the Project
- Load a sample flight program (`Assets/FlightPrograms/sample.txt`).
- Load music file (optional)
- Adjust simulation speed and other parameters in the UI.
- Check for real-time collision detection.
- Enable debug logs to inspect trajectory calculations.
- Enable Gizmos to view trajectory path

## 📦 Installing the Built Version
1. Download the latest build from [Releases](https://github.com/TIS2024-FMFI/drony/releases).
2. Extract the files.
3. Run `DroneSimulator.exe` (Windows) or `DroneSimulator.app` (macOS).

## ⚙️ Configuration & Customization
- Modify simulation settings in the UI (playback speed, dimensions of drones, dimensions of room, colors of walls, etc.).

## 🛠️ Troubleshooting
| Issue | Solution |
|------|---------|
| Drones don't move | Ensure a valid flight program is loaded. |
| Collision detection not working | Check simulation logs. |
| Poor performance | Reduce drone count or number of operations |

## 🤝 Contributing
Contributions are welcome! To contribute:
1. Fork the repository.
2. Create a new branch (`feature-new-functionality`).
3. Commit your changes.
4. Submit a Pull Request.

## 📜 License
This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

🎯 **Need help?** Feel free to open an issue in the repository or reach out via email!

