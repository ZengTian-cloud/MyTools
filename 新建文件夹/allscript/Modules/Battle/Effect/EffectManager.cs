using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特效管理类
/// </summary>
public class EffectManager 
{


    private static EffectManager Ins;
    public static EffectManager ins
    {
        get
        {
            if (Ins == null)
            {
                Ins = new EffectManager();
            }
            return Ins;
        }
    }

    public Dictionary<string, float> dicEffectTime = new Dictionary<string, float>();

    /// <summary>
    /// 播放特效
    /// </summary>
    /// <param name="skillBuffCfg"></param>
    public async UniTask<GameObject> CreatEffect(string effName, BaseObject target)
    {
        GameObject eff = BattlePoolManager.Instance.OutPool(ERootType.Effect, effName);
        if (eff == null)
        {
            eff = await ResourcesManager.Instance.LoadAssetSyncByName($"{effName}.prefab") as GameObject;
            if (eff == null)
            {
                Debug.LogError($"未找到{effName}特效资源，请检查！");
            }
            eff.SetActive(false);
        }

        eff.transform.SetParent(target.effectRoot);

        eff.transform.localPosition = Vector3.zero;
        eff.transform.localRotation = Quaternion.Euler(Vector3.zero);
        return eff;
    }


    /// <summary>
    /// 播放特效 在地板
    /// </summary>
    /// <param name="skillBuffCfg"></param>
    public async UniTask<GameObject> CreatEffect(string effName, BaseObject holder, Vector3 targetPos)
    {
        GameObject eff = BattlePoolManager.Instance.OutPool(ERootType.Effect, effName);
        if (eff == null)
        {
            eff = await ResourcesManager.Instance.LoadAssetSyncByName($"{effName}.prefab") as GameObject;
            if (eff == null)
            {
                Debug.LogError($"未找到{effName}特效资源，请检查！");
            }
            eff.SetActive(false);
        }
        eff.transform.SetParent(holder.prefabObj.transform);
        eff.transform.position = targetPos;
        eff.transform.localRotation = Quaternion.Euler(Vector3.zero);
        return eff;
    }

    /// <summary>
    /// 获得特效下层级最长的持续时间
    /// </summary>
    /// <returns></returns>
    public float GetLonggerTimeByEffectItem(GameObject item)
    {
        float time = 0;
        float newtime = 0;
        ParticleSystem particleSystem = item.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            time = particleSystem.main.duration;
        }
        if (item.transform.childCount > 0)
        {
            for (int i = 0; i < item.transform.childCount; i++)
            {
                newtime = GetLonggerTimeByEffectItem(item.transform.GetChild(i).gameObject);
                if (newtime > time)
                {
                    time = newtime;
                }
            }
        }
        return time;
    }
}

public class EffectItem
{
    public ParticleSystem particle;

    public GameObject obj;

    public string name;

    public bool isLoop;//是否循环播放动画
    //特效计时器
    public float timer;

    public int state;//状态 1-播放中 2-为播放

    public EffectItem(ParticleSystem particle,string name, GameObject obj, bool isLoop, float timer = 0)
    {
        this.particle = particle;
        this.timer = timer;
        this.name = name;
        this.obj = obj;
        this.isLoop = isLoop;
    }
}
