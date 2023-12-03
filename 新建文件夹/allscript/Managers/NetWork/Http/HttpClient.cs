using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using Basics;
using CSharpZip.GZip;

namespace NetWork.Http
{
	public enum RequestState
	{
		None,
		Waiting,
		Reading,
		Done
	}

	public class HttpClient
	{
		private string murl;
		private string mmethod;
		private string postParam;
		private bool isreport;
		private HttpWebRequest mWebRequest;
		private HttpWebResponse mWebResponse;

		public RequestState reqState = RequestState.None;
		public Stopwatch curCall = new Stopwatch();
		public int mtotaltime;
		public Action<int, string, int> mRespCallback;
		public int mRespCode;
		public string mRespData;
		public int mRespTime;

		public HttpClient(string tmethod, string turl, string sparam, int temtime, Action<int, string, int> tcallback)
		{
			isreport = tmethod.Equals("Report");
			mmethod = isreport ? "POST" : tmethod;
			murl = turl;
			postParam = sparam;
			mtotaltime = temtime;
			mRespCallback = tcallback;
			if (!nettool.isconnect)
			{
				DoResponse(NetStatus.SNetError, "No network connection");
				return;
			}
			Send();
		}

		private void Send()
		{
			reqState = RequestState.Waiting;
			try
			{
				curCall.Start();
				mWebRequest = WebRequest.Create(murl) as HttpWebRequest;
				mWebRequest.Proxy = null;
				mWebRequest.Method = mmethod;
				mWebRequest.Timeout = mtotaltime;
				mWebRequest.ReadWriteTimeout = mtotaltime;

				if (mmethod.Equals("POST"))
				{
					if (murl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
					{
						mWebRequest.ProtocolVersion = HttpVersion.Version10;
					}
					mWebRequest.ContentType = "application/x-www-form-urlencoded";
					mWebRequest.BeginGetRequestStream(AsyncWriteRequestStream, null);
					return;
				}
				var asyncCallback = new AsyncCallback(OnResultResponse);
				mWebRequest.BeginGetResponse(asyncCallback, null);
			}
			catch (WebException webex)
			{
				DoResponse(NetException.CheckWebException(webex), webex.ToString());
			}
			catch (System.Exception ex)
			{
				DoResponse(NetStatus.CSystemException, ex.ToString());
			}
		}

		private void AsyncWriteRequestStream(IAsyncResult ar)
		{
			if (!string.IsNullOrEmpty(this.postParam))
			{
				byte[] bytes = Encoding.UTF8.GetBytes(this.postParam);
				Stream stream = mWebRequest.EndGetRequestStream(ar);
				stream.Write(bytes, 0, bytes.Length);
				stream.Close();
			}
			var asyncCallback = new AsyncCallback(OnResultResponse);
			mWebRequest.BeginGetResponse(asyncCallback, null);
		}

		private void OnResultResponse(IAsyncResult result)
		{
			try
			{
				mWebResponse = mWebRequest.EndGetResponse(result) as HttpWebResponse;

				if (mWebResponse.StatusCode == HttpStatusCode.OK)
				{
					var respStream = mWebResponse.GetResponseStream();

					reqState = RequestState.Reading;

					if (isreport)
					{
						using (var treader = new StreamReader(respStream))
						{
							string respcontent = treader.ReadToEnd();
							DoResponse((int)mWebResponse.StatusCode, respcontent);
						}
						respStream.Close();
					}
					else
					{
						MemoryStream ms = new MemoryStream();
						if (!nettool.useAes) // 新的AES解密方式不使用gzip压缩
						{
							GZipInputStream gstream = new GZipInputStream(respStream);
							gstream.CopyTo(ms);
							gstream.Close();
						}
						else
						{
							respStream.CopyTo(ms);
							respStream.Close();
						}
						byte[] recbyte1 = ms.ToArray();
						string respcontent = string.Empty;
						if (nettool.useAes)
							respcontent = nettool.unescapeurl(nettool.DecodeAES(Encoding.UTF8.GetString(recbyte1), nettool.aesKey, nettool.aesIV)); //aes解密
						else
						{
							byte[] recbyte2 = nettool.decodebyte(recbyte1);
							respcontent = nettool.unescapeurl(Encoding.UTF8.GetString(recbyte2));
						}
						DoResponse((int)mWebResponse.StatusCode, respcontent);
						ms.Close();
					}
				}
				else
				{
					DoResponse((int)mWebResponse.StatusCode, mWebResponse.StatusDescription);
				}
			}
			catch (WebException webex)
			{
				DoResponse(NetException.CheckWebException(webex), webex.ToString());
			}
			catch (System.Exception ex)
			{
				DoResponse(NetStatus.CSystemException, ex.ToString());
			}
		}

		public void DoResponse(int tcode, string trespdata)
		{
			if (reqState != RequestState.Done)
			{
				reqState = RequestState.Done;

				mRespCode = tcode;
				mRespData = trespdata;
				mRespTime = (int)curCall.ElapsedMilliseconds;

				curCall.Stop();
				if (mWebRequest != null)
				{
					mWebRequest.Abort();
					mWebRequest = null;
				}
				if (mWebResponse != null)
				{
					mWebResponse.Close();
					mWebResponse = null;
				}
			}
		}
	}
}