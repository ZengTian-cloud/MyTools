using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public partial class CommonPopWinPrefabUI : BaseWin
{

    public override string Prefab => "window";
    public GameObject _Root;
    public GameObject _Content;
    public TextMeshProUGUI txtTitle;
    public CommonPopWinPrefabUI()
    {
    }
    protected override void InitUI()
    {
        _Root = uiRoot;
        txtTitle = _Root.transform.Find("imgBg/txTitle").GetComponent<TextMeshProUGUI>();
        _Content = _Root.transform.Find("content").gameObject;
        
    }
}

