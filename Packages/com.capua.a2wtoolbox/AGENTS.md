# AGENTS.md — A2WToolbox AI 协作规范

> 本文件供 AI 编码代理（CodeBuddy 等）在本包内工作时遵循。包名 `com.capua.a2wtoolbox`，Unity 2021.3+。

## 包定位与边界

- 本包是**通用框架层**：只提供与具体游戏无关的能力（单例、UI 管理、适配、加载、场景、音频、池、事件）。
- **禁止**在包内引用任何游戏工程类型（`Gameplay.*` 命名空间）。依赖方向必须永远是：游戏工程 → 本包 → 第三方（YooAsset）。
- 包路径：宿主工程内以 **embedded package** 形态存在（`Packages/com.capua.a2wtoolbox/`）。修改会直接影响编译，无 PackageCache 只读限制。

## 修改规则

1. **包内改动必须考虑同步回 git 仓库**（gitee `smallcorpse_rui/A2WToolbox`）。凡修改 `Runtime/`、`Editor/`、`3rd/` 源码，在提交信息或 README「维护备忘」中记录改动点，供作者同步。
2. **不升级 3rd 目录内第三方库**，除非用户明确要求；升级时同步更新 README「第三方同步状态」与 `package.json` 版本。
3. `Runtime/` 代码必须放对子目录（AudioManager/Loader/Singleton/UIManager/...），新模块先建目录再加 asmdef 兼容的脚本。
4. UI 相关改动注意三个平台语义差异：ScreenSpaceOverlay 常驻 Image 会**挡 3D 并拦截射线**（务必 `raycastTarget=false` 或默认隐藏）；`Canvas.willRenderCanvases` 订阅必须在 `OnDestroy` 退订；黑边/适配逻辑以 `ReferenceResolution` 为唯一基准，不得再读 `Screen.width/height` 做参考分辨率快照。

## 已知约束（踩坑记录，勿重蹈）

- `UIManager.Init` 的 `referenceResolution` 参数默认值是 **720x1280 竖屏**——横屏游戏调用方必须显式传 `(1920, 1080)` 这类横屏分辨率。
- `Singleton.instance` getter 触发创建；`UIManager.panels` 在 `Init()` 前为 null（包内已加 null 防御，调用方仍应在场景 Awake 里先 `Init` 再操作面板）。
- `DestroyAllExceptLoading` 会销毁所有非 `ITransition` 面板——跨场景切换场景时依赖此语义，勿改为软隐藏。
- `FitWidthWithSafeBorder` 通过 `Canvas.willRenderCanvases` 每帧校正，改动需保持幂等且轻量（禁止在回调里实例化对象）。

## API 契约速查

| API | 语义 |
|---|---|
| `UIManager.instance.Init(Vector2)` | 初始化 Canvas + 适配（CreateCanvas 幂等，仅首次生效） |
| `ShowPanel<T>()` | 无则加载实例化（`Assets/Gameplay/Prefabs/UIPanel/<T>.prefab`）→ `panel.Init()` → `panel.Show()` |
| `HidePanel<T>(strategy)` | `panel.Hide()` 后按 `PanelRecycleStrategy` 回收（默认 KeepInMemory） |
| `RecycleAllInactivePanels(strategy)` | 回收全部非活跃（`activeSelf == false`）面板 |
| `AssetsLoader.instance.Init()` | YooAsset OfflinePlayMode 初始化（UNITY_ANDROID 走 BuildinFileSystem 分支） |
| `Singleton<T>.instance` | 全局单例；`SceneSingleton<T>.instance` 场景单例 |

## 验证要求

包内任何代码改动后：
1. 宿主工程编译 0 错误（宿主工程有一键编译检查脚本时优先使用）。
2. 涉及 UI 的改动：在宿主工程 Play 验证——面板显示/隐藏/回收、不同窗口比例下的黑边与点击响应。
3. 涉及 `3rd/` 的改动：确认 asmdef 引用与 .meta GUID 未被破坏（第三方文件应连同 .meta 一起整体替换）。
