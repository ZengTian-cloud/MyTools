using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// UI特效管理
/// </summary>
public class UIEffectManager : SingletonNotMono<UIEffectManager>
{
    public async void LoadUIEffect(string name,Action<GameObject> callBack)
    {
        //TODO: 资源没有进行分类处理  后面把特效单独分出来 减少Resources中的查找操作
        GameObject go = await ResourcesManager.Instance.LoadAssetSyncByName($"{name}.prefab");

        callBack?.Invoke(go);
    }
}

