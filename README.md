# Unity Log-Delivery Truck: Robotic Loading, Voice Commands and Mission Control in Unity

A Unity simulation of an automated log-delivery job. Logs are spawned, carried along a conveyor, inspected by a sensor, loaded onto a truck by a robot arm, and delivered to the correct zone. The truck can be driven by keyboard, typed commands or spoken commands, and a mission system checks every delivery.

Developed as a Mini Project for the AR/VR course at Manipal Institute of Technology (MIT), Manipal, under the guidance of Dr. Asha C S.

## About This Project

This repository contains a complete, runnable simulation of a log-handling chain, written in C#. A spawner creates logs from a list of prefab types and a conveyor belt moves them forward. A sensor inspects each log, gives it an ID (for example LOG-001), measures its length and diameter, and shows the result on an inspection dashboard.

Logs are loaded onto the truck in two ways. A robot arm can pick a log up and place it at a truck drop point, and the code contains three alternative implementations of this robot. The truck can also collect the nearest log of the required type through a command. The truck then drives to the delivery zone, either under player control or by steering itself there.

The game is organised into missions. Each mission pairs one required log prefab with one delivery zone, and only the current mission's zone is active. When the correct log enters the zone, the log is destroyed and the mission is marked complete. The next mission begins only after the truck drives through the return point at base. After the final delivery and return, the game ends.

## Background & Motivation

Log handling involves several separate stages: supply, transport, inspection, loading and delivery. Research on forestry automation, robot simulation and voice control usually studies one of these stages at a time. Testing a full workflow on real machinery is expensive and risky, while a simulation allows the whole chain to be tried and compared safely.

This project, built for the AR/VR course, explores how a real-time 3D engine can serve as a testbed that connects all of these stages. It uses accessible tooling:

- **Unity** for physics, triggers and the 3D scene
- **C#** for all gameplay, robot, truck and mission logic
- **Cyclic Coordinate Descent (CCD) inverse kinematics** for the procedural robot arm
- **Windows keyword speech recognition** for voice commands
- **TextMeshPro** for the inspection dashboard and on-screen messages

## Objectives

1. Build a working log supply chain with a spawner, conveyor belt and inspection sensor.
2. Load logs onto a truck using a robot arm, with three alternative implementations for comparison.
3. Drive the truck through keyboard control, typed text commands and spoken commands.
4. Route typed and spoken input through one shared command parser.
5. Control the game with missions that validate each delivery and require a return to base.

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
- **Voice Control:** `TruckVoiceCommands` uses the Windows keyword recogniser to map 12 spoken phrases to the same text commands, so voice and typed input share one parser.
- **Mission Control:** `LogDeliveryManager` tracks the current mission, validates deliveries by log type using `LogIdentity`, enforces a return to base through `ReturnPoint`, and ends the game after the last mission.
- **Camera:** `truckcamera` provides a third-person follow camera with right-mouse orbit and collision avoidance.

## Technical Overview

| Component | Details |
|-----------|---------|
| Engine | Unity |
| Language | C# |
| Truck Physics | Rigidbody forces and rotation (no WheelCollider) |
| Robot Arm | CCD inverse kinematics, plus animator-based alternatives |
| Voice Input | Windows keyword recognition (12 phrases) |
| Command Input | Typed text and voice, through one shared parser |
| Mission Logic | Mission list with active, waiting-for-return and complete states |
| UI | TextMeshPro |
| Platform | Windows (voice control depends on the Windows speech recogniser) |

## Getting Started

1. Clone the repository.
2. Open the project in Unity Hub.
3. Open the main scene and press Play.
4. Drive the truck with the keyboard, type commands such as `collect log` and `deliver log` into the input field, or use the spoken commands (Windows only).

**Setup note:** the spawner chooses prefabs by list order and the mission manager requires prefabs by mission order. Make sure the two lists are arranged in the same order in the Inspector, or the spawned log will not match the mission.

## Tech Stack

- Unity
- C#
- TextMeshPro
- Windows Speech Recognition (Unity keyword recogniser)

## Acknowledgements

- Dr. Asha C S, for guidance and supervision throughout the project
- Department of Mechatronics, Manipal Institute of Technology, Manipal
- Unity Technologies

## License

This project was developed for academic purposes as part of a university course mini project.