using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 机关实例化item
/// </summary>
public class BattleTrapItem : BaseObject
{
    // 机关实例化对象
    public GameObject trapObj;
    // 机关id
    public long trapId;
    // 机关配置数据
    public BattleTrapCfgData battleTrapCfgData;
    // 所在格子索引
    public V2 index;
    // 所在格子世界坐标
    public V3 pos;
    // 父节点
    public Transform parent;
    //关卡的路线点位 非路线机关为-1
    public int point;

    // 缓存受机关影响的对象
    public List<TrapDoBattleObj> cacheTrapDoObjs = new List<TrapDoBattleObj>();

    /// <summary>
    /// 构造
    /// </summary>
    /// <param name="trapId"></param>
    /// <param name="index"></param>
    /// <param name="pos"></param>
    /// <param name="parent"></param>
    /// <param name="guid"></param>
    public BattleTrapItem(long trapId, V2 index, V3 pos, Transform parent, long guid,int point = -1)
    {
        this.trapId = trapId;
        this.index = index;
        this.pos = pos;
        this.parent = parent;
        this.point = point;
        battleTrapCfgData = BattleTrapManager.Instance.GetTrapConfig(trapId);

        // - base obj param set!
        objID = battleTrapCfgData.trapid;
        GUID = guid;
        // 设置属性
        battleAttr = new Dictionary<long, float>();
        if (!string.IsNullOrEmpty(battleTrapCfgData.attr) && battleTrapCfgData.attr != "-1")
        {
            // 10100101;45|10100102;40|10100103;1600|10100104;200|10100113;50
            string[] split_attr_kv_arr = battleTrapCfgData.attr.Split(new char[] { '|' });
            if (split_attr_kv_arr.Length > 0)
            {
                foreach (var kv in split_attr_kv_arr)
                {
                    string[] kv_split = kv.Split(new char[] { ';' });
                    if (kv_split.Length == 2)
                    {
                        battleAttr.Add(long.Parse(kv_split[0]), float.Parse(kv_split[1]));
                    }
                }
            }
        }
        curHp = GetBattleAttr(EAttr.HP);
        // - base obj param set over

        if (battleTrapCfgData == null)
        {
            zxlogger.logerror($"Error: create battle trap cfg fail! trap id:{trapId}");
            return;
        }

        CreateTrapObj();
    }

    /// <summary>
    /// 创建机关对象
    /// </summary>
    public virtual void CreateTrapObj()
    {
        string modelName = battleTrapCfgData.modelid;
        if (string.IsNullOrEmpty(modelName))
        {
            zxlogger.logwarning($"Warning: create battle trap obj fail! the model id is null! trap id:{trapId}");
            return;
        }

        // 加载模型对象
        ResourcesManager.Instance.LoadAssetAsyncByName($"{modelName}.prefab", (o) =>
        {
            trapObj = o;
            SetTrapObjStyle(o);
            SetBoxColliderActive(false);
        });
    }

    /// <summary>
    /// 销毁
    /// </summary>
    public virtual void Destroy()
    {
        if (prefabObj != null)
        {
            GameObject.Destroy(prefabObj);
        }
    }


    /// <summary>
    /// 重写死亡, TODO:不同机关的死亡动画
    /// </summary>
    public override void OnChildDie()
    {
        OnStop();
        //EventManager.Dispatch(EventName.battle_monsterDie, prefabObj);
        //死亡动画配置
        //AnimatorEventData dataCfg = animatorEventData.death;
        //回收血条
        HpSliderManager.ins.OnOneObjDisappear(this);
        // 移除
        BattleTrapManager.Instance.RemoveBattleTrapItem(this);
        Destroy();
    }

    /// <summary>
    /// 设置机关展现风格
    /// </summary>
    /// <param name="o"></param>
    private void SetTrapObjStyle(GameObject o)
    {
        // - base obj param set!
        roleObj = o;
        prefabObj = o;
        // - base obj param set over
        o.name = $"trap_{trapId}_{index.x}_{index.y}";
        if (parent != null)
        {
            o.transform.SetParent(parent);
            o.transform.localScale = Vector3.one;
            o.transform.position = new Vector3((float)pos.x, (float)pos.y, (float)pos.z);
            // 修正y==0
            o.transform.localPosition = new Vector3(o.transform.localPosition.x, 0, o.transform.localPosition.z);
            string[] dire = this.battleTrapCfgData.direction.Split('|');
            switch (int.Parse(dire[0]))
            {
                case 1://上
                    o.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    break;
                case 2://下
                    o.transform.localRotation = Quaternion.Euler(Vector3.zero);
                    break;
                case 3://左
                    o.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));
                    break;
                case 4://右
                    o.transform.localRotation = Quaternion.Euler(new Vector3(0, 270, 0));
                    break;
                default:
                    break;
            }
        }

        // 无法通过的
        BoxCollider boxCollider = o.GetComponent<BoxCollider>();
        if (battleTrapCfgData.pass == 0)
        {
            if (boxCollider == null)
                boxCollider = o.AddComponent<BoxCollider>();

            boxCollider.size = new Vector3(2, 20, 2);
            boxCollider.center = new Vector3(0, 10, 0);
        }
        else
        {
            if (boxCollider != null)
                GameObject.Destroy(boxCollider);
        }

        CreateHPNode(prefabObj);
    }

    /// <summary>
    /// 构造带血量机关的血条节点
    /// </summary>
    /// <param name="o"></param>
    private void CreateHPNode(GameObject o)
    {
        if (o != null && battleAttr.ContainsKey((long)EAttr.HP))
        {
            GameObject point_headtop = new GameObject("point_headtop");
            point_headtop.transform.localScale = Vector3.one;
            point_headtop.transform.parent = o.transform;
            point_headtop.transform.localPosition = new Vector3(0, 1.2f, 0);

            FBXBindBonesHelper fBXBindBonesHelper = o.GetOrAddCompoonet<FBXBindBonesHelper>();
            fBXBindBonesHelper.Bind();
            HpSliderManager.ins.CreatOneSliderByMonster(this);
        }
    }

    /// <summary>
    /// 添加一个对象到受机关影响缓存列表
    /// </summary>
    /// <param name="baseObject"></param>
    /// <param name="battleTrapCfgData"></param>
    /// <param name="doTriggerResult"></param>
    public void AddCacheTrapDoObj(BaseObject baseObject, BattleTrapCfgData battleTrapCfgData, string doTriggerResult)
    {
        foreach (var co in cacheTrapDoObjs)
        {
            if (co.baseObject == baseObject)
            {
                co.Update(doTriggerResult);
                return;
            }
        }
        cacheTrapDoObjs.Add(new TrapDoBattleObj(baseObject, battleTrapCfgData, doTriggerResult));
    }

    /// <summary>
    /// 移除一个对象从受机关影响缓存列表
    /// </summary>
    /// <param name="baseObject"></param>
    /// <returns></returns>
    public string RemoveCacheTrapDoObj(BaseObject baseObject)
    {
        for (int i = cacheTrapDoObjs.Count - 1; i >= 0; i--)
        {
            if (cacheTrapDoObjs[i].baseObject == baseObject)
            {
                string retStr = cacheTrapDoObjs[i].doTriggerResult;
                cacheTrapDoObjs.RemoveAt(i);
                return retStr;
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// 获取一个对象从受机关影响缓存列表
    /// </summary>
    /// <param name="baseObject"></param>
    /// <returns></returns>
    public TrapDoBattleObj GetCacheTrapDoObj(BaseObject baseObject)
    {
        for (int i = cacheTrapDoObjs.Count - 1; i >= 0; i--)
        {
            if (cacheTrapDoObjs[i].baseObject == baseObject)
            {
                return cacheTrapDoObjs[i];
            }
        }
        return null;
    }

    /// <summary>
    /// 上阵时隐藏， 开始战斗打开
    /// </summary>
    public void SetBoxColliderActive(bool b)
    {
        if (trapObj != null)
        {
            BoxCollider[] boxColliders = trapObj.GetComponentsInChildren<BoxCollider>();
            if (boxColliders != null)
            {
                Debug.LogError("~~ boxColliders len:" + boxColliders.Length);
                foreach (var bc in boxColliders)
                {
                    bc.enabled = b;
                }
            }
        }
    }

    /// <summary>
    /// ToString
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"trapId:{trapId}, index-x:{index.x}-y:{index.y}, pos-x:{pos.x}-y:{pos.y}-z:{pos.z}, cfg:{battleTrapCfgData.ToString()}";
    }
}

/// <summary>
/// 缓存受机关影响的战斗对象
/// </summary>
public class TrapDoBattleObj
{
    public BaseObject baseObject = null;
    public BattleTrapCfgData battleTrapCfgData;
    public string doTriggerResult;
    public TrapDoBattleObj(BaseObject baseObject, BattleTrapCfgData battleTrapCfgData, string doTriggerResult)
    {
        this.baseObject = baseObject;
        this.battleTrapCfgData = battleTrapCfgData;
        this.doTriggerResult = doTriggerResult;
    }

    public void Update(string doTriggerResult)
    {
        this.doTriggerResult = doTriggerResult;
    }
}