using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

// 通用工具类
public static class commontool
{
    /// 时间戳转换为DataTime
    public static DateTime TimestampToDataTime(long unixTimeStamp)
    {
        DateTime dt = TimeZoneInfo.ConvertTimeToUtc(new DateTime(1970, 1, 1));
        long lTime = unixTimeStamp * 10000;
        TimeSpan toNow = new TimeSpan(lTime);
        return dt.Add(toNow);
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

    public static void RegisterButtonListen(GameObject o, Action action, bool isClearBefore = true)
    {
        if (o == null)
        {
            return;
        }

        if (o.GetComponent<Button>() == null)
        {
            return;
        }

        RegisterButtonListen(o.GetComponent<Button>(), action, isClearBefore);
    }

    public static void RegisterButtonListen(Button btn, Action action, bool isClearBefore = true)
    {
        if (btn == null)
        {
            return;
        }

        if (isClearBefore)
        {
            btn.onClick.RemoveAllListeners();
        }

        btn.onClick.AddListener(() =>
        {
            action?.Invoke();
        });
    }

    public static void RegisterButtonListen<T>(Button btn, Action<T> action, T t, bool isClearBefore = true)
    {
        if (btn == null)
        {
            return;
        }

        if (isClearBefore)
        {
            btn.onClick.RemoveAllListeners();
        }

        btn.onClick.AddListener(() =>
        {
            action?.Invoke(t);
        });
    }

    public static void SetText(GameObject o, string s, int limitCount = -1)
    {
        if (o == null)
        {
            return;
        }

        if (o.GetComponent<Text>() != null)
        {
            SetText(o.GetComponent<Text>(), s, limitCount);
        }

        else if (o.GetComponent<TMP_Text>() != null)
        {
            SetText(o.GetComponent<TMP_Text>(), s, limitCount);
        }
    }

    public static void SetText(Text text, string s, int limitCount = -1)
    {
        if (text == null)
        {
            return;
        }

        // TODO: limit count and block word library

        text.text = s;
    }

    public static void SetText(TMP_Text text, string s, int limitCount = -1)
    {
        if (text == null)
        {
            return;
        }

        // TODO: limit count and block word library

        text.text = s;
    }

    public static void SetButtonText(GameObject o, string s)
    {
        if (o == null)
        {
            return;
        }

        SetButtonText(o.GetComponent<Button>(), s);
    }

    public static void SetButtonText(Button b, string s)
    {
        if (b == null)
        {
            return;
        }

        Text text = b.GetComponentInChildren<Text>();
        if (text != null)
        {
            SetText(text, s);
            return;
        }

        TMP_Text tmptext = b.GetComponentInChildren<TMP_Text>();
        if (tmptext != null)
        {
            SetText(tmptext, s);
        }
    }

    public static Color GetColor(string color)
    {
        if (color.Length == 0)
        {
            return Color.black;//设为黑色
        }
        else
        {
            //#ff8c3 除掉#
            color = color.Substring(1);
            int v = int.Parse(color, System.Globalization.NumberStyles.HexNumber);
            //转换颜色
            return new Color(
            //int>>移位 去低位
            //&按位与 去高位
            ((float)(((v >> 16) & 255))) / 255,
            ((float)((v >> 8) & 255)) / 255,
            ((float)((v >> 0) & 255)) / 255
            );
        }
    }

    /// <summary>
    /// 提前获取text文本内容宽度
    /// </summary>
    /// <param name="text">文本内容</param>
    /// <param name="fontName">字体</param>
    /// <param name="fontsize">字体大小</param>
    /// <returns></returns>
    public static float AdvanceGetTextWidth(string text, string fontName, int fontsize)
    {
        string tempText = text;
        // 将表情等特殊文本做替换
        // TODO:目前简单处理
        int si = tempText.IndexOf("<sprite=");
        bool hasEmo = si >= 0;
        int counter = 0;
        while (counter < 10 && si >= 0)
        {
            si = tempText.IndexOf("<sprite=");
            if (si >= 0)
            {
                string s = tempText.Substring(si, 25);
                tempText = tempText.Replace(s, "em");
            }
            counter++;
        }

        // 非表情存文本加点长度..
        if (!hasEmo)
        {
            tempText += "add";
        }

        if (string.IsNullOrEmpty(tempText))
            return 0.0f;
        if (string.IsNullOrEmpty(fontName))
            fontName = "Arial";
        if (fontsize == 0)
            fontsize = 35;
        Font font = Font.CreateDynamicFontFromOSFont(fontName, fontsize);
        if (font != null)
        {
            font.RequestCharactersInTexture(tempText, fontsize, FontStyle.Normal);
            CharacterInfo characterInfo;
            float width = 0f;
            for (int i = 0; i < tempText.Length; i++)
            {
                font.GetCharacterInfo(tempText[i], out characterInfo, fontsize);
                //width+=characterInfo.width; unity5.x提示此方法将来要废弃
                width += characterInfo.advance;
            }
            return width;
        }
        return 0.0f;
    }

    /// <summary>
    /// 数字转text mesh pro sprite 字符串文本形式
    /// </summary>
    /// <param name="number">需要转换的数字</param>
    /// <param name="sType">艺术字类型前缀如:damage</param>
    /// <param name="artType">需要表现的数字类型</param>
    /// <param name="groupNumber">一组艺术字形式表现个数(目前是一组:11个:(0~9和'-'号))</param>
    /// <param name="size">字体大小</param>
    /// <returns></returns>
    public static string GetArtNumberString(float number, int artType = 0, int groupNumber = -1, float size = -1, string sType = "damage")
    {
        groupNumber = groupNumber == -1 ? 11 : groupNumber;
        StringBuilder stringBuilder = new StringBuilder();
        // 大小
        if (size > 0)
            stringBuilder.Append($"<size={size}>");

        // 正负
        if (number < 0)
            stringBuilder.Append($"<sprite name={sType}_{10 + artType * groupNumber}>");

        int[] digits = GetNumberDigits((int)number);
        foreach (var i in digits)
        {
            stringBuilder.Append($"<sprite name={sType}_{i + artType * groupNumber}>");
        }

        // 大小
        if (size > 0)
            stringBuilder.Append($"</size>");

        return stringBuilder.ToString();
    }

    /// <summary>
    /// 队伍艺术字转换. 队伍1~5=> 0:黄1,1:白1,2:黑1 ...
    /// </summary>
    /// <param name="number"></param>
    /// <param name="colorType">颜色，0 = 白色， 1 = 黄色， 2 = 黄色</param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static string GetArtNumberString_DuiWu(int number, int colorType, float size = -1)
    {
        StringBuilder stringBuilder = new StringBuilder();
        if (colorType == 0)
        {
            // 1,4,7,10,13
            // 1,2,3,4,5
            number = number * 3 - 2;
        }
        else if (colorType == 1)
        {
            // 0,3,6,9,12
            // 1,2,3,4,5
            number = number * 3 - 3;
        }
        else if (colorType == 2)
        {
            // 2,5,8,12,14
            // 1,2,3,4,5
            number = number * 3 - 1;
        }
        // 大小
        if (size > 0)
            stringBuilder.Append($"<size={size}>");

        //int[] digits = GetNumberDigits(number);
        //foreach (var i in digits)
        //{
        //    stringBuilder.Append($"<sprite name=duiwu_{i}>");
        //}
        stringBuilder.Append($"<sprite name=duiwu_{number}>");

        // 大小
        if (size > 0)
            stringBuilder.Append($"</size>");

        return stringBuilder.ToString();
    }

    public static string GetArtNumberString_DuiWu_SpriteName(int number, int colorType)
    {
        if (colorType == 0)
        {
            // 1,4,7,10,13
            // 1,2,3,4,5
            number = number * 3 - 2;
        }
        else if (colorType == 1)
        {
            // 0,3,6,9,12
            // 1,2,3,4,5
            number = number * 3 - 3;
        }
        else if (colorType == 2)
        {
            // 2,5,8,12,14
            // 1,2,3,4,5
            number = number * 3 - 1;
        }
        return "duiwu_" + number;
    }

    /// <summary>
    /// 等级艺术字转换
    /// </summary>
    /// <param name="number"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static string GetArtNumberString_Lv(int number, float size = -1)
    {
        StringBuilder stringBuilder = new StringBuilder();
        // 大小
        if (size > 0)
            stringBuilder.Append($"<size={size}>");

        int[] digits = GetNumberDigits(number);
        foreach (var i in digits)
        {
            stringBuilder.Append($"<sprite name=lv_{i}>");
        }

        // 大小
        if (size > 0)
            stringBuilder.Append($"</size>");

        return stringBuilder.ToString();
    }

    /// <summary>
    /// 计时艺术字转换
    /// </summary>
    /// <param name="number"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static string GetArtNumberStringTimer(int number, float size = -1)
    {
        StringBuilder stringBuilder = new StringBuilder();
        // 大小
        if (size > 0)
            stringBuilder.Append($"<size={size}>");

        int[] digits = GetNumberDigits(number);
        foreach (var i in digits)
        {
            stringBuilder.Append($"<sprite name=timer_{i+1}>");
        }
        stringBuilder.Append($"<sprite name=timer_{0}>");
        // 大小
        if (size > 0)
            stringBuilder.Append($"</size>");

        return stringBuilder.ToString();
    }

    /// <summary>
    /// 获取int型数字的每位, 高位->低位
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public static int[] GetNumberDigits(int num)
    {
        num = Math.Abs(num);
        int len = num.ToString().Length;
        int[] digits = new int[len];
        int quotient;
        int remainder;
        int index = len - 1;
        do
        {
            quotient = num / 10;
            remainder = num % 10;
            digits[index--] = remainder;
            num = quotient;
        } while (quotient != 0);
        return digits;
    }

    public static void ResetUIBaseParam(GameObject uiRootRectObj, GameObject parent = null)
    {
        if (uiRootRectObj == null) return;

        RectTransform uiRootRect = uiRootRectObj.GetComponent<RectTransform>();
        if (uiRootRect == null) return;

        if (parent != null)
            uiRootRectObj.transform.SetParent(parent.transform);

        uiRootRectObj.transform.localScale = Vector3.one;
        uiRootRectObj.transform.rotation = Quaternion.identity;
        uiRootRectObj.transform.localPosition = Vector3.zero;

        uiRootRect.anchorMax = new Vector2(1, 1);
        uiRootRect.anchorMin = new Vector2(0, 0);
        uiRootRect.pivot = new Vector2(0.5f, 0.5f);
        uiRootRect.anchoredPosition = Vector2.zero;
        uiRootRect.offsetMin = new Vector2(0, 0);
        uiRootRect.offsetMax = new Vector2(0, 0);
    }


    //设置灰度
    public static async void SetGary(Image img,bool isGary)
    {
        if (isGary)
        {
            img.material = await ResourcesManager.Instance.LoadAssetSync<Material>("Assets/allres/materials/Gary.mat");
        }
        else
        {
            img.material = null;
        }
    }

    /// <summary>
    /// 获得元素图标
    /// </summary>
    /// <param name="ysID">元素id</param>
    /// <returns></returns>
    public static Sprite GetYuansuIcon(int ysID)
    {
        switch (ysID)
        {
            case 1:
                return SpriteManager.Instance.GetSpriteSync("ui_c_icon_shui");
            case 2:
                return SpriteManager.Instance.GetSpriteSync("ui_c_icon_huo");
            case 3:
                return SpriteManager.Instance.GetSpriteSync("ui_c_icon_feng");
            case 4:
                return SpriteManager.Instance.GetSpriteSync("ui_c_icon_lei");
            default:
                return null;
        }
    }

    /// <summary>
    /// 获得元素背景条图标
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static string GetYuansuBgIcon(int element)
    {
        return element == 0 ? "" :
                (element == 1 ? "ui_fight_pnl_jineng_shui" :
                (element == 2 ? "ui_fight_pnl_jineng_huo" :
                (element == 3 ? "ui_fight_pnl_jineng_feng" : "ui_fight_pnl_jineng_lei")));
    }

    /// <summary>
    /// 获得职业图标
    /// </summary>
    /// <param name="zyID"></param>
    /// <param name="isWhite"></param>
    /// <returns></returns>
    public static Sprite GetZhiYeIcon(int zyID, bool isWhite = false)
    {
        switch (zyID)
        {
            case 1:
                return !isWhite ? SpriteManager.Instance.GetSpriteSync("ui_c_icon_jianmie") : SpriteManager.Instance.GetSpriteSync("ui_c_icon_jianmie_bai");
            case 2:
                return !isWhite ? SpriteManager.Instance.GetSpriteSync("ui_c_icon_zhanshu") : SpriteManager.Instance.GetSpriteSync("ui_c_icon_zhanshu_bai");
            case 3:
                return !isWhite ? SpriteManager.Instance.GetSpriteSync("ui_c_icon_zhiyuan") : SpriteManager.Instance.GetSpriteSync("ui_c_icon_zhanshu_bai");
            case 4:
                return !isWhite ? SpriteManager.Instance.GetSpriteSync("ui_c_icon_diyu") : SpriteManager.Instance.GetSpriteSync("ui_c_icon_zhanshu_bai");
            default:
                return null;
        }
    }

    /// <summary>
    /// 获得技能品质条
    /// ui_fight_img_pingzhi_lan ui_fight_img_pingzhi_zi ui_fight_img_pingzhi_cheng ui_fight_img_pingzhi_hong
    /// 3=蓝 4=紫 5=橙 6=红
    /// </summary>
    /// <param name="quality"></param>
    /// <returns></returns>
    public static Sprite GetQualityBarIcon(int quality)
    {
        switch (quality)
        {
            case 3:
                return SpriteManager.Instance.GetSpriteSync("ui_fight_img_pingzhi_lan");
            case 4:
                return SpriteManager.Instance.GetSpriteSync("ui_fight_img_pingzhi_zi");
            case 5:
                return SpriteManager.Instance.GetSpriteSync("ui_fight_img_pingzhi_cheng");
            case 6:
                return SpriteManager.Instance.GetSpriteSync("ui_fight_img_pingzhi_hong");
            default:
                return SpriteManager.Instance.GetSpriteSync("ui_fight_img_pingzhi_lan");
        }
    }

    /// <summary>
    /// 获取技能类型描述图片
    /// 秘技/终结技标签，读取t_skill.skilltype 若skilltype=3资源使用：ui_fight_font_mj 若skilltype = 4资源使用：ui_fight_font_zjj
    /// </summary>
    /// <param name="skilltype"></param>
    /// <returns></returns>
    public static Sprite GetSkillTypeDesIcon(int skilltype)
    {
        switch (skilltype)
        {
            case 1:
                return null;
            case 2:
                return null;
            case 3:
                return SpriteManager.Instance.GetSpriteSync("ui_fight_font_mj");
            case 4:
                return SpriteManager.Instance.GetSpriteSync("ui_fight_font_zjj");
            default:
                return null;
        }
    }

    /// <summary>
    /// 获取英雄背景图资源名
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static string GetHeroInfoBgResName(int element)
    {
        return element == 1 ? "ui_fight_pnl_juese" :
               (element == 2 ? "ui_fight_pnl_juese_3" :
               (element == 3 ? "ui_fight_pnl_juese_2" : "ui_fight_pnl_juese_4"));
    }

    /// <summary>
    /// 获取技能消耗icon资源名称
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static string GetSkillConsumeIconResName(int skilltype)
    {
        return skilltype == 1 ? "ui_fight_icon_suanli_xiao03" :
                (skilltype == 2 ? "ui_fight_icon_suanli_xiao03" :
                (skilltype == 3 ? "ui_fight_icon_suanli_xiao01" : "ui_fight_icon_suanli_xiao02"));
    }
}
