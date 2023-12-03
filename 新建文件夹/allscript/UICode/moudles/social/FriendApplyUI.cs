using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FriendApplyUI : FriendListUIBase
{
    public override string LIST_TYPE => "FriendApply";
    public bool B_INIT { get; set; }

    private GameObject m_root { get; set; }
    private ScrollRect m_listsr { get; set; }
    private TableView m_tableView { get; set; }
    private TableItem m_tableItem { get; set; }
    private List<ContactListItem> m_contactListItems { get; set; }
    private ContactListItem m_currSelectItem { get; set; }
    public override GameObject root { get => m_root; set => m_root = value; }
    public override ScrollRect listsr { get => m_listsr; set => m_listsr = value; }
    public override TableView tableView { get => m_tableView; set => m_tableView = value; }
    public override TableItem tableItem { get => m_tableItem; set => m_tableItem = value; }
    public override List<ContactListItem> contactListItems { get => m_contactListItems; set => m_contactListItems = value; }
    public override ContactListItem currSelectItem { get => m_currSelectItem; set => m_currSelectItem = value; }


    public override void OnInit(GameObject root)
    {
        B_INIT = false;
        base.OnInit(root);
    }

    public override void InitListData(Action<bool> callback)
    {
        List<ContactData> datas = FriendManager.Instance.GetContactDatas(EnumContactType.Apply);
        if (datas != null && datas.Count > 0)
        {
            base.InitListItems(datas, callback);
            B_INIT = true;
        }
    }

    protected override ContactListItem GetMailItemByIndex(int index)
    {
        return base.GetMailItemByIndex(index);
    }

    protected override void OnItemRender(GameObject obj, int index)
    {
        base.OnItemRender(obj, index);
    }

    protected override void OnItemDispose()
    {
        base.OnItemDispose();
    }
}