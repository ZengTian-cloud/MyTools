using UnityEngine;
using System.Collections;

public class AnimationController : MonoBehaviour
{
	private Animator animator;

    public string curName;
    private void Awake()
    {
        animator = this.gameObject.GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        curName = "";
    }

    private void Update()
    {
    }

    /// <summary>
    /// 根据名字播放动画
    /// delaytime  - 预计时间 实际时间超出预计时间
    /// </summary>
    public void PlayAnimatorByName(string name,float speedValue = 1)
    {
        animator.speed = speedValue;
        curName = name;
        animator.CrossFade(name, 0.1f,0,0);
    }
}

