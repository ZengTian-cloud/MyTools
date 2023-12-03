using System.Collections.Generic;
using System.IO;

// CS调用游戏后缀
public static class zxcssuffix
{
	//public static readonly List<string> lualist = new List<string>() { ".bytes" };
	public static readonly List<string> textlist = new List<string>() { ".txt" };
	public static readonly List<string> texturelist = new List<string>() { ".exr", ".jpg", ".png", ".tga", ".tif", ".psd" };
	public static readonly List<string> voicelist = new List<string>() { ".mp3", ".ogg", ".wav" };
	public static readonly List<string> videolist = new List<string>() { ".mp4" };
	public static readonly List<string> shaderlist = new List<string>() { ".shader" };
	public static readonly List<string> matlist = new List<string>() { ".mat" };
	public static readonly List<string> fontlist = new List<string>() { ".ttf", ".otf", ".fontsettings" };
	public static readonly List<string> anilist = new List<string>() { ".anim", ".controller" };
	public static readonly List<string> fbxlist = new List<string>() { ".fbx" };
	public static readonly List<string> prefablist = new List<string>() { ".prefab" };
	public static readonly List<string> otherlist = new List<string>() { ".asset" };
	public static readonly List<string> atlaslist = new List<string>() { ".spriteatlas" };

	public static List<string> assetbundlelist = new List<string>();

	public static bool istext(string filePath)
	{
		var temex = Path.GetExtension(filePath).ToLower();
		return filePath != null && (textlist.Contains(temex));
		//return filePath != null && (lualist.Contains(temex) || textlist.Contains(temex));
	}

	public static bool ispicture(string filePath)
	{
		return filePath != null && texturelist.Contains(Path.GetExtension(filePath).ToLower());
	}

	public static bool isvoice(string filePath)
	{
		return filePath != null && voicelist.Contains(Path.GetExtension(filePath).ToLower());
	}

	public static bool isvideo(string filePath)
	{
		return filePath != null && videolist.Contains(Path.GetExtension(filePath).ToLower());
	}

	public static bool isshader(string filePath)
	{
		return filePath != null && shaderlist.Contains(Path.GetExtension(filePath).ToLower());
	}

	public static bool ismat(string filePath)
	{
		return filePath != null && matlist.Contains(Path.GetExtension(filePath).ToLower());
	}

	public static bool isfont(string filePath)
	{
		return filePath != null && fontlist.Contains(Path.GetExtension(filePath).ToLower());
	}

	public static bool isani(string filePath)
	{
		return filePath != null && anilist.Contains(Path.GetExtension(filePath).ToLower());
	}

	public static bool isfbx(string filePath)
	{
		return filePath != null && fbxlist.Contains(Path.GetExtension(filePath).ToLower());
	}

	public static bool isprefab(string filePath)
	{
		return filePath != null && prefablist.Contains(Path.GetExtension(filePath).ToLower());
	}

	public static bool isother(string filePath)
	{
		return filePath != null && otherlist.Contains(Path.GetExtension(filePath).ToLower());
	}

	public static bool isinassetbundlepath(string filePath)
	{
		for (int i = 0; i < zxconfig.vassetfolder.Length; i++)
		{
			if (filePath.Contains(zxconfig.vassetfolder[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool isassetbundle(string filePath)
	{
		return isabasset(filePath) && isinassetbundlepath(filePath);
	}

	public static bool isabasset(string filePath)
	{
		return filePath != null && assetbundlelist.Contains(Path.GetExtension(filePath).ToLower()) && !isignored(filePath);
	}

	public static bool isignored(string filePath)
	{
		return filePath.EndsWith("LightingData.asset");
	}

	public static bool islanassetbundle(string abName)
	{
		abName = zxsuffix.delassetbundle(abName);
		return abName.EndsWith("_cn") ||
			abName.EndsWith("_en") ||
			abName.EndsWith("_tw") ||
			abName.EndsWith("_fr") ||
			abName.EndsWith("_gr") ||
			abName.EndsWith("_ru") ||
			abName.EndsWith("_jp") ||
			abName.EndsWith("_kr");
	}

	public static bool checkfiletype(string tarpath, params string[] vsuffix)
	{
		if (tarpath != null && vsuffix != null)
		{
			for (var i = 0; i < vsuffix.Length; i++)
			{
				if (tarpath.EndsWith(vsuffix[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	static zxcssuffix()
	{
		//assetbundlelist.AddRange(lualist);
		assetbundlelist.AddRange(textlist);
		assetbundlelist.AddRange(texturelist);
		assetbundlelist.AddRange(voicelist);
		assetbundlelist.AddRange(videolist);
		assetbundlelist.AddRange(shaderlist);
		assetbundlelist.AddRange(matlist);
		assetbundlelist.AddRange(fontlist);
		assetbundlelist.AddRange(anilist);
		assetbundlelist.AddRange(fbxlist);
		assetbundlelist.AddRange(prefablist);
		assetbundlelist.AddRange(otherlist);
		assetbundlelist.AddRange(atlaslist);
	}
}

// lua调用游戏后缀
public static class zxsuffix
{
	public static string addassetbundle(string filePath)
	{
		if (!string.IsNullOrEmpty(filePath) && !filePath.EndsWith(".dat"))
		{
			filePath = string.Format("{0}.dat", filePath);
		}
		return filePath;
	}

	public static string delassetbundle(string filePath)
	{
		if (filePath != null && filePath.EndsWith(".dat"))
		{
			filePath = filePath.Substring(0, filePath.Length - 4);
		}
		return filePath;
	}
}