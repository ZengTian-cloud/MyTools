using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine;

class ZUText
{

		// 获取所有子节点文本组件
	private static List<Text> getcomponentsinchildren(int temid, string tempath)
	{
		var listcom = new List<Text>();
		var temobj = GameNode.GetChildGameObject(temid, tempath);
		if (temobj != null)
		{
			var vcam = temobj.GetComponentsInChildren<Text>(true);
			if (vcam != null)
			{
				listcom.AddRange(vcam);
			}
		}
		return listcom;
	}

	public static void clearchildren(int temid, string tempath)
	{
		var listut = getcomponentsinchildren(temid, tempath);
		for (var i = 0; i < listut.Count; i++)
		{
			listut[i].text = string.Empty;
		}
	}

	// 设置所有子节点文本置灰
	public static void setchildrengrey(int temid, string tempath, bool bgrey, bool offeffct)
	{
		//var listtext = getcomponentsinchildren(temid, tempath);
		//for (var i = 0; i < listtext.Count; i++)
		//{
		//	activetexteffect(temid, tempath, bgrey ? (bgrey && offeffct) : true);
		//	listtext[i].material = bgrey ? zxload.addsync("uishaders", "uiblack") as Material : null;
		//}
	}

	//开关所有文本的 texteffect
	public static void activetexteffect(int temid, string tempath, bool open)
	{
		List<Text> teffects = getcomponenttexteffectchildren(temid, tempath);
		for (var i = 0; i < teffects.Count; i++)
		{
			teffects[i].enabled = open;
		}
	}

	// 获取所有子节点文本组件
	private static List<Text> getcomponenttexteffectchildren(int temid, string tempath)
	{
		var listcom = new List<Text>();
		var temobj = GameNode.GetChildGameObject(temid, tempath);
		if (temobj != null)
		{
			var vcam = temobj.GetComponentsInChildren<Text>(true);
			if (vcam != null)
			{
				listcom.AddRange(vcam);
			}
		}
		return listcom;
	}
}
