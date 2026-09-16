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
- **Project-local Storage** — Favorites, ordering and recent lists are saved in `UserSettings/QuickAccessSettings.asset`, independently for each project, and survive Unity restarts.
- **Stable Asset References** — GUIDs keep favorites intact when assets or folders are renamed or moved (preserve their `.meta` files).
- **Auto Cleanup** — Deleted or missing assets are automatically removed from both lists.

## Installation

### Option 1: Install via Git URL (Recommended)

1. Open Unity Package Manager (**Window > Package Manager**)
2. Click the **+** button in the top-left corner
3. Select **Add package by git URL...**
4. Enter the repository URL: `https://github.com/SHthemW/Unity-Favorites-Tool.git`
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

### Reordering Favorites

Drag the native list handle on the left of a Favorites row and release at the insertion line.
The handle calls Unity's IMGUI `ReorderableList` renderer directly, using the current editor's style.
This API belongs to `UnityEditorInternal`; compatibility should still be checked when upgrading Unity.
Long lists scroll automatically when dragging near the top or bottom edge. Press Esc,
switch windows, or release outside the list to cancel. The order is saved automatically
and survives Unity restarts. New favorites still appear first; Recent keeps its automatic order.

### Searching

The toolbar filters Favorites and Recent together without changing their order. Search by name (`Player`), type (`t:prefab`, `t:SceneAsset`), or both (`Player t:prefab`). Queries use Unity's `AssetDatabase.FindAssets` syntax and are case insensitive.
Use **Type** to select Scenes, Prefabs, Materials, Textures, Scripts, ScriptableObjects, Audio or Animations. A selection replaces existing `t:` filters and preserves the name query; **All Types** removes only type filters. The search field's clear button removes the entire query.
Search is limited to entries already in these lists. While filtering, clear the search to reorder favorites; selection, opening and star toggles remain available. Queries are cached after a short typing delay and refreshed after project asset changes. Search text belongs to the window state, with no new EditorPrefs keys or changes to the favorites settings format.
### Storage and Legacy Migration

`ScriptableSingleton` saves preferences in `UserSettings/QuickAccessSettings.asset` relative to the project root, outside the plugin folder. PlayerPrefs is not used.
Keep `UserSettings` when moving or renaming the project. Deleting the entire project also removes its settings; uninstalling only the plugin leaves the settings file for you to remove if desired.
Ignore `/UserSettings/` in the project's root Git ignore rules, or ignore that directory in SVN. These are personal workspace preferences, not team settings; copy the file separately when moving to another computer.

When creating the settings for the first time, existing `QuickAccess_Favorites` and `QuickAccess_Recent` EditorPrefs are imported in order and converted to GUIDs, using only paths that resolve in the current project.
Legacy data has no project identity, so matching is necessarily based on the current project's paths. Subsequent runs use only the project file. An intentionally empty list stays empty on restart; deleting the settings file triggers migration again.
Legacy keys are neither written nor automatically deleted, since other projects may still need them. Clean up those two keys only after all relevant projects have migrated.

## Requirements

- Unity 2021.3 or later (uses C# 9 target-typed `new()` syntax)

## License

MIT
