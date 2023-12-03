using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class HeroGrowCfg
{
    public int selectTab = 0;
    public long selectHero = -1;
}

public static class HeroGrowUtils
{

    //储存返回类型，用于判断返回按扭的事件
    public static BackType backType = BackType.Info; 
    public static Dictionary<long,int> heroWeaponSelectedTypes = new Dictionary<long, int>();
    /// <summary>
    /// 获取英雄切换时应该选中的武器
    /// </summary>
    /// <param name="heroid"></param>
    /// <param name="allWeapon"></param>
    /// <param name="weaponid"></param>
    /// <returns></returns>
    public static int getHeroWeaponSelectedType(long heroid, Dictionary<int, WeaponDataCfg> allWeapon,long weaponid)
    {
        if (heroWeaponSelectedTypes.ContainsKey(heroid))
        { 
            return heroWeaponSelectedTypes[heroid];
        }
        foreach(var item in allWeapon)
        {
            if (item.Value.weaponid == weaponid)
            {
                return item.Value.type;
            }
        }
        Debug.Log($"没有找到武器->>>heroid:{heroid},weaponid:{weaponid}");
        return 1;
    }
    public static void setHeroWeaponSelected(long heroid,int type)
    {
        bool ex = heroWeaponSelectedTypes.TryAdd(heroid, type);
        if (!ex) heroWeaponSelectedTypes[heroid] = type;
    }
    /// <summary>
    /// 设置星级的UI样式
    /// </summary>
    /// <param name="star"></param>
    /// <param name="isG">是否显示黄色星星，这个在突破的时候用得着</param>
    /// <param name="parent"></param>
    public static void setStarUi(int state, bool isG, Transform parent)
    {
        for (int i = 1; i <= 6; i++)
        {
            GameObject starImgH = parent.Find("star" + i + "/h")?.gameObject;
            GameObject starImgG = parent.Find("star" + i + "/g")?.gameObject;
            GameObject starImgC = parent.Find("star" + i + "/c")?.gameObject;
            if (i <= state)
            {
                starImgH.SetActive(false);
                starImgG?.SetActive(false);
                starImgC.SetActive(true);
            }
            else if (isG && i == state + 1 && starImgG != null)
            {
                starImgH.SetActive(false);
                starImgG?.SetActive(true);
                starImgC.SetActive(false);
            }
            else
            {
                starImgH.SetActive(true);
                starImgG?.SetActive(false);
                starImgC.SetActive(false);
            }

        }
    }

    public static string setCoinCountUI(int count,int max)
    {
        if (count > max)
            return "<color=#a92020>" + count+"</color>";
        else if(count==0)
            return "<color=#303030>" + count + "</color>";
        else
            return "<color=green>" + count + "</color>";
    }

    public static string parsePropCountStr(int count, int max)
    {
        string c = "green";
        if (count == 0) c = "#303030";
        if (count > max) c = "#a92020";
        max = max > 999 ? 999 : max;
        return "<color=" + c + ">" + count + "</color><color=#303030>/" + max + "</color>";
    }
    public static string getWeaponStarIcon(int star)
    { 
        switch (star)
        {
            case 1:
            default:
                return "ui_d_icon_wuqixinhao_02";
            case 2:
                return "ui_d_icon_wuqixinhao_02";
            case 3:
                return "ui_d_icon_wuqixinhao_02";
            case 4:
                return "ui_d_icon_wuqixinhao_01";
            case 5:
                return "ui_d_icon_wuqixinhao_01";
            case 6:
                return "ui_d_icon_wuqixinhao_01";
        }
    }
}

public enum BackType
{
    //第一层信息界面
    Info = 0,
    //升级子界面
    Sj = 1,
    //突破界面
    Tp = 2,
    //技能子界面
    Jn = 3,
    //武器子界面
    Wq =4,
    //天赋子界面
    TF = 5,
    //武器升级界面
    WqSj = 6,
    //武器突破界面
    WqTp = 7,
    //武器型号界面
    WqXh = 8
}