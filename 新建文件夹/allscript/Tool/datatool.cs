using UnityEngine;
using UnityEngine.Networking;

// 本地数据工具
public static class datatool
{
	public static void setbool(string key, bool value)
	{
		setint(key, value ? 1 : 0);
	}

	public static bool getbool(string key, bool defaultValue)
	{
		int temv = getint(key, defaultValue ? 1 : 0);
		return temv == 1;
	}

	public static bool getdefaultbool(string key)
	{
		return getbool(key, false);
	}

	public static void setint(string key, int value)
	{
		PlayerPrefs.SetInt(key, value);
	}

	public static int getint(string key, int defaultValue)
	{
		return PlayerPrefs.GetInt(key, defaultValue);
	}

	public static int getdefaultint(string key)
	{
		return getint(key, 0);
	}

	public static void setfloat(string key, float value)
	{
		PlayerPrefs.SetFloat(key, value);
	}

	public static float getfloat(string key, float defaultValue)
	{
		return PlayerPrefs.GetFloat(key, defaultValue);
	}

	public static float getdefaultfloat(string key)
	{
		return getfloat(key, 0f);
	}

	public static void setstring(string key, string value)
	{
		PlayerPrefs.SetString(key, UnityWebRequest.EscapeURL(value));
	}

	public static string getstring(string key, string defaultValue)
	{
		return UnityWebRequest.UnEscapeURL(PlayerPrefs.GetString(key, defaultValue));
	}

	public static string getdefaultstring(string key)
	{
		return getstring(key, string.Empty);
	}

	public static void cleardata(string key)
	{
		deletekey(key);
	}

	public static void clearall()
	{
		PlayerPrefs.DeleteAll();
	}

	public static void copybuffer(string content)
	{
		GUIUtility.systemCopyBuffer = content;
	}

	private static bool haskey(string key)
	{
		return PlayerPrefs.HasKey(key);
	}

	private static void deletekey(string key)
	{
		PlayerPrefs.DeleteKey(key);
	}
}