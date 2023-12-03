using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneManager : SingletonNotMono<MainSceneManager>
{
    private Camera m_MainCamera;

    public Camera m_MainSceneCamera;

    private TestMainSceneHeroCtrl m_HeroCtrl;

    public GameObject m_HeroObj;

    private GameObject m_MainSceneRoot;
    private GameObject m_NodeHeros;
    private GameObject npcNode;

    /// <summary>
    ///场景初始化
    /// </summary>
    public void Init()
    {
        m_MainCamera = GameObject.Find("mainCamera").GetComponent<Camera>();
        m_MainSceneCamera = m_MainCamera.transform.FindHideInChild("mainSceneCamera").GetComponent<Camera>();

        m_MainCamera.transform.AddMissingComponent<AudioListener>().enabled = true;
        //m_MainSceneCamera.transform.AddMissingComponent<AudioListener>();

        // TODO: Test ����ɾ���ڵ�
        m_MainSceneRoot = GameObject.Find("mainsceneroot");
        m_NodeHeros = GameObject.Find("mainsceneroot/main01(Clone)/nodeHeros");

        //MonoCoroutineTool.DelayInvokeBySecond(() => {
        //    m_MainSceneRoot = GameObject.Find("mainsceneroot");
        //    m_NodeHeros = GameObject.Find("mainsceneroot/nodeHeros");
        //    OnEnter();
        //}, 0.2f);
        OnEnter();
        if (m_HeroObj == null)
        {
            LoadMainHero();
        }
        LoadNpc();

        GameEventMgr.Register(GEKey.NpcInteracton_OnInteractionCompleted,RefreshNpc);
    }

    public Camera GetMainSceneCamera()
    {
        if (m_MainSceneCamera == null && m_MainCamera != null)
        {
            Transform mainSceneCameraTran = m_MainCamera.transform.FindHideInChild("mainSceneCamera");
            if (mainSceneCameraTran != null)
            {
                m_MainSceneCamera = mainSceneCameraTran.GetComponent<Camera>();
            }
        }
        return m_MainSceneCamera;
    }

    public TestMainSceneHeroCtrl GetHeroCtrl()
    {
        if (m_HeroCtrl == null && m_MainCamera != null)
        {
            Transform mainSceneCameraTran = m_MainCamera.transform.FindHideInChild("mainSceneCamera");
            Debug.Log($"GIANTGAME ====== mainSceneCameraTran:{mainSceneCameraTran}");
            if (mainSceneCameraTran != null)
            {
                m_HeroCtrl = mainSceneCameraTran.GetComponent<TestMainSceneHeroCtrl>();
            }
        }
        return m_HeroCtrl;
    }
    public GameObject GetMainScenRoot()
    {
        if (m_MainSceneRoot == null)
        {
            m_MainSceneRoot = GameObject.Find("mainsceneroot");
        }
        return m_MainSceneRoot;
    }

    public GameObject GetNodeHeroRoot()
    {
        if (m_NodeHeros == null)
        {
            m_NodeHeros = GameObject.Find("mainsceneroot/main01(Clone)/nodeHeros");
        }
        return m_NodeHeros;
    }


    /// <summary>
    /// 进入场景
    /// </summary>
    public void OnEnter()
    {
        Debug.Log($"GIANTGAME  m_HeroCtrl:{m_HeroCtrl},  m_MainCamera:{m_MainCamera} , GetHeroCtrl()：{GetHeroCtrl()} ,GetMainScenRoot():{GetMainScenRoot()}");
        TestMainSceneHeroCtrl heroctrl = GetHeroCtrl();
        if (GetMainSceneCamera() != null)
        {
            GetHeroCtrl()?.SetActive(true);
            //GetMainSceneCamera().gameObject.SetActive(true);
        }
        if (GetMainScenRoot() != null)
            GetMainScenRoot()?.SetActive(true);
        // todo

        //Debug.LogError("OnEnter!!!!!!!!!");

        GameCenter.mIns.m_FogManager.SetSceneSetting("scene_main01");
        //GameCenter.mIns.m_UIMgr.Close<NpcInteractionWidget>();
        if (npcDic!=null)
        {
            RefreshNpc(null);
        }
    }

    /// <summary>
    /// 离开场景
    /// </summary>
    public void OnLeave()
    {
        if (GetMainSceneCamera() != null)
            GetHeroCtrl().SetActive(false);
        if (GetMainScenRoot() != null)
            GetMainScenRoot().SetActive(false);

        GameEventMgr.Distribute(GEKey.OnLeaveMainScene);
    }

    /// <summary>
    /// 加载主场景英雄
    /// </summary>
    private void LoadMainHero(int heroId = 101006)
    {
        if (m_MainSceneCamera == null)
        {
            return;
        }
        // test
        ResourcesManager.Instance.LoadPrefabAsync("role", "role_101006", async (roleObj) =>
        {
            if (roleObj != null)
            {
                Physics.autoSyncTransforms = true;

                m_HeroObj = roleObj;
                //m_HeroCtrl = m_MainSceneCamera.gameObject.GetOrAddCompoonet<TestMainSceneHeroCtrl>();
                m_HeroCtrl = m_MainSceneCamera.gameObject.AddMissingComponent<TestMainSceneHeroCtrl>();
                m_HeroCtrl.Init(m_MainSceneCamera, m_HeroObj.transform);
                if (GetNodeHeroRoot() != null)
                    roleObj.transform.SetParent(GetNodeHeroRoot().transform);
                roleObj.transform.localScale = Vector3.one;
                roleObj.transform.localRotation = Quaternion.Euler(0, -79, 0);
                roleObj.transform.localPosition = new Vector3(1, 0.036f,-9.24f);

                Debug.Log($"<color=#ff0000> 角色{roleObj.name}  当前位置 ：{roleObj.transform.localPosition} </color>");
               
                roleObj.gameObject.tag = "Player";

                m_MainSceneCamera.transform.localRotation = Quaternion.Euler(0, roleObj.transform.localEulerAngles.y, 0);

                // �����ܲ�����
                m_HeroObj.GetComponent<Animator>().runtimeAnimatorController = await ResourcesManager.Instance.LoadAssetSync("Assets/allmodel/main/prefabs/101006anim.controller") as RuntimeAnimatorController;

                // ���ý�ɫlayer
                Transform[] transforms = roleObj.transform.GetComponentsInChildren<Transform>();
                foreach (var item in transforms)
                {
                    item.gameObject.layer = 9;
                }
                // test
                m_HeroCtrl.PlayIdle();

                GameEventMgr.Distribute(GEKey.OnEnterMainScene, m_HeroObj, m_MainSceneCamera);
            }
        });
    }

    public void SetRolePosAndRot(Vector3 pos,Vector3 ro)
    {
        if (m_HeroObj!= null)
        {
            m_HeroObj.transform.localPosition = pos;
            m_HeroObj.transform.localRotation = Quaternion.Euler(ro);
        }
    }

    private Dictionary<long, GameObject> npcDic;

    private void LoadNpc()
    {
        if (npcNode == null)
        {
            npcNode = new GameObject();

            npcNode.name = "NpcNode";

            npcNode.transform.SetParent(GameObject.Find("mainsceneroot/main01(Clone)").transform);

            npcNode.transform.localPosition = Vector3.zero;
        }
        else
        {
            npcNode.transform.ClearChild();
        }
        NpcMapConfig config = GameConfig.Get<NpcMapConfig>(1);

        npcDic =new Dictionary<long, GameObject>();

        for (int i = 0; i < config.NpcList.Length; i++)
        {
            NpcConfig npcConfig = GameConfig.Get<NpcConfig>(config.NpcList[i]);

            if (ConditionCheck.Check(npcConfig.showCondition, npcConfig.showCheck == 1))
            {
                ResourcesManager.Instance.LoadPrefabAsync("role", npcConfig.prefabPath, (obj) =>
                {
                    if (obj != null)
                    {
                        obj.name = npcConfig.id.ToString();

                        obj.transform.SetParent(npcNode.transform);

                        obj.AddComponent<Npc>().InitNpc(npcConfig);

                        npcDic[npcConfig.id] = obj;
                    }
                });
            }
        }
    }

    private async void RefreshNpc(GEventArgs gEventArgs)
    {
        NpcMapConfig config = GameConfig.Get<NpcMapConfig>(1);

        for (int i = 0; i < config.NpcList.Length; i++)
        {
            NpcConfig npcConfig = GameConfig.Get<NpcConfig>(config.NpcList[i]);

            if (ConditionCheck.Check(npcConfig.showCondition, npcConfig.showCheck == 1))
            {
                if (npcDic.ContainsKey(npcConfig.id)) continue;

                npcDic[npcConfig.id] = null;//先占位

                GameObject obj = await ResourcesManager.Instance.LoadPrefabAsync2("role", npcConfig.prefabPath);

                if (obj != null)
                {
                    obj.name = npcConfig.id.ToString();

                    obj.transform.SetParent(npcNode.transform);

                    obj.AddComponent<Npc>().InitNpc(npcConfig);

                    npcDic[npcConfig.id] = obj;
                }
            }
            else
            {
                if (npcDic.TryGetValue(npcConfig.id, out GameObject npcObj))
                {
                    UnityEngine.Object.DestroyImmediate(npcObj);

                    npcDic.Remove(npcConfig.id);
                }
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public void ResetLens()
    {
        if (m_HeroCtrl != null)
        {
            m_HeroCtrl.ResetLens();
        }
    }
}
