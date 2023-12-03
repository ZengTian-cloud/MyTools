using Cysharp.Threading.Tasks;
using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CloseUpObject
{
    private GameObject m_Target;
    private long m_HeroId;
    private Vector3 m_TargetPosition;
    private int m_Index;

    public string prefabPath;

    public GameObject closeUpObj;
    public BattleCloseUpHelper battleCloseUpHelper = null;

    public bool isRunning = false;
    public bool isRuned = false;

    private Quaternion m_Rotation = default;

    private async UniTask<GameObject> GetCloseUpPrefabObj(string closeUpPrefabName)
    {
        return await ResourcesManager.Instance.LoadAssetSyncInstantiate($"Assets/allres/prefabs/closeup/{closeUpPrefabName}.prefab");
    }

    public void Init(GameObject target, long heroId, Vector3 targetPosition, int index)
    {
        if (target == null || heroId <= 0) return;

        m_Target = target;
        m_HeroId = heroId;
        m_TargetPosition = targetPosition;
        m_Index = index;
        m_Rotation = target.transform.GetChild(0).rotation;
    }

    public async void Create()
    {
        if (m_Target == null || m_HeroId <= 0) return;

        string closeUpPrefabName = $"closeup_{m_HeroId}_{m_Index}";
        //Debug.LogError("closeUpPrefabName:" + closeUpPrefabName);
        closeUpObj = await GetCloseUpPrefabObj(closeUpPrefabName);
        if (closeUpObj != null)
        {
            closeUpObj.transform.position = m_Target.transform.position;
            //closeUpObj.transform.rotation = m_Rotation;
            closeUpObj.transform.LookAt(m_TargetPosition);
            battleCloseUpHelper = closeUpObj.GetOrAddCompoonet<BattleCloseUpHelper>();
            // SetSceneActive(false);
        }
    }

    public void Run()
    {
        //Debug.LogError("~~~~~~~ Run:" + m_Index);
        Create();
        battleCloseUpHelper.Begin(m_Target, this);
        isRunning = true;
    }

    public void End()
    {
        isRuned = true;
        isRunning = false;
        if (closeUpObj != null)
        {
            GameObject.Destroy(closeUpObj);
        }
    }
}

public class BattleCloseUpMgr : Singleton<BattleCloseUpMgr>
{
    private GameObject mainCamera = null;
    private List<CloseUpObject> m_CloseUpObjs = null;
    private BaseObject m_BaseObject;
    private bool m_IsRunning = false;
    //private float m_Angle = 0.0f;
    public void OnStartCloseUp(BaseObject baseObject, GameObject target, long heroId, Vector3 targetPosition)
    {
        if (baseObject == null || target == null || heroId <= 0) return;

        if (m_IsRunning) return;
        m_BaseObject = baseObject;
        m_CloseUpObjs = new List<CloseUpObject>();

        //Vector3 dire = targetPosition - target.transform.position;
        //Debug.LogError("~~ targetPosition:" + targetPosition + " - target.transform.position:" + target.transform.position + " - dire:" + dire);
        //var v = Vector3.Dot(dire.normalized, Vector3.forward);
        //float angle = Mathf.Acos(v);
        //angle *= Mathf.Rad2Deg;
        //Debug.LogError("v:" + v + " - angle:" + angle + " - aa:" + Mathf.Acos(v));

        int count = 1;
        for (int i = 1; i <= count; i++)
        {
            CloseUpObject closeUpObject = new CloseUpObject();
            closeUpObject.Init(target, heroId, targetPosition, i);
            m_CloseUpObjs.Add(closeUpObject);
        }

        if (Run())
        {
            if (mainCamera == null)
                mainCamera = GameObject.Find("mainCamera");
            mainCamera.SetActive(false);
            SetSceneActive(false);
        }
    }

    public void OnExitCloseUp(CloseUpObject closeUpObject)
    {
        // Debug.LogError("~~~~~~~ OnExitCloseUp:" + closeUpObject);
        if (closeUpObject != null)
        {
            closeUpObject.End();
            if (!Run())
            {
                Over();
            }
        }
    }

    public bool Run()
    {
        if (m_CloseUpObjs != null && m_CloseUpObjs.Count > 0)
        {
            foreach (var cuo in m_CloseUpObjs)
            {
                // Debug.LogError("~~~~~~~ cuo:" + cuo.isRuned);
                if (!cuo.isRuned)
                {
                    cuo.Run();
                    return true;
                }
            }
        }
        return false;
    }

    public void Over()
    {
        SetSceneActive(true);

        if (mainCamera == null)
            mainCamera = GameObject.Find("mainCamera");
        mainCamera.SetActive(true);
        m_CloseUpObjs = null;
        m_IsRunning = false;

        UnBindSceneRoots();
    }

    /// <summary>
    /// TODO: �Ż�-������ʽ����?
    /// </summary>

    private GameObject sceneNode_maptest;
    private GameObject sceneNode_mod;
    private GameObject sceneNode_shadow;
    private GameObject sceneNode_effect;
    private GameObject sceneNode_sky;
    // ս������Ҫ���ص�
    private GameObject battle_BattleRoot;
    private GameObject battle_RolePointList;
    private GameObject battle_HeroListRoot;
    private GameObject battle_MonsterListRoot;
    private GameObject battle_BulletListRoot;
    private GameObject battle_start_point;
    private GameObject battle_end_point;


    private void SetSceneActive(bool bActive)
    {
        if (sceneNode_maptest == null)
            sceneNode_maptest = GameObject.Find("maptest");
        if (sceneNode_mod == null)
            sceneNode_mod = GameObject.Find("mod");
        if (sceneNode_shadow == null)
            sceneNode_shadow = GameObject.Find("shadow");
        if (sceneNode_effect == null)
            sceneNode_effect = GameObject.Find("effect");

        sceneNode_sky = GameCenter.mIns.m_BattleMgr.battleScence.transform.Find("sky").gameObject;
       

        battle_BattleRoot = GameCenter.mIns.m_BattleMgr.battleRoot.gameObject;
        battle_RolePointList = battle_BattleRoot.transform.Find("RolePointList").gameObject;
        battle_HeroListRoot = GameCenter.mIns.m_BattleMgr.bulletListRoot.gameObject;
        battle_BulletListRoot = GameCenter.mIns.m_BattleMgr.heroListRoot.gameObject;
        battle_MonsterListRoot = GameCenter.mIns.m_BattleMgr.monsterListRoot.gameObject;
        battle_start_point = battle_BattleRoot.transform.Find("start_point(Clone)").gameObject;
        battle_end_point = battle_BattleRoot.transform.Find("end_point(Clone)").gameObject;

        if (sceneNode_maptest != null)
            sceneNode_maptest.SetActive(bActive);
        if (sceneNode_mod != null)
            sceneNode_mod.SetActive(bActive);
        if (sceneNode_shadow != null)
            sceneNode_shadow.SetActive(bActive);
        if (sceneNode_effect != null)
            sceneNode_effect.SetActive(bActive);
        if (sceneNode_sky != null)
            sceneNode_sky.SetActive(!bActive);

        if (battle_RolePointList != null)
            battle_RolePointList.SetActive(bActive);
        if (battle_start_point != null)
            battle_start_point.SetActive(bActive);
        if (battle_end_point != null)
            battle_end_point.SetActive(bActive);
    }

    private void UnBindSceneRoots()
    {
        sceneNode_maptest = null;
        sceneNode_mod = null;
        sceneNode_shadow = null;
        sceneNode_effect = null;
        sceneNode_sky = null;
        // ս������Ҫ���ص�
        battle_BattleRoot = null;
        battle_RolePointList = null;
        battle_HeroListRoot = null;
        battle_MonsterListRoot = null;
        battle_BulletListRoot = null;
        battle_start_point = null;
        battle_end_point = null;
    }
}
