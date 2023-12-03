using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 英雄管理器
/// </summary>
public class BattleHeroManager :SingletonNotMono<BattleHeroManager>
{
    //战斗内上阵的英雄（根据战斗中实时变化）
    public List<BaseHero> depolyHeroList = new List<BaseHero>();

    /// <summary>
    /// 初始化英雄管理数据
    /// </summary>
    /// <param name="baseHeroes"></param>
    public void InitDepolyHeroList(List<BaseHero> baseHeroes)
    {
        for (int i = 0; i < baseHeroes.Count; i++)
        {
            depolyHeroList.Add(baseHeroes[i]);
        }
    }

    /// <summary>
    /// 生成一个英雄
    /// </summary>
    /// <param name="data">英雄数据</param>
    /// <param name="heroListRoot">实例对象节点</param>
    /// <param name="cfgData">英雄配置数据</param>
    /// <returns></returns>
    public async UniTask<BaseHero> CreatOneHero(HeroData data,Transform heroListRoot, HeroInfoCfgData cfgData)
    {
        GameObject root = null;
        BaseHero baseHero = null;
        //检索对象池
        root = BattlePoolManager.Instance.OutPool(ERootType.Hero, data.heroID.ToString());
        if (root == null)
        {
            //加载英雄对象节点
            root = await ResourcesManager.Instance.LoadUIPrefabSync("battleObjRoot");
            root.name = $"heroRoot_{data.heroID}";
            root.layer = LayerMask.NameToLayer("HeroRoot");
            root.AddComponent<SphereCollider>().isTrigger = true;
            //加载英雄
            GameObject hero = await LoadHeroModelByHeroID(root, data.heroID);
            hero.layer = LayerMask.NameToLayer("HeroItem");
            hero.name = data.heroID.ToString();
            hero.transform.localPosition = new Vector3(0, 0.07f, 0);
            CapsuleCollider collider = hero.AddComponent<CapsuleCollider>();
            collider.isTrigger = true;
            collider.height = 1.5f;
            collider.center = new Vector3(0, 0.75f, 0);


            HeroController heroctr = root.GetOrAddCompoonet<HeroController>();
            //构造basehero数据
            baseHero = new BaseHero(data, heroctr, cfgData, root, hero);
            baseHero.InitSkillCfg();
            //初始化英雄控制器
            heroctr.Oninit(baseHero, hero);
            heroctr.enabled = true;

            root.GetOrAddCompoonet<AnimationController>();

            root.transform.SetParent(heroListRoot);
        }
        else
        {
            root.transform.SetParent(heroListRoot);
        }
       
        HpSliderManager.ins.CreatOneSliderByMonster(root.GetComponent<HeroController>().baseHero);
        return root.GetComponent<HeroController>().baseHero;
    }

    
    public BaseHero GetBaseHeroByHeroID(long heroid)
    {
        return depolyHeroList.Find(data => data.objID == heroid);
    }

    /// <summary>
    /// 加载英雄模型
    /// </summary>
    /// <param name="heroid"></param>
    /// <returns></returns>
    public async UniTask<GameObject> LoadHeroModelByHeroID(GameObject parent, long heroid)
    {
        //加载英雄ab包
        GameObject heroPrefab = await ResourcesManager.Instance.LoadPrefabSync("role", "role_" + heroid.ToString(), parent == null ? null : parent.transform);
        return heroPrefab;
    }

    /// <summary>
    /// 英雄死亡
    /// </summary>
    /// <param name="GUID"></param>
    public void HeroDie(long heroid)
    {

        BaseHero baseHero = depolyHeroList.Find(data => data.objID == heroid);
        if (baseHero!=null)
        {
            depolyHeroList.Remove(baseHero);
        }
        
    }

    /// <summary>
    /// 删除指定英雄的所有卡牌
    /// </summary>
    /// <param name="heroid"></param>
    public void RemoveCardByHero(long heroid)
    {
        DrawCardMgr.Instance.RemveHeroCard(heroid);
        for (int i = CardManager.Instance.curCardList.Count -1; i >= 0; i--)
        {
            if (CardManager.Instance.curCardList[i].heroid == heroid)
            {
                CardManager.Instance.RemoveOneCard(CardManager.Instance.curCardList[i], CardManager.Instance.curCardList[i].item);
            }
        }
    }
}

