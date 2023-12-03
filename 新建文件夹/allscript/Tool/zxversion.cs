using LitJson;

public static class zxversion
{
	public static bool splitpackage = false;
	public static bool hotupdate = false;
	public static string languagestr = string.Empty;

	public static int packageversion = 0;
	public static int resversion = 0;
	public static JsonData lualistjson = jsontool.newwithstring("[]");
	public static JsonData reslistjson = jsontool.newwithstring("[]");
	public static JsonData audiolistjson = jsontool.newwithstring("[]");
}