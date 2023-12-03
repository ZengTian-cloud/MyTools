using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public partial class battlewindup
{
    public override UILayerType uiLayerType => UILayerType.Pop;

    public override string uiAtlasName => "battlewindup";

    private bool bWin;//是否胜利
    private bool bDecode;//是否还有交互界面

    public Action backCall;

    protected override void OnInit()
    {
        
    }

    protected override void OnOpen()
    {
        bWin = (bool)this.openArgs[0];
        bDecode = (bool)this.openArgs[1];
        this.win.gameObject.SetActive(bWin);
        this.lose.gameObject.SetActive(!bWin);
        this.win.alpha = 0;
        this.lose.alpha = 0;
        this.buttons.SetActive(!bDecode);
        this.mask.enabled = !bDecode;

        this.btn_back.gameObject.SetActive(false);
        this.btn_next.gameObject.SetActive(false);
        this.btn_restar.gameObject.SetActive(false);

        backCall = null;
        if (bWin)
        {
            ShowWin();
        }
        else
        {
            ShowLose();
        }
    }

    private void ShowWin()
    {
        if (bDecode)//有战斗交互自动退出界面
        {
            this.win.DOFade(1, 1f).SetEase(Ease.Linear).OnComplete(() =>
            {
                GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke(1000, () =>
                {
                    this.win.DOFade(0, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
                    {
                        this.Close();
                    });
                });
            });
        }
        else//点击退出按钮退出
        {
            this.win.DOFade(1, 1f).SetEase(Ease.Linear).OnComplete(() =>
            {
                
            });
        }
        
    }


    private void ShowLose()
    {
        this.lose.DOFade(1, 1f).SetEase(Ease.Linear).OnComplete(() =>
        {

        });
        this.btn_back.gameObject.SetActive(true);
        this.btn_restar.gameObject.SetActive(true);
    }

    /// <summary>
    /// 返回
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    partial void btn_back_Click()
    {
        backCall?.Invoke();
        this.Close();
    }

    /// <summary>
    /// 下一关
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    partial void btn_next_Click()
    {
        backCall?.Invoke();
        this.Close();
    }

    /// <summary>
    /// 再来一次
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    partial void btn_restar_Click()
    {
        backCall?.Invoke();
        this.Close();
    }

    public void SetCallBack(Action cb1)
    {
        backCall = cb1;
    }
}
