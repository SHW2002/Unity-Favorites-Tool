# Unity QuickAccess - Favorites Tool

[English](README.md) | [简体中文](README_CN.md)

A lightweight Unity Editor window that provides quick access to your favorite and recently edited assets.

<p align="center">
  <img src="res/Main.png" width="40%" alt="Main Window">
</p>

## Features

- **Favorites Panel** — Star any asset to pin it to your favorites list for instant access.
- **Recent Assets Panel** — Automatically tracks the last 50 assets you saved, imported, or modified.
- **Star Toggle** — Each asset row has a star icon on the right side. Click it to add/remove the asset from favorites.
- **Single Click** — Select and ping the asset in the Project window.
- **Double Click** — Open the asset directly (e.g., open a script in your IDE, a prefab in Prefab Mode, etc.).
- **Context Menu** — Right-click any asset in the Project window and choose **Add to QuickAccess Favorites** or **Remove from QuickAccess Favorites**.
- **Resizable Split View** — Drag the splitter bar between the Favorites and Recent panels to adjust their sizes.
- **Light and Dark Themes** — Automatically follows the active Unity Editor theme.
- **Persistent Storage** — Favorites and recent lists are saved via `EditorPrefs` and survive Unity restarts.
- **Auto Cleanup** — Deleted or missing assets are automatically removed from both lists.

## Installation

### Option 1: Install via Git URL (Recommended)

1. Open Unity Package Manager (**Window > Package Manager**)
2. Click the **+** button in the top-left corner
3. Select **Add package by git URL...**
4. Enter the repository URL: `https://github.com/SHW2002/Unity-Favorites-Tool.git`
5. Click **Add**

<p align="center">
  <img src="res/UPM.png" width="70%" alt="Unity Package Manager">
</p>

### Option 2: Manual Install

Copy the repository's `Editor` folder and assembly definition into your Unity project.

## Usage

Open the window via the menu: **Tools > QuickAccess** (shortcut: `Ctrl+Shift+Q` / `Cmd+Shift+Q`).

### Adding Favorites

| Method                      | How                                                     |
| --------------------------- | ------------------------------------------------------- |
| From the QuickAccess window | Click the star icon on any asset row                    |
| From the Project window     | Right-click an asset > **Add to QuickAccess Favorites** |

<p align="center">
  <img src="res/MenuOptions.png" width="60%" alt="MenuOptions">
</p>

### Removing Favorites

| Method                      | How                                                          |
| --------------------------- | ------------------------------------------------------------ |
| From the QuickAccess window | Click the star icon again to unstar                          |
| From the Project window     | Right-click an asset > **Remove from QuickAccess Favorites** |

## Requirements

- Unity 2021.3 or later (uses C# 9 target-typed `new()` syntax)

## License

MIT
