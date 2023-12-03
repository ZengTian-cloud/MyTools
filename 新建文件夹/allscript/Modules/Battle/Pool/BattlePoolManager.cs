using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗对象池
/// </summary>
public class BattlePoolManager : Singleton<BattlePoolManager>
{
    private Dictionary<ERootType, Transform> allPoolRoot;//所有节点
    private Dictionary<ERootType, Dictionary<string, List<GameObject>>> allBattlePool;

    private void Awake()
    {
        
    }

    public void InitPool()
    {
        if (allPoolRoot == null && allBattlePool == null)
        {
            allPoolRoot = new Dictionary<ERootType, Transform>();
            allBattlePool = new Dictionary<ERootType, Dictionary<string, List<GameObject>>>();

            foreach (ERootType type in Enum.GetValues(typeof(ERootType)))
            {
                GameObject root = new GameObject();
                root.transform.SetParent(this.transform);
                root.name = $"{type.ToString()}Root";
                root.SetActive(false);
                allPoolRoot.Add(type, root.transform);
                allBattlePool.Add(type, new Dictionary<string, List<GameObject>>());
            }
        }
    }

    public void DestroyPool()
    {
        CleanAll();
        /*foreach (var item in allPoolRoot)
        {
            GameObject.Destroy(item.Value.gameObject);
        }
        CleanAll();
        GameObject.Destroy(this.GetComponent<BattlePoolManager>());*/
    }


    //入池
    public void InPool(ERootType type,GameObject obj,string key ="common",bool bDealy = false)
    {
        if (!allPoolRoot.ContainsKey(type))
        {
            Debug.LogError($"{type.ToString()}类型的对象池不存在，请检查！");
            return;
        }
        if (!allBattlePool.ContainsKey(type))
        {
            allBattlePool.Add(type, new Dictionary<string, List<GameObject>>());
        }
        if (!allBattlePool[type].ContainsKey(key))
        {
            allBattlePool[type].Add(key, new List<GameObject>());
        }
        if (allBattlePool[type][key].Contains(obj))
        {
            //Debug.LogError($"重复入池,请检查:{obj}");
            return;
        }
        obj.transform.SetParent(allPoolRoot[type]);
        //obj.transform.localPosition = Vector3.zero;
        allBattlePool[type][key].Add(obj);    
    }

    //出池
    public GameObject OutPool(ERootType type,string key = "common")
    {
        if (!allPoolRoot.ContainsKey(type) || !allBattlePool.ContainsKey(type))
        {
            return null;
        }

        if (!allBattlePool[type].ContainsKey(key))
        {
            return null;
        }

        if (allBattlePool[type][key].Count > 0)
        {
            GameObject obj = allBattlePool[type][key][0];
            allBattlePool[type][key].Remove(obj);
            return obj;
        }
        return null;
    }

    //清空
    public void CleanAll()
    {
        if (allPoolRoot != null)
        {
            foreach (var item in allPoolRoot)
            {
                if (item.Value.gameObject!=null)
                {
                    GameObject.Destroy(item.Value.gameObject);
                }

            }
            allPoolRoot.Clear();
        }
        if (allBattlePool!=null)
        {
            allBattlePool.Clear();
        }

        
        allPoolRoot = null;
        allBattlePool = null;
    }
}

/// <summary>
/// 节点类型
/// </summary>
public enum ERootType
{
    Monster = 0,//怪物
    Hero,//英雄
    HPui,//血条ui
    Bullet,//子弹
    SkillCard,//技能卡片ui
    DamageTMP,//伤害字体
    Effect,//特效
}

