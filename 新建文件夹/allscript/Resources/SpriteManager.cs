/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 * 图集管理
 *	TODO：当前手动管理
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;
using YooAsset;
using System.Runtime.Serialization.Formatters.Binary;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using Spine;
using DG.Tweening.Plugins.Core.PathCore;

public class AtlasItem
{
    public string name;
    public string path;
    public int instanceID;
    public SpriteAtlas atlas;
    public AtlasItem(string name, string path, SpriteAtlas sa)
    {
        this.name = name;
        this.path = path;
        instanceID = sa.GetInstanceID();
        atlas = sa;
    }

    public Sprite GetSprite(string sname)
    {
        return atlas.GetSprite(sname);
    }
}

public class SpriteManager : Singleton<SpriteManager>
{
    // 常驻图集
    //private Dictionary<string, AtlasItem> m_ResidentSpriteAtlas = new Dictionary<string, AtlasItem>();
    // 动态图集，随需要加载卸载
    private Dictionary<string, AtlasItem> m_DynamicSpriteAtlas = new Dictionary<string, AtlasItem>();

    private static Action<SpriteAtlas> m_RequestAtlasCallback = null;

    public void DebugAtlas()
    {
     /*   foreach (var item in m_ResidentSpriteAtlas)
        {
            Debug.Log($"m_ResidentSpriteAtlas> key:{item.Key}, atlas:{item.Value.atlas}, atlas:{item.Value.atlas.spriteCount}");
        }*/
        foreach (var item in m_DynamicSpriteAtlas)
        {
            Debug.Log($"m_DynamicSpriteAtlas> key:{item.Key}, atlas:{item.Value.atlas}, atlas:{item.Value.atlas.spriteCount}");
        }
    }

    public void Init()
    {
        // 暂定此事件不移除，存在整个游戏周期中
        try { 
            SpriteAtlasManager.atlasRequested += RequestAtlas;
        }catch (Exception ex)
        {
            Debug.LogError("初始化图集出错。。。。。"+ex.ToString());
        }
    }

   /* private void AddToResidentSpriteAtlas(string name, SpriteAtlas spriteAtlas)
    {
        if (m_ResidentSpriteAtlas.ContainsKey(name))
        {
            return;
        }
        m_ResidentSpriteAtlas.Add(name, new AtlasItem(name, GetAtlasAssetPath(name), spriteAtlas));
    }*/

    private void AddToDynamicSpriteAtlas(string name, SpriteAtlas spriteAtlas)
    {
        if (m_DynamicSpriteAtlas.ContainsKey(name))
        {
            return;
        }
        m_DynamicSpriteAtlas.Add(name, new AtlasItem(name, ResourcesManager.Instance.GetAtlasAssetPath(name), spriteAtlas));
    }

    /// <summary>
    /// 当加载图片没有对应图集时的反馈事件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="callback"></param>
    void RequestAtlas(string name, Action<SpriteAtlas> callback)
    {
        string packname = "AllResPackage";
        string atlasPath = string.Empty;
        string[] vals = ResourcesManager.Instance.GetPrefabsDicByName(name);
        if (vals != null) {
            packname = vals[0];
            atlasPath = vals[2];
        }
        else
        {
            atlasPath = "allres";
        }
        if (!name.Contains("Assets/"))
            atlasPath = ResourcesManager.Instance.GetAtlasAssetPath(atlasPath, name);
        else
            atlasPath = name;
        RequestAtlasCall(packname, name, atlasPath, callback);
    }

    void RequestAtlasCall(string packname,string name, string atlasPath,Action<SpriteAtlas> callback)
    { 
        Debug.Log($"Need Load Atlas packname:{packname}, name:{name},path:{atlasPath} ,IsCommonAtlas(path):{Instance.IsCommonAtlas(name)}" );
        if (m_RequestAtlasCallback == null)
            m_RequestAtlasCallback = callback;

        LoadAtlasAsync(packname, name, atlasPath, (atlas) =>
        {
            // 不执行回调无法显示图片
            if (atlas != null)
            {
                callback(atlas);
            }
        });

        // 只有常驻内存的图集用此方式加载
        /*if (Instance.IsCommonAtlas(name))
        {
            Instance.LoadAtlas(packname,name, atlasPath, (atlas) =>
            {
                // 不执行回调无法显示图片
                if (atlas != null)
                {
                    callback(atlas);
                }
            });
        }
        else
        {
            Instance.LoadAtlasAsync(packname, name, atlasPath, (atlas) =>
            {
                // 不执行回调无法显示图片
                if (atlas != null)
                {
                    callback(atlas);
                }
            });
        }*/
    }

    public void DoAtlasLoadedCallback(SpriteAtlas spriteAtlas)
    {
        //if (m_RequestAtlasCallback == null)
        //    Init();
        if (m_RequestAtlasCallback != null && spriteAtlas != null)
        {
            m_RequestAtlasCallback(spriteAtlas);
        }
    }

    public void LoadAtlasAsync(string packname, string atlasName,string path, Action<SpriteAtlas> callback)
    {
        if (m_DynamicSpriteAtlas.ContainsKey(atlasName))
        {
            callback?.Invoke(m_DynamicSpriteAtlas[atlasName].atlas);
            return;
        }
        ResourcesManager.Instance.LoadAssetAsync<SpriteAtlas>(packname, path ,(spriteAtlas) =>
        {
            AddToDynamicSpriteAtlas(atlasName, spriteAtlas);
            callback?.Invoke(spriteAtlas);
            // 这里手动执行一次回调...
            Debug.Log("LoadAtlas spriteAtlas: " + spriteAtlas);
        });
    }

    public async void LoadAtlas(string packname,string atlasName, string path,Action<SpriteAtlas> callback)
    {
        if (m_DynamicSpriteAtlas.ContainsKey(atlasName))
        {
            callback?.Invoke(m_DynamicSpriteAtlas[atlasName].atlas);
            return ;
        }
        SpriteAtlas spriteAtlas = await ResourcesManager.Instance.LoadAssetSync(packname,path) as SpriteAtlas;
        if (spriteAtlas != null)
        {
            AddToDynamicSpriteAtlas(atlasName, spriteAtlas);
            callback?.Invoke(spriteAtlas);
            // 这里手动执行一次回调...
            Debug.Log("LoadAtlas spriteAtlas: " + spriteAtlas);
            // DoAtlasLoadedCallback(spriteAtlas);
        }
    }

    public async UniTask<SpriteAtlas> LoadUiAtlas(string fpath,string packname,string atlasName)
    {
        if (m_DynamicSpriteAtlas.ContainsKey(atlasName))
        {
            return m_DynamicSpriteAtlas[atlasName].atlas;
        }
        string path = ResourcesManager.Instance.GetAtlasAssetPath(fpath, atlasName);
        SpriteAtlas spriteAtlas = await ResourcesManager.Instance.LoadAssetSync(packname,path) as SpriteAtlas;
        if (spriteAtlas != null)
        {
            AddToDynamicSpriteAtlas(atlasName, spriteAtlas);
            // 这里手动执行一次回调...
            Debug.Log($"LoadAtlas spriteAtlas: {spriteAtlas} ");
            return spriteAtlas;
        }
        Debug.LogError($"加载图集失败 atlasName:{atlasName}-------------");
        return null;
    }

    public async UniTask<SpriteAtlas> LoadAtlas(string atlasName)
    {
        if (m_DynamicSpriteAtlas.ContainsKey(atlasName))
        {
            return m_DynamicSpriteAtlas[atlasName].atlas;
        }
        string path = ResourcesManager.Instance.GetAtlasAssetPath(atlasName);
        SpriteAtlas spriteAtlas = await ResourcesManager.Instance.LoadAssetSync(path) as SpriteAtlas;
        if (spriteAtlas != null)
        {
            AddToDynamicSpriteAtlas(atlasName, spriteAtlas);
            // 这里手动执行一次回调...
            Debug.Log($"LoadAtlas spriteAtlas: {spriteAtlas} ");
            return spriteAtlas;
        }
        Debug.LogError($"加载图集失败 atlasName:{atlasName},path:{path}-------------");
        return null;
    }  

    #region 图集配置
    private AtlasConfigBytes atlasConfigBytes = null;
    private ConcurrentDictionary<string , AtlasConfigInfoForBytes> atlasConfigDic = null;

    public async UniTask<AtlasConfigBytes> InitAtlasConfig(AtlasConfigBytes atlasConfigBytes = null)
    {
        Debug.Log("GIANTGAME 图集初始化 start----------------------");
        Init();
        Debug.Log("GIANTGAME 图集初始化 end----------------------");
        if (atlasConfigBytes == null)
        {
            Debug.Log("GIANTGAME 加载图集配置 start----------------------");
            TextAsset textAsset = await ResourcesManager.Instance.LoadAssetSync<UnityEngine.TextAsset>("DefaultPackage", "Assets/afirsts/uiatlas/atlasconfig.bytes");
            if (textAsset != null)
            {
                MemoryStream stream = new MemoryStream(textAsset.bytes);
                BinaryFormatter bf = new BinaryFormatter();
                this.atlasConfigBytes = (AtlasConfigBytes)bf.Deserialize(stream);
                stream.Close();
                initDic();
                Debug.Log("GIANTGAME 同步加载图集配置 end----------------------");
            }
            else
            {
                zxlogger.logerror("Error: InitAtlasConfig fail! textAsset == null!");
            }
            /*foreach (var assetObj in handle.AllAssetObjects)
            {
                if (assetObj.name == "Assets/allres/uiatlas/atlasconfig.bytes" || assetObj.name == "atlasconfig")
                {
                    
                    
                }
            }*/
        }
        else
        {
            this.atlasConfigBytes = atlasConfigBytes;
        }
        return this.atlasConfigBytes;
    }
    private void initDic() {
        if (atlasConfigBytes == null) return;
        atlasConfigDic = new ConcurrentDictionary<string, AtlasConfigInfoForBytes>();
        foreach (AtlasConfigInfoForBytes acifb in atlasConfigBytes.m_CommonAtlasList)
        {
            foreach (AtlasConfigInfo.AtlasSpriteInfo asi in acifb.Sprites)
            {
                atlasConfigDic.TryAdd(asi.name, acifb);
            }
        }
        foreach (AtlasConfigInfoForBytes acifb in atlasConfigBytes.m_NormalAtlasList)
        {
            foreach (AtlasConfigInfo.AtlasSpriteInfo asi in acifb.Sprites)
            {
                atlasConfigDic.TryAdd(asi.name, acifb);
            }
        }
    }
    public async UniTask LoadCommonAtlas()
    {
        Debug.Log($"加载常驻图集开始：");
        int total = atlasConfigBytes.m_CommonAtlasList.Count;
        foreach (AtlasConfigInfoForBytes acifb in atlasConfigBytes.m_CommonAtlasList)
        {
            await LoadAtlas(acifb.AtlasName);
        }
        Debug.Log($"加载常驻图集完成：");
    }
    public bool IsCommonAtlas(string atlasName)
    {
        if (atlasConfigBytes == null) return false;
        foreach (AtlasConfigInfoForBytes acifb in atlasConfigBytes.m_CommonAtlasList)
        {
            if (acifb.AtlasName == atlasName)
            {
                return true;
            }
        }
        return false;
    }

    public uint GetAtlasCRCBySpritePath(string path)
    {
        return GetAtlasCRCBySpritCrc(Crc32.GetCrc32(path));
    }

    public uint GetAtlasCRCBySpritCrc(uint spriteCrc)
    {
        if (atlasConfigBytes != null)
        {
            foreach (AtlasConfigInfoForBytes acifb in atlasConfigBytes.m_CommonAtlasList)
            {
                foreach (AtlasConfigInfo.AtlasSpriteInfo asi in acifb.Sprites)
                {
                    if (asi != null && asi.crc == spriteCrc)
                    {
                        return acifb.AtlasCRC;
                    }
                }
            }
            foreach (AtlasConfigInfoForBytes acifb in atlasConfigBytes.m_NormalAtlasList)
            {
                foreach (AtlasConfigInfo.AtlasSpriteInfo asi in acifb.Sprites)
                {
                    if (asi != null && asi.crc == spriteCrc)
                    {
                        return acifb.AtlasCRC;
                    }
                }
            }
        }
        return 0;
    }

  
    public AtlasConfigInfoForBytes GetAtlasConfigBySpriteName(string spriteName, string suffix = "")
    {
        spriteName = string.IsNullOrEmpty(suffix) ? string.Format("{0}.{1}", spriteName, "png") : string.Format("{0}.{1}", spriteName, suffix);
        if (atlasConfigDic.ContainsKey(spriteName))
        {
            return atlasConfigDic.GetValueOrDefault(spriteName);
        }
        if (atlasConfigBytes != null)
        {
            foreach (AtlasConfigInfoForBytes acifb in atlasConfigBytes.m_CommonAtlasList)
            {
                foreach (AtlasConfigInfo.AtlasSpriteInfo asi in acifb.Sprites)
                {
                    if (asi != null && asi.name == spriteName)
                    {
                        return acifb;
                    }
                }
            }
            foreach (AtlasConfigInfoForBytes acifb in atlasConfigBytes.m_NormalAtlasList)
            {
                foreach (AtlasConfigInfo.AtlasSpriteInfo asi in acifb.Sprites)
                {
                    if (asi != null && asi.name == spriteName)
                    {
                        return acifb;
                    }
                }
            }
        }
        return null;
    }
    #endregion

    public async UniTask GetTextureSync(string texPath, Action<Texture> callback, string suffix = "png")
    {
        if (!string.IsNullOrEmpty(texPath))
        {
            string path = $"Assets/allres/textures/{texPath}.{suffix}";
            Texture texture = await ResourcesManager.Instance.LoadAssetSync<Texture>(path);
            if (texture)
            {
                callback?.Invoke(texture);
            }
            else
            {
                zxlogger.logwarning($"Error:GetTextureSprite fail! path=({path}) ");
            }
        }
    }

    public async UniTask<Texture> GetTextureSync(string texPath, string suffix = "png")
    {
        if (!string.IsNullOrEmpty(texPath))
        {
            string path = $"Assets/allres/textures/{texPath}.{suffix}";
            return await ResourcesManager.Instance.LoadAssetSync<Texture>(path);
        }
        return null;
    }

    public void GetTextureAsync(string texPath, Action<Texture> callback, string suffix = "png")
    {
        if (!string.IsNullOrEmpty(texPath))
        {
            string path = $"Assets/allres/textures/{texPath}.{suffix}";
            ResourcesManager.Instance.LoadAssetAsync<Texture>(path, callback);
        }
    }

    public async UniTask GetTextureSpriteSync(string texPath, Action<Sprite> callback, string suffix = "png")
    {
        if (!string.IsNullOrEmpty(texPath))
        {
            string path = $"Assets/allres/textures/{texPath}.{suffix}";
            Sprite sp = await ResourcesManager.Instance.LoadAssetSync<Sprite>(path);
            if (sp)
            {
                callback?.Invoke(sp);
            }
            else
            {
                zxlogger.logwarning($"Error:GetTextureSprite fail! path=({path}) ");
            }
        }
    }

    public async UniTask<Sprite> GetTextureSpriteSync(string texPath, string suffix = "png")
    {
        if (!string.IsNullOrEmpty(texPath))
        {
            string path = $"Assets/allres/textures/{texPath}.{suffix}";
            return await ResourcesManager.Instance.LoadAssetSync<Sprite>(path);
        }
        return null;
    }

    public void GetTextureSpriteAsync(string texPath, Action<Sprite> callback, string suffix = "png")
    {
        if (!string.IsNullOrEmpty(texPath))
        {
            string path = $"Assets/allres/textures/{texPath}.{suffix}";
            ResourcesManager.Instance.LoadAssetAsync<Sprite>(path, callback);
        }
    }

    public void DestroyTextureSpritec(string texPath, string suffix = "png")
    {
        if (!string.IsNullOrEmpty(texPath))
        {
            string path = $"Assets/allres/textures/{texPath}.{suffix}";
            // ResourcesManager.Instance.DefaultPackageName<Sprite>(path, callback);
        }
    }

    private Sprite GetSpriteAtCache(string spriteName)
    {
        Sprite sprite = null;
        /*   foreach (var item in m_ResidentSpriteAtlas)
           {
               sprite = item.Value.GetSprite(spriteName);
           }*/
        foreach (var item in m_DynamicSpriteAtlas)
        {
            sprite = item.Value.GetSprite(spriteName);
            if (sprite != null)
            {
                return sprite;
            }
        }
        return null;
    }

    public Sprite GetSpriteSync(string spriteName, string suffix = "")
    {
        Sprite sprite = null;
        if (!string.IsNullOrEmpty(spriteName))
        {

            AtlasConfigInfoForBytes atlasConfig = GetAtlasConfigBySpriteName(spriteName, suffix);
            GetSpriteSync(spriteName, atlasConfig, (sp) =>
            {
                sprite = sp;
            }, suffix);
        }
        return sprite;
    }

    public void GetSpriteSync(string spriteName, Action<Sprite> callback, string suffix = "")
    {
        //Debug.LogError("`````spriteName:" + spriteName);
        if (!string.IsNullOrEmpty(spriteName))
        {

            AtlasConfigInfoForBytes atlasConfig = GetAtlasConfigBySpriteName(spriteName, suffix);
            GetSpriteSync(spriteName, atlasConfig, callback, suffix);
        }
        else
        {
            // 没有此图片
        }
    }

    public void GetSpriteAsync(string spriteName, Action<Sprite> callback, string suffix = "")
    {
        if (!string.IsNullOrEmpty(spriteName))
        {
            AtlasConfigInfoForBytes atlasConfig = GetAtlasConfigBySpriteName(spriteName, suffix);
            GetSpriteAsync(spriteName, atlasConfig, callback, suffix);
        }
    }

    private async void GetSpriteSync(string spriteName, AtlasConfigInfoForBytes atlasConfig, Action<Sprite> callback, string suffix = "")
    {
        if (atlasConfig != null)
        {
            Sprite sprite = GetSpriteAtCache(spriteName);
            if (sprite != null)
            {
                callback?.Invoke(sprite);
            }
            else
            {
                SpriteAtlas spriteAtlas = await LoadAtlas(atlasConfig.AtlasName);
                if (spriteAtlas != null)
                {
                    callback?.Invoke(spriteAtlas.GetSprite(spriteName));
                }
            }
        }
    }
    private void GetSpriteAsync(string spriteName, AtlasConfigInfoForBytes atlasConfig, Action<Sprite> callback, string suffix = "")
    {
        if (atlasConfig != null)
        {

            Sprite sprite = GetSpriteAtCache(spriteName);
            if (sprite != null)
            {
                callback?.Invoke(sprite);
            }
            else
            {
                ResourcesManager.Instance.LoadAssetAsync(atlasConfig.AtlasPath, (_spriteAtlas) =>
                {
                    if (_spriteAtlas != null)
                    {
                        SpriteAtlas sa = (SpriteAtlas)_spriteAtlas;
                        AddToDynamicSpriteAtlas(atlasConfig.AtlasName, (SpriteAtlas)_spriteAtlas);
                        callback?.Invoke(sa.GetSprite(spriteName));
                    }
                });
            }
        }
    }

    public bool IsLoadedAtlas(string atlasName)
    {
        if (!string.IsNullOrEmpty(atlasName))
        {

        }
        return false;
    }
}
