using System.Collections;
using Basics;
using UnityEngine;

namespace Managers
{
	public class HotUpdateManager : SingletonOjbect
	{
		[HideInInspector]
		public bool bClearUpRes = false;

		public override IEnumerator LaunchOne()
		{
			if (bClearUpRes)
			{
				bClearUpRes = false;
				ClearUpResource();
			}
			yield return base.LaunchOne();
		}

		public IEnumerator LoadVersionData()
		{
#if UNITY_EDITOR && !KZC_AssetBundle
			zxlogger.log(string.Format("Editor模式第{0}次运行游戏", zxlogic.logincount));
			zxversion.languagestr = "cn|en|tw|fr|gr|ru|jp|kr|pt|es|it";
			yield return null;
#else

			zxlogger.log(string.Format("AssetBundle模式第{0}次运行游戏", zxlogic.logincount));
			string verpath = pathtool.combine(pathtool.reqabpackpath, zxconfig.recordfile);
			var trequest = UnityEngine.Networking.UnityWebRequest.Get(verpath);
			yield return trequest.SendWebRequest();
			byte[] verbytes = trequest.downloadHandler.data;
			trequest.Dispose();
			verbytes = filetool.unencry(verbytes);
			var temJson = jsontool.newwithbytes(verbytes);

			zxversion.splitpackage = temJson["split"].ToBoolean();
			zxversion.hotupdate = temJson["canupdate"].ToBoolean();
			zxversion.languagestr = temJson["lanstr"].ToString();
			zxversion.packageversion = temJson["ver"].ToInt32();

			string trespath = pathtool.combine(pathtool.loadabrespath, zxconfig.recordfile);
			if (!bClearUpRes && filetool.exists(trespath))
			{
				byte[] resbytes = filetool.unencry(filetool.readallbytes(trespath));
				var resJson = jsontool.newwithbytes(resbytes);
				int resver = resJson["ver"].ToInt32();
				if (resver >= zxversion.packageversion)
				{
					temJson = resJson;
				}
				else
				{
					bClearUpRes = true;
				}
			}

			zxversion.resversion = temJson["ver"].ToInt32();
			//zxversion.lualistjson = temJson["lualist"];
			zxversion.reslistjson = temJson["reslist"];
			if (temJson.ContainsKey("audiolist"))
				zxversion.audiolistjson = temJson["audiolist"];
#endif

        }

        private void ClearUpResource()
		{
			pathtool.recreatedirectory(pathtool.loadabrespath);
		}
	}

}