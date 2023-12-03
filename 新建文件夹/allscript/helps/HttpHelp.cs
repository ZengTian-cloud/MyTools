using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using LitJson;
using UnityEngine;
using Managers;

public static class HttpHelp
{

	public static string initBasicParams(JsonData data) {

		JsonData parsams = new JsonData();
		parsams["channelid"] = GameCenter.mIns.gameInfo.ChannelId;
		parsams["deviceid"] = GameCenter.mIns.gameInfo.Deviceid;
		parsams["os"] = GameCenter.mIns.gameInfo.Os;
		parsams["lan"] = GameCenter.mIns.gameInfo.Lan;
		parsams["hv"] = GameCenter.mIns.gameInfo.Hv;

		if (GameCenter.mIns.userInfo.Token != null) {
			parsams["token"] = GameCenter.mIns.userInfo.Token;
		}
		if (GameCenter.mIns.userInfo.Uid != null)
		{
			parsams["uuid"] = GameCenter.mIns.userInfo.Uid;
		}
		if (GameCenter.mIns.userInfo.RoleId != null)
		{
			parsams["roleid"] = GameCenter.mIns.userInfo.RoleId;
		}
		if (GameCenter.mIns.userInfo.ZoneId != 0)
		{
			parsams["zoneid"] = GameCenter.mIns.userInfo.ZoneId;
		}
		if (GameCenter.mIns.userInfo.Group != null)
		{
			parsams["group"] = GameCenter.mIns.userInfo.Group;
		}

		string strParams = "";
		string gap = "";

		List<string> das = parsams.Keys.ToList();
		for (int i = 0; i < das.Count; i++) {
			string key = das[i];
			strParams += contactstr(key, parsams[key].ToString(), gap);
			gap = "&";
		}

		string jsonstr = JsonMapper.ToJson(data);
		Debug.Log("请求的真实参数："+jsonstr);
		if (nettool.useAes)
		{
			jsonstr = nettool.EncodeAES(jsonstr, nettool.aesKey, nettool.aesIV);
		}
		else {
			jsonstr = nettool.encodestring( nettool.doescapeurl(jsonstr));
		}

		return String.Format("{0}{1}encryptdata={2}",strParams, gap, jsonstr);
	}

	/// <summary>
	/// 初始加密参数
	/// </summary>
	/// <returns></returns>
	public static JsonData initUserParams() {

		JsonData parsams = new JsonData();
		parsams["roleid"] = GameCenter.mIns.userInfo.RoleId;
		parsams["zoneid"] = GameCenter.mIns.userInfo.ZoneId;
		return parsams;
	}

	private static string contactstr(string key,string val,string gap) {
		return String.Format("{0}{1}={2}", gap, key, val);
	}

	public static void sendData(string url,JsonData data,Action<int, JsonData, int,string> tcb)
	{
		string tcontent = initBasicParams(data);
		Debug.Log("请求网络数据:url->"+url+ "?" + tcontent);
		
		GameCenter.mIns.m_HttpMgr.REPORT(url, tcontent, (status,content,val)=> {
			if (status == 200)
			{
				Debug.Log("请求 返回:" + content);
				//请求成功
				JsonData data = JsonMapper.ToObject(new JsonReader(content));
				int code = data["code"].ToInt32();
				if (code == 1)
				{
					JsonData contData = data["data"];
					tcb?.Invoke(status, contData, code, "");
				}
				else
				{
					string msg = data["msg"].ToString();
					tcb?.Invoke(status, null, code, msg);
					Debug.LogError(data["msg"]);
				}
			}
			else if (status == 1002) {
				//没有网络
				// GameCenter.mIns.
				GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
				{
					string title = GameCenter.mIns.m_LanMgr.GetLan("common_confirm_error");
                    string txt = GameCenter.mIns.m_LanMgr.GetLan("common_not_error");
                    GameCenter.mIns.m_UIMgr.PopWindowPrefab(new PopWinStyle(title, txt, 1, (confirm) => {
                        GameCenter.mIns.ExitGame();
                    }));
                });
            }
			else
			{
				//请求出错了，，，，
				Debug.LogError("数据请求失败！code:" + status);
				tcb?.Invoke(status, null, 0, null);
			}
		});
	}



}
