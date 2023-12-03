using Basics;
using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


public static class GameConfig
{
    private static Dictionary<Type, Dictionary<long, Config>> configDic = new Dictionary<Type, Dictionary<long, Config>>();

    public static Dictionary<long, Config> GetConfig<T>() where T : Config
    {
        if (configDic.TryGetValue(typeof(T), out Dictionary<long, Config> dic))

            return dic;
        else
            throw new Exception(string.Format("Type Not Found : {0}", typeof(T).Name));
    }

    public static T Get<T>(long id) where T : Config
    {
        Dictionary<long, Config> dic = GetConfig<T>();

        if (dic.TryGetValue(id, out Config cfg))

            return cfg as T;
        else
            throw new Exception(string.Format("Config Not Found : {0}:{1}", typeof(T).Name, id));
    }

    public static void AddConfig<T>(Dictionary<long, Config> dic) where T : Config
    {
        Type configType = typeof(T);

        if (configDic.ContainsKey(configType))
        {
            throw new Exception(string.Format("Config already exists : {0}", configType.Name));
        }
        configDic[configType] = dic;
    }

}




