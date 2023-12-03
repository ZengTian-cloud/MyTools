using DG.Tweening;
using System;
using UnityEngine;
using static HeroGrow;

public class HeroProfilePanel : IHeroPanel
{
    public HeroData hero;
    public HeroInfoCfgData heroInfo;

    private Transform parent;
    private float duration;
    
    private HeroLeftMenu leftMenu = null;

    public HeroProfilePanel(Transform parent, HeroLeftMenu leftMenu, float duration)
    {
        this.parent = parent;
        this.leftMenu = leftMenu;
        this.hero = leftMenu.getSelectHero().data;
        this.heroInfo = leftMenu.getSelectHero().heroInfo;
        this.duration = duration;

    }

    public void close()
    {
        parent.gameObject.SetActive(false);
    }

    public void show()
    {
        parent.gameObject.SetActive(true);
        parent.GetComponent<RectTransform>().DOAnchorPosX(0, duration);

    }
}
