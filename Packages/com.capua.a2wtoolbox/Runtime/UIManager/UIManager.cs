using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;

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
        public Vector2 CanvasSize { get; private set; }

        public const string panel_prefabs_dir_path = "Assets/Gameplay/Prefabs/UIPanel/";

        List<UIPanel> panels;
        private Canvas mainCanvas;
        private GameObject canvasObject;
        private string defaultPanelPath;

        // 对象池缓存（用于CacheInPool策略）
        private Dictionary<string, UIPanel> panelPool = new Dictionary<string, UIPanel>();

        public void Init(Vector2 canvasSize = default, string defaultPanelPath = panel_prefabs_dir_path)
        {
            this.CanvasSize = canvasSize;
            this.defaultPanelPath = defaultPanelPath;

            if (panels is null)
            {
                panels = new List<UIPanel>();
            }

            CreateCanvas(canvasSize);
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

        private void CreateCanvas(Vector2 canvasSize)
        {
            if (canvasObject is not null) return;

            canvasObject = new GameObject("UICanvas");
            canvasObject.transform.SetParent(transform, false);

            mainCanvas = canvasObject.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            UnityEngine.UI.CanvasScaler canvasScaler = canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;

            if (canvasSize.Equals(default))
            {
                canvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);
            }
            else
            {
                canvasScaler.referenceResolution = canvasSize;
            }

            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
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
                tPanel.transform.SetParent(mainCanvas.transform, false);
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
                panel.transform.SetParent(mainCanvas.transform, false);
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