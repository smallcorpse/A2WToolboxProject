# A2WToolbox

Unity 2021.3+ 自研工具包：为单机/小型多场景游戏提供**单例、UI 管理、UI 分辨率适配、资源加载、场景切换、音频、事件、状态机**等基础能力的开箱即用方案。

当前维护方式：**嵌入包（embedded package）**——宿主工程 `Packages/com.capua.a2wtoolbox/`。对源码的修改不会经 git 依赖自动更新；若曾以 git URL 引用（`https://gitee.com/smallcorpse_rui/A2WToolbox.git`），请注意把嵌入侧的修复同步回仓库，反之亦然。

## 功能总览

| 模块 | 类型 | 说明 |
|---|---|---|
| `Singleton` / `SceneSingleton` | Runtime | 全局单例与场景单例（场景卸载自动清理） |
| `UIManager` | Runtime | 动态 Canvas + 面板加载/显示/隐藏/**回收策略** + **分辨率适配** |
| `FitWidthWithSafeBorder` | Runtime | UI 安全区适配：锁定设计宽高比，非设计比例屏幕自动加黑边 |
| `AssetsLoader` | Runtime | YooAsset 封装：OfflinePlayMode 初始化（含 Android 分支）、Prefab/Asset 加载 |
| `SceneLoader` | Runtime | 场景切换与传递（配合 `ITransition` 面板） |
| `AudioManager` | Runtime | 音频播放管理 |
| `PoolManager` | Runtime | 对象池 |
| `Extensions` / `Utils` | Runtime | 常用扩展方法与工具类 |
| `Hierponent` | Editor | Hierarchy 面板增强工具 |
| NaughtyAttributes | 3rd | Inspector 特性扩展（v2.1.6，自 github.com/dbrizov 同步） |
| UniTask | 3rd | 零分配异步（v2.3.2） |
| StateMachine | 3rd | MonsterLove 状态机 |
| EventBetter | 3rd | 弱引用事件总线 |
| UnityLoggerExtended | 3rd | 日志增强 |

依赖：`com.tuyoogame.yooasset`（2.2.9，openupm scoped registry）。

## 安装

**方式 A：嵌入包（推荐，可本地修改）**

```
<工程>/Packages/com.capua.a2wtoolbox/   # 整个目录放入 Packages 即可
```

**方式 B：git URL**

在 `Packages/manifest.json` 的 `dependencies` 中加入：

```json
"com.capua.a2wtoolbox": "https://gitee.com/smallcorpse_rui/A2WToolbox.git"
```

## 快速上手

### 1. 初始化 UIManager（横屏游戏务必传入设计分辨率）

```csharp
// 默认 720x1280 是竖屏基准，横屏游戏必须显式传入，例如：
UIManager.instance.Init(new Vector2(1920, 1080));
```

### 2. UI 面板

```csharp
public class MyPanel : UIPanel
{
    public override UniTask Show() { gameObject.SetActive(true); return UniTask.CompletedTask; }
    public override UniTask Hide() { gameObject.SetActive(false); return UniTask.CompletedTask; }
    public override void Init() { /* 首次实例化后调用 */ }
}

// 显示（不存在则自动从 Assets/Gameplay/Prefabs/UIPanel/ 加载实例化）
await UIManager.instance.ShowPanel<MyPanel>();

// 隐藏（默认常驻内存；可选回收策略）
await UIManager.instance.HidePanel<MyPanel>();                                  // KeepInMemory
await UIManager.instance.HidePanel<MyPanel>(PanelRecycleStrategy.CacheInPool);  // 缓存进池
await UIManager.instance.HidePanel<MyPanel>(PanelRecycleStrategy.DestroyImmediately);

// 资源紧张时回收全部非活跃面板
UIManager.instance.RecycleAllInactivePanels();
UIManager.instance.ClearPanelPool();
```

### 3. UI 分辨率适配（FitWidthWithSafeBorder）

UIManager 创建 Canvas 时自动构建以下层级，无需手工配置：

```
UICanvas (CanvasScaler: ScaleWithScreenSize, Match Width)
├── BackgroundBlack        # 黑边（仅出现黑边时显示，不拦截点击）
└── UIMask (Mask)
    └── ContentRoot        # 所有面板挂载于此；挂 FitWidthWithSafeBorder
```

行为：窗口比例与设计比例一致时全屏铺满；更高/更窄的屏幕在上下（或左右）自动出黑边，UI 永远锁定设计宽高比、不裁切不变形。黑边颜色可在 `UIManager.CreateCanvas` 中修改。

> 注意：该方案只作用于 UI 层。相机渲染的世界画面由相机 aspect 自适应，无黑边。

### 4. 资源加载（AssetsLoader，基于 YooAsset）

```csharp
await AssetsLoader.instance.Init();          // OfflinePlayMode，含 Android 内置文件系统分支
var prefab = await AssetsLoader.instance.LoadPrefab<MyPanel>("Assets/.../MyPanel.prefab");
var sprite = await AssetsLoader.instance.LoadAsset<Sprite>("Assets/.../icon.png", false);
```

### 5. 单例

```csharp
public class GameManager : Singleton<GameManager> { }       // 全局
public class WorldManager : SceneSingleton<WorldManager> { } // 场景级，随场景卸载
```

### 6. 事件（EventBetter）

```csharp
public struct MyMessage : IEventMessage { }

this.Listen<MyMessage>(OnMyMessage);   // 订阅（弱引用）
this.DelListen<MyMessage>(OnMyMessage); // 退订——务必在 OnDestroy 配对调用
```

## 目录结构

```
com.capua.a2wtoolbox/
├── package.json
├── README.md / CHANGELOG.md / Third Party Notices.md
├── Editor/            # Hierponent（编辑器工具）
├── Runtime/
│   ├── AudioManager/  Extensions/  Loader/  PoolManager/
│   ├── SceneLoader/   Singleton/   UIManager/  Utils/
│   └── UIManager/FitWidthWithSafeBorder.cs   # UI 安全区适配
└── 3rd/               # 第三方（NaughtyAttributes / UniTask / StateMachine / EventBetter / UnityLoggerExtended）
```

## 版本记录

- **0.2.7** — UI 分辨率适配重构：`Init(referenceResolution)`、FitWidthWithSafeBorder 黑边方案、面板回收策略（DestroyImmediately / CacheInPool / KeepInMemory）、AssetsLoader Android 初始化分支。
- 0.2.6 及以前 — 基础能力建设（单例/UIManager/AssetsLoader/SceneLoader/AudioManager/PoolManager + 第三方集成）。

## 维护备忘

- 嵌入侧相对 0.2.7 的本地修复（同步回 gitee 时勿遗漏）：
  1. `UIManager.CreateCanvas`：BackgroundBlack 默认隐藏 + `raycastTarget=false`（否则挡 3D 场景、拦截点击）
  2. `UIMask` 的 Image `raycastTarget=false`（showMaskGraphic=false 只是不绘制，不阻止射线）
  3. `DestroyAllExceptLoading` 的 `panels` null 防御（编辑器直接进入非 Launch 场景时未 Init 会炸）
  4. `FitWidthWithSafeBorder` 持有 BackgroundBlack 引用并按黑边状态控制显隐
- 第三方同步状态：NaughtyAttributes **2.1.4 → 2.1.6**（2026-11-24，来自 github.com/dbrizov/NaughtyAttributes）；UniTask 2.3.2、StateMachine、EventBetter、UnityLoggerExtended 沿用当时版本。
