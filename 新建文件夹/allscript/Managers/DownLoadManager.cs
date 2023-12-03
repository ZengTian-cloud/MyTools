using System.Collections.Generic;
using Basics;
using NetWork.DownLoad;

namespace Managers
{
	public class DownLoadManager : SingletonOjbect
	{
		private int sequeueid = 0;
		private Dictionary<int, DownLoadClient> dictClient = new Dictionary<int, DownLoadClient>();
		private List<int> listdownload = new List<int>();

		public int InitDownLoad()
		{
			++sequeueid;

			var temdownload = new DownLoadClient();
			dictClient.Add(sequeueid, temdownload);
			return sequeueid;
		}

		// public void SetProgressCallback(int tsequeue, LuaFunction tcallback)
		// {
		// 	if (dictClient[tsequeue] != null)
		// 	{
		// 		dictClient[tsequeue].DownProgressCallback = tcallback;
		// 	}
		// }

		// public void SetCompleteOneFileCallback(int tsequeue, LuaFunction tcallback)
		// {
		// 	if (dictClient[tsequeue] != null)
		// 	{
		// 		dictClient[tsequeue].DownOneFileCallback = tcallback;
		// 	}
		// }

		// public void SetCompleteCallback(int tsequeue, LuaFunction tcallback)
		// {
		// 	if (dictClient[tsequeue] != null)
		// 	{
		// 		dictClient[tsequeue].DownOverCallback = tcallback;
		// 	}
		// }

		public void JoinDownLoad(int tsequeue, string filePath, string md5, long size, params string[] vUrl)
		{
			if (dictClient[tsequeue] != null)
			{
				dictClient[tsequeue].AddDownLoadList(filePath, md5, size, vUrl);
			}
		}

		public void StartDownLoad(int tsequeue)
		{
			if (dictClient[tsequeue] != null && !listdownload.Contains(tsequeue))
			{
				listdownload.Add(tsequeue);
			}
		}

		public void StopDownLoad(int tsequeue)
		{
			if (dictClient[tsequeue] != null)
			{
				dictClient[tsequeue].DestroySelf();
			}
			listdownload.Remove(tsequeue);
			dictClient.Remove(tsequeue);
		}

		private void Update()
		{
			if (listdownload.Count > 0)
			{
				var oneClient = dictClient[listdownload[0]];
				if (oneClient == null)
				{
					listdownload.RemoveAt(0);
				}
				else if (oneClient.downTag == DownLoadTag.Waitting)
				{
					oneClient.BeginDownLoad();
				}
				else if (oneClient.downTag == DownLoadTag.Downloing)
				{
					oneClient.Update();
				}
				else if (oneClient.downTag == DownLoadTag.Done)
				{
					StopDownLoad(listdownload[0]);
				}
			}
		}

		private void OnDestroy()
		{
			foreach (var oneClient in dictClient.Values)
			{
				oneClient?.DestroySelf();
			}
			dictClient.Clear();
			listdownload.Clear();
		}
	}
}