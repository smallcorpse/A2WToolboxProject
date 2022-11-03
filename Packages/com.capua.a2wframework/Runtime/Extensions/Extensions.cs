using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace A2W
{

    public static class Extensions
    {
        public static List<T> Shuffle<T>(this List<T> _list)
        {
            List<T> tempList = new List<T>(_list);
            int rand;
            T tempValue;
            for (int i = tempList.Count - 1; i >= 0; i--)
            {
                rand = Random.Range(0, i + 1);
                tempValue = tempList[rand];
                tempList[rand] = tempList[i];
                tempList[i] = tempValue;
            }
            return tempList;
        }

        public static string RemoveNameSpece(this string typeString)
        {
            string result;
            string[] str = typeString.Split('.');
            result = str[str.Length - 1];
            return result;
        }
    }

    public static class MathExtensions
    {
        /// <summary>
        /// 返回大值
        /// </summary>
        /// <param name="a">原值</param>
        /// <param name="b">比较值</param>
        /// <returns>结果</returns>
        public static float Max(this float a, float b)
        {
            return Mathf.Max(a, b);
        }

        public static float Min(this float a, float b)
        {
            return Mathf.Min(a, b);
        }

        public static int Max(this int a, int b)
        {
            return Mathf.Max(a, b);
        }

        public static int Min(this int a, int b)
        {
            return Mathf.Min(a, b);
        }

        public static float Abs(this float value)
        {
            return Mathf.Abs(value);
        }

        public static int Abs(this int value)
        {
            return Mathf.Abs(value);
        }
    }

    public static class ConfigExtensions
    {
        public static List<int> ParseStringToIntList(this string str, char sep = ',')
        {
            List<int> result = new List<int>();
            string[] subs = str.Split(sep);
            foreach (var sub in subs)
            {
                result.Add(int.Parse(sub));
            }
            return result;
        }
    }

    public static class ComponentExtensions
    {
        public static T TryAddComponent<T>(this GameObject go) where T : Component
        {
            go.TryGetComponent<T>(out T result);
            if (result is null)
            {
                result = go.AddComponent<T>();
            }
            return result;
        }

        public static void RemoveComponent<T>(this GameObject go) where T : Component
        {
            go.TryGetComponent<T>(out T result);
            if (result)
            {
                Object.Destroy(result);
            }
        }

        public static T AddChildComponent<T>(this GameObject go) where T : Component
        {
            GameObject child = new GameObject();
            string[] str = typeof(T).ToString().Split('.');
            child.name = str[str.Length - 1];
            child.transform.SetParent(go.transform, false);
            T result = child.AddComponent<T>();
            return result;
        }

    }


    public static class AnimatorExtensions
    {
        public static bool IsAnimationDone(this Animator animator)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                return true;
            else
                return false;
        }

        public static bool IsAnimationDone(this Animator animator, string name)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName(name) is false) return false;

            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                return true;
            else
                return false;
        }

        public static bool IsAnimation(this Animator animator, string name)
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsName(name);
        }
    }

}


