using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class basebattle_heroHeadPointer : MonoBehaviour
{
    Transform trans = null;
    private Vector3 bottomWorldPosition;
    private Vector3 topWorldPosition;
    private float dis = 20;

    public void DoAnim(Vector3 bottomWorldPosition)
    {
        trans.DOKill();
        trans = transform;
        this.bottomWorldPosition = bottomWorldPosition;
        topWorldPosition = bottomWorldPosition + new Vector3(0, dis, 0);
        Up();
    }
    private void Up()
    {
        trans.DOMoveY(topWorldPosition.y, 0.5f).OnComplete(() =>
        {
            Down();
        });
    }

    private void Down()
    {
        trans.DOMoveY(bottomWorldPosition.y, 0.5f).OnComplete(() =>
        {
            Up();
        });
    }
}
