using UnityEngine;
using A2W;

[RequireComponent(typeof(Animator))]
public class AnimatorTransition : MonoBehaviour, ITransition
{
    private Animator animator;

    public void Init(Transform parent)
    {
        transform.SetParent(parent, false);

        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 播放转场前的动画
    /// </summary>
    public void Begin()
    {
        animator.SetTrigger("Start");
    }

    /// <summary>
    /// 播放转场后的动画
    /// </summary>
    public void Finish()
    {
        animator.SetTrigger("End");
    }

    /// <summary>
    /// 当前动画是否播放完成
    /// </summary>
    /// <returns></returns>
    public bool IsDone()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            return true;
        else
            return false;
    }
}



