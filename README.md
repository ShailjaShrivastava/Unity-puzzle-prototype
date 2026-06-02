# Conveyor Puzzle Prototype

A grid-based conveyor routing puzzle built in Unity 6 using C#. The player modifies conveyor behavior and routing elements to guide colored parcels from their starting positions to matching destination gates.

The prototype demonstrates gameplay architecture, puzzle logic, runtime validation, and multiple interactable mechanics using generated runtime visuals rather than authored art assets.

Open `Assets/Scenes/ConveyorPuzzle.unity` and press Play.

---

# Controls

| Action                     | Input                           |
| -------------------------- | ------------------------------- |
| Toggle Switch              | Left Click on orange `SW` tile  |
| Reverse Conveyor Direction | Left Click on purple `REV` tile |
| Start / Pause Simulation   | Space or `Run` button           |
| Restart Current Level      | R or `Restart` button           |
| Jump to Level 1            | 1                               |
| Jump to Level 2            | 2                               |
| Jump to Level 3            | 3                               |

Yellow preview markers display the predicted parcel route while planning.

---

# Assessment Requirements Coverage

✓ Grid-based board

✓ Moving parcels traveling along conveyor paths

✓ Switch interactable (fork routing)

✓ Conveyor reverse button

✓ Blocker mechanic

✓ Teleporter mechanic

✓ Color gate mechanic

✓ Win condition

✓ Loss condition

✓ Three playable levels

✓ Runtime level validation

---

# Implemented Mechanics

### Conveyors

Parcels travel tile-by-tile across directional conveyor belts.

### Switches

Fork tiles that can redirect parcels between multiple paths.

### Conveyor Reverse Button

Reverses the active conveyor drive direction and changes parcel travel behavior.

### Color Gates

Destination gates require parcels of a matching color. Delivering a parcel to the wrong gate causes a failure state.

### Blockers

Reverse parcel travel direction when contacted.

### Teleporters

Instantly move parcels between paired teleporter locations while preserving puzzle flow.

### Route Preview

The game predicts and visualizes parcel routes before simulation begins, helping players understand the current configuration.

### Runtime Validation

Levels are validated when loaded to detect:

* Missing start points
* Missing destination gates
* Invalid teleporter pairs
* Duplicate tile placements
* Invalid spawn configurations

---

# Levels

## Level 1 — Fork Sorting

Introduces:

* Conveyor routing
* Switch interaction
* Color gates

The player must correctly route a parcel to the matching destination.

---

## Level 2 — Reverse Belt

Introduces:

* Conveyor reverse button
* Direction management

The player must reverse conveyor travel to successfully deliver the parcel.

---

## Level 3 — Teleporter Fork

Combines:

* Multiple switches
* Blockers
* Teleporter pairs
* Color gate routing

This level requires planning several mechanics together to reach the correct destination.

---

# Architecture

The project follows a lightweight gameplay architecture with clear separation between level data, runtime state, rendering, and gameplay logic.

```text
LevelData
    ↓
ConveyorPuzzleGame
    ↓
RuntimeState
    ↓
BoardView / ParcelView
```

## Core Scripts

### LevelData.cs

Contains all level definitions and puzzle configuration data.

### ConveyorPuzzleGame.cs

Primary gameplay coordinator responsible for:

* Simulation updates
* Input handling
* Win/loss evaluation
* Level loading
* Camera setup
* HUD integration

### RuntimeState.cs

Stores mutable runtime information for:

* Tiles
* Parcels
* Interactive elements

### BoardView.cs

Builds and updates the generated puzzle board, arrows, labels, and interactive tiles.

### ParcelView.cs

Creates and updates parcel visuals during simulation.

### HudView.cs

Handles the in-game user interface and player commands.

### PuzzleGrid.cs

Provides:

* Grid coordinate management
* World-space conversion
* Movement interpolation
* Bounds checking

### GridDirection.cs

Centralized directional utility functions used by conveyor and movement systems.

### PuzzleTheme.cs

Provides generated materials, colors, and visual theme configuration.

### LevelValidator.cs

Validates level integrity before gameplay begins.

### PuzzleTileClickTarget.cs

Marks generated board objects as interactive targets.

---

# Technical Notes

The prototype intentionally generates all gameplay visuals at runtime using Unity primitive meshes.

Generated elements include:

* Board tiles
* Conveyor arrows
* Labels
* Parcel objects
* Camera setup
* Lighting

This keeps the project lightweight and allows gameplay systems to remain independent from content creation workflows.

The architecture was designed so additional mechanics can be introduced by extending level data, tile handling, and board rendering systems without significantly increasing complexity in the main gameplay controller.

---

# Tradeoffs

* Levels are defined in C# rather than through a custom editor to keep implementation time focused on gameplay systems.
* Primitive meshes and TextMesh labels are used instead of production art assets.
* Parcel movement uses deterministic grid stepping rather than physics simulation, ensuring predictable puzzle behavior and consistent results across frame rates.
* IMGUI was selected for the HUD to minimize setup overhead and keep focus on gameplay implementation.

---

# Future Improvements

Given additional development time, the following improvements would be prioritized:

* Custom level editor with visual authoring tools
* ScriptableObject-based level assets
* Enhanced route preview and planning tools
* Multiple parcels with collision and timing mechanics
* Conveyor animations and visual effects
* Audio feedback and polish
* Save/load functionality
* Additional puzzle mechanics and level variety
* Automated puzzle solvability analysis
* Expanded UI and accessibility features

---

# Development Environment

* Unity 6
* C#
* No third-party gameplay plugins
* Runtime-generated visuals using Unity primitives
