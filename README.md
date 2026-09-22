# Sokoban (Unity)

A grid-based Sokoban puzzle game built in Unity, featuring a custom push-mechanic system, procedurally rendered Tilemap levels, and a data-driven level format.

![Unity](https://img.shields.io/badge/Unity-2022%2B-black?logo=unity)
![C#](https://img.shields.io/badge/C%23-Gameplay-blue)

## Overview

This project is a from-scratch implementation of the classic Sokoban puzzle genre: push boxes onto marked targets to clear the level. It was built to explore grid-based game architecture in Unity — separating level *data* from Unity's *rendering* and *GameObject* layers so the core puzzle logic is simple, testable, and independent of how the level looks on screen.

## Features

- **Text-based level format** — levels are authored using the classic Sokoban ASCII notation (`#` walls, `.` targets, `$` boxes, `@` player), parsed into a lightweight grid data structure at runtime
- **Decoupled data/rendering architecture** — a plain C# `SokobanGrid`/`GameState` layer holds all puzzle logic, independent of Unity's `Tilemap` and `GameObject` systems, making the push/win logic easy to reason about and extend
- **Dynamic Tilemap rendering** — walls, floor, and target tiles are painted onto Unity Tilemaps at runtime directly from parsed level data
- **Grid-locked movement & push mechanics** — the player moves exactly one cell at a time, with full validation for walls, box collisions, and double-box pushes (illegal, as in traditional Sokoban)
- **Smooth animated movement** — coroutine-based interpolation gives the player and pushed boxes a responsive, tween-style motion rather than an instant snap
- **Directional player rotation** — the player character (a forklift) rotates to visually face its last movement direction
- **Visual variety** — boxes are spawned with a randomly chosen sprite from a configurable set, avoiding repetitive visuals across a level
- **Win detection & celebration** — the game checks all target cells against box positions after every move, and triggers a randomized confetti particle effect at each box location on level completion
- **New Input System support** — built against Unity's modern Input System package rather than the legacy Input Manager

## Architecture

The project separates concerns into three layers:

| Layer | Responsibility | Key Scripts |
|---|---|---|
| **Data** | Pure grid/puzzle logic, no Unity dependencies | `SokobanGrid`, `GameState`, `LevelParser` |
| **Rendering** | Converts grid data into visible Tilemap tiles | `TileMapper` |
| **Gameplay** | Spawns entities, handles input, movement, and win state | `LevelManager`, `PlayerController` |

This split means the puzzle rules (movement validation, push logic, win checking) can be tested or modified without touching any rendering or GameObject code — and the visual layer can be swapped or restyled without affecting how the puzzle behaves.

### Data flow

```
Level text (string[])
      ↓
LevelParser.ParseLevel()
      ↓
SokobanGrid (walls/floor/targets)  +  GameState (player/box positions)
      ↓                                    ↓
TileMapper.RenderGrid()            LevelManager.SpawnEntities()
      ↓                                    ↓
Tilemap visuals                    Player & Box GameObjects
                                            ↓
                                  PlayerController (input → move validation → animation)
                                            ↓
                                  GameState.CheckWin() → confetti on success
```

## Controls

| Input | Action |
|---|---|
| `W` / `↑` | Move up |
| `S` / `↓` | Move down |
| `A` / `←` | Move left |
| `D` / `→` | Move right |

## Tech Stack

- **Engine:** Unity (2D, URP/Built-in)
- **Language:** C#
- **Systems used:** Tilemap, Input System package, Particle System (win celebration), Coroutines (movement animation)

## What I Learned / Focus Areas

This project was primarily an exercise in:
- Designing a clean separation between **game logic** and **engine-specific rendering**, so puzzle rules stay engine-agnostic and easy to unit test
- Working with Unity's **Tilemap API** to procedurally paint levels from arbitrary data rather than hand-placing tiles
- Debugging sprite import pipelines (Pixels Per Unit, texture sizing) and coordinate-space mismatches (Tilemap Y-axis vs. text-parsing row order)
- Building responsive, non-physics grid movement using coroutine-driven interpolation

## Possible Future Additions

- Multiple levels with progression/level-select
- Undo functionality (straightforward given the existing `GameState` snapshot structure)
- Move counter and restart UI
- Sound design for pushes, wins, and invalid moves

## Setup

1. Clone the repository
2. Open the project folder in Unity Hub (Unity 2022 LTS or later recommended)
3. Open the main scene and press Play

---

*Built as a personal project to explore grid-based puzzle game architecture in Unity.*
