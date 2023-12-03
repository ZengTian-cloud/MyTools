using System.Collections.Generic;
using Basics;

namespace NetWork.DownLoad
{
	public enum DownLoadTag
	{
		None,
		Waitting,
		Downloing,
		Done
	}

	public class DownLoadInfo
	{
		public string FilePath { get; private set; }
		public string Md5 { get; private set; }
		public long FileSize { get; private set; }
		public string[] VUrl { get; private set; }

		public DownLoadInfo(string filepath, string md5, long size, params string[] vUrl)
		{
			FilePath = filepath;
			Md5 = md5;
			FileSize = size;
			VUrl = vUrl;
		}
	}

	public class DownLoadClient : IDestroy
	{

		private readonly object syncRoot = new object();

		private long fileLen = 0;

		private int fileNum = 0;

		private bool candownload = false;

		private int downStatus = NetStatus.OK;

		private string downErrFile = string.Empty;

		private bool bCompleteAll = false;

		private Dictionary<string, DownLoadInfo> dictAsset = new Dictionary<string, DownLoadInfo>();

		private List<DownLoadThread> assetDowning = new List<DownLoadThread>();

		private List<string> listcompletefile = new List<string>();

		public DownLoadTag downTag { get; private set; }

		// public LuaFunction DownProgressCallback;

		// public LuaFunction DownOneFileCallback;

		// public LuaFunction DownOverCallback;

		public DownLoadClient()
		{
			downTag = DownLoadTag.Waitting;
		}

		public bool AddDownLoadList(string filePath, string md5, long size, params string[] vUrl)
		{
			if (downTag != DownLoadTag.Waitting)
			{
				return false;
			}

			if (dictAsset.ContainsKey(filePath))
			{
				return false;
			}

			var tinfo = new DownLoadInfo(filePath, md5, size, vUrl);
			dictAsset.Add(filePath, tinfo);
			return true;
		}

		public void BeginDownLoad()
		{
			downTag = DownLoadTag.Downloing;
			candownload = true;
		}

		public void Update()
		{
			if (downTag != DownLoadTag.Downloing)
			{
				return;
			}

			if (candownload)
			{
				candownload = false;
				RunTask();
			}

			// if (DownProgressCallback != null)
			// {
			// 	try
			// 	{
			// 		long tdownlen = fileLen;
			// 		for (int idx = assetDowning.Count - 1; idx >= 0; --idx)
			// 		{
			// 			if (assetDowning[idx] != null)
			// 			{
			// 				tdownlen += assetDowning[idx].CurLength;
			// 			}
			// 		}
			// 		DownProgressCallback.Action(fileNum, tdownlen);
			// 	}
			// 	catch
			// 	{

			// 	}
			// }

			while (listcompletefile.Count > 0)
			{
				var strfile = listcompletefile[0];
				// DownOneFileCallback?.Action(strfile);
				listcompletefile.RemoveAt(0);
			}

			if (bCompleteAll)
			{
				downTag = DownLoadTag.Done;
				// DownOverCallback?.Action(downStatus, downErrFile);
			}
		}

		public void DestroySelf()
		{
			lock (syncRoot)
			{
				for (var i = 0; i < assetDowning.Count; i++)
				{
					assetDowning[i]?.DestroySelf();
				}
				assetDowning.Clear();
				downTag = DownLoadTag.None;
				// DownProgressCallback?.Dispose();
				// DownOneFileCallback?.Dispose();
				// DownOverCallback?.Dispose();
				// DownProgressCallback = null;
				// DownOneFileCallback = null;
				// DownOverCallback = null;
			}
		}

		private void RunTask()
		{
			lock (syncRoot)
			{
				if (dictAsset.Count > 0)
				{
					if (nettool.isconnect)
					{
						var oneAsset = dictAsset.GetEnumerator();
						oneAsset.MoveNext();
						DownLoadThread dlthread = new DownLoadThread(
							oneAsset.Current.Value.FilePath,
							oneAsset.Current.Value.Md5,
							CompleteOneFileCallback,
							oneAsset.Current.Value.VUrl
						);
						assetDowning.Add(dlthread);
						dictAsset.Remove(oneAsset.Current.Key);
						return;
					}

					downStatus = NetStatus.SNetError;
				}

				CompleteAllTasks();
			}
		}

		private void CompleteOneFileCallback(DownLoadThread dlthread)
		{
			assetDowning.Remove(dlthread);

			downStatus = dlthread.DownStatus;
			if (downStatus == NetStatus.OK)
			{
				++fileNum;
				fileLen += dlthread.MaxLength;
				listcompletefile.Add(dlthread.FilePath);
				candownload = true;
			}
			else
			{
				downErrFile = dlthread.ListUrl.Count > 0 ? dlthread.ListUrl[0] : dlthread.FilePath;
				CompleteAllTasks();
			}
		}

		private void CompleteAllTasks()
		{
			bCompleteAll = true;
		}
	}
}