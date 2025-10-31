# QuietArchery

A Unity-based virtual reality experiment platform designed to investigate the **quiet eye** phenomenon, a research area examining how stable eye fixations correlate with improved performance in physical tasks. This project virtualizes the archery task to study quiet eye behavior and its neural mechanisms, enabling investigation into the underlying brain processes during precision aiming tasks.

## Research Context

This project was developed as part of a research collaboration investigating the neural mechanisms of the quiet eye phenomenon. The quiet eye is characterized by longer, steadier fixation on a target before executing a movement, and has been associated with superior performance in various motor tasks. By virtualizing archery within a controlled experimental environment, this platform allows for precise manipulation of experimental conditions while simultaneously recording eye movement data and motor behavior.

**Research Collaborators:**
- Nick Kreter PhD
- Michelle Marneweck PhD
- Jolinda Smith PhD
- Motor Skills Lab

## Technical Overview

### Platform & Engine
- **Unity Version:** 2022.3.18f1
- **Platform:** Windows (Standalone)

### Core Technologies

#### Eye Tracking Integration
- **EyeLink Integration:** Full integration with SR Research EyeLink eye tracking systems
- Real-time gaze data collection synchronized with trial events
- Custom C# interop layer (`EyelinkCoreInterop.cs`) for native DLL communication
- Eye tracking data recorded alongside behavioral metrics

#### Input System
- **Serial Port Communication:** Tablet input via serial port (COM4, 115200 baud)
- Real-time coordinate translation from raw tablet input to Unity world space
- Custom input normalization and calibration system
- Primary input method is tablet via serial port; mouse input support exists but requires script reconfiguration

#### Experiment Architecture
- **Phase-Based Trial Structure:** Modular experimental phase system
  - **ITI (Inter-Trial Interval):** Baseline period between trials
  - **Ready:** Preparation phase with crosshair visualization
  - **Aim:** Active aiming phase with input processing
  - **Shoot:** Execution phase with shooting mechanics
  - **Return:** Return-to-home phase with tablet visualization
  - **End:** Trial conclusion and data finalization

#### Experimental Conditions
The system supports multiple experimental conditions that can be manipulated:
- **Distance:** Near vs. Far target conditions
- **Visual Noise:** Quiet vs. Noisy visual environments
- **Distraction:** Presence or absence of distracting visual stimuli

Conditions are configured via external configuration files, allowing for flexible experimental design and randomization.

### Key Scripts & Components

#### Core Experiment Management
- **`AimShoot.cs`:** Primary physics and aiming mechanics controller
  - Crosshair movement and targeting system
  - Dart/projectile shooting mechanics
  - Input processing (tablet/serial and mouse)
  - Phase-based state management
  
- **`DataOutputAndConfig.cs`:** Data collection and experiment configuration
  - Trial and block management
  - CSV data logging
  - Configuration file parsing
  - Eye tracking data integration

- **`Phases.cs`:** Experimental phase management system
  - Phase creation and sequencing
  - Timing control
  - Phase transition logic

#### Eye Tracking
- **`EyeLinkManager.cs`:** High-level EyeLink interface
  - Connection management
  - Recording control
  - Real-time data polling
  
- **`EyelinkCoreInterop.cs`:** Low-level EyeLink DLL interop
  - Native function declarations
  - Memory management for eye tracking data structures

#### Visualization & Feedback
- **`TabletVis.cs`:** Tablet coordinate system
  - Real-time position feedback
  - Return-to-home detection
  - Serial port management for tablet

- **`DrawCrosshair.cs`:** Crosshair rendering system

### Project Structure

```
Assets/
├── Scripts/          # Core C# scripts
├── Scenes/           # Unity scene files
│   ├── ArcheryScene.unity          # Main experimental scene
│   ├── EyelinkCalibrationScene.unity
│   └── TrialBlockHandler.unity     # GUI, pre-run setup
├── Models/           # 3D models and meshes
├── Materials/        # Unity materials
├── Textures/         # Texture assets
├── Audio/            # Audio assets
└── Plugins/          # Third-party plugins (EyeLink DLLs, etc.)
```

### Dependencies

#### Unity Packages
- **Post Processing Stack v2** (v3.4.0): Visual effects and post-processing
- **TextMeshPro** (v3.0.6): Advanced text rendering
- **Visual Scripting** (v1.9.1): Visual programming support
- **Timeline** (v1.7.6): Animation and sequencing

#### External Libraries
- **DOTween:** Animation and tweening library (for UI animations)
- **EyeLink SDK:** SR Research EyeLink SDK (native DLLs required)

### Hardware Requirements

To run this experiment, the following hardware is required:
- **EyeLink Eye Tracker:** SR Research EyeLink system (with appropriate SDK installed)
- **Tablet Input Device:** Compatible tablet connected via serial port
- **Display:** Monitor/projection system suitable for experimental presentation
- **Windows PC:** Compatible with Unity 2022.3.18f1

### Configuration

Experimental parameters are controlled through:
- **Configuration Files:** Trial sequences, timing, and conditions defined in external text files
- **Unity PlayerPrefs:** Subject IDs, trial numbers, and session parameters
- **Inspector Settings:** Scriptable parameters in Unity Inspector for fine-tuning

## Development Notes

- The project uses serial port communication for tablet input, requiring COM port configuration through terminal system (TeraTerm)
- EyeLink integration requires the EyeLink SDK to be properly installed and accessible
- Some paths in the code reference specific directory structures that may need adjustment for different environments
- The system is designed for controlled experimental settings with precise timing requirements

## Acknowledgments

This project represents a collaborative effort to advance understanding of the quiet eye phenomenon and its neural correlates. Special thanks to the Nick Kreter PhD, Michelle Marneweck PhD, Jolinda Smith PhD, and the Motor Skills Lab for their guidance and contributions to this work. This work was supported by the Wu Tsai Human Performance Alliance and the Joe and Clara Tsai Foundation.