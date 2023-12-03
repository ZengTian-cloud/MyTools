using System;
using System.IO;
using UnityEngine;

// 路径工具
public static class pathtool
{
    private static string _reqpackpath = string.Empty;
    public static string requestpackpath
    {
        get
        {
            if (string.IsNullOrEmpty(_reqpackpath))
            {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IPHONE || UNITY_IOS
                _reqpackpath = "file://" + Application.streamingAssetsPath;
#elif UNITY_ANDROID
				_reqpackpath = Application.streamingAssetsPath;
#endif
            }
            return _reqpackpath;
        }
    }

    private static string _lpackpath = string.Empty;
    public static string loadpackpath
    {
        get
        {
            if (string.IsNullOrEmpty(_lpackpath))
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                _lpackpath = Application.dataPath;
#elif UNITY_IPHONE || UNITY_IOS
				_lpackpath = Application.dataPath + "/Raw";
#elif UNITY_ANDROID
				_lpackpath = Application.dataPath + "!assets";
#endif
            }
            return _lpackpath;
        }
    }

    private static string _reqrespath = string.Empty;
    public static string requestrespath
    {
        get
        {
            if (string.IsNullOrEmpty(_reqrespath))
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                _reqrespath = "file:///" + Application.streamingAssetsPath;
#elif UNITY_IPHONE || UNITY_IOS
				_reqrespath = "file:///" + Application.persistentDataPath + @"/Raw";
#elif UNITY_ANDROID
				_reqrespath = "file:///" + Application.persistentDataPath + @"/assets";
#endif
            }
            return _reqrespath;
        }
    }

    private static string _lrespath = string.Empty;
    public static string loadrespath
    {
        get
        {
            if (string.IsNullOrEmpty(_lrespath))
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                if (GameCenter.mIns == null || GameCenter.mIns.EPlayMode == YooAsset.EPlayMode.EditorSimulateMode)
                    _lrespath = Application.dataPath;
                else
                    _lrespath = Application.streamingAssetsPath;
#elif UNITY_IPHONE || UNITY_IOS
				_lrespath = Application.persistentDataPath + @"/Raw";
#elif UNITY_ANDROID
				_lrespath = Application.persistentDataPath + @"/assets";
#endif
            }
            return _lrespath;
        }
    }

    // 请求AssetBundle的包路径
    public static string reqabpackpath = combine(requestpackpath, zxconfig.encrydatafolder);
    // 加载AssetBundle的包路径
    public static string loadabpackpath = combine(loadpackpath, zxconfig.encrydatafolder);
    // 请求AssetBundle的可写路径
    public static string reqabrespath = combine(requestrespath, zxconfig.encryresfolder);
    // 加载AssetBundle的可写路径
    public static string loadabrespath = combine(loadrespath, zxconfig.encryresfolder);

    // 加载wwise的包路径
    public static string wwisepackpath = combine(loadpackpath, "Audio/GeneratedSoundBanks");
    // 加载wwise的可写路径
    public static string wwiserespath = combine(Application.persistentDataPath, "DecodedBanks");

    public static string combine(params string[] vpath)
    {
        string finalpath = string.Empty;
        if (vpath != null && vpath.Length > 0 && !string.IsNullOrEmpty(vpath[0]))
        {
            finalpath = vpath[0];
            for (int i = 1; i < vpath.Length; i++)
            {
                if (string.IsNullOrEmpty(vpath[i]))
                {
                    break;
                }
                char c2 = vpath[i][0];
                char c = finalpath[finalpath.Length - 1];

                if (c2 == '\\' || c2 == '/' || c2 == ':')
                {
                    vpath[i] = vpath[i].Substring(1);
                }

                if (c != '\\' && c != '/' && c != ':')
                {
                    finalpath = finalpath + "/" + vpath[i];
                }
                else
                {
                    finalpath = finalpath + vpath[i];
                }
            }
        }
        return finalpath;
    }

    public static bool exists(string dirpath)
    {
        return !string.IsNullOrEmpty(dirpath) && Directory.Exists(dirpath);
    }

    public static bool delete(string dirpath)
    {
        if (Directory.Exists(dirpath))
        {
            Directory.Delete(dirpath, true);
            return true;
        }
        return false;
    }

    public static string getdirectoryname(string fullPath)
    {
        return Path.GetDirectoryName(fullPath).Replace("\\", "/");
    }

    public static string[] getdirectoryfiles(string sourcepath)
    {
        if (Directory.Exists(sourcepath))
        {
            return Directory.GetFiles(sourcepath);
        }
        return null;
    }

    public static string[] getalldirectorys(string sourcepath)
    {
        if (Directory.Exists(sourcepath))
        {
            return Directory.GetDirectories(sourcepath);
        }
        return null;
    }

    public static string getfilename(string filePath)
    {
        return Path.GetFileName(filePath);
    }

    public static void checkfiledirectory(string filePath)
    {
        FileInfo finfo = new FileInfo(filePath);
        if (!finfo.Directory.Exists)
        {
            finfo.Directory.Create();
        }
    }

    public static void checkdirectory(string fullPath)
    {
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
    }

    public static void recreatedirectory(string fullPath)
    {
        if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, true);
        }
        Directory.CreateDirectory(fullPath);
    }

    public static void deepcopy(string soudir, string tardir, Action<string> callback, string ignores)
    {
        string[] vignore = null;
        if (!string.IsNullOrEmpty(ignores))
        {
            vignore = ignores.Split(',');
        }

        if (!Directory.Exists(tardir))
        {
            callback?.Invoke(tardir);
            Directory.CreateDirectory(tardir);
        }

        if (Directory.Exists(soudir))
        {
            var vfile = Directory.GetFiles(soudir);
            if (vfile != null)
            {
                for (var i = 0; i < vfile.Length; i++)
                {
                    if (!zxcssuffix.checkfiletype(soudir, vignore))
                    {
                        string targetpath = vfile[i].Replace(soudir, tardir);
                        filetool.copy(vfile[i], targetpath);
                        callback?.Invoke(targetpath);
                    }
                }
            }
            var vdir = Directory.GetDirectories(soudir);
            if (vdir != null)
            {
                for (var i = 0; i < vdir.Length; i++)
                {
                    string targetpath = vdir[i].Replace(soudir, tardir);
                    deepcopy(vdir[i], targetpath, callback, ignores);
                }
            }
        }
    }

    public static string unitytofullpath(string unitypath)
    {
        return combine(getdirectoryname(Application.dataPath), unitypath);
    }

    public static string fulltounitypath(string fullpath)
    {
        return fullpath.Replace("\\", "/").Replace(getdirectoryname(Application.dataPath) + "/", "");
    }
}