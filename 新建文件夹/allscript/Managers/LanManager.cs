using LitJson;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Basics;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;

namespace Managers
{
    public class LanManager : SingletonOjbect
    {
        private Dictionary<string, string> lanDict = new Dictionary<string, string>();

        public void Init()
        {
            lanDict = new Dictionary<string, string>();
            LoadLanCfg();
        }

        public async void LoadLanCfg()
        {
            // todo:遍历获取所有多语言文件
            string lan_type = string.IsNullOrEmpty(GameCenter.mIns.gameInfo.Lan) ? "cn" : GameCenter.mIns.gameInfo.Lan;

            string path = "";
#if UNITY_EDITOR
            path = Application.dataPath + $"/language/{lan_type}/";
            string[] files = Directory.GetFiles(path);
            foreach (var f in files)
            {
                if (f.EndsWith(".txt"))
                {
                    string name = f.Substring(f.LastIndexOf("lan_"), f.Length - f.LastIndexOf("lan_"));
                    string p = path + name;
                    LoadOneLanCfg(lan_type, p, name);
                }
            }
#else
           path = pathtool.loadrespath + $"/language/{lan_type}/";
            Dictionary<string, Object> cfgObjs = await  ResourcesManager.Instance.GetAllAssetObjectDictByTag("DefaultPackage", "language");
            foreach (var cfg in cfgObjs)
            {
                string name = cfg.Key.Substring(0, cfg.Key.LastIndexOf('.'));
                if (name.StartsWith("lan_"))
                {
                    TextAsset textAsset = (TextAsset)cfg.Value;
                    JsonData data = jsontool.newwithstring(textAsset.ToString());

                    foreach (JsonData jd in data)
                    {
                        string key = jd["id"].ToString();
                        if (!lanDict.ContainsKey(key))
                            lanDict.Add(key, jd["val"].ToString());
                        else
                            zxlogger.logwarning($"Error: has same lan key! key:{key}, lan_type:{lan_type}.");
                    }
                }
            }
#endif

        }

        private void LoadOneLanCfg(string lan_type, string path,string name)
        {
            JsonData data = jsontool.newwithstring(File.ReadAllText(path));
            foreach (JsonData jd in data)
            {
                string key =  jd["id"].ToString();
                if (!lanDict.ContainsKey(key))
                    lanDict.Add(key, jd["val"].ToString());
                else
                    zxlogger.logwarning($"Error: has same lan key! key:{key}, lan_type:{lan_type}.");
            }
        }

        public string GetLan(string key)
        {
            return !lanDict.ContainsKey(key) ? "" : lanDict[key];
        }

        public string GetLanFormat(string key, params object[] args)
        {
            string lanstr = GetLan(key);
            if (!string.IsNullOrEmpty(lanstr))
            {
                return string.Format(lanstr, args);
            }
            return lanstr;
        }

        public void SetChildsTextLan(Transform root)
        {
            Transform[] texts = root.GetComponentsInChildren<Transform>(true);
            if (texts.Length <= 0) return;
            foreach (Transform t in texts)
            {
                TextMeshProUGUI txt = t.GetComponent<TextMeshProUGUI>();
                if (txt != null&&txt.text.StartsWith("lan_key_")) {
                    txt.text = GetLan(txt.text.Replace("lan_key_",""));
                }
                if (root == t) continue;
                SetChildsTextLan(t);
            }
        }

    }
}
