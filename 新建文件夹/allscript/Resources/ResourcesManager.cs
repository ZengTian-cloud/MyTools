using System;
using UnityEngine;
using YooAsset;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using Cysharp.Threading.Tasks;
using Basics;
using GameTiJi;
using static UnityEngine.Rendering.ReloadAttribute;

public static class ABTag
{
    public static readonly string Prefab = "prefab";
    public static readonly string Atlas = "atlas";
    public static readonly string Code = "code";
    public static readonly string Texture = "texture";
    public static readonly string Shader = "shader";
    public static readonly string Font = "font";
    public static readonly string Material = "material";
    public static readonly string Scene = "scene";
    public static readonly string Config = "config";
    public static readonly string Resmap = "resmap";
    public static readonly string Reseffect = "reseffect";
    public static readonly string Resrole = "resrole";
    public static readonly string Main = "main";
}

public class ResourcesManager : SingletonOjbect// : Singleton<ResourcesManager>
{
    public static ResourcesManager m_ResourcesManager;
    public static ResourcesManager Instance
    {
        get
        {
            if (m_ResourcesManager == null)
            {
                m_ResourcesManager = GameCenter.mIns.m_ResManager;
            }
            return m_ResourcesManager;
        }
    }

    public readonly string DefaultPackageName = "DefaultPackage";
    public readonly string AllResPackageName = "AllResPackage";
    public string[] Packages = new string[] { "DefaultPackage", "AllResPackage" };
    public bool UseABResources = false;

    private Dictionary<string, string[]> m_AssetTags = new Dictionary<string, string[]>();

    private readonly string[] d_AssetTags = new string[] { "prefab", "code", "font", "config", "textures", "language" };
    private readonly string[] a_AssetTags = new string[] { "prefab", "atlas", "texture", "shader", "material", "scene", "resmap", "reseffect", "resrole", "main" };
    private static Dictionary<string, string[]> prefabsDic = new Dictionary<string, string[]>();

    private void loadPrefabsDic() {
        prefabsDic.Add("login", new string[] { DefaultPackageName, "Assets/afirsts/prefabs/login.prefab", "afirsts" });
        prefabsDic.Add("commonpopwinprefab", new string[] { DefaultPackageName, "Assets/afirsts/prefabs/commonpopwinprefab.prefab", "afirsts" });
        prefabsDic.Add("commonpopfullscreenprefab", new string[] { DefaultPackageName, "Assets/afirsts/prefabs/commonpopfullscreenprefab.prefab", "afirsts" });
        prefabsDic.Add("commonpopwin", new string[] { DefaultPackageName, "Assets/afirsts/prefabs/commonpopwin.prefab", "afirsts" });
        prefabsDic.Add("commonpopwinprefabui", new string[] { DefaultPackageName, "Assets/afirsts/prefabs/commonpopwinprefabui.prefab", "afirsts" });
        prefabsDic.Add("serverlist", new string[] { DefaultPackageName, "Assets/afirsts/prefabs/serverlist.prefab", "afirsts" });
        prefabsDic.Add("commonpopmsg", new string[] { DefaultPackageName, "Assets/afirsts/prefabs/commonpopmsg.prefab", "afirsts" });
        prefabsDic.Add("pop", new string[] { DefaultPackageName, "", "afirsts" });
        prefabsDic.Add("emo", new string[] { DefaultPackageName, "", "afirsts" }); 

        prefabsDic.Add("mainui", new string[] { AllResPackageName, "Assets/allres/prefabs/ui/mainui.prefab","allres" });
        prefabsDic.Add("chapterwnd", new string[] { AllResPackageName, "Assets/allres/prefabs/ui/chapterwnd.prefab", "allres" });
        prefabsDic.Add("herogrow", new string[] { AllResPackageName, "Assets/allres/prefabs/ui/herogrow.prefab", "allres" });
        prefabsDic.Add("warehouse", new string[] { AllResPackageName, "Assets/allres/prefabs/ui/warehouse.prefab", "allres" });
        prefabsDic.Add("socialui", new string[] { AllResPackageName, "Assets/allres/prefabs/ui/socialui.prefab", "allres" });
        prefabsDic.Add("maillist", new string[] { AllResPackageName, "Assets/allres/prefabs/ui/maillist.prefab", "allres" });
        prefabsDic.Add("maildetail", new string[] { AllResPackageName, "Assets/allres/prefabs/ui/maildetail.prefab", "allres" });
        prefabsDic.Add("topresourcebar", new string[] { AllResPackageName, "Assets/allres/prefabs/ui/topresourcebar.prefab", "allres" });
        prefabsDic.Add("npcinteractionwidget", new string[] { AllResPackageName, "Assets/allres/prefabs/ui/npcinteractionwidget.prefab", "allres" });
        prefabsDic.Add("logbookwnd", new string[] { AllResPackageName, "Assets/allres/prefabs/ui/logbookwnd.prefab", "allres" });    // ������־
        prefabsDic.Add("commonitem", new string[] { AllResPackageName, "Assets/allres/prefabs/ui/commonitem.prefab", "allres" });    // ������Ʒ
        prefabsDic.Add("netprompt", new string[] { AllResPackageName, "Assets/allres/prefabs/ui/netprompt.prefab", "allres" });
        prefabsDic.Add("minimap", new string[] { AllResPackageName, "Assets/allres/prefabs/ui/minimap.prefab", "allres" });
        prefabsDic.Add("task_wnd", new string[] { AllResPackageName, "Assets/allres/prefabs/ui/task_wnd.prefab", "allres" });
        prefabsDic.Add("commonloading_wnd", new string[] { AllResPackageName, "Assets/allres/prefabs/ui/commonloading_wnd.prefab", "allres" });
        prefabsDic.Add("showiteminfo", new string[] { AllResPackageName, "Assets/allres/prefabs/ui/showiteminfo.prefab", "allres" });
        prefabsDic.Add("showitemlist", new string[] { AllResPackageName, "Assets/allres/prefabs/ui/showitemlist.prefab", "allres" });
        prefabsDic.Add("showitemmiddle", new string[] { AllResPackageName, "Assets/allres/prefabs/ui/showitemmiddle.prefab", "allres" });
    }

    public string[] GetPrefabsDicByName(string uiName)
    {
        if (prefabsDic.ContainsKey(uiName))
        {
            return prefabsDic[uiName];
        }
        return null;
    }

    public async UniTask<ResourcePackage> getResourcePackage(string packname)
    {
        if (string.IsNullOrEmpty(packname)) {
            packname = AllResPackageName;
        }
        return await getPackage(packname);
    }

    public async UniTask<ResourcePackage> getPackage(string packname) {
        return await PackManager.mIns.getPackage(packname, (errorcode) => {
            //��ȡ��ʧ����
        });
    }

    public async UniTask<string> getCodeVer()
    {
        ResourcePackage package = await getPackage(DefaultPackageName);
        return package.GetPackageVersion();
    }

    // ������Դ·�������ƶ�Ӧ�ֵ�
    private Dictionary<string, Dictionary<string, string>> m_AssetPathDict = new Dictionary<string, Dictionary<string, string>>();

    public async void Init(Action callback)
    {
        //����Ԥ���ֵ�
        loadPrefabsDic();

        Debug.Log("GIANTGAME ��ʼ���ع�����ԴAB��=================");
        //m_AssetTags.Add(DefaultPackageName, d_AssetTags);
        m_AssetTags.Add(AllResPackageName, a_AssetTags);

        // ���������
        GameObject mainCamera = GameObject.Find("mainCamera");
        if (mainCamera == null)
        {
            mainCamera = await LoadAssetSync(DefaultPackageName, "Assets/afirsts/prefabs/mainCamera.prefab") as GameObject;
            mainCamera = UnityEngine.Object.Instantiate(mainCamera);
            mainCamera.name = "mainCamera";
            Debug.Log("GIANTGAME gameCenterObj mainCamera:" + mainCamera);
        }

        await getPackage(DefaultPackageName);
        await loadTag(DefaultPackageName, d_AssetTags);
        Debug.Log("GIANTGAME loadTag success:" );
        //��ʼ��ͼ��
        await SpriteManager.Instance.InitAtlasConfig();
        callback?.Invoke();
    }

    public async void loadAllPackageTags()
    {
        foreach (var tagmap in m_AssetTags)
        {
            await loadTag(tagmap.Key, tagmap.Value);
        }
    }

    private async UniTask loadTag(string packname, string[] tags)
    {
        foreach (var tag in tags)
        {
            AssetInfo[] assetInfos =(await getPackage(packname)).GetAssetInfos(tag);
            //Debug.LogError("assetInfos.Length:" + assetInfos.Length + " - tag:" + tag);
            foreach (var assetInfo in assetInfos)
            {
                if (!m_AssetPathDict.ContainsKey(tag))
                    m_AssetPathDict.Add(tag, new Dictionary<string, string>());

                string tempPath = assetInfo.AssetPath.Replace("\\", "/");
                int si = tempPath.LastIndexOf("/") + 1;
                if (si < tempPath.Length && si >= 0)
                {
                    string assetName = tempPath.Substring(si, tempPath.Length - si);
                    //Debug.Log("tag:" + tag + " - assetName:" + assetName);
                    if (!m_AssetPathDict[tag].ContainsKey(assetName))
                        m_AssetPathDict[tag].Add(assetName, assetInfo.AssetPath);
                    else { }
                    //zxlogger.logwarning($"Warning: at tag:[{tag}] has same name asset! AssetPath:[{assetInfo.AssetPath}]! So, this asset you should use path load! can not use asset name load!");
                }
            }
        }
    }

    public GameObject LoadGameObjectPrefab(GameObject parentobj, GameObject childobj)
		{
			GameObject temobj = null;
			if (childobj != null && parentobj != null)
			{
				temobj = Object.Instantiate(childobj);
				temobj.name = childobj.name;
				temobj.transform.SetParent(parentobj.transform, false);
				ResetGameObject(temobj);
			}
			return temobj;
		}


    private void ResetGameObject(GameObject childobj)
    {
        childobj.transform.localPosition = Vector3.zero;
        childobj.transform.localScale = Vector3.one;
        childobj.transform.localEulerAngles = Vector3.zero;
    }

    public GameObject LoadRootResourcePrefab(string assetpath)
    {
        GameObject temobj = null;
        temobj = Resources.Load<GameObject>(assetpath);
        if (temobj != null)
        {
            temobj = UnityEngine.Object.Instantiate(temobj);
            temobj.name = pathtool.getfilename(assetpath);
            ResetGameObject(temobj);
        }
        return temobj;
    }

    public string GetAllResPath(string resName, string suffix = "")
    {
        return string.IsNullOrEmpty(suffix) ? $"Assets/allres/{resName}" : $"Assets/allres/{resName}.{suffix}";
    }

    public string GetAllModelPath(string resName, string suffix = "")
    {
        return string.IsNullOrEmpty(suffix) ? $"Assets/allmodel/{resName}" : $"Assets/allmodel/{resName}.{suffix}";
    }

    public string GetAtlasAssetPath(string atlasName)
    {
        string atlasPath = string.Empty;
        if (prefabsDic.ContainsKey(atlasName))
        {
            string[] val = prefabsDic.GetValueOrDefault(atlasName);
            atlasPath = val[2];
        }
        else
        {
            atlasPath = "allres";
        }
        return GetAtlasAssetPath(atlasPath, atlasName);
    }

    public string GetAtlasAssetPath(string pack, string atlasName)
    {
        return string.IsNullOrEmpty(atlasName) ? string.Empty : string.Format("Assets/{0}/uiatlas/{1}.spriteatlas", pack, atlasName);
    }

    public async UniTask<List<Object>> GetAllAssetObjectByTag(string tag)
    {
        List<Object> objs = new List<Object>();
        if (m_AssetPathDict.ContainsKey(tag))
        {
            foreach (var kv in m_AssetPathDict[tag])
            {
                Object o = await LoadAssetSync<Object>(kv.Value);
                objs.Add(o);
            }
        }
        return objs;
    }

    public async UniTask<Dictionary<string, Object>> GetAllAssetObjectDictByTag(string packname,string tag)
    {
        Dictionary<string, Object> objs = new Dictionary<string, Object>();
        if (m_AssetPathDict.ContainsKey(tag))
        {
            foreach (var kv in m_AssetPathDict[tag])
            {
                Object o = await LoadAssetSync<Object>(packname,kv.Value);
                objs.Add(kv.Key, o);
            }
        }
        return objs;
    }

    public async UniTask<Dictionary<string, Object>> GetAllAssetObjectDictByTag(string tag)
    {
        return await GetAllAssetObjectDictByTag(AllResPackageName,tag);
    }

    /// <summary>
    /// ͨ����Դ����ȡ��Դ·��
    /// </summary>
    /// <param name="resName"></param>
    private string GetAssetPathByName(string resName)
    {
        foreach (var tag in m_AssetPathDict)
        {
            foreach (var kv in tag.Value)
            {
                if (kv.Key == resName)
                    return kv.Value;
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// ͨ��Tag����Դ����ȡ��Դ·��
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="resName"></param>
    private string GetAssetPathByTagName(string tag, string resName)
    {
        if (m_AssetPathDict.ContainsKey(tag))
        {
            foreach (var kv in m_AssetPathDict[tag])
            {
                if (kv.Key == resName)
                    return kv.Value;
            }
        }
        return string.Empty;
    }

    #region Load By Name or Tag+Name
    /// <summary>
    /// ͨ����Դ��������Դ����Ҫ����Դ��׺��
    /// </summary>
    /// <param name="resName"></param>
    /// <returns></returns>
    public async UniTask<GameObject> LoadAssetSyncByName(string resName)
    {
        string path = GetAssetPathByName(resName);
        if (string.IsNullOrEmpty(path))
        {
            zxlogger.logerror($"Error: LoadAssetSyncByName fail! resName={resName}, not find path! please use path load!");
            return null;
        }
        return await LoadAssetSyncInstantiate(path);
    }

    /// <summary>
    /// ͨ����Դ��������Դ����Ҫ����Դ��׺��
    /// </summary>
    /// <param name="resName"></param>
    /// <returns></returns>
    public async UniTask<T> LoadAssetSyncByName<T>(string resName) where T : UnityEngine.Object
    {
        string path = GetAssetPathByName(resName);
        if (string.IsNullOrEmpty(path))
        {
            zxlogger.logerror($"Error: LoadAssetSyncByName fail! resName={resName}, not find path! please use path load!");
            return null;
        }
        return await  LoadAssetSync<T>(path);
    }

    /// <summary>
    /// ͨ����Դ��������Դ����Ҫ����Դ��׺��
    /// </summary>
    /// <param name="resName"></param>
    /// <returns></returns>
    public async void LoadAssetAsyncByName(string resName, Action<GameObject> callback)
    {
        string path = GetAssetPathByName(resName);
        if (string.IsNullOrEmpty(path))
        {
            zxlogger.logerror($"Error: LoadAssetAsyncByName fail! resName={resName}, not find path! please use path load!");
            callback?.Invoke(null);
            return;
        }

         ;
        callback.Invoke(await LoadAssetAsyncInstantiate(path));
    }

    /// <summary>
    /// ͨ����Դ��������Դ����Ҫ����Դ��׺��
    /// </summary>
    /// <param name="resName"></param>
    /// <returns></returns>
    public void LoadAssetAsyncByName<T>(string resName, Action<T> callback) where T : UnityEngine.Object
    {
        string path = GetAssetPathByName(resName);
        if (string.IsNullOrEmpty(path))
        {
            zxlogger.logerror($"Error: LoadAssetAsyncByName fail! resName={resName}, not find path! please use path load!");
            callback?.Invoke(null);
            return;
        }
        LoadAssetAsync<T>(path, callback);
    }

    /// <summary>
    /// ͨ����Դ��������Դ����Ҫ����Դ��׺��
    /// </summary>
    /// <param name="resName"></param>
    /// <returns></returns>
    public async UniTask<GameObject> LoadAssetSyncByTagName(string tag, string resName)
    {
        string path = GetAssetPathByTagName(tag, resName);
        if (string.IsNullOrEmpty(path))
        {
            zxlogger.logerror($"Error: LoadAssetSyncByName fail! resName={resName}, not find path! please use path load!");
            return null;
        }
        return await LoadAssetSyncInstantiate(path);
    }

    /// <summary>
    /// ͨ����Դ��������Դ����Ҫ����Դ��׺��
    /// </summary>
    /// <param name="resName"></param>
    /// <returns></returns>
    public async UniTask<T> LoadAssetSyncByTagName<T>(string tag, string resName) where T : UnityEngine.Object
    {
        string path = GetAssetPathByTagName(tag, resName);
        if (string.IsNullOrEmpty(path))
        {
            zxlogger.logerror($"Error: LoadAssetSyncByName fail! resName={resName}, not find path! please use path load!");
            return null;
        }
        return await LoadAssetSync<T>(path);
    }

    /// <summary>
    /// ͨ����Դ��������Դ����Ҫ����Դ��׺��
    /// </summary>
    /// <param name="resName"></param>
    /// <returns></returns>
    public async void LoadAssetAsyncTagName(string tag, string resName, Action<GameObject> callback)
    {
        string path = GetAssetPathByTagName(tag, resName);
        if (string.IsNullOrEmpty(path))
        {
            zxlogger.logerror($"Error: LoadAssetAsyncByName fail! resName={resName}, not find path! please use path load!");
            callback?.Invoke(null);
            return;
        }

        callback?.Invoke(await LoadAssetAsyncInstantiate(path));
    }

    /// <summary>
    /// ͨ����Դ��������Դ����Ҫ����Դ��׺��
    /// </summary>
    /// <param name="resName"></param>
    /// <returns></returns>
    public void LoadAssetAsyncTagName<T>(string tag, string resName, Action<T> callback) where T : UnityEngine.Object
    {
        string path = GetAssetPathByTagName(tag, resName);
        if (string.IsNullOrEmpty(path))
        {
            zxlogger.logerror($"Error: LoadAssetAsyncByName fail! resName={resName}, not find path! please use path load!");
            callback?.Invoke(null);
            return;
        }

        LoadAssetAsync<T>(path, callback);
    }

    #endregion
    /// <summary>
    /// ͬ������model�ļ��µ���Դ
    /// </summary>
    /// <param name="resPath"></param>
    /// <returns></returns>
    public async UniTask<UnityEngine.Object> LoadAssetSyncModelPath(string resPath)
    {
        if (!resPath.Contains("Assets"))
            resPath = GetAllModelPath(resPath);
        return await LoadAssetSync(resPath);
    }

    /// <summary>
    /// �첽����model�ļ��µ���Դ
    /// </summary>
    /// <param name="resPath"></param>
    /// <returns></returns>
    public void LoadAssetAsyncModelPath(string resPath, Action<UnityEngine.Object> callback)
    {
        if (!resPath.Contains("Assets"))
            resPath = GetAllModelPath(resPath);
        LoadAssetAsync(resPath, callback);
    }

    /// <summary>
    /// ͬ��������Դ(allres��)
    /// </summary>
    /// <param name="resPath"></param>
    /// <returns></returns>
    public async UniTask<UnityEngine.Object> LoadAssetSync(string resPath)
    {
        return await LoadAssetSync(AllResPackageName, resPath);
    }

    public async UniTask<UnityEngine.Object> LoadAssetSync(string packname,string resPath)
    {
        if (!resPath.Contains("Assets"))
            resPath = GetAllResPath(resPath);
        ResourcePackage package = await getResourcePackage(packname);
        AssetOperationHandle handle = package.LoadAssetSync<UnityEngine.Object>(resPath);
        await handle.ToUniTask();
        return handle.AssetObject;

        //UnityEngine.Object o = null;
      //  handle.Completed += (handle) => {
       //     o = handle.AssetObject;
       // };
      //  return o;
    }

        /// <summary>
        /// �첽������Դ(allres��)
        /// </summary>
        /// <param name="resPath"></param>
        /// <returns></returns>
    public void LoadAssetAsync(string resPath, Action<UnityEngine.Object> callback)
    {
        string packname = resPath.StartsWith("Assets/afirsts") ? DefaultPackageName : AllResPackageName;
        LoadAssetAsync(packname, resPath, callback);
    }

    public async void LoadAssetAsync(string packname,string resPath, Action<UnityEngine.Object> callback)
    {
        if (!resPath.Contains("Assets"))
            resPath = GetAllResPath(resPath);
        ResourcePackage package = await getResourcePackage(packname);
        AssetOperationHandle handle = package.LoadAssetAsync<UnityEngine.Object>(resPath);
        handle.Completed += (handle) =>
        {
            callback(handle.AssetObject);
        };
    }

    /// <summary>
    /// ͬ��������Դ(�Զ���ȫ·��)
    /// </summary>
    /// <param name="resPath"></param>
    /// <returns></returns>
    public async UniTask<T> LoadAssetSync<T>(string resPath) where T : UnityEngine.Object
    {
        return await LoadAssetSync<T>(AllResPackageName,resPath);
    }

    public async UniTask<T> LoadAssetSync<T>(string packname,string resPath) where T : UnityEngine.Object
    {
        ResourcePackage package = await getResourcePackage(packname);
        AssetOperationHandle handle = package.LoadAssetSync<T>(resPath);
        await handle.ToUniTask();
        return (T)handle.AssetObject;
    }

    /// <summary>
    /// �첽������Դ(�Զ���ȫ·��)
    /// </summary>
    /// <param name="resPath"></param>
    /// <returns></returns>
    public void LoadAssetAsync<T>(string resPath, Action<T> callback) where T : UnityEngine.Object
    {
        LoadAssetAsync(AllResPackageName,resPath,callback);
    }

    public async void LoadAssetAsync<T>(string packname, string resPath, Action<T> callback) where T : UnityEngine.Object
    {
        ResourcePackage package = await getResourcePackage(packname);
        AssetOperationHandle handle = package.LoadAssetAsync<T>(resPath);
        await handle.ToUniTask();
        callback((T)handle.AssetObject);
    }

    /// <summary>
    /// ͬ��������Դ����ʵ����(�Զ���ȫ·��)
    /// </summary>
    /// <param name="resPath"></param>
    /// <returns></returns>
    public async UniTask<GameObject> LoadAssetSyncInstantiate(string resPath)
    {
        return await LoadAssetSyncInstantiate(AllResPackageName, resPath);
    }

    public async UniTask<GameObject> LoadAssetSyncInstantiate(string packname, string resPath)
    {
        ResourcePackage package = await getResourcePackage(packname);
        AssetOperationHandle handle = package.LoadAssetSync<UnityEngine.Object>(resPath);
        await handle.ToUniTask();
        return handle.InstantiateSync();
/*        GameObject o = null;
        handle.Completed += (handle) => {
            o = handle.InstantiateSync();
        };
        return o;*/
    }

    /// <summary>
    /// �첽������Դ����ʵ����(�Զ���ȫ·��)
    /// </summary>
    /// <param name="resPath"></param>
    /// <param name="callback"></param>
    public async UniTask<GameObject> LoadAssetAsyncInstantiate(string resPath)
    {
        return await LoadAssetAsyncInstantiate(AllResPackageName,resPath);

    }

    public async UniTask<GameObject> LoadAssetAsyncInstantiate(string packname, string resPath)
    {
        ResourcePackage package = await getResourcePackage(packname);
        AssetOperationHandle handle = package.LoadAssetAsync<UnityEngine.Object>(resPath);
        await handle.ToUniTask();
        return handle.InstantiateSync();
    }

    /// <summary>
    /// ͬ������Ԥ�Ƽ�����ʵ����(allres��)
    /// </summary>
    /// <param name="prefabName"></param>
    /// <returns></returns>
    public async UniTask<GameObject> LoadPrefabSync(string prefabName)
    {
        return await LoadAssetSyncInstantiate(GetAssetPathByTagName(ABTag.Prefab, prefabName));
    }

    /// <summary>
    /// ͬ������Ԥ�Ƽ�����ʵ����(allres��), �����ڵ㣬����ֵ���ض����������
    /// </summary>
    /// <param name="uiName"></param>
    /// <returns></returns>
    public async UniTask<GameObject> LoadPrefabSync(string prefabName, Transform parent)
    {
        GameObject o = await LoadAssetSyncInstantiate(GetAssetPathByTagName(ABTag.Prefab, prefabName));
        if (o != null && parent != null)
        {
            o.transform.SetParent(parent);
            o.transform.localScale = Vector3.one;
            o.transform.localPosition = Vector3.zero;
            o.transform.localRotation = Quaternion.identity;
        }
        return o;
    }

    /// <summary>
    /// ͬ������Ԥ�Ƽ�����ʵ����(allres��)
    /// </summary>
    /// <param name="prefabType"></param>
    /// <param name="prefabName"></param>
    /// <returns></returns>
    public async UniTask<GameObject> LoadPrefabSync(string prefabType, string prefabName)
    {
        string uiPath = GetAllResPath(string.Format("prefabs/{0}/{1}.prefab", prefabType, prefabName));
        return await LoadAssetSyncInstantiate(uiPath);
    }

    /// <summary>
    /// ͬ������Ԥ�Ƽ�����ʵ����(allres��), �����ڵ㣬����ֵ���ض����������
    /// </summary>
    /// <param name="prefabType"></param>
    /// <param name="prefabName"></param>
    /// <returns></returns>
    public async UniTask<GameObject> LoadPrefabSync(string prefabType, string prefabName, Transform parent)
    {
        string uiPath = GetAllResPath(string.Format("prefabs/{0}/{1}.prefab", prefabType, prefabName));
        GameObject o = await LoadAssetSyncInstantiate(uiPath);
        if (o != null && parent != null)
        {
            o.transform.SetParent(parent);
            o.transform.localScale = Vector3.one;
            o.transform.localPosition = Vector3.zero;
            o.transform.localRotation = Quaternion.identity;
        }
        return o;
    }

    /// <summary>
    /// �첽����Ԥ�Ƽ�����ʵ����(allres��)
    /// </summary>
    /// <param name="prefabType"></param>
    /// <param name="prefabName"></param>
    /// <param name="callback"></param>
    public async void LoadPrefabAsync(string prefabType, string prefabName, Action<GameObject> callback)
    {
        string uiPath = GetAllResPath(string.Format("prefabs/{0}/{1}.prefab", prefabType, prefabName));
        callback.Invoke(await LoadAssetAsyncInstantiate(uiPath));
    }

    public async UniTask<GameObject> LoadPrefabAsync2(string prefabType, string prefabName)
    {
        string uiPath = GetAllResPath(string.Format("prefabs/{0}/{1}.prefab", prefabType, prefabName));

        //callback.Invoke(await LoadAssetAsyncInstantiate(uiPath));

        return await LoadAssetAsyncInstantiate(uiPath);
    }

    public async UniTask<GameObject> LoadUIPrefabAndAtlasSync(string uiName,string uiAtlasName)
    {
        string packname = "";
        string uiPath = "";
        string atlasPath = "";
        if (prefabsDic.ContainsKey(uiName))
        {
            string[] val = prefabsDic.GetValueOrDefault(uiName);
            packname = val[0];
            uiPath = val[1];
            atlasPath = val[2];
            Debug.Log($"����Ԥ�ÿ�ʼ uiName��{uiName} packname��{packname} uiPath��{uiPath}");
        }
        else
        {
            packname = AllResPackageName;
            uiPath = GetAllResPath(string.Format("prefabs/ui/{0}.prefab", uiName));
            atlasPath = "allres";
            Debug.LogError("UI预制件未注册，请前往ResourcesManager.cs ��55行�� 注册： prefabsDic.Add(\"" + uiName + "\", new string[] { AllResPackageName, \"" + uiPath + "\" }); ");
        }

        //����ͼ��
        if (!string.IsNullOrEmpty(uiAtlasName))
        {
            await SpriteManager.Instance.LoadUiAtlas(atlasPath,packname,uiAtlasName);
        }

        return await LoadAssetSyncInstantiate(packname, uiPath);
    }

    /// <summary>
    /// ͬ������UIԤ�Ƽ�
    /// </summary>
    /// <param name="uiName"></param>
    /// <returns></returns>
    public async UniTask<GameObject> LoadUIPrefabSync(string uiName)
    {

        string packname = "";
        string uiPath = "";
        if (prefabsDic.ContainsKey(uiName))
        {
            string[] val = prefabsDic.GetValueOrDefault(uiName);
            packname = val[0];
            uiPath = val[1];
            Debug.Log($"����Ԥ�ÿ�ʼ uiName��{uiName} packname��{packname} uiPath��{uiPath}");
        }
        else {
            packname = AllResPackageName;
            uiPath = GetAllResPath(string.Format("prefabs/ui/{0}.prefab", uiName));
            Debug.LogError("未注册对应预制体，请前往resouresManager.cs 55行处添加： prefabsDic.Add(\""+ uiName + "\", new string[] { AllResPackageName, \"" + uiPath + "\" }); ");
        }

        return await LoadAssetSyncInstantiate(packname,uiPath);
    }

    /// <summary>
    /// ͬ������UIԤ�Ƽ�, �����ڵ㣬����ֵ���ض����������
    /// </summary>
    /// <param name="uiName"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public async UniTask<GameObject> LoadUIPrefabSync(string uiName, Transform parent, bool isInit = false)
    {
        string uiPath = GetAllResPath(string.Format("prefabs/ui/{0}.prefab", uiName));
        GameObject o = await LoadAssetSyncInstantiate(uiPath);
        if (o != null && parent != null)
        {
            o.transform.SetParent(parent);
            if (isInit)
            {
                o.transform.localPosition = Vector3.zero;
                o.transform.localRotation = Quaternion .identity;
                o.transform.localScale = Vector3.one;
                RectTransform rt = o.transform.GetComponent<RectTransform>();
                if (rt!= null)
                {
                    rt.anchorMax = new Vector2(1, 1);
                    rt.anchorMin = new Vector2(0, 0);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = Vector2.zero;
                    rt.offsetMin = new Vector2(0, 0);
                    rt.offsetMax = new Vector2(0, 0);
                }
            }
        }
        return o;
    }

    /// <summary>
    /// �첽����UIԤ�Ƽ�
    /// </summary>
    /// <param name="uiName"></param>
    /// <param name="callback"></param>
    public async void LoadUIPrefabASync(string uiName, Action<GameObject> callback)
    {
        string uiPath = GetAllResPath(string.Format("prefabs/ui/{0}.prefab", uiName));
        callback.Invoke( await LoadAssetAsyncInstantiate(uiPath));
    }

    public void ReleaseResources(string path)
    {
        // todo
    }

    /*
    /// <summary>
    /// WEBGL
    /// </summary>
    /// <returns></returns>
    private IEnumerator InitializeYooAsset()
    {
        string defaultHostServer = "http://127.0.0.1/CDN/WebGL/v1.0";
        string fallbackHostServer = "http://127.0.0.1/CDN/WebGL/v1.0";
        var initParameters = new WebPlayModeParameters();
        initParameters.QueryServices = new GameQueryServices(); //̫��ս��DEMO�Ľű��࣬��ϸ��StreamingAssetsHelper
        initParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
        var initOperation = package.InitializeAsync(initParameters);
        yield return initOperation;

        if (initOperation.Status == EOperationStatus.Succeed)
        {
            Debug.Log("��Դ����ʼ���ɹ���");
        }
        else
        {
            Debug.LogError($"��Դ����ʼ��ʧ�ܣ�{initOperation.Error}");
        }
    }
    */
}
