using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine.UI;

namespace A2W
{
    // 面板回收策略枚举
    public enum PanelRecycleStrategy
    {
        DestroyImmediately,  // 立即销毁
        CacheInPool,         // 缓存到对象池（后续可扩展）
        KeepInMemory         // 常驻内存（默认）
    }

    public class UIManager : Singleton<UIManager>
    {
        // 合并：统一为ReferenceResolution（原CanvasSize + ReferenceResolution）
        public Vector2 ReferenceResolution { get; private set; } = new Vector2(720, 1280); // 默认设计分辨率

        public const string panel_prefabs_dir_path = "Assets/Gameplay/Prefabs/UIPanel/";

        List<UIPanel> panels;
        private Canvas mainCanvas;
        private GameObject canvasObject;
        private string defaultPanelPath;
        // 核心UI容器（所有面板挂载到这个对象下）
        private RectTransform contentRoot;

        // 对象池缓存（用于CacheInPool策略）
        private Dictionary<string, UIPanel> panelPool = new Dictionary<string, UIPanel>();

        /// <summary>
        /// 初始化UIManager
        /// </summary>
        /// <param name="referenceResolution">设计分辨率（替代原canvasSize）</param>
        /// <param name="defaultPanelPath">面板预制体默认路径</param>
        public void Init(Vector2 referenceResolution = default, string defaultPanelPath = panel_prefabs_dir_path)
        {
            // 优先使用传入的分辨率，否则使用默认值
            if (referenceResolution != default)
            {
                this.ReferenceResolution = referenceResolution;
            }

            this.defaultPanelPath = defaultPanelPath;

            if (panels is null)
            {
                panels = new List<UIPanel>();
            }

            CreateCanvas();
        }

        public void DestroyAllExceptLoading()
        {
            var panelsToRemove = panels.Where(panel => !(panel is ITransition)).ToList();

            foreach (var panel in panelsToRemove)
            {
                Destroy(panel.gameObject);
                panels.Remove(panel);
            }
        }

        private void CreateCanvas()
        {
            if (canvasObject is not null) return;

            // 1. 创建Canvas根对象
            canvasObject = new GameObject("UICanvas");
            canvasObject.transform.SetParent(transform, false);

            mainCanvas = canvasObject.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // 2. 配置CanvasScaler（核心：Scale With Screen Size + Match Width）
            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = this.ReferenceResolution; // 使用合并后的分辨率
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0; // 0 = Match Width，1 = Match Height

            canvasObject.AddComponent<GraphicRaycaster>();

            // 3. 创建全屏黑底（用于显示上下黑边）
            GameObject bgBlack = new GameObject("BackgroundBlack");
            bgBlack.transform.SetParent(canvasObject.transform, false);
            Image bgImage = bgBlack.AddComponent<Image>();
            bgImage.color = Color.black; // 黑边颜色，可根据需求修改
            RectTransform bgRect = bgBlack.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // 4. 创建Mask遮罩（限制核心UI区域）
            GameObject maskObj = new GameObject("UIMask");
            maskObj.transform.SetParent(canvasObject.transform, false);
            Mask mask = maskObj.AddComponent<Mask>();
            mask.showMaskGraphic = false; // 隐藏遮罩自身的图形
            Image maskImage = maskObj.AddComponent<Image>();
            maskImage.color = Color.clear; // 透明遮罩
            RectTransform maskRect = maskObj.GetComponent<RectTransform>();
            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;

            // 5. 创建核心UI容器（所有面板挂载到这里）
            GameObject contentObj = new GameObject("ContentRoot");
            contentObj.transform.SetParent(maskObj.transform, false);
            contentRoot = contentObj.GetComponent<RectTransform>();
            contentRoot.anchorMin = Vector2.zero;
            contentRoot.anchorMax = Vector2.one;
            contentRoot.offsetMin = Vector2.zero;
            contentRoot.offsetMax = Vector2.zero;

            // 6. 添加适配脚本，自动计算ContentRoot的尺寸
            FitWidthWithSafeBorder fitScript = contentObj.AddComponent<FitWidthWithSafeBorder>();
            fitScript.referenceResolution = this.ReferenceResolution;
        }

        public async UniTask<T> InitPanel<T>() where T : UIPanel
        {
            return await InitPanel<T>(GetPanelPath<T>());
        }

        public async UniTask<T> InitPanel<T>(string path) where T : UIPanel
        {
            // 先检查对象池是否有缓存
            var panelName = typeof(T).Name;
            if (panelPool.TryGetValue(panelName, out var cachedPanel) && cachedPanel is T)
            {
                var tPanel = cachedPanel as T;
                // 挂载到contentRoot而非mainCanvas
                tPanel.transform.SetParent(contentRoot, false);
                tPanel.gameObject.SetActive(true);
                panels.Add(tPanel);
                panelPool.Remove(panelName);
                tPanel.Init();
                return tPanel;
            }

            // 再加载
            T panel = await AssetsLoader.instance.LoadPrefab<T>(path);
            if (panel != null)
            {
                // 挂载到contentRoot而非mainCanvas
                panel.transform.SetParent(contentRoot, false);
                panels.Add(panel);
                panel.Init();
            }

            return panel;
        }

        public async UniTask<T> ShowPanel<T>() where T : UIPanel
        {
            T panel = GetPanel<T>();
            if (panel == null)
            {
                await InitPanel<T>();
                panel = GetPanel<T>();
            }

            if (panel != null)
            {
                await panel.Show();
            }

            return panel;
        }

        // 带回收策略的HidePanel重载
        public async UniTask<T> HidePanel<T>(PanelRecycleStrategy strategy = PanelRecycleStrategy.KeepInMemory) where T : UIPanel
        {
            T panel = GetPanel<T>();
            if (panel != null)
            {
                await panel.Hide();

                // 根据策略执行回收
                if (strategy != PanelRecycleStrategy.KeepInMemory)
                {
                    RecyclePanel(panel, strategy);
                }
            }

            return panel;
        }

        // 回收指定面板
        public void RecyclePanel<T>(PanelRecycleStrategy strategy = PanelRecycleStrategy.DestroyImmediately) where T : UIPanel
        {
            var panel = GetPanel<T>();
            if (panel != null)
            {
                RecyclePanel(panel, strategy);
            }
        }

        // 回收全部非活跃面板
        public void RecycleAllInactivePanels(PanelRecycleStrategy strategy = PanelRecycleStrategy.DestroyImmediately)
        {
            if (panels == null) return;

            var panelsToRecycle = panels
                .Where(p => !p.gameObject.activeSelf) // 仅移除非活跃面板
                .ToList();

            foreach (var panel in panelsToRecycle)
            {
                RecyclePanel(panel, strategy);
            }
        }

        // 清空对象池
        public void ClearPanelPool()
        {
            foreach (var panel in panelPool.Values)
            {
                Destroy(panel.gameObject);
            }
            panelPool.Clear();
        }

        // 核心回收逻辑（移除了核心面板判断）
        private void RecyclePanel(UIPanel panel, PanelRecycleStrategy strategy)
        {
            panels.Remove(panel);

            switch (strategy)
            {
                case PanelRecycleStrategy.DestroyImmediately:
                    Destroy(panel.gameObject);
                    //Debug.Log($"已销毁面板: {panel.GetType().Name}");
                    break;

                case PanelRecycleStrategy.CacheInPool:
                    panel.gameObject.SetActive(false);
                    panel.transform.SetParent(transform, false); // 移出Canvas
                    panelPool[panel.GetType().Name] = panel;
                    //Debug.Log($"已缓存面板到对象池: {panel.GetType().Name}");
                    break;
            }
        }

        public T GetPanel<T>() where T : UIPanel
        {
            if (panels == null) return null;
            return panels.OfType<T>().FirstOrDefault();
        }

        private string GetPanelPath<T>() where T : UIPanel
        {
            return defaultPanelPath + typeof(T).Name;
        }
    }

}