using System;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class MailManager : Singleton<MailManager>
{
    /// <summary>
    /// 邮件列表
    /// </summary>
    private List<MailData> mailDatas = new List<MailData>();

    private JsonData localMailData = null;

    /// <summary>
    /// 初始化邮件列表
    /// </summary>
    public void InitMailList(JsonData jsonData)
    {
        if (jsonData == null)
        {
            return;

        }
        if (jsonData.ContainsKey("mails"))
        {
            OnGetNewMailList(jsonData["mails"]);

            mailDatas = new List<MailData>();
            if (jsonData == null)
            {
                return;
            }
            if (localMailData != null)
            {
                bool hasDelExpire = false;
                List<string> keys = new List<string>(localMailData.Keys);
                foreach (var k in keys)
                {
                    JsonData mailjd = localMailData[k];

                    // 删除过期的
                    if (mailjd.ContainsKey("expiretime"))
                    {
                        long expireTime = long.Parse(mailjd["expiretime"].ToString());
                        if (expireTime > 0 && datetool.GetNowLocalTimeStampMilliseconds() >= expireTime)
                        {
                            // 移除
                            localMailData.Remove(k);
                            hasDelExpire = true;
                        }
                        else
                        {
                            AddMail(localMailData[k]);
                        }
                    }
                    else
                    {
                        AddMail(localMailData[k]);
                    }
                }

                if (hasDelExpire)
                {
                    UpdateLocalMailData();
                }
            }
        }


        //JsonData arrayJson = null;
        //if (jsonData.IsArray)
        //{
        //    arrayJson = jsonData;
        //}
        //else if (jsonData.ContainsKey("mails"))
        //{
        //    arrayJson = jsonData["mails"];
        //}
        //if (arrayJson != null)
        //{
        //    foreach (JsonData md in arrayJson)
        //    {
        //        AddMail(md);
        //    }
        //}
        Debug.Log("`Mail manager 'InitMailList' mail count: " + mailDatas.Count);
        // RemoveFromLocalData(1);

        GameEventMgr.Register(GEKey.OnServerMailPush, OnMailServerMailPush);
    }

    /// <summary>
    /// 接受都服务端主动推送的邮件事件
    /// </summary>
    /// <param name="gEventArgs"></param>
    private void OnMailServerMailPush(GEventArgs gEventArgs)
    {
        if (gEventArgs == null)
        {
            return;
        }
        MailEventArgs mailEventArgs = (MailEventArgs)gEventArgs;
        // todo
        // 直接获取新的邮件列表
        MailManager.Instance.OnGetAllList(() =>
        {
            GameEventMgr.Distribute(GEKey.OnServerMailPush.ToString() + "_client");
        });
    }

    public async void GetServerMailList()
    {
        Instance.OnGetAllList(() =>
        {
            // ok
        });
    }

    /// <summary>
    /// 获取邮件列表
    /// </summary>
    /// <returns></returns>
    public List<MailData> GetMailDataList()
    {
        if (mailDatas.Count <= 0)
        {
            InitMailList(null);
        }
        return mailDatas;
    }

    /// <summary>
    /// 获取单个邮件
    /// </summary>
    /// <param name="mailId"></param>
    /// <returns></returns>
    public MailData GetMail(int mailId)
    {
        foreach (var data in mailDatas)
        {
            if (data.id == mailId)
            {
                return data;
            }
        }
        return null;
    }

    /// <summary>
    /// 是否存在邮件
    /// </summary>
    /// <param name="mailId"></param>
    /// <returns></returns>
    public bool HasMail(int mailId)
    {
        return GetMail(mailId) != null;
    }

    /// <summary>
    /// 邮件列表是否没有邮件
    /// </summary>
    /// <param name="mailId"></param>
    /// <returns></returns>
    public bool IsHasNotMail()
    {
        if (mailDatas == null)
            mailDatas = new List<MailData>();
        return mailDatas.Count <= 0;
    }

    /// <summary>
    /// 获取所有未读或已读或已领取或未领取邮件
    /// </summary>
    /// <param name="enumMailState"></param>
    /// <returns></returns>
    public List<MailData> GetMailsByMailState(EnumMailState enumMailState)
    {
        List<MailData> retList = new List<MailData>();
        foreach (var data in mailDatas)
        {
            if (data.state == (int)enumMailState)
            {
                retList.Add(data);
            }
        }
        return retList;
    }

    /// <summary>
    /// 获取所有指定特殊类型邮件
    /// </summary>
    /// <param name="enumMailState"></param>
    /// <returns></returns>
    public List<MailData> GetMailsByMailSpecial(EnumMailSpecial enumMailSpecial)
    {
        List<MailData> retList = new List<MailData>();
        foreach (var data in mailDatas)
        {
            if (data.special == (int)enumMailSpecial)
            {
                retList.Add(data);
            }
        }
        return retList;
    }

    /// <summary>
    /// 更新邮件状态
    /// </summary>
    /// <param name="id"></param>
    /// <param name="newState"></param>
    public void UpdateMailState(int id, EnumMailState newState)
    {
        MailData mailData = GetMail(id);
        if (mailData != null)
        {
            mailData.ChangedMailState(newState);
        }
        UpdateLocalDataState(id, newState);
    }

    /// <summary>
    /// 添加一封邮件数据
    /// </summary>
    /// <param name="jsonData"></param>
    public void AddMail(JsonData jsonData)
    {
        MailData mailData = JsonMapper.ToObject<MailData>(JsonMapper.ToJson(jsonData));
        if (mailData != null)
        {
            if (HasMail(mailData.id))
            {
                zxlogger.logerror("Error: repetition add mail! the mail data:" + mailData.ToString());
                return;
            }
            mailData.InitState();
            mailDatas.Add(mailData);
            UnityEngine.Debug.Log("~~ mailDatamailDatamailData:" + jsontool.tostring(mailData.ToJson()));
        }
    }

    /// <summary>
    /// 添加一组邮件数据
    /// </summary>
    /// <param name="jsonData"></param>
    public void AddMails(JsonData jsonData)
    {
        if (jsonData != null && jsonData.ContainsKey("mails"))
        {
            JsonData mailJsonList = jsonData["mails"];
            foreach (JsonData md in mailJsonList)
            {
                AddMail(md);
            }
        }
    }

    /// <summary>
    /// 移除一封邮件
    /// </summary>
    /// <param name="jsonData"></param>
    public void RemoveMail(JsonData jsonData, bool isUpdateLocalData = true)
    {
        MailData mailData = JsonMapper.ToObject<MailData>(JsonMapper.ToJson(jsonData));
        if (mailData != null)
        {
            RemoveMail(mailData.id, isUpdateLocalData);
        }
    }

    /// <summary>
    /// 移除一封邮件
    /// </summary>
    /// <param name="mailData"></param>
    public void RemoveMail(MailData mailData, bool isUpdateLocalData = true)
    {
        if (mailData != null)
        {
            RemoveMail(mailData.id, isUpdateLocalData);
        }
    }

    /// <summary>
    /// 移除一封邮件
    /// </summary>
    /// <param name="mailId"></param>
    public void RemoveMail(int mailId, bool isUpdateLocalData = true)
    {
        for (int i = mailDatas.Count - 1; i >= 0; i--)
        {
            if (mailDatas[i].id == mailId)
            {
                mailDatas.RemoveAt(i);
                break;
            }
        }
        if (isUpdateLocalData)
            RemoveFromLocalData(mailId);
    }

    /// <summary>
    /// 移除一组邮件
    /// </summary>
    /// <param name="jsonData"></param>
    public void RemoveMails(JsonData jsonData)
    {
        if (jsonData != null && jsonData.ContainsKey("mails"))
        {
            JsonData mailJsonList = jsonData["mails"];
            List<int> removeIds = new List<int>();
            foreach (JsonData md in mailJsonList)
            {
                RemoveMail(md, false);
                removeIds.Add(JsonMapper.ToObject<MailData>(JsonMapper.ToJson(jsonData)).id);
            }
            RemoveListFromLocalData(removeIds);
        }
    }

    /// <summary>
    /// 移除一组邮件
    /// </summary>
    /// <param name="mds"></param>
    public void RemoveMails(List<MailData> mds)
    {
        if (mds != null && mds.Count > 0)
        {
            List<int> removeIds = new List<int>();
            foreach (MailData md in mds)
            {
                RemoveMail(md, false);
                removeIds.Add(md.id);
            }
            RemoveListFromLocalData(removeIds);
        }
    }

    /// <summary>
    /// 移除一组邮件
    /// </summary>
    /// <param name="ids"></param>
    public void RemoveMails(List<int> ids)
    {
        if (ids != null && ids.Count > 0)
        {
            foreach (int id in ids)
            {
                RemoveMail(id, false);
            }
            RemoveListFromLocalData(ids);
        }
    }

    #region Mail Net

    /// <summary>
    /// 注册邮件网络事件监听
    /// </summary>
    public void RegisterMailNetListener()
    {

    }

    /// <summary>
    /// 注销邮件网络事件监听
    /// </summary>
    public void UnRegisterMailNetListener()
    {

    }

    /// <summary>
    /// 断线重连
    /// </summary>
    public void OnReconnection()
    {
        // TODO
        // UnRegisterMailNetListener();
    }

    /// <summary>
    /// 读取一封邮件
    /// </summary>
    public async void OnGetAllList(Action action)
    {
        // eqid=消息唯一编号 ,code=返回的状态码，0=成功，其他=失败,content=如果状态码为0，则为返回的数据，如果状态码为非0，则为错误信息
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        Debug.LogWarning("~~ Send OnGetAllList jsonData:" + jsontool.tostring(jsonData));
        SendMsg(MailNetInterfaceID.AllList, jsonData, (eqid, code, content) =>
        {
            Debug.LogWarning($"~~ msg return[AllList] - eqid:{eqid}, code:{code}, content:{jsontool.tostring(jsontool.newwithstring(content))}");
            if (code == 0)
            {
                // 读取成功
                JsonData mails = jsontool.newwithstring(content);
                if (mails != null)
                {
                    InitMailList(mails);
                    GameEventMgr.Distribute(GEKey.OnGetAllMails, new MailEventArgs(mails));
                    action?.Invoke();
                }
                else
                {
                    zxlogger.logwarning($"Warning: get all mail fail! mails not data!");
                }
            }
            else
            {
                zxlogger.logerror($"Error: get all mail fail! code:{code}");
            }
        });
    }

    /// <summary>
    /// 读取一封邮件
    /// </summary>
    /// <param name="mailId"></param>
    public void OnReadMail(int mailId)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["id"] = mailId;
        Debug.LogWarning("~~ Send OnReadMail jsonData:" + jsontool.tostring(jsonData));
        // eqid=消息唯一编号 ,code=返回的状态码，0=成功，其他=失败,content=如果状态码为0，则为返回的数据，如果状态码为非0，则为错误信息
        SendMsg(MailNetInterfaceID.ReadMail, jsonData, (eqid, code, content) =>
        {
            Debug.LogWarning($"~~ msg return[ReadMail] - eqid:{eqid}, code:{code}, content:{jsontool.tostring(jsontool.newwithstring(content))}");
            if (code == 0)
            {
                // 读取成功
                int readedMailId = int.Parse(jsontool.newwithstring(content)["id"].ToString());
                if (mailId == readedMailId)
                {
                    UpdateMailState(mailId, EnumMailState.Readed);
                    GameEventMgr.Distribute(GEKey.OnReadMails, new MailEventArgs(mailId));
                }
                else
                {
                    zxlogger.logerror($"Error: read mail id is not equal server return id! send id:{mailId}, server return id:{readedMailId}");
                }
            }
            else
            {
                zxlogger.logerror($"Error: on read mail fail! code:{code}");
            }
        });
    }

    /// <summary>
    /// 读取一封邮件
    /// </summary>
    /// <param name="mailData"></param>
    public void OnReadMail(MailData mailData)
    {
        if (mailData == null)
        {
            zxlogger.logerror("Error: Read mail fail! this mail data is null!");
            return;
        }
        OnReadMail(mailData.id);
    }

    /// <summary>
    /// 领取一封邮件
    /// </summary>
    /// <param name="mailId"></param>
    public void OnReceivedMail(int mailId)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["id"] = mailId;
        Debug.LogWarning("~~ Send OnReceivedMail jsonData:" + jsontool.tostring(jsonData));
        SendMsg(MailNetInterfaceID.ReceivedMail, jsonData, (eqid, code, content) =>
        {
            Debug.LogWarning($"~~ msg return[ReceivedMail] - eqid:{eqid}, code:{code}, content:{jsontool.tostring(jsontool.newwithstring(content))}");
            if (code == 0)
            {
                // 领取成功
                int receivedMailId = int.Parse(jsontool.newwithstring(content)["id"].ToString());
                if (mailId == receivedMailId)
                {
                    UpdateMailState(mailId, EnumMailState.AlreadyReceived);
                    GameEventMgr.Distribute(GEKey.OnReceivedMails, new MailEventArgs(mailId));
                }
                else
                {
                    zxlogger.logerror($"Error: received mail id is not equal server return id! send id:{mailId}, server return id:{receivedMailId}");
                }
                JsonData json = JsonMapper.ToObject(new JsonReader(content));
                JsonData chagedata = json["change"]?["changed"];
                if (chagedata != null)
                {
                    GameCenter.mIns.userInfo.onChange(chagedata);
                }
            }
            else
            {
                zxlogger.logerror($"Error: on received mail fail! code:{code}");
            }
        });
    }

    /// <summary>
    /// 领取一封邮件
    /// </summary>
    /// <param name="mailData"></param>
    public void OnReceivedMail(MailData mailData)
    {
        if (mailData == null)
        {
            zxlogger.logerror("Error: Received mail fail! this mail data is null!");
            return;
        }
        OnReceivedMail(mailData.id);
    }

    /// <summary>
    /// 删除一封邮件
    /// </summary>
    /// <param name="mailId"></param>
    public void OnDeleteMail(int mailId)
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        jsonData["id"] = mailId;
        Debug.LogWarning("~~ Send OnDeleteMail jsonData:" + jsontool.tostring(jsonData));
        SendMsg(MailNetInterfaceID.DeleteMail, jsonData, (eqid, code, content) =>
        {
            Debug.LogWarning($"~~ msg return[DeleteMail] - eqid:{eqid}, code:{code}, content:{jsontool.tostring(jsontool.newwithstring(content))}");
            if (code == 0)
            {
                // 领取成功
                int deleteMailId = int.Parse(jsontool.newwithstring(content)["id"].ToString());
                if (mailId == deleteMailId)
                {
                    RemoveMail(mailId);
                    GameEventMgr.Distribute(GEKey.OnDeleteMails, new MailEventArgs(mailId));
                }
                else
                {
                    zxlogger.logerror($"Error: delete mail id is not equal server return id! send id:{mailId}, server return id:{deleteMailId}");
                }
            }
            else
            {
                zxlogger.logerror($"Error: on delete mail fail! code:{code}");
            }
        });
    }

    /// <summary>
    /// 删除一封邮件
    /// </summary>
    /// <param name="mailData"></param>
    public void OnDeleteMail(MailData mailData)
    {
        if (mailData == null)
        {
            zxlogger.logerror("Error: Delete mail fail! this mail data is null!");
            return;
        }
        OnDeleteMail(mailData.id);
    }

    /// <summary>
    /// 一键领取邮件
    /// </summary>
    public void OnOneTimeReceivedMail()
    {
        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        Debug.LogWarning("~~ Send OnOneTimeReceivedMail jsonData:" + jsontool.tostring(jsonData));
        SendMsg(MailNetInterfaceID.OneTimeReceivedMail, jsonData, (eqid, code, content) =>
        {
            Debug.LogWarning($"~~ msg return[OneTimeReceivedMail] - eqid:{eqid}, code:{code}, content:{jsontool.tostring(jsontool.newwithstring(content))}");
            if (code == 0 && content != null)
            {
                JsonData jsonData = jsontool.newwithstring(content);
                if (jsonData.ContainsKey("ids"))
                {
                    JsonData idsJD = jsonData["ids"];
                    if (idsJD.Count <= 0)
                    {
                        GameCenter.mIns.m_UIMgr.PopMsg("没有可领取的邮件!");
                    }
                    else
                    {
                        List<int> receivedList = new List<int>();
                        foreach (JsonData jid in idsJD)
                        {
                            int id = int.Parse(jid.ToString());
                            UpdateMailState(id, EnumMailState.AlreadyReceived);
                            receivedList.Add(id);
                        }
                        GameEventMgr.Distribute(GEKey.OnReceivedMails, new MailEventArgs(receivedList));
                    }
                }
            }
            else
            {
                GameCenter.mIns.m_UIMgr.PopMsg("一键领取失败! [未知异常] 服务器返回错误码:" + code);
                zxlogger.logerror($"Error: one time received mail fail! server return code:{code}");
            }
        });
    }

    /// <summary>
    /// 一键删除邮件
    /// </summary>
    public void OnOneTimeDeleteMail()
    {
        // 查看是否有需要删除的邮件
        List<int> rkeys = new List<int>();
        if (Instance.localMailData != null)
        {
            List<string> keys = new List<string>(Instance.localMailData.Keys);
            foreach (var k in keys)
            {
                JsonData md = Instance.localMailData[k];
                if (md != null && md.ContainsKey("state"))
                {
                    EnumMailState state = (EnumMailState)int.Parse(md["state"].ToString());
                    if ((state == EnumMailState.Readed && (md["reward"] == null || md["reward"].ToString().Equals("{}")))
                        || state == EnumMailState.AlreadyReceived)
                    {
                        rkeys.Add(int.Parse(k));
                    }
                }
            }
        }
        if (rkeys.Count > 0)
        {
            foreach (var k in rkeys)
            {
                if (Instance.localMailData.ContainsKey(k.ToString()))
                {
                    Instance.localMailData.Remove(k.ToString());
                }
                RemoveMail(int.Parse(k.ToString()), false);
            }
            Instance.UpdateLocalMailData();
            GameEventMgr.Distribute(GEKey.OnDeleteMails, new MailEventArgs(rkeys));
        }
        else
        {
            GameCenter.mIns.m_UIMgr.PopMsg("没有需要删除的邮件!");
        }

        JsonData jsonData = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        Debug.LogWarning("~~ Send OnOneTimeDeleteMail jsonData:" + jsontool.tostring(jsonData));
        SendMsg(MailNetInterfaceID.OneTimeDeleteMail, jsonData, (eqid, code, content) =>
        {
            Debug.LogWarning($"~~ msg return[OneTimeDeleteMail] - eqid:{eqid}, code:{code}, content:{content}");
            if (code == 0 && content != null)
            {
                JsonData jsonData = jsontool.newwithstring(content);
                if (jsonData.ContainsKey("ids"))
                {
                    JsonData idsJD = jsonData["ids"];
                    List<int> deleteList = new List<int>();
                    foreach (JsonData jid in idsJD)
                    {
                        int id = int.Parse(jid.ToString());
                        deleteList.Add(id);
                    }
                    RemoveMails(deleteList);
                    GameEventMgr.Distribute(GEKey.OnDeleteMails, new MailEventArgs(deleteList));
                }
                //else
                //{
                //    GameCenter.mIns.m_UIMgr.PopMsg("没有可删除的邮件");
                //}
                // 删除本地已读
                RemoveFromLocalDataReaded();
            }
            else
            {
                zxlogger.logerror($"Error: one time delete mail fail! code:{code}");
            }
        });
    }


    private void SendMsg(MailNetInterfaceID mailNetInterfaceID, JsonData jsonData, Action<int, int, string> callback)
    {
        // test
        // callback?.Invoke((int)mailNetInterfaceID, 0, jsontool.tostring(jsonData));
        GameCenter.mIns.m_NetMgr.SendData((int)mailNetInterfaceID, jsonData, (eqid, code, content) =>
        {
            GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
            {
                // TODO: 错误码统一处理
                if (code == -250002)
                {
                    if (mailNetInterfaceID == MailNetInterfaceID.ReceivedMail)
                    {
                        int receivedMailId = 0;
                        int.TryParse(jsonData["id"].ToString(), out receivedMailId);
                        if (receivedMailId > 0)
                        {
                            MailData md = GetMail(receivedMailId);
                            if (md != null)
                            {
                                RemoveMail(receivedMailId);
                                List<int> deleteList = new List<int>() { receivedMailId };
                                GameEventMgr.Distribute(GEKey.OnDeleteMails, new MailEventArgs(deleteList));
                            }
                        }
                    }
                    GameCenter.mIns.m_UIMgr.PopMsg("领取失败，该邮件已经在其他账号领取!");
                    return;
                }
                callback?.Invoke(eqid, code, content);
            });
        });
    }
    #endregion

    #region mail local data

    private void OnGetNewMailList(JsonData newList)
    {
        localMailData = LocalDataManager.Instance.GetLocalData("mails");
        if (localMailData == null || (localMailData.ToString() == "{}"))
        {
            localMailData = jsontool.getemptytable();
        }
        bool isChanged = false;
        bool isAdd = false;
        // remove不需要检测，本地自行remove
        // bool isRemove = false;
        foreach (JsonData newjd in newList)
        {
            zxlogger.log("`` OnGetNewMailList newjd id:" + newjd["id"].ToString());
            if (localMailData.ContainsKey(newjd["id"].ToString()))
            {
                bool _isChanged = CheckChanged(localMailData[newjd["id"].ToString()], newjd);
                zxlogger.log("`` OnGetNewMailList changed _isChanged:" + _isChanged + " - newjd:" + jsontool.tostring(newjd));
                if (_isChanged)
                {
                    isChanged = _isChanged;
                }
            }
            else
            {
                zxlogger.log("`` OnGetNewMailList add newjd:" + jsontool.tostring(newjd));
                localMailData[newjd["id"].ToString()] = newjd;
                isAdd = true;
            }
        }

        zxlogger.log("`` OnGetNewMailList isChanged:" + isChanged + " - isAdd:" + isAdd);

        if (isChanged || isAdd)
        {
            // 需要更新邮件本地数据
            UpdateLocalMailData();
        }
    }

    public List<MailData> GetAllLocalMailDatas()
    {
        List<MailData> list = new List<MailData>();
        if (localMailData == null)
        {
            localMailData = LocalDataManager.Instance.GetLocalData("mails");
        }
        List<string> keys = new List<string>();
        foreach (var k in keys)
        {
            MailData md = new MailData(localMailData[k]);
            if (md != null)
            {
                list.Add(md);
            }
        }
        return list;
    }

    private bool CheckChanged(JsonData localData, JsonData newData)
    {
        bool b = false;
        if (!localData["content"].ToString().Equals(newData["content"].ToString())) { localData["content"] = newData["content"]; b = true; }
        if (!localData["expiretime"].ToString().Equals(newData["expiretime"].ToString())) { localData["expiretime"] = newData["expiretime"]; b = true; }
        if (!localData["reward"].ToString().Equals(newData["reward"].ToString())) { localData["reward"] = newData["reward"]; b = true; }
        if (!localData["sender"].ToString().Equals(newData["sender"].ToString())) { localData["sender"] = newData["sender"]; b = true; }
        if (!localData["sendtime"].ToString().Equals(newData["sendtime"].ToString())) { localData["sendtime"] = newData["sendtime"]; b = true; }
        if (!localData["special"].ToString().Equals(newData["special"].ToString())) { localData["special"] = newData["special"]; b = true; }
        if (!localData["state"].ToString().Equals(newData["state"].ToString())) { localData["state"] = newData["state"]; b = true; }
        if (!localData["title"].ToString().Equals(newData["title"].ToString())) { localData["title"] = newData["title"]; b = true; }
        return b;
    }

    private void UpdateLocalDataState(int id, EnumMailState newState)
    {
        if (localMailData != null)
        {
            if (localMailData.ContainsKey(id.ToString()))
            {
                localMailData[id.ToString()]["state"] = (int)newState;
                UpdateLocalMailData();
            }
        }
    }

    private void RemoveFromLocalDataReaded()
    {
        if (localMailData != null)
        {
            List<string> ks = new List<string>(localMailData.Keys);
            List<int> deleteList = new List<int>();
            for (int i = 0; i < ks.Count; i++)
            {
                bool hasReward = !string.IsNullOrEmpty(localMailData[ks[i]]["reward"].ToString());
                if ((hasReward && localMailData[ks[i]]["state"].ToString() == "2")
                    || (!hasReward && localMailData[ks[i]]["state"].ToString() == "1"))
                {
                    // 有奖励，已领取 or 没奖励 已读
                    localMailData.Remove(ks[i].ToString());
                    deleteList.Add(int.Parse(ks[i].ToString()));
                }
            }
            GameEventMgr.Distribute(GEKey.OnDeleteMails, new MailEventArgs(deleteList));

        }
    }

    private void RemoveFromLocalData(int id)
    {
        if (localMailData != null)
        {
            if (localMailData.ContainsKey(id.ToString()))
            {
                localMailData.Remove(id.ToString());
                UpdateLocalMailData();
            }
        }
    }

    private void RemoveListFromLocalData(List<int> rids)
    {
        if (localMailData != null)
        {
            foreach (var rid in rids)
            {
                if (localMailData.ContainsKey(rid.ToString()))
                {
                    localMailData.Remove(rid.ToString());
                }
            }
            UpdateLocalMailData();
        }
    }

    private void UpdateLocalMailData()
    {
        if (localMailData != null)
        {
            LocalDataManager.Instance.SetLocalData("mails", localMailData);
            localMailData = LocalDataManager.Instance.GetLocalData("mails");
        }
    }

    #endregion
}

public class MailDataSort : IComparer<MailData>
{
    // 已读的，已领取的在后面
    public int Compare(MailData mda, MailData mdb)
    {
        int tempStateA = mda.state;
        int tempStateB = mdb.state;
        if (tempStateA == 0) tempStateA = 10;
        if (tempStateA == 3) tempStateA = 11;
        if (tempStateB == 0) tempStateB = 10;
        if (tempStateB == 3) tempStateB = 11;

        return tempStateB.CompareTo(tempStateA);
    }
}

public class MailEventArgs : GEventArgs
{
    public override object[] args { get; set; }
    public List<int> disposeMails = new List<int>();
    public MailEventArgs(JsonData jsonData)
    {
        Debug.Log("`` server push mail json data:" + jsontool.tostring(jsonData));
    }

    public MailEventArgs(int mailId)
    {
        disposeMails = new List<int>();
        disposeMails.Add(mailId);
    }

    public MailEventArgs(List<int> mailIds)
    {
        disposeMails = new List<int>();
        if (mailIds != null)
        {
            disposeMails = mailIds;
        }
    }
}

public enum MailNetInterfaceID
{
    // 所有邮件列表
    AllList = 6000,
    // 读取一封邮件
    ReadMail = 6001,
    // 领取一封邮件
    ReceivedMail = 6002,
    // 一键领取
    OneTimeReceivedMail = 6003,
    // 删除一封邮件
    DeleteMail = 6004,
    // 一键删除
    OneTimeDeleteMail = 6005,
}
