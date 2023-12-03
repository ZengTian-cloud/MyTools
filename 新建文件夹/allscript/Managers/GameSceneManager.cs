using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : Singleton<GameSceneManager>
{
    public void LoadMainScene()
    {
        ResourcesManager.Instance.LoadPrefabAsync("scence", "main01", (obj) =>
        {
            GameObject root = new GameObject("mainsceneroot");

            obj.transform.SetParent(root.transform);

           
            InitMainScene();
        });
    }

    private  void InitMainScene()
    {
        MainSceneManager.Instance.Init();
    }

    /// <summary>
    /// 加载场景
    /// </summary>
    /// <param name="sceneName">地图/场景名字</param>
    /// <param name="loadedCallback">完成回调</param>
    public void LoadScene(string sceneName, Action<GameObject> callBack = null, string floderName = "scence")
    {
        ResourcesManager.Instance.LoadPrefabAsync(floderName, sceneName, async (obj) =>
        {
            // 加载场景fog全局配置
            //GameCenter.mIns.m_FogManager.SetSceneSetting(sceneName);
            callBack?.Invoke(obj);
        });
    }

    /// <summary>
    /// 移除场景（销毁）
    /// </summary>
    /// <param name="gameObject"></param>
    public void UnLoadScene(GameObject scene,Action callBack = null)
    {
        if (scene != null)
        {
            GameObject.Destroy(scene);
            //去掉最后一张光照贴图
            LightmapSettings.lightmaps = LightmapSettings.lightmaps.Take(LightmapSettings.lightmaps.Count() - 1).ToArray();
            callBack?.Invoke();

            GameEventMgr.Distribute(GEKey.OnUnLoadScene);
        }
    }

    /// <summary>
    /// 进入战斗场景 -检测战斗前后交互场景
    /// </summary>
    /// <param name="missionId"></param>
    public void EnterBattleScene(long missionId)
    {
        BattleMissionParamCfg paramCfg = BattleCfgManager.Instance.GetMissionParamCfg(missionId);
        MainSceneManager.Instance.OnLeave();
        if (paramCfg.beforemapid != -1)
        {
            BattleDecodeManager.Instance.EnterBattleInteraction(paramCfg.beforemapid, missionId, 1);
        }
        else
        {
            GameCenter.mIns.m_BattleMgr.RequstBattle(missionId);
        }
    }

    public void LeaveBattleScene()
    {
        MainSceneManager.Instance.OnEnter();
    }

}
