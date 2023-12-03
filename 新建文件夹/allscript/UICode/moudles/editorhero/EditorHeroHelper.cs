
//#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.Text;
using System.Reflection;

/// <summary>
/// 幻灵保存的数据结构(lua)
/// </summary>
/* * * * * * * * * *
 * events:
 * * * * * * * * * *
    death={
        actname={
            {anilen=2200,logiclen=2200,name="death",offset=0,tname="death"}
        },
        effects={
            {
                angle={0,0,0},
                duration=-1,
                effabname="101001_1",
                effectname="101001_sanghe_skill8",
                ignore=false,
                name="",
                point="point_foot",
                pos={0,0,0},
                scale={1,1,1},
                time=0
            }
        },
        events={1897},
        randomvoice={
            {delay=0,name="die_1",voiceabname="audio_101001_cn"},
            {delay=0,name="die_2",voiceabname="audio_101001_cn"}
        },
        sounds={
            {delay=200,name="101001_vpveskill1",voiceabname="audio_101001_cn"}
        }
    },

 * * * * * * * * * *
 * point:
 * * * * * * * * * *
    return
    {
        body={path="body"},
        foot={path="foot"},
        head={path="head"},
        logicpos={
            backside={x=0,y=1.55,z=0},
            foot={x=0,y=0.0,z=0},
            head={x=0,y=1.87,z=0},
            hproot={x=0,y=3.0,z=0},
            hurttextpos={x=0,y=2.0,z=0}
        },
        point_ass={path="model/rotate/role_101001_1/Bip001/Bip001 Pelvis/point_ass"},
        point_backside={path="model/rotate/role_101001_1/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/point_backside"},
        point_foot={path="model/rotate/role_101001_1/point_foot"},
        point_head={path="model/rotate/role_101001_1/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 Head/point_head"},
        point_headtop={path="model/rotate/role_101001_1/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 Head/point_headtop"},
        point_lefthand={path="model/rotate/role_101001_1/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 L Clavicle/Bip001 L UpperArm/Bip001 L Forearm/Bip001 L Hand/point_lefthand"},
        point_righthand={path="model/rotate/role_101001_1/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 R Clavicle/Bip001 R UpperArm/Bip001 R Forearm/Bip001 R Hand/Bone42/point_righthand"},
        point_weapon={path="model/rotate/role_101001_1/ZHF_Bone07/ZHF_Bone05/ZHF_Bone09/ZHF_Bone03/ZHF_Bone02/ZHF_Bone10/point_weapon"}
    }

 */

public static class EditorHeroHelper
{
    public static JsonData HeroActionPointToJsonData(Transform modelTran, int heroId)
    {
        if (modelTran != null)
        {
            //EditotHeroPointData editotHeroPointData = new EditotHeroPointData();
            //editotHeroPointData.paths = new Dictionary<string, string>();
            //editotHeroPointData.logicpos = new Dictionary<string, Vector3>();
            Transform[] nodes = modelTran.GetComponentsInChildren<Transform>();
            JsonData jsonData = new JsonData();
            JsonData logicposJD = new JsonData();

            FBXBindBonesHelper fbxBindBonesHelper = modelTran.GetComponent<FBXBindBonesHelper>();
            // Debug.LogError("~~ fbxBindBonesHelper:" + fbxBindBonesHelper + " - modelTran:" + modelTran);
            if (fbxBindBonesHelper != null && fbxBindBonesHelper.Bones != null)
            { 
                fbxBindBonesHelper.Bind();
                foreach (var kv in fbxBindBonesHelper.Bones)
                {
                    jsonData[kv.Key.ToString()] = kv.Key.ToString();
                    if (kv.Key == FBXBoneType.point_foot)
                    {
                        SetLogicpos(logicposJD, "foot", kv.Value);
                    }
                    if (kv.Key == FBXBoneType.point_head)
                    {
                        SetLogicpos(logicposJD, "head", kv.Value);
                    }
                    if (kv.Key == FBXBoneType.point_headtop)
                    {
                        SetLogicpos(logicposJD, "hproot", kv.Value);
                    }
                    if (kv.Key == FBXBoneType.point_headtop)
                    {
                        SetLogicpos(logicposJD, "hurttextpos", kv.Value);
                    }
                    if (kv.Key == FBXBoneType.point_backside)
                    {
                        SetLogicpos(logicposJD, "backside", kv.Value);
                    }
                }
            }
            //foreach (Transform node in nodes)
            //{
            //    if (node.name.Contains("point"))
            //    {
            //        jsonData[node.name] = node.name;
            //    }
            //    if (node.name.Contains("body"))
            //    {
            //        jsonData["body"] = node.name;
            //    }
            //    if (node.name.Contains("foot"))
            //    {
            //        jsonData["foot"] = node.name;
            //        SetLogicpos(logicposJD, "foot", node);
            //    }
            //    if (node.name.Contains("head"))
            //    {
            //        jsonData["head"] = node.name;
            //        SetLogicpos(logicposJD, "head", node);
            //    }
            //    if (node.name.Contains("headtop"))
            //    {
            //        // SetLogicpos(logicposJD, "headtop", node);
            //        SetLogicpos(logicposJD, "hproot", node);
            //        SetLogicpos(logicposJD, "hurttextpos", node);
            //    }
            //    if (node.name.Contains("backside"))
            //    {
            //        SetLogicpos(logicposJD, "backside", node);
            //    }
            //}
            jsonData["logicpos"] = logicposJD;
            WriteFile(jsonData, heroId, "actpoint");
            return jsonData;
        }
        return null;
    }

    private static void SetLogicpos(JsonData jsonData, string name, Transform node)
    {
        jsonData[name] = new JsonData();
        jsonData[name]["x"] = node.localPosition.x;
        jsonData[name]["y"] = node.localPosition.y;
        jsonData[name]["z"] = node.localPosition.z;
    }

    public static JsonData HeroActionEventDataToJsonData(EditorHeroData editotHeroData)//, AnimationClip[] animationClips)
    {
        if (editotHeroData == null)
        {
            return null;
        }

        if (editotHeroData.actDatas != null && editotHeroData.actDatas.Count > 0)// && editotHeroData.actDatas != null)
        {
            EditotHeroPointData pointData = editotHeroData.pointData;

            JsonData jd = new JsonData();
            foreach (var _actData in editotHeroData.actDatas)
            {
                EditorHeroActionData actData = _actData;
                string animName = _actData.animName;
                jd[animName] = new JsonData();
                Debug.Log("```````````````````````````````````````````````````");
                Debug.Log(" ---- animName:" + animName);
                //foreach (var item in editotHeroData.actDatas)
                //{
                //    if (animName == item.animName)
                //    {
                //        actData = item;
                //    }
                //}
                // actname
                JsonData jd_actname = new JsonData();
                if (actData != null)
                {
                    Debug.Log("`````````````````````  actData.editorHeroActionAnimListDatas: " + actData.editorHeroActionAnimListDatas.Count);
                    foreach (EditorHeroActionAnimListData data in actData.editorHeroActionAnimListDatas)
                    {
                        JsonData jd_actname_i = new JsonData();
                        jd_actname_i["anilen"] = data.len;
                        jd_actname_i["logiclen"] = data.param1;
                        jd_actname_i["name"] = data.animName;
                        jd_actname_i["offset"] = data.param2;
                        jd_actname_i["tname"] = data.animName;
                        jd_actname_i["uid"] = data.uid;
                        jd_actname.Add(jd_actname_i);
                    }
                }
                CheckEmptyTable(jd_actname);
                // 给默认
                //if (jd_actname.IsNone)
                //{
                //    JsonData jd_actname_i = new JsonData();
                //    jd_actname_i["anilen"] = (int)(clip.length * 1000);
                //    jd_actname_i["logiclen"] = (int)(clip.length * 1000);
                //    jd_actname_i["name"] = animName;
                //    jd_actname_i["offset"] = 0;
                //    jd_actname_i["tname"] = animName;
                //    jd_actname.Add(jd_actname_i);
                //}
                jd[animName]["actname"] = jd_actname;

                // effects
                JsonData jd_effects = new JsonData();
                if (actData != null && actData.effectData != null)
                {
                    foreach (var ed in actData.effectData)
                    {
                        JsonData jd_e_i = new JsonData();
                        JsonData jd_e_i_r = new JsonData();
                        jd_e_i_r["x"] = ed.rotation.x;
                        jd_e_i_r["y"] = ed.rotation.y;
                        jd_e_i_r["z"] = ed.rotation.z;

                        JsonData jd_e_i_s = new JsonData();
                        jd_e_i_s["x"] = ed.scale.x;
                        jd_e_i_s["y"] = ed.scale.y;
                        jd_e_i_s["z"] = ed.scale.z;

                        JsonData jd_e_i_o = new JsonData();
                        jd_e_i_o["x"] = ed.offest.x;
                        jd_e_i_o["y"] = ed.offest.y;
                        jd_e_i_o["z"] = ed.offest.z;

                        jd_e_i["uid"] = ed.uid;
                        jd_e_i["angle"] = jd_e_i_r;
                        jd_e_i["duration"] = ed.duration;
                        jd_e_i["effabname"] = ed.effectResName;
                        jd_e_i["effectname"] = ed.effectName;
                        jd_e_i["ignore"] = ed.useRes;
                        jd_e_i["name"] = ed.effectName;
                        jd_e_i["point"] = ed.effectPoint;
                        jd_e_i["pos"] = jd_e_i_o;
                        jd_e_i["scale"] = jd_e_i_s;
                        jd_e_i["time"] = ed.delay;
                        jd_effects.Add(jd_e_i);
                    }
                }
                CheckEmptyTable(jd_effects);
                jd[animName]["effects"] = jd_effects;
                // event
                JsonData jd_events = new JsonData();
                if (actData != null)
                {
                    foreach (int tk in actData.events)
                    {
                        jd_events.Add(tk);
                    }
                }
                CheckEmptyTable(jd_events);
                jd[animName]["events"] = jd_events;

                // randomvoice
                JsonData jd_randomvoice = new JsonData();
                CheckEmptyTable(jd_randomvoice);
                jd[animName]["randomvoice"] = jd_randomvoice;

                // sounds
                JsonData jd_sounds = new JsonData();
                CheckEmptyTable(jd_sounds);
                jd[animName]["sounds"] = jd_sounds;

                // jump data
                JsonData jd_jumps = new JsonData();
                if (actData != null)
                {
                    if (actData.jumpData != null
                        && (actData.jumpData.timeEnter1 >= 0 && actData.jumpData.timeEnter1 <= actData.jumpData.timeEnter2)
                        && (actData.jumpData.timeLevae1 >= actData.jumpData.timeEnter2 && actData.jumpData.timeLevae1 <= actData.jumpData.timeLevae2)
                        && actData.jumpData.dist > 0)
                    {
                        JsonData jd_jumps_i = new JsonData();
                        jd_jumps_i["time1"] = actData.jumpData.timeEnter1;
                        jd_jumps_i["time2"] = actData.jumpData.timeEnter2;
                        jd_jumps_i["time3"] = actData.jumpData.timeLevae1;
                        jd_jumps_i["time4"] = actData.jumpData.timeLevae2;
                        jd_jumps_i["dist"] = actData.jumpData.dist;
                        jd_jumps_i["dire"] = actData.jumpData.dire;
                        jd_jumps.Add(jd_jumps_i);
                    }
                }
                CheckEmptyTable(jd_jumps);
                jd[animName]["jump"] = jd_jumps;

                WriteFile(jd, editotHeroData.id);
            }
        }
        return null;
    }

    private static string eventFilePath = "/allcfg/actevents/";
    private static string pointFilePath = "/allcfg/actpoints/";

    private static string GetEventFilePath(int heroId)
    {
        return Application.dataPath + eventFilePath + $"event_{heroId}.txt";
    }

    private static string GetPointFilePath(int heroId)
    {
        return Application.dataPath + pointFilePath + $"point_{heroId}.txt";
    }

    private static void CheckEmptyTable(JsonData jsonData)
    {
        if (jsonData.IsNone)
        {
            jsonData.Add(new JsonData());
        }
    }

    // 写入文件
    public static void WriteFile(JsonData jsonData, int heroId, string stype = "actevent")
    {
        string filePath = "";
        if (stype.Equals("actevent"))
            filePath = GetEventFilePath(heroId);
        else
            filePath = GetPointFilePath(heroId);

        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }
     
        FileInfo file = new FileInfo(filePath);
        // 判断有没有文件，有则打开文件，，没有创建后打开文件
        StreamWriter sw = file.CreateText();
        sw.Close();
        sw.Dispose();

        string json = JsonMapper.ToJson(jsonData);
        //sw.WriteLine(json);
        UTF8Encoding m_utf8 = new UTF8Encoding(false);
        File.WriteAllText(filePath, json, m_utf8);

        //sw.Close();
        //sw.Dispose();

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    // 读取文件
    public static JsonData ReadFileToJson(int heroId, string stype = "actevent")
    {

        string filePath = "";
        if (stype.Equals("actevent"))
            filePath = GetEventFilePath(heroId);
        else
            filePath = GetPointFilePath(heroId);

        if (string.IsNullOrEmpty(filePath))
        {
            return null;
        }

        if (!File.Exists(filePath))
        {
            return null;
        }

        //StringBuilder stringBuilder = new StringBuilder();
        //// 读取文件
        //using (FileStream fs_read = File.OpenRead(filePath))
        //{
        //    byte[] byteArr = new byte[1024];
        //    UTF8Encoding coding = new UTF8Encoding(true);
        //    while (fs_read.Read(byteArr, 0, byteArr.Length) > 0)
        //    {
        //        string s = coding.GetString(byteArr);
        //        stringBuilder.Append(s);
        //    }
        //}
        string s = File.ReadAllText(filePath);
        JsonData jd = JsonMapper.ToObject(s);
        return jd;
        //EditotHeroPointData pointData = editotHeroData.pointData;
    }

    public static EditotHeroPointData ReadEventFileToEditotHeroPointData(int heroId)
    {
        JsonData jd = ReadFileToJson(heroId, "actpoint");
        EditotHeroPointData ehpd = new EditotHeroPointData();
        ehpd.paths = new Dictionary<string, string>();
        ehpd.logicpos = new Dictionary<string, Vector3>();
        if (jd != null)
        {
            List<string> keys = new List<string>(jd.Keys);
            foreach (JsonData k in keys)
            {
                if (k.ToString() != "logicpos")
                {
                    ehpd.paths.Add(k.ToString(), jd[k.ToString()].ToString());
                }
                else
                {
                    JsonData ld = jd["logicpos"];
                    List<string> keys_ld = new List<string>(ld.Keys);
                    foreach (JsonData k_ld in keys_ld)
                    {
                        string kk = k_ld.ToString();
                        JsonData ld_s = ld[kk];
                        Vector3 p = new Vector3(float.Parse(ld_s["x"].ToString()), float.Parse(ld_s["y"].ToString()), float.Parse(ld_s["z"].ToString()));
                        ehpd.logicpos.Add(kk, p);
                    }
                }
            }
         }
        return ehpd;
    }

    public static List<EditorHeroActionData> ReadEventFileToData(int heroId)
    {
        JsonData jd = ReadFileToJson(heroId, "actevent");
        List<EditorHeroActionData> actDatas = new List<EditorHeroActionData>();
        if (jd != null)
        {
            List<string> keys = new List<string>(jd.Keys);
            foreach (JsonData k in keys)
            {
                List<EditorHeroActionAnimListData> editorHeroActionAnimListDatas = new List<EditorHeroActionAnimListData>();
                List<int> events = new List<int>();
                List<EditorHeroEffectPopData> effectData = new List<EditorHeroEffectPopData>();
                //float timeScale = 0;
                EditorHeroJumpData jumpData = new EditorHeroJumpData();

                string animName = k.ToString();
                Debug.Log("~~ animName:" + animName);
                JsonData actjd = jd[animName];
                if (actjd.ContainsKey("actname"))
                {
                    foreach (JsonData _d in actjd["actname"])
                    {
                        EditorHeroActionAnimListData editorHeroActionAnimListData = new EditorHeroActionAnimListData();
                        editorHeroActionAnimListData.animName = _d["name"].ToString();
                        editorHeroActionAnimListData.len = int.Parse(_d["anilen"].ToString());
                        editorHeroActionAnimListData.param1 = int.Parse(_d["logiclen"].ToString());
                        editorHeroActionAnimListData.param2 = int.Parse(_d["offset"].ToString());
                        editorHeroActionAnimListData.uid = long.Parse(_d["uid"].ToString());
                        editorHeroActionAnimListDatas.Add(editorHeroActionAnimListData);
                    }

                }

                if (actjd.ContainsKey("effects"))
                {
                    foreach (JsonData _d in actjd["effects"])
                    {
                        EditorHeroEffectPopData editorHeroEffectPopData = new EditorHeroEffectPopData();
                        editorHeroEffectPopData.rotation = new Vector3(float.Parse(_d["angle"]["x"].ToString()), float.Parse(_d["angle"]["y"].ToString()), float.Parse(_d["angle"]["z"].ToString()));
                        editorHeroEffectPopData.offest = new Vector3(float.Parse(_d["pos"]["x"].ToString()), float.Parse(_d["pos"]["y"].ToString()), float.Parse(_d["pos"]["z"].ToString()));
                        editorHeroEffectPopData.scale = new Vector3(float.Parse(_d["scale"]["x"].ToString()), float.Parse(_d["scale"]["y"].ToString()), float.Parse(_d["scale"]["z"].ToString()));
                        editorHeroEffectPopData.duration = float.Parse(_d["duration"].ToString());
                        editorHeroEffectPopData.effectResName = _d["effabname"].ToString();
                        editorHeroEffectPopData.effectName = _d["effectname"].ToString();
                        editorHeroEffectPopData.useRes = bool.Parse(_d["ignore"].ToString());
                        editorHeroEffectPopData.animName = _d["name"].ToString();
                        editorHeroEffectPopData.effectPoint = _d["point"].ToString();
                        editorHeroEffectPopData.delay = float.Parse(_d["time"].ToString());
                        editorHeroEffectPopData.uid = long.Parse(_d["uid"].ToString());
                        effectData.Add(editorHeroEffectPopData);
                    }
                }

                if (actjd.ContainsKey("events"))
                {
                    foreach (JsonData _d in actjd["events"])
                    {
                        events.Add(int.Parse(_d.ToString()));
                    }
                }

                if (actjd.ContainsKey("randomvoice"))
                {
                    foreach (JsonData _d in actjd["randomvoice"])
                    {
                    }
                }

                if (actjd.ContainsKey("sounds"))
                {
                    foreach (JsonData _d in actjd["sounds"])
                    {
                    }
                }

                if (actjd.ContainsKey("jump"))
                {
                    foreach (JsonData _d in actjd["jump"])
                    {
                        jumpData.timeEnter1 = float.Parse(_d["time1"].ToString());
                        jumpData.timeEnter2 = float.Parse(_d["time2"].ToString());
                        jumpData.timeLevae1 = float.Parse(_d["time3"].ToString());
                        jumpData.timeLevae2 = float.Parse(_d["time4"].ToString());
                        jumpData.dist = float.Parse(_d["dist"].ToString());
                        jumpData.dire = float.Parse(_d["dire"].ToString());
                    }
                }

                EditorHeroActionData editorHeroActionData = new EditorHeroActionData();
                editorHeroActionData.animName = animName;
                editorHeroActionData.editorHeroActionAnimListDatas = editorHeroActionAnimListDatas;
                editorHeroActionData.events = events;
                editorHeroActionData.effectData = effectData;
                editorHeroActionData.jumpData = jumpData;
                actDatas.Add(editorHeroActionData);
            }
        }
        else
        {
            return actDatas;
        }
        return actDatas;
    }

    public static T DeepCopy<T>(T obj)
    {
        //如果是字符串或值类型则直接返回
        if (obj is string || obj.GetType().IsValueType) return obj;


        object retval = Activator.CreateInstance(obj.GetType());
        FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        foreach (FieldInfo field in fields)
        {
            try { field.SetValue(retval, DeepCopy(field.GetValue(obj))); }
            catch { }
        }
        return (T)retval;
    }

    public static string GetEffectResPath(int heroId)
    {
        return Application.dataPath + "/allmodel/effs/" + $"effs_{heroId}_1/" + "prefabs";
    }

    public static string GetEffectResLoadPath(int heroId)
    {
        return "Assets/allmodel/effs/" + $"effs_{heroId}_1/" + "prefabs";
    }

    public static List<string> GetEffects(int heroId)
    {
        List<string> rs = new List<string>();
        string path = GetEffectResPath(heroId);
        string[] paths = Directory.GetFiles(path);
        foreach (var p in paths)
        {
            if (!p.EndsWith(".meta") && p.EndsWith(".prefab"))
            {
                Debug.Log(p);

                   
                int si = p.LastIndexOf("/");
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    si = p.LastIndexOf("\\");
                }
                int ei = p.LastIndexOf(".");
                string s = p.Substring(si + 1, ei - (si + 1));
                Debug.Log(s);
                rs.Add(s);
            }
        }
        return rs;
    }
}
//#endif