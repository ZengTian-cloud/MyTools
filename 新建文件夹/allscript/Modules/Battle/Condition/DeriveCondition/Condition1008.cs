using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 判断指定英雄的指定技能已经解锁（英雄ID;技能序号：5=1星；6=2星；7=3星；8=4星；9=5星；10=6星；11=被动1；12=被动2；13=被动3；14=被动4；以此类推）
/// </summary>
public class Condition1008 : BaseCondition
{
    //条件参数
    public string[] conditionParm;


    public override bool OnCheck(BaseObject baseObject, string funcPm, params object[] param)
    {
        //是否满足
        bool bLock = false;
        //初始化参数
        InitParam(funcPm);
        //英雄id
        long heroId = long.Parse(conditionParm[0]);
        //序号
        int skillIndex = int.Parse(conditionParm[1]);
        //是否解锁的参数 0-判断未解锁 1-判断一解锁
        int lockPm = int.Parse(conditionParm[2]);

        if (skillIndex <= 10)//星级（天赋）
        {
            HeroData heroData = HeroDataManager.Instance.GetHeroData(heroId);
            if (heroData.star >= skillIndex - 4)
            {
                bLock = true;
            }
        }
        else//突破等级 被动
        {
            List<long> passivityIds =((BaseHero)baseObject).cfgdata.GetAllPassivityByHeroID(baseObject.objID);
            long talentID = passivityIds.Find(id => id % 100 == skillIndex);
            if (talentID > 0)
            {
                bLock = true;
            }
        }


        return lockPm == 0 ? !bLock : bLock;
    }

    private void InitParam(string funcPm)
    {
        conditionParm = funcPm.Split(';');
    }


}

