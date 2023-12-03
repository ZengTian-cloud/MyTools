using System;
using DG.Tweening;

/// <summary>
/// 通用加载界面
/// </summary>
public partial class commonloading_wnd
{
    public override UILayerType uiLayerType => UILayerType.Pop;

    public override string uiAtlasName => "";


    protected override void OnOpen()
    {
        bg.alpha = 0;
        if (openArgs.Length > 0)
        {
            Action cb = (Action)openArgs[0];
            ShowLoading(cb);
        }
    }

    public void ShowLoading(Action action)
    {
        bg.DOFade(1, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            action?.Invoke();
            GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke(500, () =>
            {
                bg.DOFade(0, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    this.Close();
                });
            });
        });
    }
}

