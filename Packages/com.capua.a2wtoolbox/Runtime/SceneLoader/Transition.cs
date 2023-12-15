using UnityEngine;

namespace A2W
{
    [RequireComponent(typeof(Animator))]
    public class Transition : MonoBehaviour
    {
        private Animator animator;

        public void Init()
        {
            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// 播放转场前的动画
        /// </summary>
        public void StartTrans()
        {
            animator.SetTrigger("Start");
        }

        /// <summary>
        /// 播放转场后的动画
        /// </summary>
        public void EndTrans()
        {
            animator.SetTrigger("End");
        }

        /// <summary>
        /// 当前动画是否播放完成
        /// </summary>
        /// <returns></returns>
        public bool IsAnimationDone()
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                return true;
            else
                return false;
        }
    }
}


