using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager 
{
    private static CardManager instance;
    public static CardManager Instance
    {
        get
        {
            if (instance == null)
                instance = new CardManager();
            return instance;
        }
    }

    public int maxCount = 3;//最大手牌数量

    public float drawCardTime = 2f;//抽卡间隔时间


    //当前手上卡牌
    public List<DrawCardData> curCardList = new List<DrawCardData>();

    /// <summary>
    /// 初始化当前手上卡组
    /// </summary>
    public void InitCurCardData(int heroCount)
    {
        curCardList = new List<DrawCardData>();
        int baseCount = int.Parse(GameCenter.mIns.m_CfgMgr.GetConstantValue("base_ram"));
        maxCount = heroCount <= baseCount ? baseCount : heroCount;
        // 应该满足公式：MAX(全局表中配置的最少数量，已上阵人数)	 即上阵3个英雄，初始可摸取3张牌
        // 初始卡牌数量
        for (int i = 0; i < maxCount; i++)
        {
            curCardList.Add(DrawCardMgr.Instance.DrawCard());
        }
    }

    /// <summary>
    /// 删除一张卡牌
    /// </summary>
    /// <param name="cardData"></param>
    public void RemoveOneCard(DrawCardData cardData,GameObject carditem)
    {
        BattleEventManager.Dispatch(BattleEventName.battle_removeCardByHeroDie, cardData.uid);
        curCardList.Remove(cardData);
        GameObject.Destroy(carditem);
        //BattlePoolManager.Instance.InPool(ERootType.SkillCard, carditem);
    }


    /// <summary>
    /// 抽取一张牌
    /// </summary>
    public void DrawOneCard()
    {
        DrawCardData card = DrawCardMgr.Instance.DrawCard();
        if (card == null)
        {
            return;
        }
        if (card.heroid > 0)
        {
            BaseHero baseHero = BattleHeroManager.Instance.GetBaseHeroByHeroID(card.heroid);
            DrawCardData newCard = baseHero.talentCompent.OnDrawSkillCard(card.heroid, card.skillCfgData.skilltype);
            if (newCard != null)
            {
                Debug.Log($"<color=yellow>[战斗日志]======> 抽卡触发-技能改变-skillID:{newCard.skillCfgData.skillid}</color>");
                card = newCard;
            }
        }
        AddOneCard(card);
    }

    public void AddOneCard(DrawCardData card)
    {
        curCardList.Add(card);
        basebattle bb = GameCenter.mIns.m_UIMgr.Get<basebattle>();
        bb.AddOneSkillCard(card);
    }

    public void AddCardLimit(int addNum)
    {
        maxCount += addNum;
        for (int i = 0; i < addNum; i++)
        {
            DrawOneCard();
        }
    }
}
