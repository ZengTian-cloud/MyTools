using System;
using UnityEngine;
using System.Collections.Generic;

public class JumpManger:SingletonNotMono<JumpManger>
{
	private string jumpPath = "t_jump_to";

	private List<JumpCfgData> jumpCfgDatas;
    private Dictionary<long, JumpCfgData> dicJumpCfg;

	public void InitJumpCfg()
	{
        jumpCfgDatas = GameCenter.mIns.m_CfgMgr.JsonToListClass<JumpCfgData>(jumpPath);
        dicJumpCfg = new Dictionary<long, JumpCfgData>();
        for (int i = 0; i < jumpCfgDatas.Count; i++)
        {
            dicJumpCfg.Add(jumpCfgDatas[i].id, jumpCfgDatas[i]);
        }
    }

    /// <summary>
    /// 根据任务ID进行跳转
    /// </summary>
    /// <param name="taskID"></param>
    public void DoJumpByTask(long taskID)
    {
        TaskBaseCfg baseCfg = TaskCfgManager.Instance.GetTaskBaseCfgByTaskID(taskID);
        if (baseCfg != null && baseCfg.jumpid != -1)
        {
            DoJumpByJumpID(baseCfg.jumpid);
        }
    }

    public void DoJumpByJumpID(long jumpID)
    {
        JumpCfgData jumpCfg = GetJumpCfgByID(jumpID);
        if (jumpCfg != null)
        {
            switch (jumpCfg.jumptype)
            {
                case 1://ui
                    DoJumpUI(jumpCfg);
                    break;
                case 2://地图
                    DoJumpMap(jumpCfg);
                    break;
                default:
                    break;
            }
        }
    }

    //跳转ui界面
    private void DoJumpUI(JumpCfgData jumpCfg)
    {
        string winname = jumpCfg.winname;
        switch (winname)
        {
            case "herogrow":
                GameCenter.mIns.m_UIMgr.curNormalOpen.DoJump<HeroGrow>();
                break;
            default:
                Debug.LogError($"界面：{winname}未注册跳转，请检查！");
                return;
        }
        Debug.Log($"======>跳转ui界面成功{winname}");
    }

    //跳转地图
    private void DoJumpMap(JumpCfgData jumpCfg)
    {
        mainui mainui = GameCenter.mIns.m_UIMgr.Get<mainui>();
        if (mainui == null || !GameCenter.mIns.m_UIMgr.IsOpenedUI(mainui))
        {
            Debug.LogError("尝试在非主场景下操作主角,请检查");
            return;
        }
        else
        {
            GameCenter.mIns.m_UIMgr.Open<commonloading_wnd>().ShowLoading(() =>
            {
                string[] pos = jumpCfg.position.Split(';');
                string[] ro = jumpCfg.rotation.Split(';');
                Vector3 vPos = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
                Vector3 vRo = new Vector3(float.Parse(ro[0]), float.Parse(ro[1]), float.Parse(ro[2]));
                MainSceneManager.Instance.SetRolePosAndRot(vPos, vRo);
                Debug.Log($"======>跳转地图成功，newPos:{vPos},newRot:{vRo}");
            });
        }
    }


    public JumpCfgData GetJumpCfgByID(long id)
    {
        if (dicJumpCfg.ContainsKey(id))
        {
            return dicJumpCfg[id];
        }
        Debug.LogError($"未在跳转表【t_jump_to】中找到跳转id为{id}的配置。请检查！");
        return null;
    }

    
}

