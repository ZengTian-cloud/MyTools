using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ResCheck
{
	public class AssetBundleRecord
	{
		public AssetBundle mAssetBundle { get; private set; }
		public int refCount { get; private set; }

		public AssetBundleRecord(AssetBundle temtab)
		{
			this.mAssetBundle = temtab;
			this.refCount = 1;
		}

		public void Add()
		{
			++this.refCount;
		}

		public void Release()
		{
			--this.refCount;
		}
	}

	public class AssetBundleLoad
	{
		private Dictionary<string, bool> _dictABLoading = new Dictionary<string, bool>();
		private Dictionary<string, AssetBundleRecord> _dictABLoadRecord = new Dictionary<string, AssetBundleRecord>();
		private Dictionary<string, Dictionary<string, Object>> _dictAssetRecord =
			new Dictionary<string, Dictionary<string, Object>>();

		public bool IsLoadAssetBundle(string tassetbundleName)
		{
			return GetABLoadRecord(tassetbundleName) != null;
		}

		public void LoadOneSyncAssetBundle(string tassetbundleName)
		{
			if (_dictABLoading.ContainsKey(tassetbundleName))
			{
				zxlogger.logerrorformat("{0}正在被异步加载, 同步加载失败" + tassetbundleName);
				return;
			}
			AssetBundleRecord abEx = GetABLoadRecord(tassetbundleName);
			if (abEx == null)
			{
#if UNITY_EDITOR && !KZC_AssetBundle
				abEx = new AssetBundleRecord(null);
#else
				string realassetBundleName = filetool.getfilenamemd5(tassetbundleName);
				string loadpath = pathtool.combine(pathtool.loadabrespath, realassetBundleName);
				if (!filetool.exists(loadpath))
				{
					loadpath = pathtool.combine(pathtool.loadabpackpath, realassetBundleName);
				}
				var temab = AssetBundle.LoadFromFile(loadpath, 0, filetool.assetoffset);
				if (temab != null)
				{
					abEx = new AssetBundleRecord(temab);
				}
#endif
			}
			AddCacheAssetBundle(tassetbundleName, abEx);
		}

		public IEnumerator LoadOneAsyncAssetBundle(string tassetbundleName)
		{
			while (_dictABLoading.ContainsKey(tassetbundleName))
			{
				yield return null;
			}
			_dictABLoading.Add(tassetbundleName, true);
			AssetBundleRecord abEx = GetABLoadRecord(tassetbundleName);
			if (abEx == null)
			{
#if UNITY_EDITOR && !KZC_AssetBundle
				abEx = new AssetBundleRecord(null);
#else
				string realassetBundleName = filetool.getfilenamemd5(tassetbundleName);
				string loadpath = pathtool.combine(pathtool.loadabrespath, realassetBundleName);
				if (!filetool.exists(loadpath))
				{
					loadpath = pathtool.combine(pathtool.loadabpackpath, realassetBundleName);
				}
				AssetBundleCreateRequest trequest = AssetBundle.LoadFromFileAsync(loadpath, 0, filetool.assetoffset);
				yield return trequest;
				if (trequest != null && trequest.assetBundle != null)
				{
					abEx = new AssetBundleRecord(trequest.assetBundle);
				}
#endif
			}
			AddCacheAssetBundle(tassetbundleName, abEx);
			_dictABLoading.Remove(tassetbundleName);
		}

		public IEnumerator LoadOneIntelligentAssetBundle(string tassetbundleName)
		{
			while (_dictABLoading.ContainsKey(tassetbundleName))
			{
				yield return null;
			}
			_dictABLoading.Add(tassetbundleName, true);
			AssetBundleRecord abEx = GetABLoadRecord(tassetbundleName);
			if (abEx == null)
			{
#if UNITY_EDITOR && !KZC_AssetBundle
				abEx = new AssetBundleRecord(null);
#else
				string realassetBundleName = filetool.getfilenamemd5(tassetbundleName);
				string loadpath = pathtool.combine(pathtool.loadabrespath, realassetBundleName);
				if (!filetool.exists(loadpath))
				{
					loadpath = pathtool.combine(pathtool.reqabpackpath, realassetBundleName);
				}
				else
				{
					loadpath = pathtool.combine(pathtool.reqabrespath, realassetBundleName);
				}
				using (var trequest = UnityEngine.Networking.UnityWebRequest.Get(loadpath))
				{
					yield return trequest.SendWebRequest();
					if (string.IsNullOrEmpty(trequest.error))
					{
						byte[] filebytes = filetool.unencry(trequest.downloadHandler.data);
						var temab = AssetBundle.LoadFromMemory(filebytes);
						if (temab != null)
						{
							abEx = new AssetBundleRecord(temab);
						}
					}
				}
#endif
			}
			AddCacheAssetBundle(tassetbundleName, abEx);
			_dictABLoading.Remove(tassetbundleName);
		}

		public void UnLoadAllLoadedAssetBundle()
		{
			while (_dictABLoadRecord.Count > 0)
			{
				var oneRecord = _dictABLoadRecord.GetEnumerator();
				oneRecord.MoveNext();
				UnLoadOneAssetBundle(oneRecord.Current.Key, true);
			}
		}

		public void UnLoadOneAssetBundle(string tassetbundleName, bool bforce)
		{
			if (!_dictABLoadRecord.ContainsKey(tassetbundleName))
			{
				zxlogger.logerrorformat("卸载了一个未加载的Bundle: {0}", tassetbundleName);
				return;
			}

			var unloadAB = _dictABLoadRecord[tassetbundleName];
			unloadAB.Release();
			if (unloadAB.refCount <= 0 || bforce)
			{
				if (unloadAB.mAssetBundle != null)
				{
					unloadAB.mAssetBundle.Unload(true);
				}
				_dictABLoadRecord.Remove(tassetbundleName);
				_dictAssetRecord.Remove(tassetbundleName);
				zxlogger.logformat("<color=#CD5C5C>{0}</color> 卸载成功", tassetbundleName);
			}
		}

		public void PreLoadOneAsset(string tassetbundleName)
		{
#if !(UNITY_EDITOR && !KZC_AssetBundle)
			if (_dictAssetRecord.ContainsKey(tassetbundleName))
			{
				AssetBundleRecord abEx = GetABLoadRecord(tassetbundleName);
				if (abEx == null)
				{
					return;
				}
				var allasset = abEx.mAssetBundle.GetAllAssetNames();
				for (int i = 0; i < allasset.Length; i++)
				{
					if (_dictAssetRecord[tassetbundleName].ContainsKey(allasset[i]))
					{
						continue;
					}
					Object temasset = abEx.mAssetBundle.LoadAsset(allasset[i]);
					if (temasset != null)
					{
						_dictAssetRecord[tassetbundleName].Add(allasset[i], temasset);
					}
				}
			}
#endif
		}

#if UNITY_EDITOR && !KZC_AssetBundle
		public Object LoadSimulateAsset(string effectpath, List<string> vabdepend)
		{
			for (int i = 0; i < vabdepend.Count; i++)
			{
				if (GetABLoadRecord(vabdepend[i]) == null)
				{
					zxlogger.logerrorformat("AssetBundle ({0}) 未加载, 加载资源 ({1}) 失败", vabdepend[i], effectpath);
					return null;
				}
			}
			Object tloadasset = null;
			if (zxcssuffix.ispicture(effectpath))
			{
				tloadasset = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(effectpath);
			}
			else if (zxcssuffix.isfont(effectpath))
			{
				tloadasset = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>(effectpath);
			}
			if (tloadasset == null)
			{
				tloadasset = UnityEditor.AssetDatabase.LoadAssetAtPath(effectpath, typeof(Object));
			}
			return tloadasset;
		}
#else
		public Object LoadOneSyncAsset(string tassetbundleName, string assetName, bool bsprite = false)
		{

			if (_dictAssetRecord.ContainsKey(tassetbundleName))
			{
				if (_dictAssetRecord[tassetbundleName].ContainsKey(assetName))
				{
					return _dictAssetRecord[tassetbundleName][assetName];
				}
				AssetBundleRecord abEx = GetABLoadRecord(tassetbundleName);
				if (abEx != null)
				{
					Object temasset = bsprite ? abEx.mAssetBundle.LoadAsset<Sprite>(assetName) : abEx.mAssetBundle.LoadAsset(assetName);
					if (temasset != null)
					{
						_dictAssetRecord[tassetbundleName].Add(assetName, temasset);
					}
					return temasset;
				}
			}
			return null;
		}

		public IEnumerator LoadOneAsyncAsset(Action<Object> tcallback, string tassetbundleName, string assetName, bool bsprite = false)
		{
			Object temasset = null;
			if (_dictAssetRecord.ContainsKey(tassetbundleName))
			{
				if (_dictAssetRecord[tassetbundleName].ContainsKey(assetName))
				{
					temasset = _dictAssetRecord[tassetbundleName][assetName];
				}
				else
				{
					AssetBundleRecord abEx = GetABLoadRecord(tassetbundleName);
					if (abEx != null)
					{
						AssetBundleRequest trequest = bsprite ? abEx.mAssetBundle.LoadAssetAsync<Sprite>(assetName) : abEx.mAssetBundle.LoadAssetAsync(assetName);
						yield return trequest;
						temasset = trequest.asset;
						if (temasset != null &&
							_dictAssetRecord.ContainsKey(tassetbundleName) &&
							!_dictAssetRecord[tassetbundleName].ContainsKey(assetName))
						{
							_dictAssetRecord[tassetbundleName].Add(assetName, temasset);
						}
					}
				}
			}
			tcallback?.Invoke(temasset);
		}
#endif

		private AssetBundleRecord GetABLoadRecord(string tassetbundleName)
		{
			if (_dictABLoadRecord.ContainsKey(tassetbundleName))
			{
				return _dictABLoadRecord[tassetbundleName];
			}
			return null;
		}

		private void AddCacheAssetBundle(string tassetbundleName, AssetBundleRecord abEx)
		{
			if (abEx != null)
			{
				if (_dictABLoadRecord.ContainsKey(tassetbundleName))
				{
					_dictABLoadRecord[tassetbundleName].Add();
				}
				else
				{
					_dictABLoadRecord.Add(tassetbundleName, abEx);
					_dictAssetRecord.Add(tassetbundleName, new Dictionary<string, Object>());
					zxlogger.logformat("<color=#43CD80>{0}</color> 加载成功", tassetbundleName);
				}
			}
			else
			{
				zxlogger.logerrorformat("文件有问题AssetBundle: {0} 加载失败", tassetbundleName);
			}
		}
	}
}