# Unity QuickAccess - 收藏夹工具

[English](README.md) | [简体中文](README_CN.md)

收藏常用资源，快速访问最近编辑的资源。支持搜索、类型筛选和拖拽排序。

<p align="center">
  <img src="res/Main.png" width="80%" alt="QuickAccess 搜索栏、收藏与最近资源列表">
</p>

## 功能

- **收藏排序**：点击星标收藏，拖动左侧手柄调整顺序。
- **最近记录**：自动记录最近保存或导入的 50 个资源。
- **搜索筛选**：按名称或类型筛选收藏与最近记录。
- **项目内保存**：重启后保留记录，资源改名或移动后保留收藏（需保留 `.meta` 文件）。
- **界面**：跟随编辑器主题，可拖动分割条调整面板大小。

## 安装

在 **Window > Package Manager** 中，点击 **+ > Add package by git URL...**，输入：

```text
https://github.com/SHW2002/Unity-Favorites-Tool.git
```

<p align="center">
  <img src="res/UPM.png" width="70%" alt="通过 Unity Package Manager 安装">
</p>

也可将仓库中的 `Editor` 文件夹和 `com.shw.favoritestool.asmdef` 复制到项目的 `Assets` 下。

## 使用

通过 **Window > QuickAccess** 或 **Tools > QuickAccess** 打开窗口，快捷键为 `Ctrl+Shift+Q` / `Cmd+Shift+Q`。

| 操作 | 方法 |
| --- | --- |
| 收藏或取消收藏 | 点击条目右侧星标 |
| 选择并定位资源 | 单击条目 |
| 打开资源 | 双击条目 |
| 调整收藏顺序 | 清除搜索后，拖动左侧手柄 |
| 调整面板大小 | 拖动 Favorites 与 Recent 之间的分割条 |

Project 窗口的右键菜单也提供 **Add to QuickAccess Favorites** 和 **Remove from QuickAccess Favorites**。

<p align="center">
  <img src="res/MenuOptions.png" width="60%" alt="Project 窗口的收藏菜单">
</p>

排序时，拖到列表边缘可自动滚动；按 Esc 或在列表外松开可取消。Recent 按最近编辑顺序自动排列。

### 搜索

顶部搜索栏同时筛选两个列表，保留原有顺序。支持名称（`Player`）、类型（`t:prefab`、`t:SceneAsset`）和组合查询（`Player t:prefab`），不区分大小写。

**Type** 菜单可快速选择常见资源类型。停止输入片刻后更新结果；清除搜索可恢复全部条目和拖拽排序。

### 存储与升级

配置保存在项目的 `UserSettings/QuickAccessSettings.asset`。建议将 `/UserSettings/` 加入 Git 忽略规则，移动项目或换电脑时一起保留。

首次使用会自动迁移旧 EditorPrefs 中可解析的资源并保留顺序，优先读取项目专属记录。旧记录不会删除，新版改动也不会同步回旧版。删除配置文件会重新触发迁移。

## 环境要求

Unity 2021.3 或更高版本。

## 许可证

MIT
