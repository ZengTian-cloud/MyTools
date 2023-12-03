using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using LitJson;

public class conditionItem
{
	public GameObject curRoot;
	public TextMeshProUGUI name;
	public Image line;
	public Image icon;

	private int curModel;
	public conditionItem(GameObject item,Transform parent,int model)
	{
		this.curModel = model;

        this.curRoot = item;
		this.name = item.transform.Find("name").GetComponent<TextMeshProUGUI>();
		this.line = item.transform.Find("line").GetComponent<Image>();
		this.icon = item.transform.Find("icon").GetComponent<Image>();
		this.curRoot.transform.SetParent(parent);
		this.curRoot.transform.localScale = Vector3.one;
    }

	public void RefreshCondition(JsonData jsonData)
	{
		string condionText = string.Empty;
		int count = 0;
		int total = 0;
		if (jsonData.ContainsKey("name"))
            condionText = GameCenter.mIns.m_LanMgr.GetLan(jsonData["name"].ToString());

		if (jsonData.ContainsKey("count"))
			count = int.Parse(jsonData["count"].ToString());

        if (jsonData.ContainsKey("total"))
            total = int.Parse(jsonData["total"].ToString());

		condionText = $"{condionText}({count}/{total})";
		this.name.text = condionText;

		this.line.gameObject.SetActive(count >= total);
		this.icon.gameObject.SetActive(count >= total);

		if (count >= total)
		{
			icon.sprite = SpriteManager.Instance.GetSpriteSync("ui_renwu_icon_gou");

        }
		else
		{
            switch (this.curModel)
            {
				case 1:
                    icon.sprite = SpriteManager.Instance.GetSpriteSync("ui_s_icon_zhuxian");
                    break;
				case 2:
                    icon.sprite = SpriteManager.Instance.GetSpriteSync("ui_renwu_icon_gou");
                    break;
				case 3:
                    icon.sprite = SpriteManager.Instance.GetSpriteSync("ui_s_icon_tansuo");
                    break;
				case 4:
                    icon.sprite = SpriteManager.Instance.GetSpriteSync("ui_s_icon_tongban");
                    break;
                default:
                    break;
            }
        }
		this.curRoot.SetActive(true);


    }

}

