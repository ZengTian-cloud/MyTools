using System.Text;
using LitJson;

// json工具
public static class jsontool
{
	public static JsonData newwithempty()
	{
		return new JsonData();
	}

	public static JsonData newwithstring(string strJson)
	{
		try
		{
			return JsonMapper.ToObject(strJson);
		}
		catch (System.Exception)
		{
			return new JsonData();
		}
	}

	public static JsonData newwithbytes(byte[] byteJson)
	{
		return newwithstring(Encoding.UTF8.GetString(byteJson));
	}

	public static string tostring(JsonData temJson)
	{
		return temJson != null ? temJson.ToJson() : string.Empty;
	}

	public static string tofilestring(JsonData temJson, bool needsmall)
	{
		return temJson != null ? temJson.ToFileJson(needsmall) : string.Empty;
	}

	public static string toluafilestring(JsonData temJson, bool needsmall)
	{
		return temJson != null ? temJson.ToLuaJson(string.Empty, needsmall) : string.Empty;
	}

	public static void setkeyval(JsonData temJson, string temKey, object temVal)
	{
		if (temJson != null)
		{
			temJson[temKey] = tojsondata(temVal);
		}
	}

	public static JsonData getkeyval(JsonData temJson, string temKey)
	{
		return temJson != null ? temJson[temKey] : null;
	}

	public static void addtolist(JsonData temJson, object temVal)
	{
		if (temJson != null)
		{
			temJson.Add(temVal);
		}
	}

	public static void setindex(JsonData temJson, int temidx, object temval)
	{
		if (temJson != null && temidx >= 0 && temidx < temJson.Count)
		{
			temJson[temidx] = tojsondata(temval);
		}
	}

	public static JsonData getindex(JsonData temJson, int temidx)
	{
		return (temJson != null && temidx >= 0 && temidx < temJson.Count) ? temJson[temidx] : null;
	}

	private static JsonData tojsondata(object objVal)
	{
		return objVal != null ? (objVal is JsonData ? (JsonData)objVal : new JsonData(objVal)) : null;
	}

	public static JsonData getemptytable()
	{
		return newwithstring("{}");
	}
}