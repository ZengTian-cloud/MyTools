using System.Text.RegularExpressions;

// 字符串
public static class stringtool
{
	// 拷贝的字符串
	public static string copybuffer
	{
		get
		{
			return UnityEngine.GUIUtility.systemCopyBuffer;
		}
		set
		{
			UnityEngine.GUIUtility.systemCopyBuffer = value;
		}
	}

	// 判断字符串是否有中文
	public static bool haschinese(string value)
	{
		return Regex.IsMatch(value, "[\u4e00-\u9fbb]");
	}
}