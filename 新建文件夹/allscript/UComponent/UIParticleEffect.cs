using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;

/// <summary>
/// UI粒子特效 （层级设置、粒子播放暂停、裁剪(需要shader配合)等）
/// </summary>
public class UIParticleEffect : MonoBehaviour
{
    public class ShowInfo
    {
        public Canvas _canvas;

        public int _offset = 0;

        public Vector3 _scale = Vector3.one;

        public RectMask2D _rectMask;
    }
    ParticleSystem particle;

    ShowInfo info;

    private void Awake()
    {
        particle = GetComponentInChildren<ParticleSystem>(true);

        Debug.Log(particle.name);

    }
    public void SetUP(ShowInfo info)
    {
        this.info = info;

        SetLayer();

        SetScale();

        Clip();
    }
    /// <summary>
    /// 设计特效层级
    /// </summary>
    private void SetLayer()
    {
        int layer;

        if (info._canvas != null)
            layer = info._canvas.sortingOrder + info._offset; //根据offset值来控制在该canvase的上面还是下面
        else
            layer = info._offset;
        ParticleSystemRenderer[] allParticalRenderer = GetComponentsInChildren<ParticleSystemRenderer>(true);

        for (int i = 0; i < allParticalRenderer.Length; i++)
        {
            allParticalRenderer[i].sortingOrder += layer;
        }
    }
    /// <summary>
    /// 设置特效缩放
    /// </summary>
    private void SetScale()
    {
        transform.localScale = info._scale;
    }

    private void Clip()//简易裁剪
    {
        if (info._rectMask != null)
        {
            Vector3[] mCorners = new Vector3[4];

            info._rectMask.GetComponent<RectTransform>().GetWorldCorners(mCorners);

            ParticleSystemRenderer[] allParticalRenderer = GetComponentsInChildren<ParticleSystemRenderer>(true);

            for (int i = 0; i < allParticalRenderer.Length; i++)
            {
                Material m = allParticalRenderer[i].material;
                m.SetFloat("_MinX", mCorners[0].x);
                m.SetFloat("_MinY", mCorners[0].y);
                m.SetFloat("_MaxX", mCorners[2].x);
                m.SetFloat("_MaxY", mCorners[2].y);
            }
        }
    }
    public void Play()
    {
        particle.Play();
    }
    public void Stop()
    {
        particle.Stop();
    }

}



