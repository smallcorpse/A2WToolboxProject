using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace A2W
{
    public static class Utils
    {
        public static T GetRandomEnumValue<T>() where T : System.Enum
        {
            T[] values = (T[])System.Enum.GetValues(typeof(T));
            return values[UnityEngine.Random.Range(0, values.Length)];
        }
        // 调用示例：MyEnum randomValue = GetRandomEnumValue<MyEnum>();

        public static Texture2D CaptureScreenshot(Rect rect)
        {
            Texture2D screenshot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);

            screenshot.ReadPixels(rect, 0, 0);
            screenshot.Apply();

            return screenshot;
        }

        public static Texture2D CaptureCamera(Camera camera, Rect rect)
        {
            RenderTexture rt = new RenderTexture((int)rect.width, (int)rect.height, 0);

            camera.targetTexture = rt;
            camera.Render();

            // 可以做多个相机

            RenderTexture.active = rt;
            Texture2D screenshot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(rect, 0, 0);
            screenshot.Apply();

            camera.targetTexture = null;

            RenderTexture.active = null;
            GameObject.Destroy(rt);

            return screenshot;
        }
    }
}

