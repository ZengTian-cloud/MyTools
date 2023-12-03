using System;
using System.IO;
using System.Collections;
using UnityEngine;
using LitJson;
using System.Text;

/// <summary>
/// 本地数据管理
/// </summary>
public class LocalDataManager : SingletonNotMono<LocalDataManager>
{
    private string m_localDataPath = "";

    private JsonData localData = null;

    public void CheckRoleLocalDataFile()
    {
        string uid = GameCenter.mIns.userInfo.Uid;
        string roleId = GameCenter.mIns.userInfo.RoleId;
        string fileName = filetool.getfilenamemd5(roleId);
        string localDataPath = "";
#if UNITY_EDITOR || UNITY_STANDALONE
        localDataPath = Application.streamingAssetsPath + $"/{fileName}.txt";
#elif UNITY_IPHONE || UNITY_IOS
		localDataPath = Application.persistentDataPath + $"/{fileName}.txt";
#elif UNITY_ANDROID
		localDataPath = Application.persistentDataPath + $"/{fileName}.txt";
#endif
        zxlogger.log($"` local data: uid:{uid}, roleId:{roleId}, fileName:{fileName}, localDataPath:{localDataPath}");
        if (!filetool.exists(localDataPath))
        {
            File.Create(localDataPath).Dispose();
        }
        m_localDataPath = localDataPath;
        localData = ReadLocalDataFile(localDataPath);

        if (!HasLocalData("uid"))
            SetLocalData("uid", uid);
        if (!HasLocalData("roleId"))
            SetLocalData("roleId", roleId);
        else
        {
            if (localData["roleId"].ToString().Contains("gta"))
                SetLocalData("roleId", roleId);
        }
        if (!HasLocalData("mails"))
        {
            CreateEmptyTable("mails");
        }
    }

    private JsonData ReadLocalDataFile(string filePath)
    {
        if (!filetool.exists(filePath))
        {
            using (FileStream fs = File.Create(filePath))
            {
                Byte[] info = new UTF8Encoding(false).GetBytes("");
                fs.Write(info, 0, info.Length);
            }
        }

        string sjs = File.ReadAllText(filePath, Encoding.UTF8);
        JsonData jd = JsonMapper.ToObject(sjs);
        return jd;
    }

    private void UpdateLocalData()
    {
        string s = jsontool.tostring(localData);

        FileInfo file = new FileInfo(m_localDataPath);
        if (filetool.exists(m_localDataPath))
        {
            UTF8Encoding m_utf8 = new UTF8Encoding(false);
            File.WriteAllText(m_localDataPath, s, m_utf8);
        }
    }

    public JsonData GetLocalData()
    {
        return localData;
    }

    public JsonData GetLocalData(string key)
    {
        if (HasLocalData(key))
            return localData[key];
        return null;
    }

    public bool HasLocalData(string key)
    {
        return localData.ContainsKey(key);
    }

    public void CreateEmptyTable(string key)
    {
        localData[key] = jsontool.getemptytable();
        UpdateLocalData();
    }

    public void SetLocalData(string key, object o)
    {
        SetLocalData(key, o.ToString());
    }

    public void SetLocalData(string key, int i)
    {
        SetLocalData(key, i.ToString());
    }

    public void SetLocalData(string key, long li)
    {
        SetLocalData(key, li.ToString());
    }

    public void SetLocalData(string key, string s)
    {
        localData[key] = s;
        UpdateLocalData();
    }

    public void SetLocalData(string key, JsonData data)
    {
        localData[key] = data;
        UpdateLocalData();
    }

    public void CoverLocalData(JsonData data)
    {
        if (data != null)
        {
            localData = data;
            UpdateLocalData();
        }
    }
}
