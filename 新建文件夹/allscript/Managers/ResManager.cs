//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using Basics;
//using ResCheck;
//using UnityEngine;
//using UnityEngine.Networking;
//using Object = UnityEngine.Object;

//namespace Managers
//{
//	public class ResManager : SingletonOjbect
//	{
//		private AssetBundleCheck mABCheck = null;
//		private AssetBundleLoad mABLoad = null;

//		public override IEnumerator LaunchOne()
//		{
//			UnLoadAllAssetBundle();
//			yield return mABCheck.Initialization();
//		}

//		public void DoCaptureScreen(Action<Texture2D> tcallback)
//		{
//			StartCoroutine(StartCaptureScreen(tcallback));
//		}

//		public void DoCaptureCamera(Action<Texture2D> tcallback, string savepath, params int[] vcamtag)
//		{
//			if (vcamtag != null && vcamtag.Length > 0)
//			{
//				var listcam = new List<Camera>();
//				for (var i = 0; i < vcamtag.Length; i++)
//				{
//					var oneobj = GameNode.GetGameObject(vcamtag[i]);
//					if (oneobj)
//					{
//						var onecam = oneobj.GetComponent<Camera>();
//						if (onecam != null && oneobj.activeSelf && onecam.enabled)
//						{
//							listcam.Add(onecam);
//						}
//					}
//				}
//				if (listcam.Count > 0)
//				{
//					listcam.Sort(delegate (Camera tcama, Camera tcamb)
//					{
//						return tcama.depth < tcamb.depth ? -1 : 1;
//					});
//					StartCoroutine(StartCaptureCamera(tcallback, listcam, savepath));
//					return;
//				}
//			}
//			tcallback?.Invoke(null);
//		}

//		public void LoadServerTexture(string turl, Action<Object> tcallback)
//		{
//			StartCoroutine(StartLoadTexture(turl, tcallback));
//		}

//		public bool CheckIsAssetBundle(params string[] vName)
//		{
//			if (vName != null)
//			{
//				for (var i = 0; i < vName.Length; i++)
//				{
//					string tassetbundleName = zxsuffix.addassetbundle(vName[i]);
//					if (mABCheck.GetIsAssetBundle(tassetbundleName, false) == null)
//					{
//						return false;
//					}
//				}
//			}
//			return true;
//		}

//		public bool CheckLoadAssetBundle(params string[] vName)
//		{
//			if (vName != null)
//			{
//				for (var i = 0; i < vName.Length; i++)
//				{
//					string tassetbundleName = zxsuffix.addassetbundle(vName[i]);
//					if (!mABLoad.IsLoadAssetBundle(tassetbundleName))
//					{
//						return false;
//					}
//				}
//			}
//			return true;
//		}

//		public string[] CheckDependencies(string oneName, bool recursive)
//		{
//			string tassetbundleName = zxsuffix.addassetbundle(oneName);
//			if (mABCheck.GetIsAssetBundle(tassetbundleName, true) == null)
//			{
//				return null;
//			}
//			var vdepend = mABCheck.GetDependencies(tassetbundleName, recursive);
//			for (int i = 0; i < vdepend.Length; i++)
//			{
//				vdepend[i] = zxsuffix.delassetbundle(vdepend[i]);
//			}
//			return vdepend;
//		}

//		public string LoadSyncAssetBundle(ArrayList vName)
//		{
//			string errstr = string.Empty;
//			if (vName != null)
//			{
//				for (var i = 0; i < vName.Count; i++)
//				{
//					string tassetbundleName = zxsuffix.addassetbundle((string)vName[i]);
//					if (mABCheck.GetIsAssetBundle(tassetbundleName, true) == null)
//					{
//						errstr = string.Format("{0}{1};", errstr, tassetbundleName);
//						continue;
//					}
//					mABLoad.LoadOneSyncAssetBundle(tassetbundleName);
//				}
//			}
//			return errstr;
//		}

//		public void LoadAsyncAssetBundle(Action<string> tcallback, ArrayList vName)
//		{
//			StartCoroutine(LoadCoroutineAsyncAssetBundle(tcallback, vName));
//		}

//		public void LoadIntelligentAssetBundle(Action<string> tcallback, ArrayList vName)
//		{
//			StartCoroutine(LoadCoroutineIntelligentAssetBundle(tcallback, vName));
//		}

//		public void UnLoadAssetBundles(bool bforce, params string[] vName)
//		{
//			if (vName != null)
//			{
//				for (var i = 0; i < vName.Length; i++)
//				{
//					string tassetbundleName = zxsuffix.addassetbundle(vName[i]);
//					if (mABCheck.GetIsAssetBundle(tassetbundleName, true) == null)
//					{
//						continue;
//					}
//					mABLoad.UnLoadOneAssetBundle(tassetbundleName, bforce);
//				}
//			}
//		}

//		public void UnLoadAssetBundles(bool bforce, string abName)
//		{
//				string tassetbundleName = zxsuffix.addassetbundle(abName);
//				if (mABCheck.GetIsAssetBundle(tassetbundleName, true) == null)
//				{
//					return;
//				}
//				mABLoad.UnLoadOneAssetBundle(tassetbundleName, bforce);
//		}

//		public void UnLoadAllAssetBundle()
//		{
//			mABLoad.UnLoadAllLoadedAssetBundle();
//		}

//		public void PreLoadAssets(params string[] vName)
//		{
//			if (vName != null)
//			{
//				for (var i = 0; i < vName.Length; i++)
//				{
//					string tassetbundleName = zxsuffix.addassetbundle(vName[i]);
//					if (mABCheck.GetIsAssetBundle(tassetbundleName, true) == null)
//					{
//						continue;
//					}
//					mABLoad.PreLoadOneAsset(tassetbundleName);
//				}
//			}
//		}

//		public Object LoadResourceRes(string assetpath)
//		{
//			return Resources.Load(assetpath);
//		}

//		public Object LoadSyncRes(string oneName, string assetName, bool bsprite = false)
//		{
//			string tassetbundleName = zxsuffix.addassetbundle(oneName);
//#if UNITY_EDITOR && !KZC_AssetBundle
//			return CheckSimulateAsset(tassetbundleName, assetName);
//#else
//			return mABLoad.LoadOneSyncAsset (tassetbundleName, assetName, bsprite);
//#endif
//		}

//		public void LoadAsyncRes(string oneName, string assetName, Action<Object> tcallback, bool bsprite = false)
//		{
//			StartCoroutine(LoadCoroutineAsyncRes(tcallback, oneName, assetName, bsprite));
//		}

//		public GameObject LoadRootResourcePrefab(string assetpath)
//		{
//			GameObject temobj = null;
//			temobj = Resources.Load<GameObject>(assetpath);
//			if (temobj != null)
//			{
//				temobj = Object.Instantiate(temobj);
//				temobj.name = pathtool.getfilename(assetpath);
//				ResetGameObject(temobj);
//			}
//			return temobj;
//		}

//		public GameObject LoadResourcePrefab(GameObject parentobj, string assetpath)
//		{
//			GameObject temobj = null;
//			if (parentobj != null)
//			{
//				temobj = Resources.Load<GameObject>(assetpath);
//				if (temobj != null)
//				{
//					temobj = Object.Instantiate(temobj);
//					temobj.name = pathtool.getfilename(assetpath);
//					temobj.transform.SetParent(parentobj.transform, false);
//					ResetGameObject(temobj);
//				}
//			}
//			return temobj;
//		}

//		public GameObject LoadGameObjectPrefab(GameObject parentobj, GameObject childobj)
//		{
//			GameObject temobj = null;
//			if (childobj != null && parentobj != null)
//			{
//				temobj = Object.Instantiate(childobj);
//				temobj.name = childobj.name;
//				temobj.transform.SetParent(parentobj.transform, false);
//				ResetGameObject(temobj);
//			}
//			return temobj;
//		}

//		public GameObject LoadRootSyncPrefab(string oneName, string assetName)
//		{
//			GameObject temobj = null;
//			temobj = LoadSyncRes(oneName, assetName) as GameObject;
//			if (temobj != null)
//			{
//				temobj = Object.Instantiate(temobj);
//				temobj.name = assetName;
//				ResetGameObject(temobj);
//			}
//			return temobj;
//		}

//		public GameObject LoadSyncPrefab(GameObject parentobj, string oneName, string assetName)
//		{
//			GameObject temobj = null;
//			if (parentobj != null)
//			{
//				temobj = LoadSyncRes(oneName, assetName) as GameObject;
//				if (temobj != null)
//				{
//					temobj = Object.Instantiate(temobj);
//					temobj.name = assetName;
//					temobj.transform.SetParent(parentobj.transform, false);
//					ResetGameObject(temobj);
//				}
//			}
//			return temobj;
//		}

//		public void LoadAsyncPrefab(GameObject parentobj, string oneName, string assetName, Action<GameObject> tcallback)
//		{
//			LoadAsyncRes(oneName, assetName, delegate (Object temasset)
//			{
//				GameObject temobj = temasset as GameObject;
//				if (temobj != null)
//				{
//					if (parentobj != null)
//					{
//						temobj = Object.Instantiate(temobj);
//						temobj.name = assetName;
//						temobj.transform.SetParent(parentobj.transform, false);
//						ResetGameObject(temobj);
//					}
//					else
//					{
//						temobj = null;
//					}
//				}
//				tcallback?.Invoke(temobj);
//			});
//		}

//		private IEnumerator StartCaptureScreen(Action<Texture2D> tcallback)
//		{
//			yield return new WaitForEndOfFrame();
//			int twidth = zxconfig.screenwidth;
//			int theight = zxconfig.screenheight;
//			Texture2D screenShot = new Texture2D(twidth, theight, TextureFormat.RGB24, false);
//			screenShot.wrapMode = TextureWrapMode.Clamp;
//			screenShot.ReadPixels(new Rect(0, 0, twidth, theight), 0, 0, false);
//			screenShot.Apply();
//			tcallback?.Invoke(screenShot);
//		}

//		private IEnumerator StartCaptureCamera(Action<Texture2D> tcallback, List<Camera> listcam, string savepath = null)
//		{
//			yield return new WaitForEndOfFrame();

//			int twidth = zxconfig.screenwidth;
//			int theight = zxconfig.screenheight;
//			// 创建一个RenderTexture对象
//			var trt = new RenderTexture(twidth, theight, 24);
//			for (var i = 0; i < listcam.Count; i++)
//			{
//				if (listcam[i] != null)
//				{
//					listcam[i].targetTexture = trt;
//					listcam[i].Render();
//				}
//			}
//			// 激活这个rt, 并从中中读取像素。
//			RenderTexture.active = trt;
//			var screenShot = new Texture2D(twidth, theight, TextureFormat.RGB24, false);
//			// 注: 这个时候，它是从RenderTexture.active中读取像素
//			screenShot.wrapMode = TextureWrapMode.Clamp;
//			screenShot.ReadPixels(new Rect(0, 0, twidth, theight), 0, 0);
//			screenShot.Apply();
//			for (var i = 0; i < listcam.Count; i++)
//			{
//				if (listcam[i] != null)
//				{
//					listcam[i].targetTexture = null;
//				}
//			}
//			RenderTexture.active = null;
//			GameObject.Destroy(trt);
//			tcallback?.Invoke(screenShot);
//			if (!string.IsNullOrEmpty(savepath))
//			{
//				byte[] bytes = screenShot.EncodeToPNG();
//				filetool.writeallbytes(savepath, bytes);
//			}
//		}

//		private IEnumerator StartLoadTexture(string turl, Action<Object> tcallback)
//		{
//			Object temtexture = null;
//			using (UnityWebRequest trequest = UnityWebRequestTexture.GetTexture(turl))
//			{
//				yield return trequest.SendWebRequest();
//				if (string.IsNullOrEmpty(trequest.error))
//				{
//					temtexture = DownloadHandlerTexture.GetContent(trequest);
//				}
//			}
//			tcallback?.Invoke(temtexture);
//		}

//		private IEnumerator LoadCoroutineAsyncAssetBundle(Action<string> tcallback, ArrayList vName)
//		{
//			string errstr = string.Empty;
//			if (vName != null)
//			{
//				for (var i = 0; i < vName.Count; i++)
//				{
//					string tassetbundleName = zxsuffix.addassetbundle((string)vName[i]);
//					if (mABCheck.GetIsAssetBundle(tassetbundleName, true) == null)
//					{
//						errstr = string.Format("{0}{1};", errstr, tassetbundleName);
//						continue;
//					}
//					yield return mABLoad.LoadOneAsyncAssetBundle(tassetbundleName);
//				}
//			}
//			tcallback?.Invoke(errstr);
//		}

//		private IEnumerator LoadCoroutineIntelligentAssetBundle(Action<string> tcallback, ArrayList vName)
//		{
//			string errstr = string.Empty;
//			if (vName != null)
//			{
//				for (var i = 0; i < vName.Count; i++)
//				{
//					string tassetbundleName = zxsuffix.addassetbundle((string)vName[i]);
//					if (mABCheck.GetIsAssetBundle(tassetbundleName, true) == null)
//					{
//						errstr = string.Format("{0}{1};", errstr, tassetbundleName);
//						continue;
//					}
//					yield return mABLoad.LoadOneIntelligentAssetBundle(tassetbundleName);
//				}
//			}
//			tcallback?.Invoke(errstr);
//		}

//		private IEnumerator LoadCoroutineAsyncRes(Action<Object> tcallback, string oneName, string assetName, bool bsprite = false)
//		{
//			string tassetbundleName = zxsuffix.addassetbundle(oneName);
//#if UNITY_EDITOR && !KZC_AssetBundle
//			tcallback?.Invoke(CheckSimulateAsset(tassetbundleName, assetName));
//			yield break;
//#else
//			yield return mABLoad.LoadOneAsyncAsset (tcallback, tassetbundleName, assetName, bsprite);
//#endif
//		}

//#if UNITY_EDITOR && !KZC_AssetBundle
//		private Object CheckSimulateAsset(string tassetbundleName, string assetName)
//		{
//			string[] vpath = mABCheck.GetIsAssetBundle(tassetbundleName, true);
//			if (vpath != null && vpath.Length > 0)
//			{
//				for (int i = 0; i < vpath.Length; i++)
//				{
//                    if (Path.GetFileNameWithoutExtension(vpath[i]) == assetName)
//					{
//						List<string> checklist = mABCheck.ChecAssetBundleDepends(vpath[i]);

//						if (tassetbundleName.Contains("effs_"))
//						{
//                            checklist.Clear();
//                        }
//                        return mABLoad.LoadSimulateAsset(vpath[i], checklist);
//					}
//				}
//				zxlogger.logwarningformat("路径下未找到资源: {0}", pathtool.combine(Path.GetDirectoryName(vpath[0]), assetName));
//			}
//			else
//			{
//				zxlogger.logwarningformat("AssetBundle文件名无效: {0}", tassetbundleName);
//			}
//			return null;
//		}
//#endif

//		private void ResetGameObject(GameObject childobj)
//		{
//			childobj.transform.localPosition = Vector3.zero;
//			childobj.transform.localScale = Vector3.one;
//			childobj.transform.localEulerAngles = Vector3.zero;
//		}

//		private void Awake()
//		{
//			mABCheck = new AssetBundleCheck();
//			mABLoad = new AssetBundleLoad();
//		}
//	}
//}