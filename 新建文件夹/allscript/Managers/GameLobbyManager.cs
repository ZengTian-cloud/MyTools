using Cysharp.Threading.Tasks;
using LitJson;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;

/// <summary>
/// 游戏大厅管理
/// </summary>
public class GameLobbyManager : SingletonNotMono<GameLobbyManager>
{
    private Dictionary<long, GameLobbyPlayerInfo> gameLobbyPlayersDic;

    public bool IsJoinedGameLobby;

    /// <summary>
    /// 加入大厅
    /// </summary>
    public async UniTask Join()
    {
        if (IsJoinedGameLobby) return;

        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();

        await GameCenter.mIns.m_NetMgr.SendDataSync(NetCfg.GameLobby_Join, jsonData, (eqid, code, content) =>
        {
            if (code == 0)
            {
                 Debug.Log($"<color=#ff0000> 加入大厅成功 开始获取信息 </color></color>");
                
                IsJoinedGameLobby = true;

                GetInfo();

               
            }
            else
            {
                //TODO:处理加入大厅失败的情况
            }

        });

    }
    /// <summary>
    /// 退出大厅
    /// </summary>
    private void Quit()
    {
        if (!IsJoinedGameLobby) return;

        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();

        GameCenter.mIns.m_NetMgr.SendData(NetCfg.GameLobby_Quit, jsonData, (eqid, code, content) =>
        {
            if (code == 0)
            {
                IsJoinedGameLobby = false;

                gameLobbyPlayersDic.Clear();
            }

        }, null, true, true);
    }
    /// <summary>
    /// 获取大厅信息 （需要先加入大厅）
    /// </summary>
    private void GetInfo()
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();

        GameCenter.mIns.m_NetMgr.SendData(NetCfg.GameLobby_GetInfo, jsonData, (eqid, code, content) =>
        {
            if(code ==0)
            {
                JsonData jd = jsontool.newwithstring(content);

                JsonData jdArray = jd["users"];

                gameLobbyPlayersDic ??= new Dictionary<long, GameLobbyPlayerInfo>();

                for (int i = 0; i < jdArray.Count; i++)
                {
                    JsonData data = jdArray[i];

                    string[] coordinate = data["coordinate"].ToString().Split("|");

                    long mapId = long.Parse(coordinate[0]);

                    string[] posStr = coordinate[1].Split(";");

                    string[] rotStr = coordinate[2].Split(";");

                    GameLobbyPlayerInfo info = new GameLobbyPlayerInfo
                    {
                        roleId = data["roleid"].ToInt64(),

                        nickName = data["nickname"].ToString(),

                        lv = data["level"].ToInt32(),

                        vipLv = data["viplevel"].ToInt32(),

                        mapId = mapId,

                        position =new Vector3(float.Parse(posStr[1]), float.Parse(posStr[2]), float.Parse(posStr[3])),

                        rotation = new Vector3(float.Parse(rotStr[1]), float.Parse(rotStr[2]), float.Parse(rotStr[3])),
                    };
                    gameLobbyPlayersDic[info.roleId] = info;
                }
            }
            else
            {

            }

        }, null, true, true);
    }
    /// <summary>
    /// 上传大厅信息 （将自己的信息同步到服务器）
    /// </summary>
    private void UplodaInfo(long mapId , Vector3 position , Vector3 rotation)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();

        jsonData["coordinate"] = CombineInfo( mapId, position,rotation);

        GameCenter.mIns.m_NetMgr.SendData(NetCfg.GameLobby_UploadInfo, jsonData, (eqid, code, content) =>
        {
            if(code == 0)
            {
                Debug.Log("上传信息成功");
            }
            else
            {
                //处理上传失败的情况
            }
        }, null, true, true);
    }

    private string CombineInfo(long mapId, Vector3 position, Vector3 rotation)
    {
        string strPos = $"{position.x};{position.y};{position.z}";

        string strRot = $"{rotation.x};{rotation.y};{rotation.z}";

        return $"{mapId}|{strPos}|{strRot}";
    }
}

public class GameLobbyPlayerInfo
{
    public long roleId;

    public string nickName;

    public int lv;

    public int vipLv;

    public long mapId;

    public Vector3 position;

    public Vector3 rotation;
}