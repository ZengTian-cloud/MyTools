using UnityEngine;
using Basics;
using LitJson;
using UnityEngine.Rendering;

namespace Managers
{
    public class FogManager : SingletonOjbect
    {
        // ambient
        public Color subtractiveShadowColor; // == realtimeShadowColor { get; set; }
        public int ambientMode;
        public Color ambientLight;
        public float ambientIntensity;

        public bool fog = true;
        public Color fogColor;
        public int fogMode;
        public float flareFadeSpeed;
        public float flareStrength;
        public float fogDensity;
        public float fogStartDistance;
        public float fogEndDistance;

        public Color JsonToColor(string jsonStr)
        {
            string[] rgba = jsonStr.Split(',');
            if (rgba.Length == 4)
            {
                return new Color(float.Parse(rgba[0]), float.Parse(rgba[1]), float.Parse(rgba[2]), float.Parse(rgba[3]));
            }
            return Color.white;
        }

        public void SetEnvironmentConfig(JsonData jsonData)
        {
            fog = bool.Parse(jsonData["fog"].ToString());
            fogColor = JsonToColor(jsonData["fogColor"].ToString());
            fogMode = int.Parse(jsonData["fogMode"].ToString());
            flareFadeSpeed = float.Parse(jsonData["flareFadeSpeed"].ToString());
            flareStrength = float.Parse(jsonData["flareStrength"].ToString());
            fogDensity = float.Parse(jsonData["fogDensity"].ToString());
            fogStartDistance = float.Parse(jsonData["fogStartDistance"].ToString());
            fogEndDistance = float.Parse(jsonData["fogEndDistance"].ToString());

            subtractiveShadowColor = JsonToColor(jsonData["subtractiveShadowColor"].ToString());
            ambientMode = int.Parse(jsonData["ambientMode"].ToString());
            ambientLight = JsonToColor(jsonData["ambientLight"].ToString());
            ambientIntensity = float.Parse(jsonData["ambientIntensity"].ToString());

            RenderSettings.fog = fog;
            RenderSettings.fogMode = (FogMode)fogMode;
            RenderSettings.fogColor = fogColor;
            RenderSettings.flareFadeSpeed = flareFadeSpeed;
            RenderSettings.flareStrength = flareStrength;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;

            RenderSettings.ambientMode = (UnityEngine.Rendering.AmbientMode)ambientMode;
            RenderSettings.ambientLight = ambientLight;
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.subtractiveShadowColor = subtractiveShadowColor;
        }

        public void SetSceneSetting(string sceneName)
        {
            JsonData jsonData = GameCenter.mIns.m_CfgMgr.GetCfg("scene_" + sceneName);
            if (jsonData != null)
            {
                SetEnvironmentConfig(jsonData);
            }
            else
            {
                // 默认取main01配置
                JsonData def_jd = GameCenter.mIns.m_CfgMgr.GetCfg("scene_main01");
                if (def_jd != null)
                {
                    SetEnvironmentConfig(def_jd);
                }
                else
                {
                    // 默认
                    RenderSettings.fog = true;
                    RenderSettings.fogMode = FogMode.Exponential;
                    RenderSettings.fogColor = new Color(6 / 255, 41 / 255, 1, 1);
                    RenderSettings.flareFadeSpeed = 3;
                    RenderSettings.flareStrength = 1;
                    RenderSettings.fogDensity = 0.01f;
                    //RenderSettings.fogStartDistance = fogStartDistance;
                    //RenderSettings.fogEndDistance = fogEndDistance;

                    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
                    // RenderSettings.ambientLight = ambientLight;
                    // RenderSettings.ambientIntensity = ambientIntensity;
                    RenderSettings.subtractiveShadowColor = new Color(107 / 255, 122 / 255, 160 / 255, 1);
                }
            }
            UpdateVol(sceneName);
        }

        /// <summary>
        /// 更新场景后效
        /// TODO:规整写法
        /// </summary>
        /// <param name="sceneName"></param>
        private void UpdateVol(string sceneName)
        {
            //if (sceneName.Contains("UI_main"))
            //{
            //    JsonData volCfg = GameCenter.mIns.m_CfgMgr.GetCfg("t_hero_vol");
            //    Debug.LogError("volCfg:" + jsontool.tostring(volCfg));
            //    if (volCfg != null)
            //    {
            //        int id = 0;
            //        string sname = "";
            //        if (sceneName.Contains("water")) {
            //            id = 1001;
            //            sname = "main_water_01";
            //        }
            //        else if (sceneName.Contains("fire")) { 
            //            id = 1004;
            //            sname = "main_fire_01";
            //        }
            //        else if (sceneName.Contains("wind")) { 
            //            id = 1003;
            //            sname = "main_wind_01";
            //        }
            //        else if (sceneName.Contains("mine")) { 
            //            id = 1002;
            //            sname = "main_mine_01";
            //        }
            //        if (id > 0)
            //        {
            //            JsonData oneCfg = null;
            //            foreach (JsonData _oneCfg in volCfg)
            //            {
            //                if (_oneCfg["id"].ToString() == id.ToString())
            //                {
            //                    oneCfg = _oneCfg;
            //                }

            //            }

            //            // 找到vol
            //            GameObject root = GameObject.Find("HeroGrowSecnceRoot(Clone)");
            //            GameObject vol = root.transform.Find(sname).Find("vol/Global Volume").gameObject;
            //            Volume volume = vol.GetComponent<Volume>();
            //            if (volume != null)
            //            {
            //                volume.sharedProfile
            //            }

            //        }
            //    }
            //}
        }
    }
}