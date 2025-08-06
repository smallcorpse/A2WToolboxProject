using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using A2W;
using Cysharp.Threading.Tasks;
using System.Linq;

namespace A2W
{
    public class UIManager : Singleton<UIManager>
    {
        public const string panel_prefabs_dir_path = "Assets/Gameplay/Prefabs/UIPanel/"; // UI面板预制体目录

        // 这里保存IUIPanel的list
        List<UIPanel> panels;
        private Canvas mainCanvas;
        private GameObject canvasObject;

        public void Init()
        {
            if (panels is null)
            {
                panels = new List<UIPanel>();
            }

            // 生成canvas
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

            canvasObject = new GameObject("UICanvas");
            canvasObject.transform.SetParent(transform, false);

            mainCanvas = canvasObject.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            //mainCanvas.sortingOrder = 1000;

            UnityEngine.UI.CanvasScaler canvasScaler = canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);

            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        public async UniTask InitPanel<T>() where T : UIPanel
        {
            // 在这里加载对应panel的预制体
            T panel = await AssetsLoader.instance.LoadPrefab<T>(GetPanelPath<T>());

            // 加载完了以后存在panels里
            if (panel != null)
            {
                panel.transform.SetParent(mainCanvas.transform, false);
                panels.Add(panel);

                panel.Init();
            }
        }

        public async UniTask ShowPanel<T>() where T : UIPanel
        {
            // 先判断panels里有没有对应的panel，没有就先调用InitPanel
            T panel = GetPanel<T>();
            if (panel == null)
            {
                await InitPanel<T>();
                panel = GetPanel<T>();
            }

            // 播放Show动画 直接调用UIPanel的 public abstract UniTask Show();
            if (panel != null)
            {
                await panel.Show();
            }
        }

        public async UniTask HidePanel<T>() where T : UIPanel
        {
            // 播放Hide动画 直接调用UIPanel的 public abstract UniTask Hide();
            T panel = GetPanel<T>();
            if (panel != null)
            {
                await panel.Hide();
            }

            // 后续会增加回收机制，资源紧张时释放不需要的panel
        }

        public T GetPanel<T>() where T : UIPanel
        {
            if (panels == null) return null;
            return panels.OfType<T>().FirstOrDefault();
        }

        private string GetPanelPath<T>() where T : UIPanel
        {
            return panel_prefabs_dir_path + typeof(T).Name; // 使用Name而不是ToString()获取更简洁的类型名
        }
    }
}