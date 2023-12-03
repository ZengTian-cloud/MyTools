using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 对象上特效管理组件
/// </summary>
public class EffectStackCompent:MonoBehaviour
{
    private long guid;
    //特效列表
    private List<GameObject> effectList;
    private void Awake()
    {
        effectList = new List<GameObject>();
    }

    public void Oninit(long guid)
    {
        this.guid = guid;
    }

    /// <summary>
    /// 表现一个特效
    /// </summary>
    /// <param name="heroID"></param>
    /// <param name="effectName"></param>
    /// <param name="isloop"></param>
    public async void ShowEffect(string effectName,Vector3 pos = default, bool isloop = false)
    {
        float duration = 0;
        BaseObject baseObject = GameCenter.mIns.m_BattleMgr.GetBaseObjByGuid(guid);
        if (baseObject == null)
        {
            Debug.LogError($"未在战场中找到guid为{guid}的对象，请检查！");
            return;
        }
        GameObject effItem = await EffectManager.ins.CreatEffect(effectName, baseObject);
        effectList.Add(effItem);
        if (EffectManager.ins.dicEffectTime.ContainsKey(effectName))
        {
            duration = EffectManager.ins.dicEffectTime[effectName];
        }
        else
        {
            duration = EffectManager.ins.GetLonggerTimeByEffectItem(effItem);
            EffectManager.ins.dicEffectTime.Add(effectName, duration);
        }

        effItem.transform.position = pos;
        effItem.SetActive(true);
        GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke((int)(duration * 1000), () => {
            if (effItem!=null)
            {
                effItem.SetActive(false);
                effectList.Remove(effItem);
                BattlePoolManager.Instance.InPool(ERootType.Effect, effItem, effectName);
            }
           
        });
    }

    /// <summary>
    /// 表现一个特效
    /// </summary>
    /// <param name="heroID"></param>
    /// <param name="effectName"></param>
    /// <param name="isloop"></param>
    public async UniTask<GameObject> ShowEffect(string effectName, Transform parent, Vector3 pos = default,Vector3 offect = default, bool isloop = false)
    {
        float duration = 0;
        BaseObject baseObject = GameCenter.mIns.m_BattleMgr.GetBaseObjByGuid(guid);
        if (baseObject == null)
        {
            return null;
        }
        GameObject effItem = await EffectManager.ins.CreatEffect(effectName, baseObject);

        if (EffectManager.ins.dicEffectTime.ContainsKey(effectName))
        {
            duration = EffectManager.ins.dicEffectTime[effectName];
        }
        else
        {
            duration = EffectManager.ins.GetLonggerTimeByEffectItem(effItem);
            EffectManager.ins.dicEffectTime.Add(effectName, duration);
        }
        effItem.transform.SetParent(parent);
        effItem.transform.position = pos;
        effItem.transform.localPosition += offect;
        effItem.SetActive(true);
        if (!isloop)
        {
            GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke((int)(duration * 1000), () => {
                if (effItem != null)
                {
                    effItem.SetActive(false);
                    BattlePoolManager.Instance.InPool(ERootType.Effect, effItem, effectName);
                }
            });
        }
        effItem.name = effectName;
        return effItem;
    }

    public void Clear()
    {
        if (effectList != null)
        {
            for (int i = 0; i < effectList.Count; i++)
            {
                if (effectList[i] != null)
                {
                    effectList[i].SetActive(false);
                    GameObject.Destroy(effectList[i].gameObject);
                }
            }
            effectList.Clear();
        }
    }


    /// <summary>
    /// 表现一个特效 在地板
    /// </summary>
    /// <param name="heroID"></param>
    /// <param name="effectName"></param>
    /// <param name="isloop"></param>
    public async void ShowEffectOnPanel(string effectName, Vector3 pos , bool isloop = false)
    {
        float duration = 0;
        BaseObject baseObject = GameCenter.mIns.m_BattleMgr.GetBaseObjByGuid(guid);
        if (baseObject == null)
        {
            Debug.LogError($"未在战场中找到objid为{guid}的对象，请检查！");
            return;
        }
        GameObject effItem = await EffectManager.ins.CreatEffect(effectName, baseObject, pos);

        if (EffectManager.ins.dicEffectTime.ContainsKey(effectName))
        {
            duration = EffectManager.ins.dicEffectTime[effectName];
        }
        else
        {
            duration = EffectManager.ins.GetLonggerTimeByEffectItem(effItem);
            EffectManager.ins.dicEffectTime.Add(effectName, duration);
        }
        effItem.transform.position = pos;
        effItem.transform.rotation = Quaternion.Euler(Vector3.zero);
        effItem.SetActive(true);
        GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke((int)(duration * 1000), () => {
            effItem.SetActive(false);
            BattlePoolManager.Instance.InPool(ERootType.Effect, effItem, effectName);
        });
    }

    /// <summary>
    /// 表现一个特效 不回收
    /// </summary>
    /// <param name="effectName"></param>
    /// <param name="pos"></param>
    public void ShowEffectOnPanelUnRecycle(string effectName, Vector3 pos)
    {

    }
}


