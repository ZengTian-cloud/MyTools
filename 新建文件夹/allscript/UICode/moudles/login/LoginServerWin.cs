using Codice.Client.BaseCommands.BranchExplorer.ExplorerData;
using Codice.Client.BaseCommands.Merge;
using LitJson;
using Spine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LoginServerWin : BaseWindow
{
    private BaseWin _win;
    private GameObject _Root;
    public override string Prefab => "serverlist";
    public override string uiAtlasName => "";

    private GameObject serverlistObj;
    private GameObject serverItem;
    private ExButton btnOk;
    private ExButton btnCancle;
    private JsonData _data;
    private JsonData currZone;

    private Action<JsonData> onAction;
    private Action cancelAction;

    private List<ServerItem> zodeList = new List<ServerItem>();

    public LoginServerWin(JsonData zones, Action<JsonData> onAction, Action cancelAction) { 
        this._data=zones;
        this.onAction = onAction;
        this.cancelAction = cancelAction;
    }
    public override void initUI(BaseWin win,GameObject root)
    {
        this._win = win;
        this._Root = root;
        serverlistObj = _Root.transform.Find("list").gameObject;
        serverItem = _Root.transform.Find("list/serveritem").gameObject;
        btnOk = _Root.transform.Find("btn_ok").GetComponent<ExButton>();
        btnCancle = _Root.transform.Find("btn_no").GetComponent<ExButton>();
        btnOk.onClick.RemoveAllListeners();
        btnOk.onClick.AddListener(OnBtnOk);
        btnCancle.onClick.RemoveAllListeners();
        btnCancle.onClick.AddListener(OnBtnCancle);

        loadServerList();
    }

    public void loadServerList()
    {
        GameObject propori = serverItem.gameObject;
        propori.SetActive(false);
        //加载材料
        foreach (JsonData item in _data)
        {
            GameObject cloneObj = GameObject.Instantiate(propori);

            ServerItem server = new ServerItem(serverlistObj, cloneObj, item, (zone) => {
                
                onItemClick(zone);
            });
            zodeList.Add(server);
        }
        zodeList[0].onClick();
    }

    public void onItemClick(JsonData zone)
    {
        this.currZone = zone;
        foreach (var item in zodeList)
        {
            item.unselect();
        }
    }

    public void OnBtnCancle() {
        _win.Close();
        cancelAction?.Invoke();
    }
    public void OnBtnOk()
    {
        onAction?.Invoke(currZone);
        _win.Close();
    }
}

