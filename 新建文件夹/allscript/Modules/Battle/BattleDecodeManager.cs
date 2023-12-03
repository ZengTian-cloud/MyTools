using UnityEngine;
using Basics;
using System.IO;
using System.Collections;
using Spine;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.ComponentModel;
using LitJson;
using UnityEditor;
using System.Linq;

/// <summary>
/// 战斗交互场景-包含战斗前后的交互场景和纯解密关卡
/// </summary>
public class BattleDecodeManager : Singleton<BattleDecodeManager>
{
	public long curMission;//当前关卡
	public MissionCfgData missionCfg;//关卡配置表

	public BattleMissionDecodeCfg decodeCfg;//解密配置表
	public NpcMapConfig npcMapConfig;//npc地图配置表
	public BattleMissionParamCfg paramCfg;//战斗关卡参数表

	public Transform decodeCameraRoot;//解密相机节点
	public Camera decodeDefaultCamera;//解密固定相机
	public Camera decodeFreeCamera;//解密自由相机

	public GameObject decodeRoot;
	public GameObject curDecodeScene;
	private GameObject npcNode;

	private GameObject m_HeroObj;

	private battledecode_wnd battledecode_Wnd;
	public ETCJoystick joystick;

	private float moveSpeed = 4f;

	private string curAni;

	public int curTimeing = 0;

	bool bDecode;//是否是解密关卡
    /// <summary>
    /// 进入战斗解密交互场景
    /// </summary>d
    public void EnterBattleDecode(long mission)
	{
		BattleMsgManager.Instance.SendChallageMsg(mission, () =>
		{
            this.curMission = mission;
            this.missionCfg = MissionCfgManager.Instance.GetMissionCfgByMissionID(mission);
            if (this.missionCfg.type != 4)
            {
                Debug.LogError($"剧情关卡：{mission}的关卡类型错误，请检查！");
            }

            this.decodeCfg = BattleCfgManager.Instance.GetDecodeCfgByMission(mission);
            this.npcMapConfig = GameConfig.Get<NpcMapConfig>(decodeCfg.mapid);
            this.paramCfg = BattleCfgManager.Instance.GetMissionParamCfg(mission);
            GameCenter.mIns.m_UIMgr.Open<commonloading_wnd>().ShowLoading(() =>
            {
                GameSceneManager.Instance.LoadScene(npcMapConfig.mapasset, (obj) =>
                {
                    if (obj == null)
                    {
                        Debug.LogError($"未在npc地图表中找到{npcMapConfig.mapasset}，请检查！");
                    }
                    bDecode = true;
                    battledecode_Wnd = GameCenter.mIns.m_UIMgr.Open<battledecode_wnd>(this, bDecode);
                    joystick = battledecode_Wnd.EasyTouchCanvas.transform.Find("Joystick").GetComponent<ETCJoystick>();

                    this.InitRoot(obj);
                    this.SwitchCarmera();
                    LoadNpc(this.npcMapConfig);
                    LoadLeader();
                });

            });
        });
	}

	/// <summary>
	/// 进入战斗前后的交互场景
	/// </summary>
	/// <param name="mapID"></param>
	public void EnterBattleInteraction(long mapID,long mission,int timing,Action cb = null)
	{
        this.curMission = mission;
        this.missionCfg = MissionCfgManager.Instance.GetMissionCfgByMissionID(mission);
        this.paramCfg = BattleCfgManager.Instance.GetMissionParamCfg(mission);

        this.curTimeing = timing;
        this.npcMapConfig = GameConfig.Get<NpcMapConfig>(mapID);

       
        GameCenter.mIns.m_UIMgr.Open<commonloading_wnd>().ShowLoading(() =>
		{
            cb?.Invoke();
			bDecode = false;
            battledecode_Wnd = GameCenter.mIns.m_UIMgr.Open<battledecode_wnd>(this, bDecode);
            joystick = battledecode_Wnd.EasyTouchCanvas.transform.Find("Joystick").GetComponent<ETCJoystick>();
            GameSceneManager.Instance.LoadScene(npcMapConfig.mapasset, (obj) =>
            {
                if (obj == null)
                {
                    Debug.LogError($"未在npc地图表中找到{npcMapConfig.mapasset}，请检查！");
                }
                this.InitRoot(obj);
                this.SwitchCarmera();
                LoadNpc(this.npcMapConfig);
                LoadLeader();
            });

        });
    }


    /// <summary>
    /// 初始化节点
    /// </summary>
    /// <param name="root"></param>
    private void InitRoot(GameObject root)
	{
		this.decodeRoot = new GameObject("BattleDecodeRoot");
		this.curDecodeScene = root;
		this.curDecodeScene.transform.SetParent(this.decodeRoot.transform);

		//解密相机
		this.decodeCameraRoot = Camera.main.transform.Find("battleDecodeCamera");
		this.decodeDefaultCamera = Camera.main.transform.Find("battleDecodeCamera/decodeDefaultCamera").GetComponent<Camera>();
		this.decodeFreeCamera = Camera.main.transform.Find("battleDecodeCamera/decodeFreeCamera").GetComponent<Camera>();
	}

	/// <summary>
	/// 切换相机
	/// </summary>
	private void SwitchCarmera()
	{
		if (!this.decodeCameraRoot.gameObject.activeSelf)
		{
			this.decodeCameraRoot.gameObject.SetActive(true);
		}
		//暂时开启默认相机
		this.decodeDefaultCamera.gameObject.SetActive(true);
	}

	/// <summary>
	/// 退出解密关卡
	/// </summary>
	public void QuitBattleDecode()
	{
		GameSceneManager.Instance.UnLoadScene(this.decodeRoot);
		this.decodeCameraRoot.gameObject.SetActive(false);
	}

	public void LoadLeader()
	{
        ResourcesManager.Instance.LoadPrefabAsync("role", "role_101006", async (roleObj) => {
            if (roleObj != null)
            {
                m_HeroObj = roleObj;

                roleObj.transform.SetParent(decodeRoot.transform);
                roleObj.transform.localScale = Vector3.one;
                roleObj.transform.localRotation = Quaternion.identity;
                roleObj.transform.position = new Vector3(11, 0.24f, 7);
                roleObj.gameObject.tag = "Player";
                m_HeroObj.GetComponent<Animator>().runtimeAnimatorController = await ResourcesManager.Instance.LoadAssetSync("Assets/allmodel/main/prefabs/101006anim.controller") as RuntimeAnimatorController;
				m_HeroObj.AddComponent<CharacterController>().center = new Vector3(0, 1, 0);
                Transform[] transforms = roleObj.transform.GetComponentsInChildren<Transform>();
                foreach (var item in transforms)
                {
                    item.gameObject.layer = 9;
                }
				InitJoyStick();
				PlayAnima("loopidle");
            }
        });
    }

	private void InitJoyStick()
	{
        joystick.axisX.directTransform = m_HeroObj.transform;
        joystick.axisX.speed = moveSpeed;
        joystick.axisX.directAction = ETCAxis.DirectAction.Translate;
        joystick.axisX.axisInfluenced = ETCAxis.AxisInfluenced.X;
        joystick.axisX.invertedAxis = false;

        joystick.axisY.directTransform = m_HeroObj.transform;
        joystick.axisY.speed = moveSpeed;
        joystick.axisY.directAction = ETCAxis.DirectAction.Translate;
        joystick.axisY.axisInfluenced = ETCAxis.AxisInfluenced.Z;
        joystick.axisY.invertedAxis = false;


        joystick.onMove.AddListener((Vector2) =>
        {
            m_HeroObj.transform.rotation = Quaternion.LookRotation(new Vector3(Vector2.x, 0, Vector2.y));

            PlayAnima("run");
        });

        joystick.onMoveEnd.AddListener(() =>
        {
            PlayAnima("loopidle");
        });
    }

	/// <summary>
	/// 播发动画
	/// </summary>
	/// <param name="aniName"></param>
	private void PlayAnima(string aniName)
	{
		if (curAni == null || curAni != aniName) 
		{
			curAni = aniName;

            m_HeroObj.GetComponent<Animator>().SetTrigger(aniName);
        }
	}

	/// <summary>
	/// 加载npc
	/// </summary>
	/// <param name="config"></param>
	private void LoadNpc(NpcMapConfig config)
	{
		if (npcNode == null)
		{
			npcNode = new GameObject();

			npcNode.name = "NpcNode";

			npcNode.transform.SetParent(decodeRoot.transform);

			npcNode.transform.localPosition = Vector3.zero;
		}
		else
		{
			npcNode.transform.ClearChild();
		}

		for (int i = 0; i < config.NpcList.Length; i++)
		{
			NpcConfig npcConfig = GameConfig.Get<NpcConfig>(config.NpcList[i]);

			ResourcesManager.Instance.LoadPrefabAsync("role", npcConfig.prefabPath, (obj) =>
			{
				if (obj != null)
				{
					obj.transform.SetParent(npcNode.transform);

					obj.AddComponent<Npc>().InitNpc(npcConfig);
				}
			});
		}
	}

    /// <summary>
    /// 检测交互id是否满足
    /// </summary>
    /// <param name="interactID">交互id</param>
    public bool CheckInteract(long interactID)
	{
		if (this.curTimeing == 1)//战前
		{
			if (this.paramCfg.endbefore != "-1")
			{
				string[] befor = this.paramCfg.endbefore.Split('|');
				for (int i = 0; i < befor.Length; i++)
				{
					if (interactID == long.Parse(befor[i]))
					{
						return true;
					}
				}

			}
		}
		else if(this.curTimeing == 2)//战后
		{
            if (this.paramCfg.endafter != "-1")
            {
                string[] after = this.paramCfg.endafter.Split('|');
                for (int i = 0; i < after.Length; i++)
                {
                    if (interactID == long.Parse(after[i]))
                    {
                        return true;
                    }
                }

            }
        }
		return false;
	}

}

