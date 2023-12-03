using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerItem
{
    private ExButton btn;
    private Image imgBg;
    private Image imgBgSelect;
    private Image ImgSelect;
    private TextMeshProUGUI txtName;

    private JsonData zone;
    private GameObject _obj;
    private Action<JsonData> clickBack;
    //[{"zid":100020,"zoneid":1001,"beforezoneid":1001,"zonename":"1服","desc":"测试服","host":"47.96.123.103","port":60201,"apiurl":"","state":2,"zonenum":1,"newzone":1,"recommend":1,"starttime":1693497600000,"isreg":0,"isdebug":0,"isplacard":0},{"zid":100021,"zoneid":1002,"beforezoneid":1002,"zonename":"2服","desc":"王也服","host":"192.168.0.53","port":8200,"state":2,"zonenum":1,"newzone":1,"recommend":1,"starttime":1700755200000,"isreg":0,"isdebug":0,"isplacard":0}]
    public ServerItem(GameObject parent, GameObject obj,JsonData zone,Action<JsonData> clickBack) 
    {
        this._obj = obj;
        this.zone = zone;
        this.clickBack = clickBack;
        btn = obj.GetComponent<ExButton>();
        imgBg = obj.transform.Find("bg").GetComponent<Image>();
        imgBgSelect = obj.transform.Find("bg_select").GetComponent<Image>();
        ImgSelect = obj.transform.Find("unselect/select").GetComponent<Image>();
        txtName = obj.transform.Find("name").GetComponent<TextMeshProUGUI>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(onClick);

        txtName.text = $"{zone["zonename"].ToString()}  {zone["desc"].ToString()}";
        obj.gameObject.SetActive(true);
        obj.transform.SetParent(parent.transform);
        obj.transform.localScale = Vector3.one;
        RectTransform uiRect = obj.GetComponent<RectTransform>();
        uiRect.anchoredPosition = Vector2.zero;
        uiRect.localScale = Vector3.one;
        uiRect.anchorMax = new Vector2(1, 1);
        uiRect.anchorMin = new Vector2(0, 0);
        uiRect.pivot = new Vector2(0.5f, 0.5f);
        uiRect.anchoredPosition = Vector2.zero;
        uiRect.offsetMin = new Vector2(0, 0);
        uiRect.offsetMax = new Vector2(0, 0);
    }

    public void onClick()
    {
        clickBack?.Invoke(zone);
        select();
    }
    public void select()
    {
        imgBg.gameObject.SetActive(false);
        imgBgSelect.gameObject.SetActive(true);
        ImgSelect.gameObject.SetActive(true);
    }
    public void unselect() {
        imgBg.gameObject.SetActive(true);
        imgBgSelect.gameObject.SetActive(false);
        ImgSelect.gameObject.SetActive(false);
    }
}

