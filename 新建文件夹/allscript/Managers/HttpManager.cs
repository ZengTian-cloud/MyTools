using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Basics;
using NetWork.Http;
using UnityEngine;
using LitJson;

namespace Managers
{
	public class HttpManager : SingletonOjbect
	{
		public Action<int> updateEvent = null;

		private List<HttpClient> listClient = new List<HttpClient>();
		private HttpClient tmpClient;

		private static int TIME_OUT = 3000;

		public override IEnumerator LaunchOne()
		{
			listClient.Clear();
			yield return base.LaunchOne();
		}

		public void POST(string turl, string tcontent, Action<int, string, int> tcb)
		{
			SendData("POST", TIME_OUT,  turl,  tcontent,  tcb);
		}

		public void REPORT(string turl, string tcontent, Action<int, string, int> tcb)
		{

			SendData("REPORT", TIME_OUT, turl, tcontent, tcb);
		}

		public void GET(string turl, string tcontent, Action<int, string, int> tcb)
		{
			SendData("GET", TIME_OUT, turl, tcontent, tcb);
		}

		public void SendData(string method, int timeout, string turl, string tcontent, Action<int, string, int> tcb)
		{
			if (!method.Equals("POST"))
			{
				turl = string.IsNullOrEmpty(tcontent) ? turl : string.Format("{0}?{1}", turl, tcontent);
				tcontent = null;
			}
			listClient.Insert(0, new HttpClient(method, turl, tcontent, timeout, tcb));
		}

		private void Awake()
		{
			ServicePointManager.DefaultConnectionLimit = 50;
			ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(Check);
		}

		private void Update()
		{
			for (int i = listClient.Count - 1; i >= 0; --i)
			{
				tmpClient = listClient[i];
				if (CheckOver(tmpClient))
				{
					listClient.RemoveAt(i);
					tmpClient?.mRespCallback(tmpClient.mRespCode, tmpClient.mRespData, tmpClient.mRespTime);
				}
			}

			updateEvent?.Invoke((int)(Time.unscaledDeltaTime * 1000));
		}

		private bool CheckOver(HttpClient temClient)
		{
			bool bremove = false;
			if (temClient == null)
			{
				bremove = true;
			}
			else if (temClient.reqState == RequestState.Done)
			{
				bremove = true;
			}
			else if (temClient.curCall.ElapsedMilliseconds >= temClient.mtotaltime)
			{
				temClient.DoResponse(NetStatus.WTimeout, "Time Out");
				bremove = true;
			}
			return bremove;
		}

		private bool Check(object s, X509Certificate ct, X509Chain ch, SslPolicyErrors e)
		{
			return true;
		}
	}
}