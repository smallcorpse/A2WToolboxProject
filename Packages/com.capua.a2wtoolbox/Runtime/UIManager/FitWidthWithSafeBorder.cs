using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace A2W
{
    // 适配脚本（挂载到ContentRoot）
    [RequireComponent(typeof(RectTransform))]
    public class FitWidthWithSafeBorder : MonoBehaviour
    {
        private RectTransform rectTrans;
        private CanvasScaler canvasScaler;
        // 存储从UIManager同步的分辨率（避免频繁访问单例）
        private Vector2 referenceResolution;

        /// <summary>
        /// 初始化适配脚本（由UIManager调用）
        /// </summary>
        /// <param name="refRes">从UIManager传入的设计分辨率</param>
        public void Init(Vector2 refRes)
        {
            // 1. 初始化基础组件
            rectTrans = GetComponent<RectTransform>();
            canvasScaler = GetComponentInParent<CanvasScaler>();

            if (canvasScaler == null)
            {
                Debug.LogError("ContentRoot未找到CanvasScaler组件！");
                return;
            }

            // 2. 同步UIManager的设计分辨率
            referenceResolution = refRes;

            // 3. 立即执行一次适配
            AdjustContentSize();

            // 4. 注册分辨率变化监听
            Canvas.willRenderCanvases += AdjustContentSize;

            Debug.Log($"FitWidthWithSafeBorder初始化完成，同步分辨率：{referenceResolution}");
        }

        void OnDestroy()
        {
            // 移除监听，避免内存泄漏
            Canvas.willRenderCanvases -= AdjustContentSize;
        }

        /// <summary>
        /// 外部可调用的适配方法（如需手动触发）
        /// </summary>
        public void AdjustContentSize()
        {
            if (canvasScaler == null || rectTrans == null) return;

            // 1. 获取设计分辨率和当前屏幕的高宽比
            float designAspect = referenceResolution.y / referenceResolution.x;
            float screenAspect = (float)Screen.height / Screen.width;

            // 2. 如果屏幕高宽比大于设计值（屏幕过长），限制ContentRoot的高度
            if (screenAspect > designAspect)
            {
                // 计算目标高度 = 屏幕宽度 * 设计高宽比（保持设计比例）
                float targetHeight = Screen.width * designAspect;
                // 计算上下黑边的像素高度
                float topBottomPadding = (Screen.height - targetHeight) / 2f;
                // 转换为Canvas内的像素值（CanvasScaler是Match Width，所以用宽度比例）
                float canvasScale = canvasScaler.referenceResolution.x / Screen.width;
                float canvasPadding = topBottomPadding * canvasScale;

                // 设置ContentRoot的上下偏移，实现居中显示
                rectTrans.offsetMin = new Vector2(0, canvasPadding);
                rectTrans.offsetMax = new Vector2(0, -canvasPadding);
            }
            else
            {
                // 屏幕比例正常，恢复全屏
                rectTrans.offsetMin = Vector2.zero;
                rectTrans.offsetMax = Vector2.zero;
            }
        }

        /// <summary>
        /// 提供外部更新分辨率的接口（如需动态修改分辨率时调用）
        /// </summary>
        /// <param name="newRefRes">新的设计分辨率</param>
        public void UpdateReferenceResolution(Vector2 newRefRes)
        {
            referenceResolution = newRefRes;
            AdjustContentSize(); // 立即重新适配
        }
    }
}