using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;
using Basics;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Managers
{
    public class CfgManager : SingletonOjbect
    {
        private bool Loaded = false;
        /// <summary>
        /// 保存所有配置的json数据
        /// </summary>
        private Dictionary<string, JsonData> cfgDict = new Dictionary<string, JsonData>();

        /// <summary>
        /// 缓存所有物品配置数据
        /// </summary>
        private Dictionary<long, ItemCfgData> itemCfgDict = new Dictionary<long, ItemCfgData>();

        /// <summary>
        /// 英雄配置列表
        /// </summary>
        private List<HeroInfoCfgData> heroInfoCfgDatasList = new List<HeroInfoCfgData>();
        private Dictionary<long, HeroInfoCfgData> dicHeroInfoCfgDatas = new Dictionary<long, HeroInfoCfgData>();

        /// <summary>
        /// 常量表
        /// </summary>
        private List<CommonCfgData> commonCfgDatasList = new List<CommonCfgData>();
        private Dictionary<string, string > dicCommonCfgDatas = new Dictionary<string, string>();

        //消耗表配置
        private Dictionary<long,CostCfgData> costCfgDatas = new Dictionary<long, CostCfgData>();

        //英雄属性表
        private Dictionary<long , HeroStatsCfg> heroStatsCfgs = new Dictionary<long, HeroStatsCfg>();
        //技能升级消耗表
        private Dictionary<long, List<HeroSkillUpCostCfg>> heroSkillUpCostCfgs = new Dictionary<long, List<HeroSkillUpCostCfg>>();
        //技能树配置
        private List<HeroSkillTreeCfg> heroSkillTrees = new List<HeroSkillTreeCfg>();
        //武器配置表
        private Dictionary<long, WeaponDataCfg> weaponDataCfgs = new Dictionary<long, WeaponDataCfg>();

        /// <summary>
        /// 获取所有物品配置数据
        /// </summary>
        /// <returns></returns>
        public Dictionary<long, ItemCfgData> GetItemCfgDatas()
        {
            return itemCfgDict;
        }

        /// <summary>
        /// 获取一个物品配置数据
        /// </summary>
        /// <param name="pid">pid</param>
        /// <returns></returns>
        public ItemCfgData GetItemCfgData(long pid)
        {
            return itemCfgDict.ContainsKey(pid) ? itemCfgDict[pid] : null;
        }
        public HeroStatsCfg getHeroStstsCfgByAttrId(long attrid)
        {
            return heroStatsCfgs == null ? null : heroStatsCfgs[attrid];
        }

        /// <summary>
        /// 获得一个英雄配置
        /// </summary>
        /// <param name="heroid"></param>
        /// <returns></returns>
        public HeroInfoCfgData GetHeroCfgDataByHeroID(long heroid)
        {
            return dicHeroInfoCfgDatas.ContainsKey(heroid) ? dicHeroInfoCfgDatas[heroid] : null;
        }

        /// <summary>
        /// 获得英雄技能升级消耗 by 技能id和技能当前等级
        /// </summary>
        /// <returns></returns>
        public HeroSkillUpCostCfg GetHeroSkillUpCostCfgBySkillIDAndSkillLevel(long skillId,int lv)
        {
            if (heroSkillUpCostCfgs.ContainsKey(skillId))
            {
                return heroSkillUpCostCfgs[skillId].Find(c => c.level == lv);
            }
            return null;
        }


        /// <summary>
        /// 获得某级的技能升级的突破等级条件
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="lv"></param>
        /// <returns></returns>
        public int GetUnLockActiveSkillState(long skillID, int lv)
        {
            if (heroSkillUpCostCfgs.ContainsKey(skillID))
            {
                HeroSkillUpCostCfg cfg = heroSkillUpCostCfgs[skillID].Find(x => x.level == lv);
                if (cfg!= null)
                {
                    return cfg.statelevel;
                }
                Debug.LogError($"未找到技能{skillID}-等级{lv}的消耗配置，请检查");

            }
            return 0;
        }

        /// <summary>
        /// 获得英雄技能所有等级升级消耗
        /// </summary>
        /// <returns></returns>
        public List<HeroSkillUpCostCfg> GetAllHeroSkillUpCostCfgBySkillID(long skillId)
        {
            if (heroSkillUpCostCfgs.ContainsKey(skillId))
            {
                return heroSkillUpCostCfgs[skillId];
            }
            return null;
        }


        /// <summary>
        /// 获得消耗组配置
        /// </summary>
        /// <param name="costID"></param>
        /// <returns></returns>
        public CostCfgData GetCostByCostID(long costID)
        {
            if (costCfgDatas.ContainsKey(costID))
            {
                return costCfgDatas[costID];
            }
            else
            {
                Debug.LogError($"未在消耗表中找到id为{costID}的道具组！！！！！！！！！！！！！！");
            }
            return null;
        }

        /// <summary>
        /// 获得武器配置
        /// </summary>
        /// <param name="weaponID"></param>
        /// <returns></returns>
        public WeaponDataCfg GetWeaponCfgByWeaponID(long weaponID)
        {
            if (weaponDataCfgs.ContainsKey(weaponID))
            {
                return weaponDataCfgs[weaponID];
            }
            return null;
        }

        /// <summary>
        /// 获得英雄的所有武器
        /// </summary>
        /// <param name="heroID"></param>
        /// <returns></returns>
        public Dictionary<int,WeaponDataCfg> GetHeroAllWeapon(long heroID)
        {
            Dictionary<int, WeaponDataCfg> cfgs = new Dictionary<int, WeaponDataCfg>();
            foreach (var item in weaponDataCfgs)
            {
                if (item.Value.heroid == heroID)
                {
                    cfgs.Add(item.Value.type,item.Value);
                }
            }
            return cfgs;
        }

        /// <summary>
        /// 获得技能树配置表
        /// </summary>
        /// <param name="heroID"></param>
        /// <param name="skillID"></param>
        /// <returns></returns>
        public HeroSkillTreeCfg GetHeroSkillTreeCfg(long heroID,long skillID)
        {
            HeroSkillTreeCfg cfg = heroSkillTrees.Find(x => x.heroid == heroID && x.skillid == skillID);
            if (cfg == null)
            {
                Debug.LogError($"未在技能树配置表中找到heroid:{heroID},skillID:{skillID}的配置，请检查");
            }
            return cfg;
        }


        /// <summary>
        /// 获取json数据
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        private JsonData GetJsonData(string fullPath)
        {
            return jsontool.newwithstring(File.ReadAllText(fullPath));
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="cfgPath"></param>
        /// <param name="cfgName"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public JsonData LoadCfg(string cfgPath, string cfgName, string suffix = "txt")
        {
            return GetJsonData($"{cfgPath}/{cfgName}.{suffix}");
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="cfgPath"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public JsonData LoadCfg(string cfgPath, string suffix = "txt")
        {
            return GetJsonData($"{cfgPath}.{suffix}");
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="cfgPath"></param>
        /// <returns></returns>
        public JsonData LoadCfg(string cfgPath)
        {
            return GetJsonData($"{cfgPath}");
        }

        /// <summary>
        /// 加载所有配置
        /// </summary>
        public async  void LoadAllCfg()
        {
            if (Loaded)
                return;
            Loaded = true;

            if (cfgDict != null && cfgDict.Count > 0)
            {
                return;
            }

            string path = "";
#if UNITY_EDITOR
            path = Application.dataPath + "/allcfg";
            foreach (var p in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                if (!p.Contains(".mate") && p.EndsWith(".txt"))
                {
                    int sindex = p.LastIndexOf('/');
                    if (Application.platform == RuntimePlatform.WindowsEditor)
                    {
                        sindex = p.LastIndexOf("\\");
                    }
                    int eindex = p.LastIndexOf('.');
                    if (sindex > 0 && eindex > 0 && (sindex + 1) <= eindex)
                    {
                        string name = p.Substring(sindex + 1, eindex - sindex - 1);
                        JsonData jd = LoadCfg(p);
                        if (jd != null && !string.IsNullOrEmpty(name))
                        {
                            cfgDict.Add(name, jd);
                        }
                    }
                }
            }

#else
            path = pathtool.loadrespath + "/allcfg";
             Debug.LogError("~`2 allcfg path:" + path);
            Dictionary<string, Object> cfgObjs = await ResourcesManager.Instance.GetAllAssetObjectDictByTag("DefaultPackage", "config");
             Debug.LogError("~`2 allcfg cfgObjs:" + cfgObjs);
            Debug.LogError("~`2 allcfg cfgObjs.Count:" + cfgObjs.Count);
            foreach (var cfg in cfgObjs)
            {
                string name = cfg.Key.Substring(0, cfg.Key.LastIndexOf('.'));
                Debug.LogError("~` allcfg cfg:" + cfg.Key + " - name:" + name);
                TextAsset textAsset = (TextAsset)cfg.Value;
                JsonData jd = jsontool.newwithstring(textAsset.ToString());
                Debug.LogError("~` allcfg jd:" + jsontool.tostring(jd));
                if (jd != null && !string.IsNullOrEmpty(name))
                {
                    cfgDict.Add(name, jd);
                }
            }
#endif
            //初始化消耗表数据
            List<CostCfgData> costCfgList = JsonToListClass<CostCfgData>("t_cost_prop");
            if(costCfgList != null)
            {
                foreach (var item in costCfgList)
                {
                    costCfgDatas.Add(item.costid, item);
                }
            }

            // 初始化物品数据
            List<ItemCfgData> listItemCfg = JsonToListClass<ItemCfgData>("t_prop");
            if (listItemCfg != null)
            {
                foreach (var item in listItemCfg)
                {
                    itemCfgDict.Add(item.pid, item);
                }
            }

            //初始化英雄配置数据
            heroInfoCfgDatasList = JsonToListClass<HeroInfoCfgData>("t_hero_info");
            if (heroInfoCfgDatasList!= null)
            {
                for (int i = 0; i < heroInfoCfgDatasList.Count; i++)
                {
                    HeroInfoCfgData data = heroInfoCfgDatasList[i];
                    dicHeroInfoCfgDatas.Add(heroInfoCfgDatasList[i].heroid, data);
                }
            }
            Debug.Log("英雄配置加载完成。。。。");
            //初始化英雄属性表
            List<HeroStatsCfg> heroStatsCfgList = JsonToListClass<HeroStatsCfg>("t_hero_attr");
            if (heroStatsCfgList != null)
            {
                for (int i = 0; i < heroStatsCfgList.Count; i++)
                {
                    HeroStatsCfg data = heroStatsCfgList[i];
                    heroStatsCfgs.Add(heroStatsCfgList[i].attrid, data);

                }
            }
            Debug.Log("英雄属性配置加载完成。。。。");
            //初始化英雄技能升级消耗表
            List<HeroSkillUpCostCfg> heroSkillUpCostCfgList = JsonToListClass<HeroSkillUpCostCfg>("t_hero_skillupcost");
            if (heroSkillUpCostCfgList != null)
            {
                for (int i = 0; i < heroSkillUpCostCfgList.Count; i++)
                {
                    if (!heroSkillUpCostCfgs.ContainsKey(heroSkillUpCostCfgList[i].id))
                    {
                        heroSkillUpCostCfgs.Add(heroSkillUpCostCfgList[i].id, new List<HeroSkillUpCostCfg>());
                    }
                    heroSkillUpCostCfgs[heroSkillUpCostCfgList[i].id].Add(heroSkillUpCostCfgList[i]);
                }
            }
            Debug.Log("英雄升级消耗配置加载完成。。。。");

            //初始化英雄升级\突破配置
            List<HeroLevelCfgData> heroLevelCfgDataList = JsonToListClass<HeroLevelCfgData>("t_hero_level");
            if (heroLevelCfgDataList != null)
            {
                for (int i = 0; i < heroLevelCfgDataList.Count; i++)
                {
                    HeroLevelCfgData data = heroLevelCfgDataList[i];

                    HeroInfoCfgData hero = GetHeroCfgDataByHeroID(data.heroid);

                    //加载升级信息
                    HeroLevelData level = new HeroLevelData();
                    level.heroid = data.heroid;
                    level.exp = data.exp;
                    level.state = data.state;
                    level.costid = data.breakcost;
                    level.herolevel = data.herolevel;
                    level.talentid = data.talentid;
                    if (data.attr != null)
                    {
                        //取出每一项属性
                        string[] stats = data.attr.Split("|");
                        List<StatData> statsList = new List<StatData>();
                        foreach (var item in stats)
                        {
                            string[] vals = item.Split(";");
                            long statid = long.Parse(vals[0]);
                            HeroStatsCfg statCfg = getHeroStstsCfgByAttrId(statid);
                            if (statCfg.heroshow==1) {
                                string text = GameCenter.mIns.m_LanMgr.GetLan(statCfg.name.ToString());
                                statsList.Add(new StatData(statid, text, statCfg.heroshow,statCfg.valuetype,statCfg.sort, int.Parse(vals[1])));
                            }
                        }
                        level.attrs = statsList.OrderBy(o=>o.sort).ToList();
                    }

                    hero.addLevelCfg(level);

                    if (data.exp == -1)
                    {
                        //加载突破数据
                        HeroBreakData breakdata = new HeroBreakData();
                        breakdata.herolevel = data.herolevel;
                        breakdata.heroid = data.heroid;
                        breakdata.state = data.state;
                        breakdata.attrs = level.attrs;
                        long costsid = data.breakcost;
                        CostCfgData costCfg = costCfgDatas.GetValueOrDefault(costsid);
                        if (costCfg != null)
                        {
                            breakdata.costs = costCfg.getCosts();
                        }

                        hero.addBreakCfg(breakdata);
                    }

                }
            }
            Debug.Log("英雄等级配置加载完成。。。。");
            commonCfgDatasList = JsonToListClass<CommonCfgData>("t_constant");
            if (commonCfgDatasList != null)
            {
                for (int i = 0; i < commonCfgDatasList.Count; i++)
                {
                    dicCommonCfgDatas.Add(commonCfgDatasList[i].constant, commonCfgDatasList[i].value);
                }
            }
            Debug.Log("常量配置加载完成。。。。");
            //初始化战斗以及技能相关配置表
            BattleCfgManager.Instance.InitBattleCfg();

            //武器数值模版表
            Dictionary<long, List<WeaponAttrModeCfg>> weaponAttrModeCfgMap = new Dictionary<long, List<WeaponAttrModeCfg>>();
            List<WeaponAttrModeCfg> weaponAttrModeCfgs = JsonToListClass<WeaponAttrModeCfg>("t_prop_weapon_attrmode");
            if (weaponAttrModeCfgs != null)
            {
                for (int i = 0; i < weaponAttrModeCfgs.Count; i++)
                {
                    WeaponAttrModeCfg data = weaponAttrModeCfgs[i];
                    if (!weaponAttrModeCfgMap.ContainsKey(data.attrmode))
                    {
                        weaponAttrModeCfgMap.Add(data.attrmode, new List<WeaponAttrModeCfg>());
                    }
                    weaponAttrModeCfgMap[data.attrmode].Add(data);
                }
            }

                //武器配置表数据
            List<WeaponDataCfg> weaponDataCfgs = JsonToListClass<WeaponDataCfg>("t_prop_weapon");
            if (weaponDataCfgs != null)
            {
                for (int i = 0; i < weaponDataCfgs.Count; i++)
                {
                    //加载武器数值数据
                    WeaponDataCfg weapon = weaponDataCfgs[i];
                    if (weapon.weaponid == 10100105)
                    {
                        Debug.Log("");
                    }
                    if (weaponAttrModeCfgMap.ContainsKey(weapon.attrmode)) {
                        List<WeaponAttrModeCfg> list = weaponAttrModeCfgMap.GetValueOrDefault(weapon.attrmode);
                        foreach (var item in list)
                        {
                            WeaponAttrMode mode =new WeaponAttrMode();
                            mode.attrmode = item.attrmode;
                            mode.level = item.level;
                            mode.state  = item.state;

                            if (item.attr != null)
                            {
                                //取出每一项属性
                                string[] stats = item.attr.Split("|");
                                List<StatData> statsList = new List<StatData>();
                                foreach (var stat in stats)
                                {
                                    string[] vals = stat.Split(";");
                                    long statid = long.Parse(vals[0]);
                                    HeroStatsCfg statCfg = getHeroStstsCfgByAttrId(statid);
                                    string text = GameCenter.mIns.m_LanMgr.GetLan(statCfg.name.ToString());
                                    statsList.Add(new StatData(statid, text, statCfg.heroshow, statCfg.valuetype, statCfg.sort, int.Parse(vals[1])));
                                }
                                mode.attrs = statsList.OrderBy(o => o.sort).ToList();
                            }

                            weapon.addWeaponAttrMode(mode);
                        }
                    }

                    this.weaponDataCfgs.Add(weapon.weaponid, weapon);
                }
            }

            //武器升级消耗表
            List<WeaponUpCostCfg> weaponUpCostCfgs = JsonToListClass<WeaponUpCostCfg>("t_prop_weapon_level");
            if (weaponUpCostCfgs != null)
            {
                for (int i = 0; i < weaponUpCostCfgs.Count; i++)
                {
                    WeaponUpCostCfg cfg = weaponUpCostCfgs[i];
                    HeroInfoCfgData hero = GetHeroCfgDataByHeroID(cfg.heroid);
                    if (hero == null) continue;

                    hero.addWeaponLevelCfg(cfg);
                    if (cfg.weaponexp == -1)
                    {
                        WeaponBreakCfg breakcfg  = new WeaponBreakCfg();
                        breakcfg.state = cfg.state;
                        breakcfg.heroid = cfg.heroid;
                        breakcfg.level = cfg.level;
                        CostCfgData costCfg = costCfgDatas.GetValueOrDefault(cfg.breakcost);
                        if (costCfg != null)
                        {
                            breakcfg.costs = costCfg.getCosts();
                        }
                        hero.addWeaponBreakCfg(breakcfg);
                    }
                }
            }

            //武器升星表（解析）
            List<WeaponStarDataCfg> weaponStarDataCfgs = JsonToListClass<WeaponStarDataCfg>("t_prop_weapon_star");
            if (weaponStarDataCfgs != null)
            {
                for (int i = 0; i < weaponStarDataCfgs.Count; i++)
                {
                    WeaponStarDataCfg data = weaponStarDataCfgs[i];

                    WeaponDataCfg weapon = GetWeaponCfgByWeaponID(data.weaponid);
                    if (weapon == null) continue;

                    WeaponStarData starData = new WeaponStarData();
                    starData.weaponid = data.weaponid;
                    starData.weaponskill = data.weaponskill;
                    starData.star = data.star;
                    if (data.cost != -1) {
                        CostCfgData costCfg = costCfgDatas.GetValueOrDefault(data.cost);
                        if (costCfg != null)
                        {
                            starData.costs = costCfg.getCosts();
                        }
                    }

                    weapon.addWeaponStarData(starData);
                }
            }
            Debug.Log("武器配置加载完成。。。。");
            heroSkillTrees = JsonToListClass<HeroSkillTreeCfg>("t_hero_skilltree");


            //配置表id要唯一

            RegisterConfig<NpcConfig>("t_npc_info");//npc属性表

            RegisterConfig<InteractionConfig>("t_npc_interact");//交互配置

            RegisterConfig<DialogueConfig>("t_npc_talk");//对话配置

            RegisterConfig<NpcMapConfig>("t_npc_map");//npc地图表

            LogBookManager.Instance.InitCfg();

            DropManager.Instance.InitAllDropCfg();

            TaskCfgManager.Instance.InitAllCfg();

            JumpManger.Instance.InitJumpCfg();
        }


        /// <summary>
        /// 获得常量表配置
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetCommonCfgByName(string name)
        {

            if (dicCommonCfgDatas.ContainsKey(name))
            {
                return dicCommonCfgDatas[name];
            }
            return null;
        }


        /// <summary>
        /// 获取对应配置表的json数据
        /// </summary>
        public JsonData GetCfg(string cfgName)
        {
            return cfgDict.ContainsKey(cfgName) ? cfgDict[cfgName] : null;
        }

        /// <summary>
        /// json转对应class
        /// </summary>
        /// <typeparam name="T">对应class类</typeparam>
        /// <param name="cfgName">配置表名称</param>
        /// <param name="idkey">用于定位的key名称</param>
        /// <param name="idValue">用于定位的key值</param>
        /// <returns>填充数据后的对应类</returns>
        public T JsonToClass<T>(string cfgName, string idkey, int idValue) where T : class
        {
            if (!string.IsNullOrEmpty(idkey) && idValue > 0 && cfgDict.ContainsKey(cfgName))
            {
                foreach (JsonData _jd in cfgDict[cfgName])
                {
                    if (_jd[idkey] != null && int.Parse(_jd[idkey].ToString()) == idValue)
                    {
                        return JsonMapper.ToObject<T>(JsonMapper.ToJson(_jd));
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// json转列表类(针对整个类型为数组类型的json表)
        /// </summary>
        /// <typeparam name="T">对应class类</typeparam>
        /// <param name="cfgName">配置表名称</param>
        /// <returns>填充数据后的对应类列表</returns>
        public List<T> JsonToListClass<T>(string cfgName) where T : class
        {
            if (cfgDict.ContainsKey(cfgName))
            {
                return JsonMapper.ToObject<List<T>>(JsonMapper.ToJson(cfgDict[cfgName]));
            }
            return null;
        }

        /// <summary>
        /// json转列表类(针对非数组表)
        /// </summary>
        /// <typeparam name="T">对应class类</typeparam>
        /// <param name="cfgName">配置表名称</param>
        /// <returns>填充数据后的对应类</returns>
        public T JsonToSingleClass<T>(string cfgName) where T : class
        {
            if (cfgDict.ContainsKey(cfgName))
            {
                return JsonMapper.ToObject<T>(JsonMapper.ToJson(cfgDict[cfgName]));
            }
            return null;
        }

        /// <summary>
        /// 根据某个key的值，获取key所在数据组中对应getkey的值-object
        /// </summary>
        /// <param name="cfgName">配置表名称</param>
        /// <param name="idkey">定位key</param>
        /// <param name="idValue">定位值</param>
        /// <param name="getKey">需要获取的key</param>
        /// <returns>需要获取的值</returns>
        public object GetCfgValue(string cfgName, string idkey, int idValue, string getKey)
        {
            if (!string.IsNullOrEmpty(idkey) && idValue > 0 && cfgDict.ContainsKey(cfgName))
            {
                foreach (JsonData _jd in cfgDict[cfgName])
                {
                    if (_jd[idkey] != null && int.Parse(_jd[idkey].ToString()) == idValue)
                    {
                        return _jd[getKey];
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// 根据某个key的值，获取key所在数据组中对应getkey的值-object
        /// </summary>
        /// <param name="cfgName">配置表名称</param>
        /// <param name="idkey">定位key</param>
        /// <param name="idValue">定位值</param>
        /// <param name="getKey">需要获取的key</param>
        /// <returns>需要获取的值</returns>
        public object GetCfgValue(string cfgName, string idkey, long idValue, string getKey)
        {
            if (!string.IsNullOrEmpty(idkey) && idValue > 0 && cfgDict.ContainsKey(cfgName))
            {
                foreach (JsonData _jd in cfgDict[cfgName])
                {
                    if (_jd[idkey] != null && long.Parse(_jd[idkey].ToString()) == idValue)
                    {
                        return _jd[getKey];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 根据某个key的值，获取key所在数据组中对应getkey的值-string
        /// </summary>
        /// <param name="cfgName">配置表名称</param>
        /// <param name="idkey">定位key</param>
        /// <param name="idValue">定位值</param>
        /// <param name="getKey">需要获取的key</param>
        /// <returns>需要获取的值</returns>
        public string GetCfgStringValue(string cfgName, string idkey, int idValue, string getKey)
        {
            return GetCfgValue(cfgName, idkey, idValue, getKey).ToString();
        }

        /// <summary>
        /// 根据某个key的值，获取key所在数据组中对应getkey的值-int
        /// </summary>
        /// <param name="cfgName">配置表名称</param>
        /// <param name="idkey">定位key</param>
        /// <param name="idValue">定位值</param>
        /// <param name="getKey">需要获取的key</param>
        /// <returns>需要获取的值</returns>
        public int GetCfgIntValue(string cfgName, string idkey, int idValue, string getKey)
        {
            return int.Parse(GetCfgValue(cfgName, idkey, idValue, getKey).ToString());
        }

        /// <summary>
        /// 根据某个key的值，获取key所在数据组中对应getkey的值-bool
        /// </summary>
        /// <param name="cfgName">配置表名称</param>
        /// <param name="idkey">定位key</param>
        /// <param name="idValue">定位值</param>
        /// <param name="getKey">需要获取的key</param>
        /// <returns>需要获取的值</returns>
        public bool GetCfgBoolValue(string cfgName, string idkey, int idValue, string getKey)
        {
            return bool.Parse(GetCfgValue(cfgName, idkey, idValue, getKey).ToString());
        }

        /// <summary>
        /// 获取全局表配置
        /// </summary>
        /// <param name="idkeyy">对应全局表constant值</param>
        public string GetConstantValue(string idkey)
        {
            foreach (JsonData _jd in cfgDict["t_constant"])
            {
                if (_jd["constant"] != null && _jd["constant"].ToString() == idkey)
                {
                    return _jd["value"].ToString();
                }
            }
            return string.Empty;
        }

        private void RegisterConfig<T>(string path) where T : Config
        {
            List<T> configList = JsonToListClass<T>(path);

            Dictionary<long, Config> dic = new Dictionary<long, Config>();

            if(configList != null)
            {
                foreach (var item in configList)
                {
                    dic.Add(item.id, item);
                }
            }
            GameConfig.AddConfig<T>(dic);
        }

    }
}
