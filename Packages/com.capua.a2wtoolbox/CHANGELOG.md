# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.2.8] - 2026-11-24

### Fixed

- `UIManager.CreateCanvas`: `BackgroundBlack` 默认隐藏并关闭 `raycastTarget`（此前全屏黑色 Image 常驻会挡住 3D 场景、拦截所有 UI 点击）；出现黑边时由 `FitWidthWithSafeBorder` 控制显示。
- `UIMask` 的 Image 关闭 `raycastTarget`（`showMaskGraphic=false` 只是不绘制图形，不会阻止射线检测）。
- `UIManager.DestroyAllExceptLoading`: 增加 `panels` null 防御（编辑器直接进入非 Launch 场景时 UIManager 尚未初始化会抛 ArgumentNullException）。

### Changed

- `FitWidthWithSafeBorder`: 持有 `BackgroundBlack` 引用，按黑边状态控制其显隐。
- 第三方同步：NaughtyAttributes 2.1.4 → **2.1.6**（FilePath/FolderPath/RequiredType 等新特性，来自 github.com/dbrizov/NaughtyAttributes）。
- 重写 README（真实功能文档），新增 AGENTS.md（AI 协作规范）。

## [0.2.7]

- UI 分辨率适配重构：`Init(referenceResolution)` 可传设计分辨率；`FitWidthWithSafeBorder`（Match Width + 长屏黑边）；面板回收策略 `PanelRecycleStrategy`（DestroyImmediately / CacheInPool / KeepInMemory）与对象池；`AssetsLoader` Android 初始化分支。

## [0.1.0] - 2022-11-01

- 首次发布（单例/UIManager/AssetsLoader/SceneLoader/AudioManager/PoolManager 与第三方集成）。
