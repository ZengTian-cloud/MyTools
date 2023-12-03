using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public partial class ChatUI : BaseWin
{
    public ChatUI() { }
    public override string Prefab => "chatui";

    public GameObject _Root;
    public Transform nodeLeft;

    public Button btnReturn;
    public Button btnSet;
    public TableView chatContactListTableView;
    public TableItem chatContactListTableItem;

    public Transform chatArea;
    public TMP_Text chatAreaTitleName;
    public Transform chatAreaBottom;
    public TMP_InputField inputChat;
    public Button btnVoice;
    public Button btnSend;
    public Button btnEmote;

    public Transform chatContentNode;
    public ScrollRect chatsr;
    // public TableView chatsrTableView;
    // public TableItem chatsrItem;

    public GameObject emoPanel;

    protected override void InitUI()
    {
        btns = new List<Button>();
        _Root = uiRoot;

        btnReturn = _Root.transform.Find("nodeLeft/btnReturn").GetComponent<Button>();
        chatContactListTableView = _Root.transform.Find("nodeLeft/chatContactList/chatContactListConetnt").GetComponent<TableView>();
        chatContactListTableItem = chatContactListTableView.transform.FindHideInChild("chatContactListItem").GetComponent<TableItem>();
        btnSet = _Root.transform.Find("nodeLeft/btnSet").GetComponent<Button>();

        chatArea = _Root.transform.Find("chatArea");
        chatAreaTitleName = chatArea.Find("imgTitleRoleName/titleRoleName").GetComponent<TMP_Text>();

        chatAreaBottom = chatArea.Find("bottom");
        inputChat = chatArea.Find("bottom/inputChat").GetComponent<TMP_InputField>();
        btnVoice = chatArea.Find("bottom/btnVoice").GetComponent<Button>();
        btnSend = chatArea.Find("bottom/btnSend").GetComponent<Button>();
        btnEmote = chatArea.Find("bottom/btnEmote").GetComponent<Button>();

        chatContentNode = chatArea.Find("content");
        chatsr = chatContentNode.Find("chatsr").GetComponent<ScrollRect>();
        // chatsrTableView = chatsr.transform.Find("content").GetComponent<TableView>();
        // chatsrItem = chatsrTableView.transform.FindHideInChild("chatItem").GetComponent<TableItem>();

        emoPanel = chatArea.FindHideInChild("emoPanel").gameObject;

        btnReturn.AddListenerBeforeClear(OnClickClose);
        btnSet.AddListenerBeforeClear(OnClickSet);
        btnVoice.AddListenerBeforeClear(OnClickVoice);
        btnSend.AddListenerBeforeClear(OnClickSend);
        btnEmote.AddListenerBeforeClear(OnClickEmote);
    }
    partial void OnClickClose();
    partial void OnClickSet();
    partial void OnClickVoice();
    partial void OnClickSend();
    partial void OnClickEmote();
}
