using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSelectedHelper : SingletonNotMono<BattleSelectedHelper>
{
    // ��Ӧ��������ֵ
    // ��
    private float darkValue = 0.5f;
    // ����
    private float norValue = 1f;
    // ��
    private float lightValue = 2f;

    // ���Ķ���
    private List<GameObject> m_HighlightObjs = new List<GameObject>();
    // ���Ķ���
    private List<GameObject> m_DarkObjs = new List<GameObject>();

    private List<BaseObject> m_PotentialTargetObjs = new List<BaseObject>();

    private Camera m_BattleCamera;
    private GameObject m_GlobalMask;
    private Material m_GlobalMaskMat;
    // private List<GameObject> m_HighLightModels;
    /// <summary>
    /// 开始战斗的时候初始化
    /// </summary>
    public void Init()
    {
        m_BattleCamera = GameCenter.mIns.m_BattleMgr.battleCamer;
        Transform mainCamera = GameObject.Find("mainCamera").transform;
        if (mainCamera.FindHideInChild("battleGlobalMask") == null)
        {
            m_GlobalMask = m_BattleCamera.transform.FindHideInChild("battleGlobalMask").gameObject;
        }
        else
        {
            m_GlobalMask = mainCamera.FindHideInChild("battleGlobalMask").gameObject;
        }

        m_GlobalMask.transform.parent = m_BattleCamera.transform.parent;
        m_GlobalMask.transform.localPosition = m_BattleCamera.transform.localPosition;
        m_GlobalMask.transform.localRotation = m_BattleCamera.transform.localRotation;
        m_GlobalMask.transform.parent = m_BattleCamera.transform;
        m_GlobalMask.transform.localScale = Vector3.one * 2;
        m_GlobalMask.transform.localPosition = new Vector3(0, 0, 0.5f);
        Material cloneMat = new Material(m_GlobalMask.GetComponent<SpriteRenderer>().material);
        cloneMat.name = cloneMat.name + "(Clone)";
        cloneMat.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
        m_GlobalMask.GetComponent<SpriteRenderer>().material = cloneMat;
        m_GlobalMaskMat = cloneMat;

        m_GlobalMaskMat.renderQueue = 3010;
    }

    public void OnSetBattleGlobalMaskActive(bool bActive, DrawCardData drawCardData = null, BaseHero curHero = null)
    {
        if (bActive)
        {
            m_PotentialTargetObjs = GetPotentialTarget(drawCardData, curHero);
            if (m_PotentialTargetObjs != null && m_PotentialTargetObjs.Count > 0)
            {
                foreach (var item in m_PotentialTargetObjs)
                {
                    ModelRenderQueueHelper modelRenderQueueHelper = item.roleObj.GetComponentInChildren<ModelRenderQueueHelper>();
                    if (modelRenderQueueHelper == null)
                        modelRenderQueueHelper = item.roleObj.GetComponentInParent<ModelRenderQueueHelper>();
                    if (modelRenderQueueHelper != null)
                    {
                        modelRenderQueueHelper.ToRenderQueue(3030);
                    }
                }
            }
        }

        if (bActive != m_GlobalMask.activeSelf)
            m_GlobalMask.SetActive(bActive);
    }

    /// <summary>
    /// ����
    /// </summary>
    public void Clear()
    {
        foreach (GameObject o in m_HighlightObjs)
            DoObjLightParam(o, 0);
        foreach (GameObject o in m_DarkObjs)
            DoObjLightParam(o, 0);
        m_HighlightObjs.Clear();
        m_DarkObjs.Clear();
        m_GlobalMask.SetActive(false);
    }

    /// <summary>
    /// �Ƿ������Ķ���
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    private bool HasHighlightObj(GameObject o)
    {
        GameObject findObj = m_HighlightObjs.Find(delegate (GameObject _fo)
        {
            return _fo.GetInstanceID() == o.GetInstanceID();
        });
        return findObj != null;
    }

    /// <summary>
    /// �Ƿ��ǰ��Ķ���
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    private bool HasDarkObj(GameObject o)
    {
        GameObject findObj = m_DarkObjs.Find(delegate (GameObject _fo)
        {
            return _fo.GetInstanceID() == o.GetInstanceID();
        });
        return findObj != null;
    }

    /// <summary>
    /// �޸Ķ���ķ����̶�
    /// </summary>
    /// <param name="o"></param>
    /// <param name="lightType"></param>
    private void DoObjLightParam(GameObject o, int lightType)
    {
        if (o == null)
            return;
        SkinnedMeshRenderer[] skinnedMeshRenderers = o.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshRenderers != null && skinnedMeshRenderers.Length > 0)
        {
            foreach (SkinnedMeshRenderer smr in skinnedMeshRenderers)
            {
                if (smr != null)
                {
                    Material material = smr.material;
                    //material.renderQueue = lightType == 1 ? 3011 : 3000;
                    if (material != null && material.HasFloat("_ReduceBright"))
                    {
                        //material.SetFloat("_ReduceBright", lightType == -1 ? darkValue : (lightType == 1 ? lightValue : norValue));
                        // 现在不变暗
                        material.SetFloat("_ReduceBright", lightType == -1 ? norValue : (lightType == 1 ? lightValue : norValue));
                    }
                }
            }
        }
        //BattleSelectHighLight.Instance.OnHighLightModels(m_HighlightObjs);

        //ModelRenderQueueHelper modelRenderQueueHelper = o.GetComponentInChildren<ModelRenderQueueHelper>();
        //if (modelRenderQueueHelper == null)
        //    modelRenderQueueHelper = o.GetComponentInParent<ModelRenderQueueHelper>();
        //if (modelRenderQueueHelper != null)
        //{
        //    if (lightType == 1)
        //        modelRenderQueueHelper.ToRenderQueue(3030);
        //    else
        //        modelRenderQueueHelper.ToRenderQueue(3000);
        //}
    }

    /// <summary>
    /// �������б����Ƴ�
    /// </summary>
    /// <param name="o"></param>
    public void RemoveFromHighlights(GameObject o)
    {
        int count = m_HighlightObjs.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            if (m_HighlightObjs[i].GetInstanceID() == o.GetInstanceID())
            {
                DoObjLightParam(o, 0);
                m_HighlightObjs.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// �������б����Ƴ�
    /// </summary>
    /// <param name="os"></param>
    public void RemoveFromHighlights(List<GameObject> os)
    {
        foreach (GameObject o in os)
            RemoveFromHighlights(o);
    }

    /// <summary>
    /// �Ӱ����б����Ƴ�
    /// </summary>
    /// <param name="o"></param>
    public void RemoveFromDarks(GameObject o)
    {
        int count = m_DarkObjs.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            if (m_DarkObjs[i].GetInstanceID() == o.GetInstanceID())
            {
                DoObjLightParam(o, 0);
                m_DarkObjs.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// �Ӱ����б����Ƴ�
    /// </summary>
    /// <param name="os"></param>
    public void RemoveFromDarks(List<GameObject> os)
    {
        foreach (GameObject o in os)
            RemoveFromDarks(o);
    }

    /// <summary>
    /// ���������Ķ����б�
    /// </summary>
    /// <param name="o"></param>
    public void AddHighlightObj(GameObject o)
    {
        if (HasHighlightObj(o))
            return;

        if (HasDarkObj(o))
            RemoveFromDarks(o);

        m_HighlightObjs.Add(o);
        DoObjLightParam(o, 1);
    }

    /// <summary>
    /// ���������Ķ����б�
    /// </summary>
    /// <param name="os"></param>
    public void AddHighlightObj(List<GameObject> os)
    {
        foreach (GameObject o in os)
            AddHighlightObj(o);
    }

    /// <summary>
    /// ���������Ķ����б�
    /// </summary>
    /// <param name="o"></param>
    public void AddDarkObj(GameObject o)
    {
        if (HasDarkObj(o))
            return;

        if (HasHighlightObj(o))
            RemoveFromDarks(o);

        m_DarkObjs.Add(o);
        DoObjLightParam(o, -1);
    }

    /// <summary>
    /// ���������Ķ����б�
    /// </summary>
    /// <param name="os"></param>
    public void AddDarkObj(List<GameObject> os)
    {
        foreach (GameObject o in os)
            AddDarkObj(o);
    }

    #region 战斗ui单体指示器
    private GameObject m_SingleIndicatorObj = null;
    private BattleUIIndicator m_BattleUIIndicator = null;
    public async void CreateSingleUIIndicator(GameObject parent)
    {
        if (parent == null) return;

        if (m_SingleIndicatorObj == null)
        {
            m_SingleIndicatorObj = await ResourcesManager.Instance.LoadPrefabSync("skillIndicator_ui_single.prefab");
            commontool.ResetUIBaseParam(m_SingleIndicatorObj, parent);
            m_BattleUIIndicator = m_SingleIndicatorObj.GetOrAddCompoonet<BattleUIIndicator>();
            m_BattleUIIndicator.Begin();
        }

        if (m_BattleUIIndicator == null) return;

        m_BattleUIIndicator.Drag();
    }

    public void DestroySingleUIIndicator()
    {
        if (m_SingleIndicatorObj != null)
            GameObject.Destroy(m_SingleIndicatorObj);
        m_SingleIndicatorObj = null;
        m_BattleUIIndicator = null;
    }

    #endregion

    #region 查找技能潜在作用对象

    private List<BaseObject> GetPotentialTarget(DrawCardData drawCardData, BaseHero curHero)
    {
        if (drawCardData == null || curHero == null)
        {
            return null;
        }

        // MonsterManager.Instance.dicMonsterList

        // HeroManager.Instance.dicHeroList

        string checkTarget = drawCardData.skillCfgData.hightlight;
        string[] targetType = checkTarget.Split('|');
        //获得技能目标类型 获得检测列表
        List<BaseObject> checkList = new List<BaseObject>();
        for (int i = 0; i < targetType.Length; i++)
        {
            if (targetType[i] == "1")//敌人
            {
                foreach (var kv in BattleMonsterManager.Instance.dicMonsterList)
                {
                    checkList.Add(kv.Value);
                }
                // 还原
                foreach (var kv in BattleHeroManager.Instance.depolyHeroList)
                {
                    ModelRenderQueueHelper modelRenderQueueHelper = kv.prefabObj.GetComponentInChildren<ModelRenderQueueHelper>();
                    if (modelRenderQueueHelper == null)
                        modelRenderQueueHelper = kv.prefabObj.GetComponentInParent<ModelRenderQueueHelper>();
                    if (modelRenderQueueHelper != null)
                    {
                        modelRenderQueueHelper.ToRenderQueue(3000);
                    }
                }
            }
            else if (targetType[i] == "2")//友军
            {
                // 还原
                foreach (var kv in BattleMonsterManager.Instance.dicMonsterList)
                {
                    ModelRenderQueueHelper modelRenderQueueHelper = kv.Value.prefabObj.GetComponentInChildren<ModelRenderQueueHelper>();
                    if (modelRenderQueueHelper == null)
                        modelRenderQueueHelper = kv.Value.prefabObj.GetComponentInParent<ModelRenderQueueHelper>();
                    if (modelRenderQueueHelper != null)
                    {
                        modelRenderQueueHelper.ToRenderQueue(3000);
                    }
                }

                foreach (var kv in BattleHeroManager.Instance.depolyHeroList)
                {
                    checkList.Add(kv);
                }
            }
            //else if (targetType[i] == "3")//自己
            //{
            //    if (!checkList.Contains(curHero))
            //    {
            //        checkList.Add(curHero);
            //    }
            //}
        }
        // 始终加入自己
        if (!checkList.Contains(curHero))
            checkList.Add(curHero);

        return checkList;
    }

    #endregion
}
