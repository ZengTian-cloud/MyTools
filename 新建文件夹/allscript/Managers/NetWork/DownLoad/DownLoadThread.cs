using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Threading;
using Basics;

namespace NetWork.DownLoad
{
	public class DownLoadThread : IDestroy
	{
		private const int MAX_TRYCOUNT = 3;
		private const int BUFFER_SIZE = 2048;
		private bool threadFlag = false;
		private Action<DownLoadThread> completeCallback;
		private int readstream = 0;
		private byte[] bytebuffer = new byte[BUFFER_SIZE];
		private bool hascomplete = false;

		public List<string> ListUrl = new List<string>();
		public string FilePath { get; private set; }
		public string Md5 { get; private set; }
		public long MaxLength { get; private set; }
		public long CurLength { get; private set; }
		public bool HasStarted { get; private set; }
		public bool Complete { get; private set; }
		public int DownStatus { get; private set; }

		public DownLoadThread(string filepath, string md5, Action<DownLoadThread> completecb, params string[] vUrl)
		{
			Complete = false;
			HasStarted = false;
			DownStatus = NetStatus.OK;
			CurLength = 0;
			MaxLength = 1;

			FilePath = filepath;
			Md5 = md5;
			completeCallback = completecb;
			if (vUrl != null)
			{
				ListUrl.AddRange(vUrl);
			}

			threadFlag = true;
			try
			{
				var thread = new Thread(Run);
				thread.Start();
			}
			catch
			{
				DownStatus = NetStatus.CSystemException;
				OnComplete();
			}
		}

		public void DestroySelf()
		{
			threadFlag = false;
			completeCallback = null;
		}

		private void Run()
		{
			if (filetool.getfilemd5(FilePath) != Md5)
			{
				if (ListUrl.Count > 0)
				{
					int curTryCount = 1;
					string downurl = ListUrl[0];
					DownTask(downurl);

					int maxTryTime = ListUrl.Count * MAX_TRYCOUNT;
					while (threadFlag && CheckContinue() && curTryCount < maxTryTime)
					{
						Thread.Sleep(2000);

						curTryCount++;
						downurl = ListUrl[(curTryCount / MAX_TRYCOUNT) % ListUrl.Count];
						zxlogger.logformat("下载失败{0}->第{1}次下载{2}", DownStatus, curTryCount, downurl);

						DownStatus = NetStatus.OK;
						Complete = false;
						HasStarted = false;

						DownTask(downurl);
					}
				}
				else
				{
					DownStatus = NetStatus.SEmptyUrl;
				}
			}

			OnComplete();
		}

		private bool CheckContinue()
		{
			switch (DownStatus)
			{
				case NetStatus.CSizeLEqual:
				case NetStatus.CSizeREqual:
				case NetStatus.CCheckError:
				case NetStatus.CIOException:
				case NetStatus.CSystemException:
					{
						return true;
					}
			}
			return false;
		}

		private void DownTask(string downurl)
		{
			CurLength = 0;
			Stream trespStream = null;
			HttpWebRequest twebRequest = null;
			HttpWebResponse twebResponse = null;

			try
			{
				pathtool.checkfiledirectory(FilePath);

				HasStarted = true;

				var norcp = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
				twebRequest = (HttpWebRequest)HttpWebRequest.Create(downurl);
				twebRequest.Timeout = 60000;
				twebRequest.ReadWriteTimeout = 60000;
				twebRequest.CachePolicy = norcp;
				twebRequest.Proxy = null;
				twebRequest.Credentials = CredentialCache.DefaultCredentials;

				twebResponse = (HttpWebResponse)twebRequest.GetResponse();
				MemoryStream mStream = null;
				DownStatus = (int)twebResponse.StatusCode;
				if (DownStatus == (int)HttpStatusCode.OK)
				{
					trespStream = twebResponse.GetResponseStream();
					MaxLength = twebResponse.ContentLength;
					mStream = new MemoryStream();
					while ((readstream = trespStream.Read(bytebuffer, 0, BUFFER_SIZE)) > 0)
					{
						CurLength += readstream;
						mStream.Write(bytebuffer, 0, readstream);
					}

					if (CurLength < MaxLength)
					{
						DownStatus = NetStatus.CSizeLEqual;
					}
					else if (CurLength > MaxLength)
					{
						DownStatus = NetStatus.CSizeREqual;
					}
					else if (threadFlag)
					{
						ReplaceOriginalFile(mStream.ToArray());
					}
				}
			}
			catch (WebException webEx)
			{
				DownStatus = NetException.CheckWebException(webEx);
			}
			catch (IOException ioEx)
			{
				DownStatus = NetException.CheckIOException(ioEx);
			}
			catch (ThreadAbortException)
			{
				DownStatus = NetStatus.CThreadAbort;
			}
			catch (Exception)
			{
				DownStatus = NetStatus.CSystemException;
			}
			finally
			{
				Complete = true;
				bool flag1 = trespStream != null;
				if (flag1)
				{
					trespStream.Flush();
					trespStream.Close();
					trespStream.Dispose();
					trespStream = null;
				}
				bool flag2 = twebRequest != null;
				if (flag2)
				{
					twebRequest.Abort();
					twebRequest = null;
				}
				bool flag3 = twebResponse != null;
				if (flag3)
				{
					twebResponse.Close();
					twebResponse = null;
				}
			}
		}

		private void ReplaceOriginalFile(byte[] downbyte)
		{
			bool flag1 = !string.IsNullOrEmpty(Md5);
			if (flag1)
			{
				bool flag2 = filetool.getbytesmd5(downbyte) != Md5;
				if (flag2)
				{
					DownStatus = NetStatus.CCheckError;
					return;
				}
			}

			DownStatus = filetool.writeallbytes(FilePath, downbyte);
		}

		private void OnComplete()
		{
			if (!hascomplete)
			{
				hascomplete = true;
				completeCallback?.Invoke(this);
				DestroySelf();
			}
		}
	}
}