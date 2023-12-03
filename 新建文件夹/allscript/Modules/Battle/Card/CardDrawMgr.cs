using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCardMgr
{
    private static DrawCardMgr instance;
    public static DrawCardMgr Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new DrawCardMgr();
                instance.RegisterEvent();
            }
            return instance;
        }
    }

    public List<DrawCardData> drawCardDatas = new List<DrawCardData>();

    public void InitDrawCardDatas(List<BaseHero> heros, int gameLevelID = 0)
    {
        //drawCardDatas = new List<DrawCardData>();
        BattleSkillCfg cfg1;
        BattleSkillCfg cfg2;
        BattleSkillCfg cfg3;
        for (int i = 0; i < heros.Count; i++)
        {
            cfg1 = BattleCfgManager.Instance.GetSkillCfgBySkillID((int)heros[i].skill1);
            cfg2 = BattleCfgManager.Instance.GetSkillCfgBySkillID((int)heros[i].skill2);
            cfg3 = BattleCfgManager.Instance.GetSkillCfgBySkillID((int)heros[i].skill3);
            drawCardDatas.Add(new DrawCardData(new Snowflake().GetId(), (int)heros[i].skill1, heros[i].objID, cfg1));
            drawCardDatas.Add(new DrawCardData(new Snowflake().GetId(), (int)heros[i].skill2, heros[i].objID, cfg2));
            drawCardDatas.Add(new DrawCardData(new Snowflake().GetId(), (int)heros[i].skill3, heros[i].objID, cfg3));
        }
    }

    private void RegisterEvent()
    {
        BattleEventManager.RegisterEvent(BattleEventName.battle_upskillStar, OnUpSkillStar, true);
        BattleEventManager.RegisterEvent(BattleEventName.battle_upskillEnd, OnUpSkillEnd, true);
    }


    private void OnUpSkillStar(long skillid)
    {
        DrawCardData drawCard = drawCardDatas.Find(x => x.skillid == skillid);
        if (drawCard!= null)
        {
            drawCard.place = 3;
        }
    }

    private void OnUpSkillEnd(long skillid)
    {
        DrawCardData drawCard = drawCardDatas.Find(x => x.skillid == skillid);
        if (drawCard != null)
        {
            drawCard.place = 1;
        }
    }

    /// <summary>
    /// 获取非英雄卡牌
    /// </summary>
    public List<DrawCardData> GetNotHeroCrads()
    {
        List<DrawCardData> retDcds = new List<DrawCardData>();
        foreach (var dcd in drawCardDatas)
        {
            if (dcd.heroid <= 0)
            {
                retDcds.Add(dcd);
            }
        }
        return retDcds;
    }

    /// <summary>
    /// 在卡池中删除某个英雄的卡组
    /// </summary>
    public void RemveHeroCard(long heroid)
    {
        for (int i = drawCardDatas.Count - 1; i >= 0; i--)
        {
            if (drawCardDatas[i].heroid == heroid)
            {
                drawCardDatas.Remove(drawCardDatas[i]);
            }
        }
    }

    #region Test
    /*
int testTimes = 10000;
Dictionary<int, int> testStatistics = new Dictionary<int, int>();
public void Test()
{
    while (testTimes > 0)
    {
        DrawCard();
        testTimes--;
    }
    foreach (var item in testStatistics)
    {
        Debug.Log("`` Statistics id:" + item.Key + " - count:" + item.Value);
    }
}*/
    #endregion

    /// <summary>
    /// 向卡池添加一张新卡
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cfgid"></param>
    public void OnAddNewCard(long id, long heroid , long cfgid) {
        DrawCardData dcd = GetDrawCardData(id);
        DrawCardData draw = drawCardDatas.Find(x => x.skillid == cfgid);
        if (draw == null)
        {
            if (dcd == null)
            {
                DrawCardData drawCardData = new DrawCardData(id, cfgid, heroid);
                drawCardDatas.Add(drawCardData);
            }
        }
        
    }

    /// <summary>
    /// 从卡池移除一张新卡(彻底移除)
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cfgid"></param>
    public void OnRemoveNewCard(long id, int cfgid)
    {
        for (int i = drawCardDatas.Count - 1; i >= 0; i--)
        {
            if (drawCardDatas[i].uid == id)
            {
                drawCardDatas.RemoveAt(i);
                break;
            }
        }
    }

    /// <summary>
    /// 改变卡牌所在位置(1=卡尺,2=手牌,3=状态栏(状态类卡牌打出后，持续时间内后存在状态栏处))
    /// </summary>
    /// <param name="id"></param>
    /// <param name="newPlace"></param>
    public void OnChangedCardPlace(long id, int newPlace)
    {
        DrawCardData dcd = GetDrawCardData(id);
        if (dcd != null)
            dcd.place = newPlace;
    }

    /// <summary>
    /// 改变卡牌的状态
    /// </summary>
    /// <param name="id"></param>
    /// <param name="drawCardStatus"></param>
    public void OnChangedCardStatus(long id, DrawCardStatus drawCardStatus)
    {
        DrawCardData dcd = GetDrawCardData(id);
        if (dcd != null)
            dcd.ChangedStatus(drawCardStatus);
    }

    /// <summary>
    /// 获取卡牌数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public DrawCardData GetDrawCardData(long id)
    {
        foreach (var dcd in drawCardDatas)
        {
            if (dcd.uid == id)
                return dcd;
        }
        return null;
    }

    /// <summary>
    /// 获取卡牌数据 By Skill Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public DrawCardData GetDrawCardDataBySkillId(long skillId)
    {
        foreach (var dcd in drawCardDatas)
        {
            if (dcd.skillid == skillId)
                return dcd;
        }
        return null;
    }

    /// <summary>
    /// 执行抽卡
    /// </summary>
    public DrawCardData DrawCard()
    {
        // 在状态栏和被禁用的卡牌无法抽取(单概率计数正常)
        // 初始权重值==5
        // 更新权重值=当前权重值+当前未抽中次数^4*100, 最大保护值21亿(int.MaxValue=2147483647)
        /// 1:权重值总和计算
        float weightTotal = 0;
        lock (drawCardDatas)
        {
            foreach (var dcd in drawCardDatas)
            {
                float currWeight = dcd.weight;
                if (dcd.counter > 0)
                {
                    currWeight = dcd.weight + Mathf.Pow(dcd.counter, 4) * 100;
                    if (currWeight > int.MaxValue)
                        currWeight = int.MaxValue;
                    dcd.UpdateWeight(currWeight);
                }
                else
                {
                    if (currWeight != 5)
                    {
                        currWeight = 5;
                        dcd.UpdateWeight(currWeight);
                    }
                }

                // 正常状态和非状态栏卡牌才将其权重计和，后面随机也会跳过这类卡牌...
                if (dcd.dradwCardStatus == DrawCardStatus.Normal && dcd.place != 3)
                    weightTotal = (weightTotal + dcd.weight) >= int.MaxValue ? int.MaxValue : weightTotal + dcd.weight;
                //Debug.Log("1 - currWeight:" + currWeight.ToString() + " - weightTotal:" + weightTotal.ToString() + " - dcd:" + dcd.ToString());
            }
        }

        // TODO:随机数, 若后续有一次抽取多张，则需要重置随机种子，这里暂不处理...
        float randf = Random.Range(0, 1000000);
        // 临时概率和统计: <1
        float tempProbability = 0.0f;
        // 临时概率和统计: 放大1000000
        int tempIntPro = 0;
        // 选中的卡牌
        DrawCardData tempDCD = null;
        /// 本轮遍历计算该卡牌的出现概率，并转换为放大1000000后值于randf比较，确定是否被选中...
        lock (drawCardDatas)
        {
            foreach (var dcd in drawCardDatas)
            {
                // 只处理可抽取卡牌
                if (dcd.dradwCardStatus == DrawCardStatus.Normal && dcd.place != 3)
                {
                    float probability = dcd.weight / weightTotal;
                    dcd.UpdateProbability(probability);
                    tempProbability += probability;
                    tempIntPro = int.Parse(Mathf.Floor(tempProbability * 1000000 + 0.5f).ToString());
                    int iPro = int.Parse(Mathf.Floor(probability * 1000000 + 0.5f).ToString());

                    if (tempIntPro >= randf && tempDCD == null)
                    {
                        // 幸运小伙
                        tempDCD = dcd;
                        dcd.ResetCounter();
                        dcd.UpdateWeight(5);
                        //Debug.LogError("幸运小伙 - dcd:" + dcd.ToString());

                        #region Test
                        /* test
                        if (!testStatistics.ContainsKey(dcd.id))
                            testStatistics.Add(dcd.id, 1);
                        else
                            testStatistics[dcd.id] = testStatistics[dcd.id] + 1;
                        */
                        #endregion

                    }
                    else
                    {
                        dcd.IncreaseCounter();
                    }
                    //Debug.Log("可抽 - iPro:" + iPro + " - tempIntPro:" + tempIntPro + " - dcd:" + dcd.ToString());
                }
                else
                {
                    // 不可抽取卡牌一次抽卡后，计数也需要累加
                    dcd.IncreaseCounter();
                }
                //Debug.Log("2 - randf:" + randf + " - tempProbability:" + tempProbability + " - dcd:" + dcd.ToString());
            }
        }
        if (tempDCD == null)
        {
            return null;
        }
        tempDCD.place = 2;
        return tempDCD.Clone();
    }

    public Dictionary<DrawCardData, float> GetNextProp()
    {
        Dictionary<DrawCardData, float> kv = new Dictionary<DrawCardData, float>();
        // 初始权重值==5
        // 更新权重值=当前权重值+当前未抽中次数^4*100, 最大保护值21亿(int.MaxValue=2147483647)
        /// 1:权重值总和计算
        float weightTotal = 0;
        lock (drawCardDatas)
        {
            foreach (var dcd in drawCardDatas)
            {
                float currWeight = dcd.weight;
                if (dcd.counter > 0)
                {
                    currWeight = dcd.weight + Mathf.Pow(dcd.counter, 4) * 100;
                    if (currWeight > int.MaxValue)
                        currWeight = int.MaxValue;
                    dcd.UpdateWeight(currWeight);
                }
                else
                {
                    if (currWeight != 5)
                    {
                        currWeight = 5;
                        dcd.UpdateWeight(currWeight);
                    }
                }

                // 正常状态和非状态栏卡牌才将其权重计和，后面随机也会跳过这类卡牌...
                if (dcd.dradwCardStatus == DrawCardStatus.Normal && dcd.place != 3)
                    weightTotal = (weightTotal + dcd.weight) >= int.MaxValue ? int.MaxValue : weightTotal + dcd.weight;
            }
        }

        lock (drawCardDatas)
        {
            foreach (var dcd in drawCardDatas)
            {
                // 只处理可抽取卡牌
                if (dcd.dradwCardStatus == DrawCardStatus.Normal && dcd.place != 3)
                {
                    float probability = dcd.weight / weightTotal;
                    kv.Add(dcd, probability);
                }
            }
        }
        return kv;
    }
}

/// <summary>
/// 用于抽卡的数据
/// </summary>
public class DrawCardData
{
    // uid
    public long uid { get; private set; }
    // 配置id
    public long skillid { get; private set; }

    public long heroid{ get; private set; }
    // 权重(初始权重值==5, 最大21亿(int.MaxValue=2147483647))
    public float weight { get; private set; }
    // 计数(当前未抽中次数)
    public int counter { get; private set; }
    // 概率(带小数百分比)
    public float probability { get; private set; }
    // 所需能量
    public int energy { get;  set; }
    // 所在位置(1=卡尺,2=手牌,3=状态栏)
    public int place { get; set; }
    // 状态
    public DrawCardStatus dradwCardStatus { get; private set; }
    //卡牌数据
    public BattleSkillCfg skillCfgData { get; private set; }

    public GameObject item;

    public DrawCardData(long uid, long skillid,long heroid, BattleSkillCfg skillCfgData = null)
    {
        this.uid = uid;
        this.skillid = skillid;
        this.heroid = heroid;
        if (skillCfgData == null)
        {
            skillCfgData = BattleCfgManager.Instance.GetSkillCfgBySkillID(skillid);
        }
        this.energy = skillCfgData.energycost;
        this.skillCfgData = skillCfgData;
        ResetCfg();
    }

    public DrawCardData Clone()
    {
        DrawCardData dcd = new DrawCardData(new Snowflake().GetId(), skillid, heroid, skillCfgData);
        dcd.weight = weight;
        dcd.counter = counter;
        dcd.probability = probability;
        dcd.energy = energy;
        dcd.place = place;
        dcd.dradwCardStatus = dradwCardStatus;
        return dcd;
    }

    public void ResetCfg()
    {
        weight = 5;
        counter = 0;
        probability = 0;
        //energy = 0;
        place = 1;
        dradwCardStatus = DrawCardStatus.Normal;
    }

    public void IncreaseCounter()
    {
        counter++;
    }

    public void ResetCounter()
    {
        counter = 0;
    }

    public void UpdateWeight(float weight)
    {
        this.weight = weight;
    }

    public void ChangedStatus(DrawCardStatus dradwCardStatus)
    {
        this.dradwCardStatus = dradwCardStatus;
    }

    public void ChangedPlace(int place)
    {
        this.place = place;
    }

    public void UpdateProbability(float probability)
    {
        this.probability = probability;
    }

    public override string ToString()
    {
        return string.Format("uid:{0}, skillid:{1}, weight:{2}, counter:{3}, probability:{4}, energy:{5}, place:{6}, status:{7}",
                                    uid, skillid, weight, counter, probability, energy, place, dradwCardStatus);
    }
}

/// <summary>
/// 抽卡的卡牌状态
/// </summary>
public enum DrawCardStatus
{
    // 正常状态
    Normal = 1,
    // 禁用，无法计数
    Forbidden = 2
}