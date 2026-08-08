# Unity QuickAccess - 收藏夹工具

[English](README.md) | [简体中文](README_CN.md)

一个轻量级的 Unity 编辑器窗口，提供对收藏资源和最近编辑资源的快速访问。

<p align="center">
  <img src="res/Main.png" width="40%" alt="主窗口">
</p>

## 功能特性

- **收藏夹面板** — 将任意资源标星，固定到收藏夹以便随时访问。
- **最近访问面板** — 自动追踪最近保存、导入或修改过的 50 个资源。
- **星标切换** — 每个资源行右侧都有星标图标，点击即可添加或移除收藏。
- **单击** — 选中资源并在 Project 窗口中高亮定位。
- **双击** — 直接打开资源，例如在 IDE 中打开脚本或进入 Prefab 编辑模式。
- **右键菜单** — 在 Project 窗口中右键任意资源，选择 **Add to QuickAccess Favorites** 或 **Remove from QuickAccess Favorites**。
- **可调节分割视图** — 拖拽收藏夹和最近访问面板之间的分割条来调整各区域大小。
- **浅色与深色主题** — 自动跟随当前 Unity 编辑器主题。
- **持久化存储** — 收藏列表和最近访问列表通过 `EditorPrefs` 保存，重启 Unity 后数据不丢失。
- **自动清理** — 已删除或丢失的资源会自动从列表中移除。

## 安装方法

### 方法 1：通过 Git URL 安装（推荐）

1. 打开 Unity Package Manager（**Window > Package Manager**）。
2. 点击左上角的 **+** 按钮。
3. 选择 **Add package by git URL...**。
4. 输入仓库 URL：`https://github.com/SHthemW/Unity-Favorites-Tool.git`。
5. 点击 **Add**。

<p align="center">
  <img src="res/UPM.png" width="70%" alt="Unity Package Manager">
</p>

### 方法 2：手动安装

将仓库中的 `Editor` 文件夹和程序集定义文件复制到 Unity 项目中。

## 使用方法

通过菜单打开窗口：**Tools > QuickAccess**（快捷键：`Ctrl+Shift+Q` / `Cmd+Shift+Q`）。

### 添加收藏

| 方式                  | 操作                                        |
| --------------------- | ------------------------------------------- |
| 在 QuickAccess 窗口中 | 点击资源行右侧的星标图标                    |
| 在 Project 窗口中     | 右键资源 > **Add to QuickAccess Favorites** |

<p align="center">
  <img src="res/MenuOptions.png" width="60%" alt="菜单选项">
</p>

### 移除收藏

| 方式                  | 操作                                             |
| --------------------- | ------------------------------------------------ |
| 在 QuickAccess 窗口中 | 再次点击星标图标取消收藏                         |
| 在 Project 窗口中     | 右键资源 > **Remove from QuickAccess Favorites** |

## 环境要求

- Unity 2021.3 或更高版本（使用了 C# 9 的目标类型 `new()` 语法）。

## 许可证

MIT
