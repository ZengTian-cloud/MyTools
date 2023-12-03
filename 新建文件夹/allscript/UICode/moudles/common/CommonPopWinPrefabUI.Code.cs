using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Codice.CM.WorkspaceServer.WorkspaceTreeDataStore;
using UnityEditor.PackageManager.UI;

public partial class CommonPopWinPrefabUI
{
    public override UILayerType uiLayerType => UILayerType.Pop;
    public override string uiAtlasName => "pop";

    protected override void OnInit()
    {
    }

    public async void init(string title,BaseWindow window) {
        txtTitle.text = title;
        GameObject uiroot = await ResourcesManager.Instance.LoadUIPrefabAndAtlasSync(window.Prefab, window.uiAtlasName);
        if (uiroot != null)
        {
            uiroot.transform.SetParent(_Root.transform);
            uiroot.transform.localScale = Vector3.one;
            RectTransform uiRect = uiroot.GetComponent<RectTransform>();
            uiRect.anchoredPosition = Vector2.zero;
            uiRect.localScale = Vector3.one;
            uiRect.anchorMax = new Vector2(1, 1);
            uiRect.anchorMin = new Vector2(0, 0);
            uiRect.pivot = new Vector2(0.5f, 0.5f);
            uiRect.anchoredPosition = Vector2.zero;
            uiRect.offsetMin = new Vector2(0, 0);
            uiRect.offsetMax = new Vector2(0, 0);
            window.initUI(this,uiroot);
        }
    }
}
