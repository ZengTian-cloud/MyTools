using System;
using LitJson;
public class BattleMsgManager:SingletonNotMono<BattleMsgManager>
{
    public JsonData randomSed;
    /// <summary>
    /// 发起战斗
    /// </summary>
    public void SendChallageMsg(long missionid,Action cb = null)
    {
        //获取关卡信息-需要拿到当前处于的模块才能开始加载
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["missionid"] = missionid;
        GameCenter.mIns.m_NetMgr.SendData(NetCfg.CHAPTER_MISSION_CHALLAGE, jsonData, (eqid, code, content) =>
        {
            if (code == 0)
            {
                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                {

                    JsonData msg = jsontool.newwithstring(content);
                    if (msg.ContainsKey("random"))
                    {
                        this.randomSed = msg["random"];
                        cb?.Invoke();
                    }
                });
            }
        });
    }

    /// <summary>
    /// 发起结算
    /// </summary>
    public void SendResultMsg(long missionID, int star, JsonData randnum, string story = "", Action cb = null)
    {

        //获取关卡信息-需要拿到当前处于的模块才能开始加载
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["missionid"] = missionID;
        jsonData["star"] = star;
        jsonData["randnum"] = randnum;
        jsonData["story"] = story;
        GameCenter.mIns.m_NetMgr.SendData(NetCfg.CHAPTER_MISSION_RESULT, jsonData, (eqid, code, content) =>
        {
            if (code == 0)
            {
                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                {
                    if (star > 0)
                    {
                        GameCenter.mIns.userInfo.AddCompletedGameLevelDic(missionID, star);
                    }
                    GameEventMgr.Distribute(GEKey.Battle_OnBattleEnd, star > 0 ? 1 : 0, missionID);
                    cb?.Invoke();
                });
            }
        });
    }
}

