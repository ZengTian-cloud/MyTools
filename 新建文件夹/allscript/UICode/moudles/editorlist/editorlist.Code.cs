using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UIManager;

public partial class editorlist
{
    public override UILayerType uiLayerType => UILayerType.Normal;
    public override string uiAtlasName => "";

    public List<editorData> editorDatas = new List<editorData>
    {
        new editorData{ name = "mapeditor",desc = "地图编辑器",goFunc = ()=>{ GameCenter.mIns.m_UIMgr.Open<mapeditor>(); } },
        new editorData{ name = "missioneditor",desc = "关卡编辑器",goFunc = ()=>{ GameCenter.mIns.m_UIMgr.Open<missioneditor>(); } },
        // new editorData{ name = "battletest",desc = "战斗测试",goFunc = ()=>{ GameCenter.mIns.m_BattleMgr.RequstBattle(101001); } },
        new editorData{ name = "battletest",desc = "英雄编辑器",goFunc = ()=>{ GameCenter.mIns.m_UIMgr.Open<EditorHero>(); } },
    };



    protected override void OnInit()
    {
        RefreshEditorList();
    }

    //刷新编辑器列表
    private void RefreshEditorList()
    {
        for (int i = 0; i < editorDatas.Count; i++)
        {
           editorData oneData = editorDatas[i];
           Button oneBtn = GameObject.Instantiate(btn_tool, toolList.transform);
            oneBtn.gameObject.SetActive(true);
            oneBtn.gameObject.name = oneData.name;
            oneBtn.GetComponentInChildren<Text>().text = oneData.desc;
            oneBtn.onClick.AddListener(()=> {
                oneData.goFunc?.Invoke();
            } );
        }
    }

    partial void btn_back_Click()
    {
        this.Close();
        GameCenter.mIns.m_UIMgr.Open<login>();
    }
}

//编辑器数据
public class editorData
{
    public string name;

    public string desc;

    public UnityAction goFunc;
}


