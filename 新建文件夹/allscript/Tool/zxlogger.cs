using UnityEngine;

public static class zxlogger
{
	// 设置是否开启cs日志打印
	public static bool enablecsmsg = true;

	// 设置是否开启cs警告打印
	public static bool enablecswarning = true;

	// 设置是否开启cs错误打印
	public static bool enablecserror = true;

	// 调用lua打印日志
	public static void lualoginfo(object message)
	{
		Debug.Log(message);
	}

	// 调用lua打印警告
	public static void lualogwarning(object message)
	{
		Debug.LogWarning(message);
	}

	// 调用lua打印错误
	public static void lualogerror(object message)
	{
		Debug.LogError(message);
	}

	public static void log(object message)
	{
		if (enablecsmsg)
		{
			Debug.Log(message);
		}
	}

	public static void logwarning(object message)
	{
		if (enablecswarning)
		{
			Debug.LogWarning(message);
		}
	}

	public static void logerror(object message)
	{
		if (enablecserror)
		{
			Debug.LogError(message);
		}
	}

	public static void logerrorobject(object message, Object temobj)
	{
		if (enablecserror)
		{
			Debug.LogError(message, temobj);
		}
	}

	public static void logformat(string format, params object[] args)
	{
		if (enablecsmsg)
		{
			Debug.LogFormat(format, args);
		}
	}

	public static void logwarningformat(string format, params object[] args)
	{
		if (enablecswarning)
		{
			Debug.LogWarningFormat(format, args);
		}
	}

	public static void logerrorformat(string format, params object[] args)
	{
		if (enablecserror)
		{
			Debug.LogErrorFormat(format, args);
		}
	}
}