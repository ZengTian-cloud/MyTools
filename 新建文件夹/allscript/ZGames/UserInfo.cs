using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System;
using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;

public class UserInfo
{
    #region 基础数据
    public string Uid { get; set; }
    public string Token { get; set; }
    public bool IsNew { get; set; }
    public int ZoneId { get; set; }
    public string RoleId { get; set; }
    public string Group { get; set; }
    #endregion

    #region 其他模块数据
    public int CE { get; set; }
    public int Coin { get; set; }
    public float Exp { get; set; }
    public int Gem { get; set; }
    public float HP { get; set; }
    public float HPTime { get; set; }
    public float MaxHP { get; set; }
    public float PayMoney { get; set; }
    public float VipLevel { get; set; }


    public float Level { get; set; }
    public string NickName { get; set; }
    // 登陆时间
    public string CTime { get; set; }
    public string Lastlogintime { get; set; }
    // 指引？
    public int Guide { get; set; }

    //最近一次通关的关卡
    public long nowmissionid { get; set; }


    // 仓库数据 -- 存放在WarehouseData单例中
    /*
     "props":{ "map":{ 1:{"id":1,"num":10,"pid":6001}, 2:{"id":2,"num":10,"pid":6002} } },
     */

    // 英雄数据
    /*
     "heros":{ "map":{} },
     */
    // 邮件数据
    /*
     "mails":{ "map":{} },
     */

    Dictionary<long, int> completedGameLevelDic;

    #endregion


    public ConcurrentDictionary<string, Action> onChangeCallbacks = new ConcurrentDictionary<string, Action>();

    public void onChange(JsonData data)
    {
        if (data.ContainsKey("user"))
        {
            JsonData user = data["user"];
            CE = int.Parse(user.ContainsKey("coin") ? user["coin"].ToString() : CE + "");
            Coin = int.Parse(user.ContainsKey("coin") ? user["coin"].ToString() : Coin + "");
            Exp = int.Parse(user.ContainsKey("exp") ? user["exp"].ToString() : Exp + "");
            Gem = int.Parse(user.ContainsKey("gem") ? user["gem"].ToString() : Gem + "");
            HP = int.Parse(user.ContainsKey("hp") ? user["hp"].ToString() : HP + "");
            MaxHP = int.Parse(user.ContainsKey("maxhp") ? user["maxhp"].ToString() : MaxHP + "");
            PayMoney = int.Parse(user.ContainsKey("paymoney") ? user["paymoney"].ToString() : PayMoney + "");
            VipLevel = 0;//int.Parse(playdata["viplevel"].ToString());
        }

        if (data.ContainsKey("prop"))
        {
            JsonData props = data["prop"];
            foreach (JsonData item in props)
            {
                WarehouseManager.Instance.OnAddItem(item, true);
            }
        }

        if (data.ContainsKey("hero"))
        {
            JsonData heros = data["hero"];
            foreach (JsonData item in heros)
            {
                HeroDataManager.Instance.AddOrUpdateHero(item);
            }
        }
        foreach (var call in onChangeCallbacks)
        {
            call.Value?.Invoke();
        }
    }

    public void addChangeCallback(string name, Action callback)
    {
        onChangeCallbacks.TryAdd(name, callback);
    }
    public void removeChangeCallback(string name)
    {
        Action callback;
        onChangeCallbacks.TryRemove(name, out callback);
    }

    // uid等基础信息登陆时已经设置
    public void InitUserInfo(string content)
    {
        //try
        //{
        Debug.Log("content:" + content);
        JsonData jsonData = JsonMapper.ToObject(new JsonReader(content));
        if (!jsonData.ContainsKey("role"))
        {
            Debug.LogError("不存在玩家数据!");
            return;
        }
        JsonData roleData = jsonData["role"];
        if (roleData != null)
        {
            if (roleData.ContainsKey("playdata"))
            {
                JsonData playdata = roleData["playdata"];
                CE = int.Parse(playdata["coin"].ToString());
                Coin = int.Parse(playdata["coin"].ToString());
                Exp = int.Parse(playdata["exp"].ToString());
                Gem = int.Parse(playdata["gem"].ToString());
                HP = int.Parse(playdata["hp"].ToString());
                //HPTime = int.Parse(playdata["hptime"].ToString());
                MaxHP = int.Parse(playdata["maxhp"].ToString());
                PayMoney = int.Parse(playdata["paymoney"].ToString());
                VipLevel = 0;//int.Parse(playdata["viplevel"].ToString());
                nowmissionid = playdata.ContainsKey("nowmissionid") ? playdata["nowmissionid"].ToInt64() : 0;
                // 仓库数据
                if (playdata.ContainsKey("props"))
                {
                    foreach (JsonData item in playdata["props"]["map"])
                    {
                        WarehouseManager.Instance.OnAddItem(item, true);
                    }
                    // test
                    for (int i = 411201; i <= 411203; i++)
                        WarehouseManager.Instance.OnAddItem(i, i, 10);
                    //for (int i = 411201; i <= 411236; i++)
                    //    WarehouseData.Instance.OnAddItem(i, i, 10);
                    //for (int i = 411301; i <= 411336; i++)
                    //    WarehouseData.Instance.OnAddItem(i, i, 10);
                    //for (int i = 411401; i <= 411436; i++)
                    //    WarehouseData.Instance.OnAddItem(i, i, 10);
                }
                // 英雄数据
                if (playdata.ContainsKey("heros"))
                {
                    foreach (JsonData item in playdata["heros"]["map"])
                    {
                        HeroDataManager.Instance.AddOrUpdateHero(item);
                    }
                    //测试数据，读取的配置表，后面需要改成解析后端数据

                }

                // 邮件数据 主动拉取
                //if (playdata.ContainsKey("mails") && playdata["mails"].ContainsKey("map"))
                //{
                //MailManager.Instance.InitMailList(playdata["mails"]["map"]);
                //}
            }

            Level = int.Parse(roleData["level"].ToString());
            NickName = roleData["nickname"].ToString();
            //CTime = roleData["ctime"].ToString();
            //Lastlogintime = roleData["lastlogintime"].ToString();
            Guide = int.Parse(roleData["guide"].ToString());

            // 本地数据
            LocalDataManager.Instance.CheckRoleLocalDataFile();
            
            // friend
            FriendManager.Instance.InitFriendList(null);
            // 章节配置表
            ChapterCfgManager.Instance.InitChapterData();
            MissionCfgManager.Instance.InitMissionCfg();
        }
    }

    public override string ToString()
    {
        return string.Format("Uid:{0}, IsNew:{1}, ZoneId:{2}, RoleId:{3}, Group:{4}, Token:{5}", Uid, IsNew, ZoneId, RoleId, Group, Token);
    }

    /// <summary>
    /// 获取道具数量
    /// </summary>
    /// <param name="propid"></param>
    /// <returns></returns>
    public int getPropCount(long propid)
    {
        ItemCfgData item = GameCenter.mIns.m_CfgMgr.GetItemCfgData(propid);
        if (item == null)
        {
            Debug.LogError($"找不到propid为{propid}的物品，请检查");
            return 0;
        }
        switch (item.pakge)
        {
            case 1:
                return getPakgeCount(propid);
            case 2:
                return WarehouseManager.Instance.GetItemNumber(propid);
            default:
                return 0;
        }
    }

    /// <summary>
    /// 根据道具编号获取货币的数量
    /// </summary>
    /// <param name="propid"></param>
    /// <returns></returns>
    public int getPakgeCount(long propid)
    {
        switch (propid)
        {
            case 801:
                return Coin;
            case 804:
                return Gem;
            default:
                return 0;
        }
    }

    public string getCoinNumStr()
    {
        if (Coin < 1000000)
        {
            return Coin.ToString();
        }
        else
        {
            float c = Coin / 1000;
            c = ((float)Math.Round(c, 2));
            return c.ToString() + "K";
        }
    }

    public async UniTask InitCompletedGameLevelDic()
    {
        completedGameLevelDic = new Dictionary<long, int>();

        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();

        await GameCenter.mIns.m_NetMgr.SendDataSync(NetCfg.GetAllCompletedGameLevelInfo, jsonData, (eqid, code, content) =>
        {
            if (code == 0 && !string.IsNullOrEmpty(content))
            {
                JsonData jd = jsontool.newwithstring(content);

                JsonData jdArray = jd["missions"];

                for (int i = 0; i < jdArray.Count; i++)
                {
                    JsonData data = jdArray[i];

                    completedGameLevelDic[data["id"].ToInt64()] = data["star"].ToInt32();
                }
            }
        });
    }

    public void AddCompletedGameLevelDic(long levelID,int star)
    {
        if (completedGameLevelDic.ContainsKey(levelID))
        {
            completedGameLevelDic[levelID] = star;
        }
        else
        {
            completedGameLevelDic.Add(levelID, star);
        }
    }
    public bool CheckGameLevelIsCompleted(long levelId)
    {
        return completedGameLevelDic.ContainsKey(levelId);             
    }

}

