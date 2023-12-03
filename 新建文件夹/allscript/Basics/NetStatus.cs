using System.IO;
using System.Net;

namespace Basics
{
	public static class NetStatus
	{
		public const int OK = 200;

		public const int SEmptyUrl = 1001;
		public const int SNetError = 1002;
		public const int SWebException = 1003;
		public const int SNotFind = 1004;

		public const int WSuccess = 1100;
		public const int WNameResolutionFailure = 1101;
		public const int WConnectFailure = 1102;
		public const int WReceiveFailure = 1103;
		public const int WSendFailure = 1104;
		public const int WPipelineFailure = 1105;
		public const int WRequestCanceled = 1106;
		public const int WProtocolError = 1107;
		public const int WConnectionClosed = 1108;
		public const int WTrustFailure = 1109;
		public const int WSecureChannelFailure = 1110;
		public const int WServerProtocolViolation = 1111;
		public const int WKeepAliveFailure = 1112;
		public const int WPending = 1113;
		public const int WTimeout = 1114;
		public const int WProxyNameResolutionFailure = 1115;
		public const int WUnknownError = 1116;
		public const int WMessageLengthLimitExceeded = 1117;
		public const int WCacheEntryNotFound = 1118;
		public const int WRequestProhibitedByCachePolicy = 1119;
		public const int WRequestProhibitedByProxy = 1120;

		public const int CSizeLEqual = 1201;
		public const int CSizeREqual = 1202;
		public const int CCheckError = 1203;
		public const int CThreadAbort = 1204;
		public const int CIOException = 1205;
		public const int CDiskFull = 1206;
		public const int CNotFind = 1207;
		public const int CSystemException = 1208;
	}

	public static class NetException
	{
		private const int HR_ERROR_DISK_FULL = unchecked((int)0x80070070);
		private const int HR_ERROR_HANDLE_DISK_FULL = unchecked((int)0x80070027);

		public static int CheckWebException(WebException webEx)
		{
			if (webEx != null)
			{
				if (webEx.Status == WebExceptionStatus.ProtocolError &&
					(webEx.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
				{
					return NetStatus.SNotFind;
				}
				return NetStatus.WSuccess + (int)webEx.Status;
			}
			return NetStatus.SWebException;
		}

		public static int CheckIOException(IOException ioEx)
		{
			if (ioEx != null && (ioEx.HResult == HR_ERROR_DISK_FULL || ioEx.HResult == HR_ERROR_HANDLE_DISK_FULL))
			{
				return NetStatus.CDiskFull;
			}
			return NetStatus.CIOException;
		}
	}
}