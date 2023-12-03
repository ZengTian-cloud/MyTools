using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LitJson;
using UnityEngine;

public class AssetBundleCheck
{
	private Dictionary<string, string[]> _dictMapping = new Dictionary<string, string[]>();
	private Dictionary<string, List<string>> _dictDepend = new Dictionary<string, List<string>>();
	private Dictionary<string, List<string>> _dictAllDep = new Dictionary<string, List<string>>();

	public IEnumerator Initialization()
	{
		_dictMapping.Clear();
		_dictDepend.Clear();
		_dictAllDep.Clear();
#if UNITY_EDITOR && !KZC_AssetBundle
		var vcheckpath = Directory.GetDirectories(Application.dataPath);
		for (int i = 0; i < vcheckpath.Length; i++)
		{
			if (!vcheckpath[i].EndsWith("StreamingAssets"))
			{
				GenerateEffectAssetBundle(vcheckpath[i]);
			}
		}
		UnityEditor.AssetDatabase.Refresh();

		yield return null;
#else
		yield return LoadFileManifest ();
#endif
	}

	public string[] GetDependencies(string abName, bool recursive)
	{
#if UNITY_EDITOR && !KZC_AssetBundle
		return GetDependsByABName(abName, recursive);
#else
		List<string> strablist = new List<string>();
		var temdep = recursive ? _dictAllDep : _dictDepend;
		if (temdep.ContainsKey(abName))
		{
			foreach (string str in temdep[abName])
			{
				if (!strablist.Contains(str) && !string.IsNullOrEmpty(str))
				{
					strablist.Add(str);
				}
			}
		}
		strablist.Add(abName);
		string[] vstrabname = strablist.ToArray();
		return vstrabname;
#endif
	}

	public string[] GetIsAssetBundle(string tassetbundleName, bool showlog)
	{
		if (_dictMapping.ContainsKey(tassetbundleName))
		{
			return _dictMapping[tassetbundleName];
		}
		if (showlog)
		{
			zxlogger.logerrorformat("不存在的AssetBundle: {0} 加载失败", tassetbundleName);
		}
		return null;
	}

#if UNITY_EDITOR && !KZC_AssetBundle
	public List<string> ChecAssetBundleDepends(string effectpath)
	{
		if (!_dictAllDep.ContainsKey(effectpath))
		{
			string[] vdepend = UnityEditor.AssetDatabase.GetDependencies(effectpath, true).Where(t =>
			  zxcssuffix.isassetbundle(t)
			).ToArray();
			for (int i = 0; i < vdepend.Length; i++)
			{
				vdepend[i] = zxsuffix.addassetbundle(Path.GetFileName(Path.GetDirectoryName(vdepend[i])));
			}
			_dictAllDep.Add(effectpath, new List<string>(vdepend));
		}
		return _dictAllDep[effectpath];
	}

	private bool GenerateEffectAssetBundle(string checkpath)
	{
		checkpath = checkpath.Replace("\\", "/");
		var listdir = new List<string>(Directory.GetDirectories(checkpath));
		for (int i = listdir.Count - 1; i >= 0; i--)
		{
			if (GenerateEffectAssetBundle(listdir[i]))
			{
				listdir.RemoveAt(i);
			}
		}
		var vfile = Directory.GetFiles(checkpath).Where(t =>
		  !Path.GetFileName(t).StartsWith(".") && !t.EndsWith(".meta")
		).ToArray();
		if (!checkpath.Contains("uiabmap") && listdir.Count <= 0 && vfile.Length == 0)
		{
			Directory.Delete(checkpath, true);
			File.Delete(checkpath + ".meta");
			zxlogger.logerrorformat("删除空的文件夹: {0}", checkpath);
			return true;
		}
		vfile = vfile.Where(t => zxcssuffix.isassetbundle(t)).ToArray();
		if (vfile.Length > 0)
		{
			for (int i = 0; i < vfile.Length; i++)
			{
				vfile[i] = vfile[i].Replace("\\", "/").Replace(Application.dataPath, "Assets");
			}
			var tassetbundleName = zxsuffix.addassetbundle(Path.GetFileName(checkpath));
			// TODO： 临时处理eff
			if (checkpath.Contains("effs_"))
			{
				int si = checkpath.IndexOf("effs_");
                int ei = checkpath.LastIndexOf("/");
				string tempABName = checkpath.Substring(si, checkpath.Length - si);
				if (tempABName.Contains("/"))
				{
                    tempABName = tempABName.Substring(0, tempABName.LastIndexOf("/"));
				}
				tassetbundleName = tempABName + ".dat";
            }

            for (int idx = 0; idx < zxconfig.vassetfolder.Length; idx++)
			{
				if (checkpath.Contains(zxconfig.vassetfolder[idx]))
				{
					if (_dictMapping.ContainsKey(tassetbundleName))
					{
                        if (checkpath.Contains("effs_"))
                            EffectAddFiles(tassetbundleName, vfile);

                        // TODO: 2023-7-12-17:40, 暂时屏蔽（后续打包修改） 
                        zxlogger.logformat("存在相同名字的资源文件夹: {0}", checkpath);
						//zxlogic.setunitypaused(true);
						return false;
					}
					if (checkpath.Contains("effs_"))
						EffectAddFiles(tassetbundleName, vfile);
					else
						_dictMapping.Add(tassetbundleName, vfile);
					break;
				}
			}
        }
        return false;
	}

	private void EffectAddFiles(string tassetbundleName, string[] vfile)
	{
		if (_dictMapping.ContainsKey(tassetbundleName))
		{
            List<string> oldArrList = _dictMapping[tassetbundleName].ToList();
            List<string> newArrList = vfile.ToList();
            foreach (var s in newArrList)
            {
                bool hasSame = false;
                foreach (var os in oldArrList)
                {
                    if (os.Equals(s))
                    {
                        hasSame = true;
                        break;
                    }
                }
                if (!hasSame)
                    oldArrList.Add(s);
            }
            _dictMapping[tassetbundleName] = oldArrList.ToArray();
        }
		else
		{
            _dictMapping.Add(tassetbundleName, vfile);
        }
    }

	private string[] GetDependsByABName(string tassetbundleName, bool recursive)
	{
		List<string> vlist = new List<string>();
		if (_dictMapping.ContainsKey(tassetbundleName))
		{
			foreach (var item in _dictMapping[tassetbundleName])
			{
				var vabdepend = CalcDependsByPath(item, recursive);
				foreach (var onedepend in vabdepend)
				{
					if (!vlist.Contains(onedepend))
					{
						vlist.Add(onedepend);
					}
				}
			}
		}
		return vlist.ToArray();
	}

	private List<string> CalcDependsByPath(string folderpath, bool recursive)
	{
		var temdep = recursive ? _dictAllDep : _dictDepend;
		if (!temdep.ContainsKey(folderpath))
		{
			var listDepend = new List<string>(UnityEditor.AssetDatabase.GetDependencies(folderpath, recursive));
			for (int i = listDepend.Count - 1; i >= 0; i--)
			{
				listDepend[i] = zxsuffix.addassetbundle(Path.GetFileName(Path.GetDirectoryName(listDepend[i])));
				if (!_dictMapping.ContainsKey(listDepend[i]))
				{
					listDepend.RemoveAt(i);
				}
			}
			temdep.Add(folderpath, listDepend);
		}
		return temdep[folderpath];
	}
#else
	public IEnumerator LoadFileManifest()
	{
		var luaAssetBundleJson = zxversion.lualistjson;
		for (int i = 0; i < luaAssetBundleJson.Count; i++)
		{
			string strAssetName = luaAssetBundleJson[i][0].ToString();
			if (!_dictMapping.ContainsKey(strAssetName))
			{
				_dictMapping.Add(strAssetName, new string[] { });
			}
		}
		yield return LoadTargetManifest(zxconfig.resmanifest);
		yield return LoadTargetManifest(zxconfig.unmanifest);
	}

	private IEnumerator LoadTargetManifest(string maniName)
	{
		AssetBundle ab = null;
		string md5Name = filetool.getfilenamemd5(zxsuffix.addassetbundle(maniName));
		string loadpath = pathtool.combine(pathtool.loadabrespath, md5Name);
		if (!filetool.exists(loadpath))
		{
			loadpath = pathtool.combine(pathtool.reqabpackpath, md5Name);
		}
		else
		{
			loadpath = pathtool.combine(pathtool.reqabrespath, md5Name);
		}
		using (var trequest = UnityEngine.Networking.UnityWebRequest.Get(loadpath))
		{
			yield return trequest.SendWebRequest();
			if (string.IsNullOrEmpty(trequest.error))
			{
				byte[] filebytes = filetool.unencry(trequest.downloadHandler.data);
				ab = AssetBundle.LoadFromMemory(filebytes);
			}
		}
		if (ab == null)
		{
			yield break;
		}
		var tmanifest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
		if (tmanifest == null)
		{
			zxlogger.logerror("can't find AssetBundleManifest.");
			yield break;
		}
		string[] strAllAssetBundleArray = tmanifest.GetAllAssetBundles();
		for (int i = 0; i < strAllAssetBundleArray.Length; i++)
		{
			string strAssetName = strAllAssetBundleArray[i];
			if (!_dictMapping.ContainsKey(strAssetName))
			{
				_dictMapping.Add(strAssetName, new string[] { });
			}
			if (!_dictDepend.ContainsKey(strAssetName))
			{
				var listDepend = new List<string>();
				_dictDepend.Add(strAssetName, listDepend);
				string[] strAllDependenciesArray = tmanifest.GetDirectDependencies(strAssetName);
				for (int j = 0; j < strAllDependenciesArray.Length; j++)
				{
					listDepend.Add(strAllDependenciesArray[j]);
				}
			}
			if (!_dictAllDep.ContainsKey(strAssetName))
			{
				var listDepend = new List<string>();
				_dictAllDep.Add(strAssetName, listDepend);
				string[] strAllDependenciesArray = tmanifest.GetAllDependencies(strAssetName);
				for (int j = 0; j < strAllDependenciesArray.Length; j++)
				{
					listDepend.Add(strAllDependenciesArray[j]);
				}
			}
		}
		zxlogger.logformat("<color=#43CD80>{0}</color> Manifest加载成功", maniName);
		ab.Unload(true);
	}
#endif
}