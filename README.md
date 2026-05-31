# Conveyor Puzzle Prototype

## Overview

This project is a small puzzle prototype built in Unity 6 that demonstrates conveyor-based item routing on a grid-based board.

The player interacts with puzzle elements to guide moving items from a spawn point to the correct destination while avoiding incorrect routes and failed deliveries. The prototype focuses on clean gameplay architecture, extensibility, and puzzle-solving mechanics rather than visual presentation.

---

## Features

### Core Systems

* Grid-based board
* Tile-driven movement system
* Conveyor belt routing
* Item movement between tiles
* Win/Loss conditions
* Three playable levels with increasing difficulty

### Interactive Puzzle Elements

#### 1. Switches

Switches can be activated by the player to change the routing path at conveyor forks.

#### 2. Teleporters

Items entering a teleporter instantly exit from a linked teleporter elsewhere on the board.

#### 3. Color Goals

Items must reach a destination matching their assigned color. Delivering an item to the wrong goal results in level failure.

#### 4. Conveyor Reverse Button

Special buttons reverse the direction of connected conveyors, allowing players to alter item flow and solve routing challenges.

---

## Project Architecture

The project follows a modular tile-based architecture.

### Main Components

#### Grid Manager

Responsible for:

* Creating and managing the grid
* Tile lookup
* Coordinate management

#### Tile System

All gameplay tiles inherit from a common base tile class.

Examples:

* ConveyorTile
* SwitchTile
* TeleporterTile
* GoalTile
* SpawnTile

This allows new puzzle mechanics to be added without modifying the item movement system.

#### Item Controller

Responsible for:

* Tile-to-tile movement
* Reading conveyor directions
* Triggering tile interactions
* Detecting win/loss conditions

#### Level Manager

Responsible for:

* Level completion
* Failure detection
* Restart functionality

---

## Levels

### Level 1 – Basic Routing

Introduces:

* Conveyor movement
* Goal delivery
* Simple switch interaction

Objective:
Guide the item from spawn to goal.

---

### Level 2 – Teleporters

Introduces:

* Teleporter mechanics
* Alternate routing paths

Objective:
Use switches and teleporters to reach the destination.

---

### Level 3 – Advanced Routing

Introduces:

* Conveyor reversal buttons
* Multiple routing decisions
* Color-based delivery

Objective:
Manipulate the conveyor network to deliver the item to the correct goal.

---

## Win Conditions

A level is completed when:

* All required items successfully reach their destination.
* Correct color matching requirements are satisfied.

---

## Loss Conditions

A level fails when:

* An item reaches an incorrect destination.
* An item exits the valid board area.
* An item enters an invalid routing state.

---

## Design Decisions & Tradeoffs

### Why a Tile-Based Architecture?

A tile-based system provides:

* Clear separation of responsibilities
* Easy addition of new mechanics
* Better maintainability
* Faster level creation

### Why Manual Level Creation?

Levels are created directly inside Unity for simplicity and rapid iteration during the assessment.

For a larger project, level data would be stored using ScriptableObjects or external level files.

---

## Improvements With More Time

If given additional development time, I would add:

### Gameplay

* Multiple simultaneous items
* Additional puzzle mechanics
* Dynamic obstacles
* Undo system
* Puzzle hints

### Tools

* Custom level editor
* ScriptableObject-based level definitions
* Automated puzzle validation

### Technical

* Object pooling
* Save/Load system
* Event-driven architecture
* Unit tests for gameplay systems

### Presentation

* Visual conveyor animations
* Particle effects
* Audio feedback
* Improved UI and transitions

---

## Controls

### Mouse

* Left Click: Interact with switches and buttons

### Keyboard

* R: Restart current level

---

## Unity Version

Unity 6

---

## Author

Shailja Shrivastava
Game Developer
