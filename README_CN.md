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
- **项目内存储** — 收藏、排序和最近访问记录保存到 `UserSettings/QuickAccessSettings.asset`，各项目独立，重启 Unity 后保留。
- **稳定的资源引用** — 使用资源 GUID，资源或文件夹改名、移动后仍保留收藏（需要保留 `.meta` 文件）。
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

### 调整收藏顺序

拖动 Favorites 条目左侧的原生列表手柄，在提示线所在位置松开即可调整顺序。
手柄直接调用 Unity IMGUI `ReorderableList` 的绘制方法，使用当前编辑器提供的样式。
该接口属于 `UnityEditorInternal`，升级 Unity 时仍需验证兼容性。
长列表拖到面板上、下边缘时会自动滚动。按 Esc、切换窗口或在列表区域外松开可取消拖动。
顺序会自动保存，重启 Unity 后仍然保留。新收藏仍添加到最前面，Recent 列表不参与手动排序。

### 搜索

顶部搜索栏同时筛选 Favorites 和 Recent，并保留原有顺序。支持名称（`Player`）、类型（`t:prefab`、`t:SceneAsset`）和组合查询（`Player t:prefab`），采用 Unity `AssetDatabase.FindAssets` 的查询语法，不区分大小写。
点击 **Type** 可快速选择场景、Prefab、材质、纹理、脚本、ScriptableObject、音频或动画。选择类型会替换已有的 `t:` 条件并保留名称；**All Types** 仅移除类型条件。搜索框的清除按钮会清除整个查询。
搜索范围仅限这两个列表已有的条目。筛选期间需清除搜索后才能拖拽排序，选择、打开和收藏切换仍可使用。输入短暂延迟后缓存查询结果，资源变化后自动更新。搜索内容属于窗口状态，不新增 EditorPrefs，也不改变收藏配置格式。
### 配置位置与旧数据迁移

配置通过 `ScriptableSingleton` 保存到项目根目录下的 `UserSettings/QuickAccessSettings.asset`，不在插件目录内，也不写入 PlayerPrefs。
移动或改名整个项目时，请一起保留 `UserSettings`；删除整个项目时配置也随之删除。仅卸载插件时，这个文件会保留，可自行删除。
建议在项目根目录的 Git 忽略规则中加入 `/UserSettings/`，或在 SVN 中忽略该目录。它是个人工作区偏好，不建议团队共用；换电脑时需要额外复制该文件。

首次生成配置时，会从旧 `EditorPrefs` 的 `QuickAccess_Favorites` 和 `QuickAccess_Recent` 中迁入当前项目可解析的资源，保留顺序并转换为 GUID。
旧数据没有项目标识，只能按当前项目中的路径匹配。之后只使用项目配置，清空收藏后重启不会重新导入旧收藏；删除配置文件则会重新触发迁移。
旧注册表键不会被写入或自动删除，因为其他项目可能仍需读取它们。确认所有相关项目已迁移后，才适合清理这两个旧键。

## 环境要求

- Unity 2021.3 或更高版本（使用了 C# 9 的目标类型 `new()` 语法）。

## 许可证

MIT
