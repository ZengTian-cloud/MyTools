using System;
using System.Collections;
using System.Collections.Generic;
using Basics;
using Cysharp.Threading.Tasks;
using UnityEngine;

//怪物管理器
public class BattleMonsterManager : SingletonNotMono<BattleMonsterManager>
{
    public Dictionary<long, BaseMonster> dicMonsterList = new Dictionary<long, BaseMonster>();//怪物列表  ke-guid（单场战斗的唯一id）

    //怪物计数
    public int monsterCount = 0;
    //guid计数
    public long guidTime = 200000;

    /// <summary>
    /// 生成一只怪物
    /// </summary>
    public async void CreatOneMonster(long monsterID, V3 startPos,int pathIndex)
    {
        guidTime += 1;
        monsterCount += 1;
        GameObject root = null;
        GameObject oneMonster = null;
        root = BattlePoolManager.Instance.OutPool(ERootType.Monster, monsterID.ToString());
        MonsterDataCfg cfg = MonsterDataManager.Instance.GetMonsterCfgByMonsterID(monsterID);
        if (root == null)
        {
            //加载怪物对象节点
            root = await ResourcesManager.Instance.LoadUIPrefabSync("battleObjRoot");
            root.transform.SetParent(GameCenter.mIns.m_BattleMgr.monsterListRoot);
            root.tag = "monster";
            root.layer = 8;
            root.AddComponent<SphereCollider>().isTrigger = true;
            root.GetComponent<SphereCollider>().radius = 0.7f;
            oneMonster = await LoadMonsterModelByHeroID(root, cfg.modelid);
            oneMonster.transform.SetParent(root.transform);
            oneMonster.transform.localPosition = Vector3.zero;

        }
        else
        {
            root.transform.SetParent(GameCenter.mIns.m_BattleMgr.monsterListRoot);
            oneMonster = root.transform.GetChild(0).gameObject;
        }
        root.name = $"{monsterID}_{guidTime}";
        root.transform.position = new Vector3((float)startPos.x, 0, (float)startPos.z);
        root.GetOrAddCompoonet<AnimationController>().PlayAnimatorByName("");
        MonsterController monsterController = root.GetOrAddCompoonet<MonsterController>();

        // 添加碰撞体相关组件
        BattleTrapManager.Instance.AddBoxCollider(root);
        // 添加辅助脚本
        root.GetOrAddCompoonet<ModelRenderQueueHelper>();

        MonsterDataCfg curMonsterCfg = MonsterDataManager.Instance.GetMonsterCfgByMonsterID(monsterID);
        BaseMonster baseMonster = new BaseMonster(curMonsterCfg, monsterController, root, oneMonster, guidTime);
        monsterController.monsterData = baseMonster;
        monsterController.RefreshCurPath(pathIndex);
        if (!dicMonsterList.ContainsKey(baseMonster.GUID))
        {
            dicMonsterList.Add(baseMonster.GUID, baseMonster);
        }
        else
        {
            dicMonsterList[baseMonster.GUID] = baseMonster;
        }
        TalentManager.Instance.CheckTalentByMonster(baseMonster);
        HpSliderManager.ins.CreatOneSliderByMonster(baseMonster);
        monsterController.StartMove();
    }


    /// <summary>
    /// 怪物死亡
    /// </summary>
    /// <param name="GUID"></param>
    public void MonsterDie(long GUID)
    {
        if (dicMonsterList.ContainsKey(GUID))
        {
            dicMonsterList.Remove(GUID);
            monsterCount -= 1;
        }
    }

    public BaseMonster GetOneMonsterByGUID(long GUID)
    {
        if (dicMonsterList.ContainsKey(GUID))
        {
            return dicMonsterList[GUID];
        }
        return null;
    }


    /// <summary>
    /// 根据配置生成怪物
    /// </summary>
    public void CreatMonsterByCfg(MissionData missionData)
    {
        //获得关卡路线配置
        List<PathData> pathDatas = missionData.pathDatas;
        PathData onePath;//单跳路线 
        MonsterData monsterData;//单个波次的怪物数据

        int waveDelay = 0;//当前波次的延时
        for (int i = 0; i < pathDatas.Count; i++)
        {
            onePath = pathDatas[i];
            if (onePath.monsterDatas != null)
            {
                for (int m = 0; m < onePath.monsterDatas.Count; m++)
                {
                    monsterData = onePath.monsterDatas[m];
                    if (monsterData.waveDatas.Count <= 0)
                    {
                        continue;
                    }
                    if (m > 0)
                    {
                        //从第二波开始，延时取上一波的结束时间
                        waveDelay = int.Parse(monsterData.waveDatas[monsterData.waveDatas.Count - 1].End);
                    }
                    for (int w = 0; w < monsterData.waveDatas.Count; w++)
                    {
                        waveData waveData = monsterData.waveDatas[w];
                        int delay = int.Parse(waveData.Start);
                        V3 startPos = onePath.pointDatas[0].pos;
                        int pathindex = i;
                        GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke(waveDelay + delay, () => { CreatMonsterByWavedata(waveData, startPos, pathindex); });
                        if (w == monsterData.waveDatas.Count - 1 && m == onePath.monsterDatas.Count - 1)
                        {
                            GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke(int.Parse(waveData.End), () => { GameCenter.mIns.m_BattleMgr.monsterCreatEnd = true; });
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 根据波次数据生成怪物
    /// </summary>
    public void CreatMonsterByWavedata(waveData waveData, V3 startPos,int pathindex)
    {
        int count = int.Parse(waveData.Count);//怪物数量
        long monsterID = long.Parse(waveData.MonsterID);
        int interval = int.Parse(waveData.Interval);//间隔时间
        for (int i = 0; i < count; i++)
        {
            int time = i * interval;
            GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke(time, () => { CreatOneMonster(monsterID, startPos, pathindex); });

        }
    }



    /// <summary>
    /// 加载怪物模型
    /// </summary>
    /// <param name="heroid"></param>
    /// <returns></returns>
    public async UniTask<GameObject> LoadMonsterModelByHeroID(GameObject parent, long monsterid)
    {
        //加载英雄ab包
        GameObject heroPrefab = await ResourcesManager.Instance.LoadPrefabSync("role", "role_" + monsterid.ToString(), parent.transform);
        return heroPrefab;
    }
}
