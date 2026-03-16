using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine.UI;

namespace A2W
{
    // 适配脚本（挂载到ContentRoot）
    [RequireComponent(typeof(RectTransform))]
    public class FitWidthWithSafeBorder : MonoBehaviour
    {
        [Header("设计分辨率（宽x高）")]
        public Vector2 referenceResolution = new Vector2(720, 1280);
        private RectTransform rectTrans;
        private CanvasScaler canvasScaler;

        void Awake()
        {
            rectTrans = GetComponent<RectTransform>();
            // 查找父级的CanvasScaler
            canvasScaler = GetComponentInParent<CanvasScaler>();
            if (canvasScaler == null)
            {
                Debug.LogError("ContentRoot未找到CanvasScaler组件！");
                return;
            }
            // 初始化适配
            AdjustContentSize();
        }

        void Start()
        {
            // 监听Canvas渲染事件（分辨率变化/横竖屏切换时重新适配）
            Canvas.willRenderCanvases += AdjustContentSize;
        }

        void OnDestroy()
        {
            // 移除监听，避免内存泄漏
            Canvas.willRenderCanvases -= AdjustContentSize;
        }

        void AdjustContentSize()
        {
            if (canvasScaler == null || rectTrans == null) return;

            // 1. 获取设计分辨率和当前屏幕的高宽比
            float designAspect = referenceResolution.y / referenceResolution.x;
            float screenAspect = (float)Screen.height / Screen.width;

            // 2. 如果屏幕高宽比大于设计值（屏幕过长），限制ContentRoot的高度
            if (screenAspect > designAspect)
            {
                // 计算目标高度 = 屏幕宽度 * 设计高宽比
                float targetHeight = Screen.width * designAspect;
                // 计算缩放比例（相对于Canvas的缩放）
                float scaleRatio = canvasScaler.referenceResolution.x / Screen.width;
                // 转换为Canvas内的高度值
                float canvasTargetHeight = targetHeight * scaleRatio;

                // 设置ContentRoot的尺寸（居中显示）
                rectTrans.sizeDelta = new Vector2(0, canvasTargetHeight);
                rectTrans.anchoredPosition = Vector2.zero;
            }
            else
            {
                // 屏幕比例正常，恢复全屏
                rectTrans.sizeDelta = Vector2.zero;
                rectTrans.anchoredPosition = Vector2.zero;
            }
        }
    }
}