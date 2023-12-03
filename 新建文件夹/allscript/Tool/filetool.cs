using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Basics;
using UnityEditor;
using UnityEngine;

// 文件操作工具
public static class filetool
{
	private static StringBuilder _textSB = new StringBuilder(2048);

	private static string _recordfpath = string.Empty;
	private static string recordfolderpath
	{
		get
		{
			if (string.IsNullOrEmpty(_recordfpath))
			{
				_recordfpath = PlayerPrefs.GetString("exportcfgfolder");
			}
			return _recordfpath;
		}
		set
		{
			PlayerPrefs.SetString("exportcfgfolder", value);
			_recordfpath = value;
		}
	}
	private static string _recordfi = string.Empty;
	private static string recordfilepath
	{
		get
		{
			if (string.IsNullOrEmpty(_recordfi))
			{
				_recordfi = PlayerPrefs.GetString("exportcfgfile");
			}
			return _recordfi;
		}
		set
		{
			PlayerPrefs.SetString("exportcfgfile", value);
			_recordfi = value;
		}
	}

	public const int assetoffset = 7;

	public static byte[] onstringencry(string soustring)
	{
		byte[] soubytes = Encoding.UTF8.GetBytes(soustring);
		return onencry(soubytes);
	}

	public static byte[] onencry(byte[] soubytes)
	{
		List<byte> readbytes = new List<byte>();
		byte[] empty = new byte[assetoffset];
		readbytes.AddRange(empty);
		readbytes.AddRange(soubytes);
		return readbytes.ToArray();
	}

	public static byte[] unencry(byte[] soubytes)
	{
		List<byte> readbytes = new List<byte>(soubytes);
		readbytes.RemoveRange(0, assetoffset);
		return readbytes.ToArray();
	}

	public static bool exists(string path)
	{
		bool flag = !string.IsNullOrEmpty(path);
		if (flag)
		{
			return File.Exists(path);
		}
		return false;
	}

	public static bool delete(string path)
	{
		bool flag = exists(path);
		if (flag)
		{
			File.Delete(path);
		}
		return flag;
	}

	public static bool copy(string source, string target)
	{
		bool flag = exists(source);
		if (flag)
		{
			delete(target);
			File.Copy(source, target);
		}
		return flag;
	}

	public static int writealltext(string path, string strcontent)
	{
		return writeallbytes(path, Encoding.UTF8.GetBytes(strcontent));
	}

	public static int writeallbytes(string path, byte[] bytecontent)
	{
		var temstatus = NetStatus.OK;
		try
		{
			File.WriteAllBytes(path, bytecontent);
		}
		catch (IOException ioex)
		{
			temstatus = NetException.CheckIOException(ioex);
		}
		catch (Exception e)
		{
			Debug.LogError("写入错误: " + e);
			temstatus = NetStatus.CSystemException;
		}
		return temstatus;
	}

	public static string readalltext(string path)
	{
		bool flag = exists(path);
		string result = string.Empty;
		if (flag)
		{
			result = File.ReadAllText(path);
		}
		return result;
	}

	public static byte[] readallbytes(string path)
	{
		bool flag = exists(path);
		byte[] result = null;
		if (flag)
		{
			result = File.ReadAllBytes(path);
		}
		return result;
	}

	public static long getfilesize(string filePath)
	{
		if (!exists(filePath))
		{
			return 0;
		}
		FileInfo file = new FileInfo(filePath);
		return file.Length;
	}

	public static string getbytesmd5(byte[] bytedata)
	{
		bool flag1 = bytedata != null && bytedata.Length > 0;
		if (flag1)
		{
			var md5byte = new MD5CryptoServiceProvider().ComputeHash(bytedata);
			return BitConverter.ToString(md5byte).Replace("-", "").ToLower();
		}
		return string.Empty;
	}

	public static string getstringmd5(string sourcestr)
	{
		bool flag1 = !string.IsNullOrEmpty(sourcestr);
		if (flag1)
		{
			return getbytesmd5(Encoding.UTF8.GetBytes(sourcestr));
		}
		return string.Empty;
	}

	public static string getfilemd5(string filepath)
	{
		bool flag1 = File.Exists(filepath);
		if (flag1)
		{
			return getbytesmd5(File.ReadAllBytes(filepath));
		}
		return string.Empty;
	}

	public static string getfilenamemd5(string filepath)
	{
		if (string.IsNullOrEmpty(filepath))
		{
			return string.Empty;
		}
		int fileIndex, fileLen, dotIndex, suffixLen;
		analysepathname(filepath, out fileIndex, out fileLen, out dotIndex, out suffixLen);

		string fname = filepath.Substring(fileIndex, fileLen);
		string md5 = getstringmd5(fname);
		_textSB.Length = 0;
		_textSB.Append(filepath);
		if (fileLen > 0)
		{
			_textSB.Replace(fname, md5, fileIndex, fileLen);
		}
		fname = _textSB.ToString();
		return fname;
	}

	internal static void analysepathname(string pathName, out int fileIndex, out int fileLen, out int dotIndex, out int suffixLen)
	{
		int len = pathName.Length;
		fileIndex = 0;
		int firstDotIndex = len;
		int lastDotIndex = len;
		int questIndex = len;

		int i = len - 1;
		char onechar;
		while (i >= 0)
		{
			onechar = pathName[i];
			if (onechar == '/' || onechar == '\\')
			{
				fileIndex = i + 1;
				break;
			}
			if (onechar == '?')
			{
				questIndex = i;
			}
			if (onechar == '.')
			{
				firstDotIndex = i;
				if (lastDotIndex == len) lastDotIndex = i;
			}
			--i;
		}

		if (firstDotIndex > questIndex) firstDotIndex = questIndex;
		dotIndex = lastDotIndex;
		suffixLen = questIndex - lastDotIndex;
		fileLen = firstDotIndex - fileIndex;
	}

	public static string getonefolder()
	{
#if UNITY_EDITOR
		string tpath = EditorUtility.OpenFolderPanel("选择文件夹", recordfolderpath, "文件夹");
		if (!string.IsNullOrEmpty(tpath))
		{
			recordfolderpath = tpath;
		}
		return tpath;
#else
		return string.Empty;
#endif
	}

	public static string getonefile(string exten)
	{
#if UNITY_EDITOR
		string tpath = EditorUtility.OpenFilePanel("选择文件", recordfilepath, exten);
		if (!string.IsNullOrEmpty(tpath))
		{
			recordfilepath = tpath;
		}
		return tpath;
#else
		return string.Empty;
#endif
	}
}