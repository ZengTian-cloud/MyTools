using System;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public partial class FriendManager : SingletonNotMono<FriendManager>
{
    /// <summary>
    /// 联系人字典，包含：好友，陌生人，黑名单
    /// </summary>
    private Dictionary<EnumContactType, List<ContactData>> contactDict = new Dictionary<EnumContactType, List<ContactData>>();

    /// <summary>
    /// 开局初始化同步所有联系人数据
    /// </summary>
    /// <param name="jsonData"></param>
    public void InitFriendList(JsonData jsonData)
    {
        CheckLocalDataHasKey();
        ChatManager.Instance.Init();
        foreach (EnumContactType contactType in Enum.GetValues(typeof(EnumContactType)))
        {
            contactDict.Add(contactType, new List<ContactData>());
        }

        // test
        //if (jsonData == null)
        //{
        //    jsonData = new JsonData();
        //    jsonData["friends"] = new JsonData();
        //    jsonData["blacklist"] = new JsonData();
        //    jsonData["lastest"] = new JsonData();
        //    jsonData["apply"] = new JsonData();
        //    int j = 1;
        //    for (int i = 0; i < 10; i++)
        //    {
        //        int rid = j * 10 + i;
        //        ContactData contactData = new ContactData(EnumContactType.Friend, rid, "avatar", "friendname_" + rid, UnityEngine.Random.Range(1, 100));
        //        jsonData["friends"].Add(contactData.ToJson());
        //    }
        //    for (int i = 0; i < 10; i++)
        //    {
        //        int rid = (j + 1) * 10 + i;
        //        ContactData contactData1 = new ContactData(EnumContactType.Blacklist, rid, "avatar", "friendname_" + rid, UnityEngine.Random.Range(1, 100));
        //        jsonData["blacklist"].Add(contactData1.ToJson());
        //    }
        //    for (int i = 0; i < 10; i++)
        //    {
        //        int rid = (j + 2) * 10 + i;
        //        ContactData contactData2 = new ContactData(EnumContactType.Stranger, rid, "avatar", "friendname_" + rid, UnityEngine.Random.Range(1, 100));
        //        jsonData["lastest"].Add(contactData2.ToJson());
        //    }
        //    for (int i = 0; i < 10; i++)
        //    {
        //        int rid = (j + 3) * 10 + i;
        //        ContactData contactData3 = new ContactData(EnumContactType.Apply, rid, "avatar", "friendname_" + rid, UnityEngine.Random.Range(1, 100));
        //        jsonData["apply"].Add(contactData3.ToJson());
        //    }
        //}
        if (jsonData == null)
        {
            return;
        }


        if (jsonData.ContainsKey("friends"))
        {
            JsonData friendsJson = jsonData["friends"];
            foreach (JsonData fdjs in friendsJson)
            {
                ContactData contactData = new ContactData(EnumContactType.Friend, fdjs);
                if (contactData != null)
                {
                    contactDict[EnumContactType.Friend].Add(contactData);
                }
            }
            SynchroContactLocalData(EnumContactType.Friend, contactDict[EnumContactType.Friend]);
        }
        if (jsonData.ContainsKey("blacklist"))
        {
            JsonData friendsJson = jsonData["blacklist"];
            foreach (JsonData fdjs in friendsJson)
            {
                ContactData contactData = new ContactData(EnumContactType.Blacklist, fdjs);
                if (contactData != null)
                {
                    contactDict[EnumContactType.Blacklist].Add(contactData);
                }
            }

            SynchroContactLocalData(EnumContactType.Blacklist, contactDict[EnumContactType.Blacklist]);
        }
        if (jsonData.ContainsKey("lastest"))
        {
            JsonData friendsJson = jsonData["lastest"];
            foreach (JsonData fdjs in friendsJson)
            {
                ContactData contactData = new ContactData(EnumContactType.Stranger, fdjs);
                if (contactData != null)
                {
                    contactDict[EnumContactType.Stranger].Add(contactData);
                }
            }
            SynchroContactLocalData(EnumContactType.Stranger, contactDict[EnumContactType.Stranger]);
        }
        if (jsonData.ContainsKey("apply"))
        {
            JsonData friendsJson = jsonData["apply"];
            foreach (JsonData fdjs in friendsJson)
            {
                ContactData contactData = new ContactData(EnumContactType.Apply, fdjs);
                if (contactData != null)
                {
                    contactDict[EnumContactType.Apply].Add(contactData);
                }
            }
            SynchroContactLocalData(EnumContactType.Apply, contactDict[EnumContactType.Apply]);
        }
    }

    public void OnGetNewAllFriendList(string content)
    {
        List<ContactData> contactDatas = new List<ContactData>();
        JsonData jd = jsontool.newwithstring(content);
        if (jd.ContainsKey("list"))
        {
            foreach (JsonData _jd in jd["list"])
            {
                ContactData cd = new ContactData(EnumContactType.Friend, _jd);
                if (cd != null)
                {
                    contactDatas.Add(cd);
                }
            }
            AddContactDatas(EnumContactType.Friend, contactDatas);
        }
        GameEventMgr.Distribute(NetCfg.FRIEND_LIST.ToString(), new FriendEventArgs(contactDatas));
    }

    public void OnGetNewAllBlackList(string content)
    {
        List<ContactData> contactDatas = new List<ContactData>();
        JsonData jd = jsontool.newwithstring(content);
        if (jd.ContainsKey("list"))
        {
            foreach (JsonData _jd in jd["list"])
            {
                ContactData cd = new ContactData(EnumContactType.Blacklist, _jd);
                if (cd != null)
                {
                    contactDatas.Add(cd);
                }
            }
            AddContactDatas(EnumContactType.Blacklist, contactDatas);
        }
        Debug.Log("b Count" + contactDict[EnumContactType.Blacklist].Count);
        GameEventMgr.Distribute(NetCfg.FRIEND_BLACK_LIST.ToString(), new FriendEventArgs(contactDatas));
    }

    public void OnGetNewApplyFriendList(string content)
    {
        // {"list":[{"ce":250,"face":"face","lan":"cn","lasttime":1690356186000,"level":1,"line":0,"nickname":"GTA-伏凡之","os":0,"roleid":"5610406","uuid":"gta5610407","viplevel":0}]}
        List<ContactData> contactDatas = new List<ContactData>();
        JsonData jd = jsontool.newwithstring(content);
        if (jd.ContainsKey("list"))
        {
            foreach (JsonData _jd in jd["list"])
            {
                ContactData cd = new ContactData(EnumContactType.Apply, _jd);
                if (cd != null)
                {
                    contactDatas.Add(cd);
                }
            }
            AddContactDatas(EnumContactType.Apply, contactDatas);
        }
        GameEventMgr.Distribute(NetCfg.FRIEND_APPLY_ADD.ToString(), new FriendEventArgs(contactDatas));
    }

    #region contact real time data
    /// <summary>
    /// 获取联系人数据
    /// </summary>
    /// <param name="contactType"></param>
    /// <returns></returns>
    public List<ContactData> GetContactDatas(EnumContactType contactType, bool isSort = false)
    {
        if (contactDict.ContainsKey(contactType))
        {
            // 目前只处理好友列表排序，其他排序请添加类型
            if (isSort)
            {
                List<ContactData> cdlist = contactDict[contactType];
                foreach (var cd in cdlist)
                {
                    ChatData chatdata = ChatManager.Instance.GetLatestChat(cd.roleid);
                    if (chatdata != null)
                    {
                        cd.lastchattime = chatdata.sendtime;
                        cd.lastchattext = chatdata.chattext;
                    }
                }
                cdlist.Sort(new ContactDataSort_FriendList());
                return cdlist;
            }
            else
            {
                List<ContactData> cdlist = contactDict[contactType];
                if (contactType == EnumContactType.Apply)
                {
                    // 推荐列表, 移除掉已经是好友的
                    for (int i = cdlist.Count - 1; i >= 0; i--)
                    {
                        if (GetContactData(EnumContactType.Friend, cdlist[i].roleid) != null)
                        {
                            cdlist.RemoveAt(i);
                        }
                    }
                }
                return cdlist;
            }
        }
        return null;
    }

    private class ContactDataSort_FriendList : IComparer<ContactData>
    {
        public int Compare(ContactData cda, ContactData cdb)
        {
            if (cda.line != cdb.line)
            {
                return cdb.line.CompareTo(cda.line);
            }
            else
            {
                if (cda.lastchattime > 0)
                {
                    return cdb.lastchattime.CompareTo(cda.lastchattime);
                }
                if (cda.lasttime != cdb.lasttime)
                {
                    return cdb.lasttime.CompareTo(cda.lasttime);
                }
            }
            return 0;
        }
    }

    /// <summary>
    /// 获取联系人数据
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public ContactData GetContactData(EnumContactType contactType, string roleId)
    {
        if (contactDict.ContainsKey(contactType))
        {
            foreach (var cd in contactDict[contactType])
            {
                if (cd.roleid == roleId)
                {
                    return cd;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 获取联系人数据
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public ContactData GetContactData(string roleId)
    {
        foreach (EnumContactType contactType in Enum.GetValues(typeof(EnumContactType)))
        {
            if (contactDict.ContainsKey(contactType))
            {
                foreach (var cd in contactDict[contactType])
                {
                    if (cd.roleid == roleId)
                    {
                        return cd;
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 是否存在联系人
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public bool HasContactData(EnumContactType contactType, string roleId)
    {
        return GetContactData(contactType, roleId) != null;
    }

    /// <summary>
    /// 添加联系人
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="jd"></param>
    /// <param name="isUpdateLocalData"></param>
    public void AddContactData(EnumContactType contactType, JsonData jd, bool isUpdateLocalData = true)
    {
        if (jd != null)
        {
            ContactData contactData = new ContactData(contactType, jd);
            AddContactData(contactType, contactData, isUpdateLocalData);
        }
    }

    /// <summary>
    /// 添加联系人
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="cd"></param>
    /// <param name="isUpdateLocalData"></param>
    public void AddContactData(EnumContactType contactType, ContactData cd, bool isUpdateLocalData = true)
    {
        if (contactDict.ContainsKey(contactType) && !HasContactData(contactType, cd.roleid))
        {
            contactDict[contactType].Add(cd);
            if (isUpdateLocalData)
            {
                AddContactLocalData(contactType, new List<ContactData>() { cd });
            }
        }
    }

    /// <summary>
    /// 添加一组联系人
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="jdlist"></param>
    public void AddContactDatas(EnumContactType contactType, JsonData jdlist)
    {
        if (jdlist != null && jdlist.IsArray)
        {
            List<ContactData> cdlist = new List<ContactData>();
            foreach (JsonData jd in jdlist)
            {
                ContactData contactData = new ContactData(contactType, jd);
                cdlist.Add(contactData);
            }
            if (cdlist.Count > 0)
            {
                AddContactDatas(contactType, cdlist);
            }

        }
    }

    /// <summary>
    /// 添加一组联系人
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="cdlist"></param>
    public void AddContactDatas(EnumContactType contactType, List<ContactData> cdlist)
    {
        if (cdlist != null && cdlist.Count > 0)
        {
            foreach (ContactData cd in cdlist)
            {
                AddContactData(contactType, cd, false);
            }
            Debug.Log("``AddContactDatas CountCountCount:" + contactDict[EnumContactType.Apply].Count);
            AddContactLocalData(contactType, cdlist);
        }
    }

    /// <summary>
    /// 更新联系人数据
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="jd"></param>
    /// <param name="isUpdateLocalData"></param>
    public void UpdateContactData(EnumContactType contactType, JsonData jd, bool isUpdateLocalData = true)
    {
        if (jd != null)
        {
            ContactData contactData = new ContactData(contactType, jd);
            UpdateContactData(contactType, contactData, isUpdateLocalData);
        }
    }

    /// <summary>
    /// 更新联系人数据
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="cd"></param>
    /// <param name="isUpdateLocalData"></param>
    public void UpdateContactData(EnumContactType contactType, ContactData cd, bool isUpdateLocalData = true)
    {
        if (contactDict.ContainsKey(contactType))
        {
            ContactData old = GetContactData(contactType, cd.roleid);
            if (old != null)
            {
                old.Paste(cd);
                if (isUpdateLocalData)
                {
                    UpdateContactLocalData(contactType, new List<ContactData>() { cd });
                }
            }
        }
    }

    /// <summary>
    /// 更新一组联系人数据
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="jdlist"></param>
    public void UpdateContactDatas(EnumContactType contactType, JsonData jdlist)
    {
        if (jdlist != null && jdlist.IsArray)
        {
            List<ContactData> cdlist = new List<ContactData>();
            foreach (JsonData jd in jdlist)
            {
                ContactData contactData = new ContactData(contactType, jd);
                cdlist.Add(contactData);
            }
            if (cdlist.Count > 0)
            {
                UpdateContactDatas(contactType, cdlist);
            }

        }
    }

    /// <summary>
    /// 更新一组联系人数据
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="cdlist"></param>
    public void UpdateContactDatas(EnumContactType contactType, List<ContactData> cdlist)
    {
        if (cdlist != null && cdlist.Count > 0)
        {
            foreach (ContactData cd in cdlist)
            {
                ContactData old = GetContactData(contactType, cd.roleid);
                if (old != null)
                {
                    old.Paste(cd);
                }
            }
            UpdateContactLocalData(contactType, cdlist);
        }
    }

    /// <summary>
    /// 更新联系人的某个字段
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="roleId"></param>
    /// <param name="strParamType"></param>
    /// <param name="strParamValue"></param>
    /// <param name="isUpdateLocalData"></param>
    public void UpdateContactParam(EnumContactType contactType, string roleid, string strParamType, string strParamValue, bool isUpdateLocalData = true)
    {
        if (contactDict.ContainsKey(contactType))
        {
            ContactData old = GetContactData(contactType, roleid);
            if (old != null)
            {
                switch (strParamType)
                {
                    case "avatar":
                        old.OnUpdateAvatar(strParamValue);
                        break;
                    case "name":
                        old.OnUpdateName(strParamValue);
                        break;
                    case "lv":
                        old.OnUpdateLv(int.Parse(strParamValue));
                        break;
                    case "line":
                        old.OnUpdateLine(int.Parse(strParamValue));
                        break;
                    case "lastOnline":
                        old.OnUpdateLastOnline(long.Parse(strParamValue));
                        break;
                    default:
                        break;
                }
                if (isUpdateLocalData)
                {
                    UpdateContactLocalData(contactType, new List<ContactData>() { old });
                }
            }
        }
    }

    /// <summary>
    /// 移除联系人数据
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="jd"></param>
    /// <param name="isUpdateLocalData"></param>
    public void RemoveContactData(EnumContactType contactType, JsonData jd, bool isUpdateLocalData = true)
    {
        if (jd != null)
        {
            ContactData contactData = new ContactData(contactType, jd);
            RemoveContactData(contactType, contactData, isUpdateLocalData);
        }
    }

    /// <summary>
    /// 移除联系人数据
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="cd"></param>
    /// <param name="isUpdateLocalData"></param>
    public void RemoveContactData(EnumContactType contactType, ContactData cd, bool isUpdateLocalData = true)
    {
        if (cd != null)
        {
            RemoveContactData(contactType, cd.roleid, isUpdateLocalData);
        }
    }

    /// <summary>
    /// 移除联系人数据
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="cd"></param>
    /// <param name="isUpdateLocalData"></param>
    public void RemoveContactData(EnumContactType contactType, string roleId, bool isUpdateLocalData = true)
    {
        if (contactDict.ContainsKey(contactType))
        {

            for (int i = contactDict[contactType].Count - 1; i >= 0; i--)
            {
                if (contactDict[contactType][i].roleid == roleId)
                {
                    contactDict[contactType].RemoveAt(i);
                    break;
                }
            }

            if (isUpdateLocalData)
            {
                RemoveContactLocalData(contactType, new List<string>() { roleId });
            }
        }
    }

    /// <summary>
    /// 移除一组联系人数据
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="jdlist"></param>
    public void RemoveContactDatas(EnumContactType contactType, JsonData jdlist)
    {
        if (jdlist != null && jdlist.IsArray)
        {
            List<ContactData> cdlist = new List<ContactData>();
            foreach (JsonData jd in jdlist)
            {
                ContactData contactData = new ContactData(contactType, jd);
                cdlist.Add(contactData);
            }
            if (cdlist.Count > 0)
            {
                RemoveContactDatas(contactType, cdlist);
            }
        }
    }

    /// <summary>
    /// 移除一组联系人数据
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="cdlist"></param>
    public void RemoveContactDatas(EnumContactType contactType, List<ContactData> cdlist)
    {
        if (cdlist != null && cdlist.Count > 0)
        {
            List<string> cdlistids = new List<string>();
            foreach (ContactData cd in cdlist)
            {
                cdlistids.Add(cd.roleid);
            }
            RemoveContactDatas(contactType, cdlistids);
        }
    }

    /// <summary>
    /// 移除一组联系人数据
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="cdlist"></param>
    public void RemoveContactDatas(EnumContactType contactType, List<string> cdlistids)
    {
        if (cdlistids != null && cdlistids.Count > 0)
        {
            foreach (string rid in cdlistids)
            {
                for (int i = contactDict[contactType].Count - 1; i >= 0; i--)
                {
                    if (contactDict[contactType][i].roleid == rid)
                    {
                        contactDict[contactType].RemoveAt(i);
                        break;
                    }
                }
            }
            RemoveContactLocalData(contactType, cdlistids);
        }
    }

    /// <summary>
    /// 移除一类型联系人数据
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="cdlist"></param>
    public void RemoveContactDatas(EnumContactType contactType)
    {
        if (!contactDict.ContainsKey(contactType) || contactDict[contactType].Count <= 0)
        {
            return;
        }

        string localDataKey = contactType.ToString().ToLower();
        JsonData localData = LocalDataManager.Instance.GetLocalData();
        JsonData localListData = null;
        if (localData.ContainsKey(localDataKey))
        {
            localListData = localData[localDataKey];
        }

        for (int i = contactDict[contactType].Count - 1; i >= 0; i--)
        {
            string rid = contactDict[contactType][i].roleid;
            if (localListData != null && localListData.ContainsKey(rid))
            {
                localListData.Remove(rid);
            }
        }
        contactDict[contactType].Clear();
        if (localListData != null)
        {
            LocalDataManager.Instance.SetLocalData(localDataKey, localListData);
        }
    }

    /// <summary>
    /// 不同类型联系人之间数据转移
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="cdlist"></param>
    public void RemoveContactDatas(EnumContactType fromContactType, EnumContactType toContactType)
    {
        if (!contactDict.ContainsKey(fromContactType) || contactDict[fromContactType].Count <= 0 || !contactDict.ContainsKey(toContactType))
        {
            return;
        }

        string localDataKey_from = fromContactType.ToString().ToLower();
        string localDataKey_to = fromContactType.ToString().ToLower();
        JsonData localData = LocalDataManager.Instance.GetLocalData();

        JsonData localListData_from = localData[localDataKey_from];
        JsonData localListData_to = localData[localDataKey_to];

        // 先处理实时数据
        for (int i = contactDict[fromContactType].Count - 1; i >= 0; i--)
        {
            bool hasSame = false;
            for (int j = contactDict[toContactType].Count - 1; j >= 0; j--)
            {
                if (contactDict[fromContactType][i].roleid == contactDict[toContactType][j].roleid)
                    hasSame = true;
            }
            if (!hasSame)
            {
                contactDict[fromContactType][i].OnContactType(toContactType);
                contactDict[toContactType].Add(contactDict[fromContactType][i]);
            }
        }
        contactDict[fromContactType].Clear();

        // 再处理本地数据
        List<string> from_keys = new List<string>(localListData_from.Keys);
        List<string> to_keys = new List<string>(localListData_to.Keys);

        for (int i = 0; i < from_keys.Count; i++)
        {
            if (!to_keys.Contains(from_keys[i]))
            {
                JsonData onedata = localListData_from[from_keys[i]];
                if (onedata.ContainsKey("contactType"))
                {
                    onedata[from_keys[i]]["contactType"] = (int)toContactType;
                }
                localListData_to.Add(onedata);
            }
            localListData_from.Remove(from_keys[i]);
        }

        LocalDataManager.Instance.SetLocalData(localDataKey_from, localListData_from);
        LocalDataManager.Instance.SetLocalData(localDataKey_to, localListData_to);
    }

    #endregion

    #region contact local data
    /// <summary>
    /// 检测本地数据是否含有联系人json数据块
    /// </summary>
    private void CheckLocalDataHasKey()
    {
        JsonData localData = LocalDataManager.Instance.GetLocalData();
        bool bNeedUpdate = false;
        foreach (var contactType in Enum.GetValues(typeof(EnumContactType)))
        {
            string localDataKey = contactType.ToString().ToLower();
            if (!localData.ContainsKey(localDataKey))
            {
                localData[localDataKey] = jsontool.getemptytable();
                bNeedUpdate = true;
            }
        }
        if (bNeedUpdate)
        {
            LocalDataManager.Instance.CoverLocalData(localData);
        }
    }

    /// <summary>
    /// 开局同步一次所有联系人的本地数据
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="contactList"></param>
    private void SynchroContactLocalData(EnumContactType contactType, List<ContactData> contactList)
    {
        string localDataKey = contactType.ToString().ToLower();
        if (contactList != null)
        {
            JsonData localData = LocalDataManager.Instance.GetLocalData();

            if (localData.ContainsKey(localDataKey))
            {
                JsonData localListData = localData[localDataKey];
                List<string> ldKeys = new List<string>(localListData.Keys);
                List<string> newKeys = new List<string>(localListData.Keys);
                List<string> waitRemoveKeys = new List<string>();
                bool bNeedUpdata = false;
                foreach (var contactData in contactList)
                {
                    string strId = contactData.roleid.ToString();
                    newKeys.Add(contactData.roleid.ToString());
                    if ((localListData.ContainsKey(strId) && IsContactDataUpdated(localListData[strId], contactData))
                        || !localListData.ContainsKey(strId))
                    {
                        // update or add
                        localListData[strId] = contactData.ToJson();
                        bNeedUpdata = true;
                    }
                }

                foreach (var ldk in ldKeys)
                {
                    if (!newKeys.Contains(ldk))
                    {
                        waitRemoveKeys.Add(ldk);
                        bNeedUpdata = true;
                    }
                }

                // remove
                foreach (var wrkey in waitRemoveKeys)
                {
                    localListData.Remove(wrkey);
                }
                if (bNeedUpdata)
                {
                    LocalDataManager.Instance.SetLocalData(localDataKey, localListData);
                }
            }
        }
    }

    /// <summary>
    /// 联系人的本地数据是否有改变
    /// </summary>
    /// <param name="localData"></param>
    /// <param name="contactData"></param>
    /// <returns></returns>
    private bool IsContactDataUpdated(JsonData localData, ContactData contactData)
    {
        // 处理可能变动的字段，关系字段单独处理
        if ((localData["avatar"] == null || localData["avatar"].ToString() != contactData.avatar)
            || (localData["name"] == null || localData["name"].ToString() != contactData.nickname)
            || (localData["lv"] == null || localData["lv"].ToString() != contactData.level.ToString())
            || (localData["line"] == null || localData["line"].ToString() != contactData.line.ToString())
            || (localData["lastOnline"] == null || localData["lastOnline"].ToString() != contactData.lasttime.ToString()))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 设置联系人本地数据
    /// </summary>
    /// <param name="setType"></param>
    /// <param name="contactType"></param>
    /// <param name="contactList"></param>
    private void SetContactLocalData(string setType, EnumContactType contactType, List<ContactData> contactList)
    {
        string localDataKey = contactType.ToString().ToLower();
        if (contactList != null && contactList.Count > 0)
        {
            JsonData localData = LocalDataManager.Instance.GetLocalData();
            if (localData.ContainsKey(localDataKey))
            {
                JsonData localListData = localData[localDataKey];
                foreach (var contactData in contactList)
                {
                    string strId = contactData.roleid.ToString();
                    if (setType == "add")
                    {
                        if (!localListData.ContainsKey(strId))
                        {
                            localListData[strId] = contactData.ToJson();
                        }
                    }
                    else if (setType == "update")
                    {
                        if (localListData.ContainsKey(strId) && IsContactDataUpdated(localListData[strId], contactData))
                        {
                            localListData[strId] = contactData.ToJson();
                        }
                    }
                    else if (setType == "remove")
                    {
                        if (localListData.ContainsKey(strId))
                        {
                            localListData.Remove(strId);
                        }
                    }
                }
                Debug.Log("``localDataKey:" + localDataKey + " - localListData:" + jsontool.tostring(localListData));
                LocalDataManager.Instance.SetLocalData(localDataKey, localListData);
            }
        }
    }

    /// <summary>
    /// 添加联系人数据到本地
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="contactList"></param>
    public void AddContactLocalData(EnumContactType contactType, List<ContactData> contactList)
    {
        SetContactLocalData("add", contactType, contactList);
    }

    /// <summary>
    /// 更新联系人本地数据
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="contactList"></param>
    public void UpdateContactLocalData(EnumContactType contactType, List<ContactData> contactList)
    {
        SetContactLocalData("update", contactType, contactList);
    }

    /// <summary>
    /// 移除联系人本地数据
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="contactList"></param>
    public void RemoveContactLocalData(EnumContactType contactType, List<ContactData> contactList)
    {
        SetContactLocalData("remove", contactType, contactList);
    }

    /// <summary>
    /// 移除联系人本地数据
    /// </summary>
    /// <param name="contactType"></param>
    /// <param name="contactList"></param>
    public void RemoveContactLocalData(EnumContactType contactType, List<string> contactIdList)
    {
        string localDataKey = contactType.ToString().ToLower();
        if (contactIdList != null && contactIdList.Count > 0)
        {
            JsonData localData = LocalDataManager.Instance.GetLocalData();
            if (localData.ContainsKey(localDataKey))
            {
                JsonData localListData = localData[localDataKey];
                foreach (var rid in contactIdList)
                {
                    string strId = rid.ToString();
                    if (localListData.ContainsKey(strId))
                    {
                        localListData.Remove(strId);
                    }
                }
                LocalDataManager.Instance.SetLocalData(localDataKey, localListData);
            }
        }
    }

    /// <summary>
    /// 比较本地列表和传入列表差异(目前之比较个数), 后续添加其他比较
    /// </summary>
    /// <param name="compareList"></param>
    /// <returns>有差异:返回本地新列表， 没差异:返回null</returns>
    public List<ContactData> CompareContactListDiffWithLocal(EnumContactType contactType, List<ContactData> compareList)
    {
        string sk = contactType == EnumContactType.Apply ? "apply" :
            (contactType == EnumContactType.Blacklist ? "blacklist" :
            (contactType == EnumContactType.Stranger ? "stranger" : "friend"));
        JsonData localFriends = LocalDataManager.Instance.GetLocalData(sk);
        List<string> keys = new List<string>(localFriends.Keys);
        List<ContactData> contactDatas = new List<ContactData>();
        foreach (string key in keys)
        {
            ContactData contactData = JsonMapper.ToObject<ContactData>(JsonMapper.ToJson(localFriends[key]));
            if (contactData != null)
            {
                contactDatas.Add(contactData);
            }
        }
        if (contactDatas.Count != compareList.Count)
        {
            return contactDatas;
        }

        return null;
    }
    #endregion
}
