# DriftRunner_AllInOne.cs

A complete, self-contained Unity endless runner game in a single C# script.

## Overview

DriftRunner is a mobile endless runner game implemented as a single MonoBehaviour script. The player automatically moves forward and must dodge obstacles by moving left and right using touch or mouse input.

## Features

- **Player Movement**: Auto-forward movement with smooth horizontal control
- **Touch & Mouse Input**: Works on mobile (touch) and desktop (mouse drag)
- **Dynamic Obstacles**: Randomly spawned cube obstacles with increasing spawn rate
- **Progressive Difficulty**: Player speed increases gradually over time
- **Camera System**: Smooth follow camera with tilt based on player movement
- **Score Tracking**: Distance-based scoring system
- **Game Over & Restart**: Collision detection with tap/click to restart
- **Simple UI**: OnGUI-based score and game over display

## Installation

1. Open your Unity project (Unity 2022.3 LTS or newer)
2. Copy `DriftRunner.cs` to your Assets folder
3. Create an empty GameObject in your scene (GameObject > Create Empty)
4. Attach the `DriftRunner.cs` script to the empty GameObject
5. Press Play!

The script will automatically create:
- Player (green cube)
- Ground plane (gray)
- Camera (if none exists)
- Obstacles (red cubes)

## Controls

### Mobile
- **Touch and drag** left/right to move the player horizontally

### Desktop/Editor
- **Click and drag** with mouse to move the player horizontally
- **Click** after game over to restart

## Game Mechanics

- Player automatically moves forward (Z-axis)
- Speed increases gradually as you progress
- Avoid red cube obstacles
- Game ends on collision
- Score = distance traveled
- Tap/click anywhere to restart after game over

## Technical Details

- **Language**: C#
- **Unity Version**: 2022.3 LTS or newer
- **Dependencies**: None (uses only UnityEngine)
- **Lines of Code**: 369
- **External Assets**: None required
- **Setup Time**: < 1 minute

## Code Structure

The script is organized into logical sections:
- Player settings and movement
- Obstacle spawning and management
- Camera follow and tilt
- Game state management
- Input handling (touch & mouse)
- UI rendering (OnGUI)

## Performance Optimizations

- GUI styles cached to avoid per-frame allocations
- Obstacles destroyed when far behind player
- Input sensitivity defined as constant for easy tuning

## License

Open source - feel free to use and modify for your projects!