# Unity Log-Delivery Truck: Robotic Loading and Mission Control in Unity

A Unity simulation of an automated log-delivery job. Logs are spawned, carried along a conveyor, inspected by a sensor, loaded onto a truck by a robot arm, and delivered to the correct zone. The truck can be driven by keyboard or typed commands, and a mission system checks every delivery.

Developed as a Mini Project for the AR/VR course at Manipal Institute of Technology (MIT), Manipal, under the guidance of Dr. Asha C S.

## About This Project

This repository contains a complete, runnable simulation of a log-handling chain, written in C# for Unity. A spawner creates logs from a list of prefab types (the project includes Cheap, Standard and Premium log prefabs) and a conveyor belt moves them forward. A sensor inspects each log, gives it an ID (for example LOG-001), measures its length and diameter, and shows the result on an inspection dashboard.

Logs are loaded onto the truck in two ways. A robot arm can pick a log up and place it at a truck drop point, and the code contains three alternative implementations of this robot. The truck can also collect the nearest log of the required type through a typed command. The truck then drives to the delivery zone, either under player control or by steering itself there.

The game is organised into missions. Each mission pairs one required log prefab with one delivery zone, and only the current mission's zone is active. When the correct log enters the zone, the log is destroyed and the mission is marked complete. The next mission begins only after the truck drives through the return point at base. After the final delivery and return, the game ends.

## Background & Motivation

Log handling involves several separate stages: supply, transport, inspection, loading and delivery. Research on forestry automation and robot simulation usually studies one of these stages at a time. Testing a full workflow on real machinery is expensive and risky, while a simulation allows the whole chain to be tried and compared safely.

This project, built for the AR/VR course, explores how a real-time 3D engine can serve as a testbed that connects all of these stages. It uses accessible tooling:

- **Unity** (Universal Render Pipeline) for physics, triggers and the 3D scene
- **C#** for all gameplay, robot, truck and mission logic
- **Cyclic Coordinate Descent (CCD) inverse kinematics** for the procedural robot arm
- **TextMeshPro** for the inspection dashboard, command input and on-screen messages

## Objectives

1. Build a working log supply chain with a spawner, conveyor belt and inspection sensor.
2. Load logs onto a truck using a robot arm, with three alternative implementations for comparison.
3. Drive the truck through keyboard control and typed text commands.
4. Control the game with missions that validate each delivery and require a return to base.

## Team

B.Tech Mechatronics, Manipal Institute of Technology (MIT), Manipal

| Name | Roll No. |
|------|----------|
| Mihika Usgaonker | 230929318 |
| Aastha Dhruv | 230929028 |
| Ayaan Zubair Shaikh | 230929027 |
| Sheikh Riza | 230929004 |

**Faculty Guide:** Dr. Asha C S
**Course:** AR/VR (Mini Project)
**Repository owner:** @Mckzne

## Project Status

| Phase | Description | Progress |
|-------|-------------|----------|
| Phase 1 | Core Simulation, Robot Loading and Truck Control | Completed |

### Completed: Phase 1 (Core Simulation, Robot Loading and Truck Control)

- **Log Supply:** `LogSpawner` creates logs round-robin from a list of prefabs and respawns only after a completed delivery plus truck return. `ConveyorBelt` moves logs along the belt.
- **Inspection:** `LogSensor` detects each log once, assigns an ID, reads its type, length and diameter from `LogData`, and updates the six-label `InspectionDashboard`.
- **Robot Loading:** Three approaches were implemented: a procedural arm using CCD inverse kinematics (`AutomatedLogRobot`), an animator-driven four-step sequence (`RobotAnimationSequence`), and a one-shot animation trigger (`RobotLogAnimationTrigger`).
- **Truck Physics:** `TruckController` drives the truck with Rigidbody forces, with a ground check, speed limit, steering, braking and tyre animation, and accepts keyboard or autonomous input.
- **Command Interface:** `TruckCommandController` parses typed commands such as "collect log" and "deliver log", collects the nearest log of the required type, and auto-steers to the delivery zone.
- **Mission Control:** `LogDeliveryManager` tracks the current mission, validates deliveries by log type using `LogIdentity`, enforces a return to base through `ReturnPoint`, and ends the game after the last mission.
- **Camera:** `truckcamera` provides a third-person follow camera with right-mouse orbit and collision avoidance.

## Technical Overview

| Component | Details |
|-----------|---------|
| Engine | Unity (version listed in `ProjectSettings/ProjectVersion.txt`) |
| Render Pipeline | Universal Render Pipeline (URP) |
| Language | C# |
| Input | Unity's legacy Input Manager, plus a TextMeshPro input field for typed commands |
| Truck Physics | Rigidbody forces and rotation (no WheelCollider) |
| Robot Arm | CCD inverse kinematics, plus animator-based alternatives |
| Command Input | Typed text commands, through one shared parser |
| Mission Logic | Mission list with active, waiting-for-return and complete states |
| UI | TextMeshPro |
| Platform | Windows (see the setup notes on the speech script) |

## Repository Structure

The repository stores only the files Unity needs to rebuild the project. It contains three folders:

```
├── Assets/
│   ├── PROJECT.unity           Main scene
│   ├── PROJECT/                Lighting data for the main scene
│   ├── Scripts/                All C# scripts for the simulation
│   ├── CheapLog.prefab         Log prefabs
│   ├── StandardLog.prefab
│   ├── PremiumLog.prefab
│   ├── Conveyor Belt.fbx       Conveyor model
│   ├── Robot Industrial Template (.fbx, .prefab)   Robot arm models and prefabs
│   ├── ROBOTICARM/             Animated robot arm model
│   ├── IndustrialSmallTruck/   Truck model, materials and prefab
│   ├── BarnHouse/, CozyMountainCabin/, _DLNK/, BushGrassBend/   Environment and machinery asset packs
│   ├── URPDefaultResources/    URP pipeline, renderer and quality-level assets
│   ├── TextMeshPro/            TextMeshPro shaders
│   └── XR/                     OpenXR settings
├── Packages/
│   ├── manifest.json           Unity packages the project depends on
│   └── packages-lock.json      Exact package versions
├── ProjectSettings/            Unity version, tags, layers, physics, input and graphics settings
└── README.md
```

Unity's generated folders, such as `Library/` and `Temp/`, are **not** included. Unity recreates them automatically the first time the project is opened, which is why the first launch takes longer than usual. The `.meta` files are included on purpose, because they keep references between scenes, prefabs and scripts intact.

## Setup Guide

### Requirements

- **Git**, to clone the repository
- **Unity Hub** and the **Unity Editor version this project was built with**. The exact version is written in `ProjectSettings/ProjectVersion.txt` (look for the `m_EditorVersion` line).
- An internet connection for the first open, so Unity can download the packages listed in `Packages/manifest.json`
- A few GB of free disk space for the regenerated `Library/` folder

### Steps

1. **Clone the repository**

   ```bash
   git clone https://github.com/Mckzne/Cobot-RL-in-Unity.git
   cd Cobot-RL-in-Unity
   ```

2. **Install the right Unity version.** Open `ProjectSettings/ProjectVersion.txt` to find the version number. In Unity Hub, go to **Installs → Install Editor** and install that version (use the Archive page if it is not listed).

3. **Add the project to Unity Hub.** Click **Add → Add project from disk** and select the cloned folder (the one that contains `Assets`, `Packages` and `ProjectSettings`).

4. **Open the project.** Click the project in Unity Hub. If Hub offers a different Unity version, choose the one from step 2. The first open can take several minutes while Unity imports assets, downloads packages and generates `Library/`.

5. **Import TextMeshPro essentials.** The TextMeshPro font and resource files are not stored in the repository. If a TMP Importer window appears, click **Import TMP Essentials**. If it does not, go to **Window → TextMeshPro → Import TMP Essential Resources**. Without this step the dashboard and message text will not display.

6. **Open the main scene.** In the Project window, open `Assets/PROJECT.unity` (double-click it).

7. **Press Play.** The first log spawns, travels along the conveyor and is inspected.

### Controls

| Input | Action |
|-------|--------|
| W / S or Up / Down | Throttle forward and reverse |
| A / D or Left / Right | Steer |
| Space | Brake |
| Right mouse button + move mouse | Orbit the camera |
| Type a command in the input field, then press Enter | Run a typed command |

Useful typed commands:

| Command | Effect |
|---------|--------|
| `help` | Show the command list |
| `status` | Show whether the truck is loaded and the mission state |
| `collect log` | Attach the nearest log of the required type (within 8 m) to the truck |
| `deliver log` | Auto-steer the truck toward the active delivery zone |
| `return to base` | Only valid after a delivery, while waiting for the truck to return |
| `stop` | Stop the truck |
| `forward for 5 seconds` | Timed drive for 0.1 to 60 seconds |

Commands must match exactly (after trimming spaces and ignoring capitals).

### How a mission plays out

1. A log spawns and moves along the conveyor to the inspection sensor.
2. The robot arm loads it onto the truck, or you type `collect log`.
3. Drive to the active delivery zone, or type `deliver log` to auto-steer.
4. When the correct log enters the zone, the mission completes.
5. Drive the truck through the return point at base to start the next mission.

### Setup notes and troubleshooting

- **Spawn order must match mission order.** The spawner chooses prefabs by list order and the mission manager requires prefabs by mission order. Arrange the two lists in the same order in the Inspector, or the spawned log will not match the mission.
- **Tags.** The code relies on the `Log` and `Tyre` tags, which are stored in `ProjectSettings/`. The truck's tyre colliders must carry the `Tyre` tag in the scene, or the return point will not detect the truck.
- **Compile errors mentioning `UnityEngine.Windows.Speech`.** These appear when the project is opened on macOS or Linux. Open it on Windows, or delete `Assets/Scripts/TruckVoiceCommands.cs` (voice control is not required for the simulation).
- **Input errors.** The project uses Unity's legacy Input Manager. If the console reports input errors, go to **Edit → Project Settings → Player → Other Settings → Active Input Handling** and select **Input Manager (Old)** or **Both**.
- **Pink or missing materials.** The project uses URP. Make sure it opened with the Unity version in `ProjectVersion.txt` and that the packages finished downloading.
- **Extra demo scenes.** The asset packs under `Assets/` include their own demo scenes. They are not part of the simulation and can be ignored.

## Tech Stack

- Unity (Universal Render Pipeline)
- C#
- TextMeshPro

## Acknowledgements

- Dr. Asha C S, for guidance and supervision throughout the project
- Department of Mechatronics, Manipal Institute of Technology, Manipal
- Unity Technologies
- The creators of the third-party Unity assets used for the environment, truck and machinery models (BarnHouse, Cozy Mountain Cabin, Industrial Small Truck, Heavy Machinery and Bush Grass Bend packs)

## License

This project was developed for academic purposes as part of a university course mini project. Third-party assets remain the property of their respective creators and are subject to their own licences.