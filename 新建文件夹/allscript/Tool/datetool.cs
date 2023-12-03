using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class datetool
{
    /// <summary>  
    /// 时间戳Timestamp转换成日期  
    /// </summary>  
    /// <param name="timeStamp"></param>  
    /// <returns></returns>  
    public static DateTime GetDateTime(long timeStamp)
    {
        DateTime dateTime = FromLongTimeStamp(timeStamp.ToString());
        return dateTime;
    }

    public static long GetNowLocalTimeStamp()
    {
        DateTime now = DateTime.Now;
        long timeStamp = new DateTimeOffset(now).ToUnixTimeSeconds();
        return timeStamp;
    }

    public static long GetNowLocalTimeStampMilliseconds()
    {
        DateTime now = DateTime.Now;
        long timeStamp = new DateTimeOffset(now).ToUnixTimeMilliseconds();
        return timeStamp;
    }

    public static TimeSpan GetTimeStampIntervalWithNowLocalTimeStamp(long targetTimeStamp, bool isReverse = false)
    {
        DateTime now = DateTime.Now;
        long timeStamp = new DateTimeOffset(now).ToUnixTimeMilliseconds();
        DateTime formDate = FromLongTimeStamp(targetTimeStamp.ToString());
        DateTime nowDate = FromLongTimeStamp(timeStamp.ToString());
        return isReverse ? formDate - nowDate : nowDate - formDate;
    }

    /// <summary>
    /// 时间戳转显示日期格式 Y Mo D H Mi S
    /// </summary>
    /// <param name="timeStamp">需要转换的时间戳</param>
    /// <param name="format">表现格式:Y Mo D H Mi S Y->年 Mo->月 D->天, H->小时, Mi->分, S->秒  如:Y-Mo-D H:Mi:S 返回:x年-x月-x日 xx:xx:xx</param>
    /// <param name="timeStamp">是否带单位显示</param>
    /// <returns></returns>
    public static string TimeStampToFormatDateString(long timeStamp, string format, bool hasUnit = true)
    {
        DateTime dt = GetDateTime(timeStamp);
        string strHour = dt.Hour >= 10 ? dt.Hour.ToString() : "0" + dt.Hour;
        string strMinute = dt.Minute >= 10 ? dt.Minute.ToString() : "0" + dt.Minute;
        string strSecond = dt.Second >= 10 ? dt.Second.ToString() : "0" + dt.Second;
        //Debug.Log($"{dt.Year}天,{dt.Month}天,{dt.Day}天,{dt.Hour}时,{dt.Minute}分,{dt.Second}秒");
        if (hasUnit)
        {
            if (format.Contains("Y"))
                format = format.Replace("Y", $"{dt.Year}年");
            if (format.Contains("Mo"))
                format = format.Replace("Mo", $"{dt.Month}月");
            if (format.Contains("D"))
                format = format.Replace("D", $"{dt.Day}日");
            if (format.Contains("H"))
                format = format.Replace("H", $"{strHour}时");
            if (format.Contains("Mi"))
                format = format.Replace("Mi", $"{strMinute}分");
            if (format.Contains("S"))
                format = format.Replace("S", $"{strSecond}秒");
        }
        else
        {
            if (format.Contains("Y"))
                format = format.Replace("Y", $"{dt.Year}");
            if (format.Contains("Mo"))
                format = format.Replace("Mo", $"{dt.Month}");
            if (format.Contains("D"))
                format = format.Replace("D", $"{dt.Day}");
            if (format.Contains("H"))
                format = format.Replace("H", $"{strHour}");
            if (format.Contains("Mi"))
                format = format.Replace("Mi", $"{strMinute}");
            if (format.Contains("S"))
                format = format.Replace("S", $"{strSecond}");
        }
        return format;
    }

    /// <summary>
    /// 间隔时间转显示日期格式 D H Mi S
    /// </summary>
    /// <param name="timeStamp">需要转换的时间戳</param>
    /// <param name="format">表现格式:D H Mi S D->天, H->小时, Mi->分, S->秒  如:D H:Mi:S 返回:x天 xx:xx:xx</param>
    /// <param name="timeStamp">是否带单位显示</param>
    /// <returns></returns>
    public static string InterValueTimeStampToFormatDateString(long timeStamp, string format, bool hasUnit = true)
    {
        long nowtimeStamp = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
        //Debug.Log(" now:" + nowtimeStamp + " - t:" + timeStamp + " - diff(n-t):" + (nowtimeStamp - timeStamp));
        TimeSpan ts = GetTimeStampIntervalWithNowLocalTimeStamp(timeStamp, nowtimeStamp > timeStamp ? false : true);

        string strHour = ts.Hours >= 10 ? ts.Hours.ToString() : "0" + ts.Hours;
        string strMinute = ts.Minutes >= 10 ? ts.Minutes.ToString() : "0" + ts.Minutes;
        string strSecond = ts.Seconds >= 10 ? ts.Seconds.ToString() : "0" + ts.Seconds;
        //Debug.Log($"{ts.Days}天,{ts.Hours}时,{ts.Minutes}分,{ts.Seconds}秒");
        if (hasUnit)
        {
            if (format.Contains("D"))
                format = ts.Days == 0 ? format.Replace("D", "") : format.Replace("D", $"{ts.Days}天");
            if (format.Contains("H"))
                format = ts.Hours == 0 ? format.Replace("H", "") : format.Replace("H", $"{strHour}时");
            if (format.Contains("Mi"))
                format = format.Replace("Mi", $"{strMinute}分");
            if (format.Contains("S"))
                format = format.Replace("S", $"{strSecond}秒");
        }
        else
        {
            if (format.Contains("D"))
                format = ts.Days == 0 ? format.Replace("D", "") : format.Replace("D", $"{ts.Days}");
            if (format.Contains("H"))
                format = ts.Hours == 0 ? format.Replace("H:", "") : format.Replace("H", $"{strHour}");
            if (format.Contains("Mi"))
                format = format.Replace("Mi", $"{strMinute}");
            if (format.Contains("S"))
                format = format.Replace("S", $"{strSecond}");
        }
        return format;
    }

    /// <summary>
    /// 间隔时间转显示日期格式 D H Mi S
    /// </summary>
    /// <param name="timeStamp">需要转换的时间戳</param>
    /// <param name="format">表现格式:D H Mi S D->天, H->小时, Mi->分, S->秒  如:D H:Mi:S 返回:x天 xx:xx:xx</param>
    /// <param name="timeStamp">是否带单位显示</param>
    /// <returns></returns>
    public static string FriendOnlineTimeFormat(long timeStamp, string format, bool hasUnit = true)
    {
        long nowtimeStamp = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
        //Debug.Log(" now:" + nowtimeStamp + " - t:" + timeStamp + " - diff(n-t):" + (nowtimeStamp - timeStamp));
        TimeSpan ts = GetTimeStampIntervalWithNowLocalTimeStamp(timeStamp, nowtimeStamp > timeStamp ? false : true);

        //string strHour = ts.Hours >= 10 ? ts.Hours.ToString() : "0" + ts.Hours;
        //string strMinute = ts.Minutes >= 10 ? ts.Minutes.ToString() : "0" + ts.Minutes;
        string strSecond = ts.Seconds >= 10 ? ts.Seconds.ToString() : "0" + ts.Seconds;
        //Debug.Log($"{ts.Days}天,{ts.Hours}时,{ts.Minutes}分,{ts.Seconds}秒");

        int min = ts.Minutes <= 1 ? 1 : ts.Minutes;

        if (ts.Days >= 7)
        {
            return "很久以";
        }
        if (ts.Days >= 3)
        {
            return "3天";
        }
        if (ts.Hours >= 1)
        {
            return $"{ts.Hours}小时";
        }
        if (hasUnit)
        {
            if (format.Contains("H"))
                format = ts.Hours == 0 ? format.Replace("H", "") : format.Replace("H", $"{ts.Hours}小时");
            if (format.Contains("Mi"))
                format = format.Replace("Mi", $"{min}分钟");
            if (format.Contains("S"))
                format = format.Replace("S", $"{strSecond}秒");
        }
        else
        {
            if (format.Contains("H"))
                format = ts.Hours == 0 ? format.Replace("H:", "") : format.Replace("H", $"{ts.Hours}");
            if (format.Contains("Mi"))
                format = format.Replace("Mi", $"{min}");
            if (format.Contains("S"))
                format = format.Replace("S", $"{strSecond}");
        }
        return format;
    }

    private static DateTime FromLongTimeStamp(string timeStamp)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(timeStamp)).DateTime.ToLocalTime();
    }

    private static long ToSecTimeStamp(long timeStamp)
    {
        int diff = timeStamp.ToString().Length - 10;
        if (diff > 0)
        {
            float t = 1;
            for (int i = 0; i < diff; i++)
                t *= 10;
            return long.Parse(Math.Floor(timeStamp * (1 / t)).ToString());
        }
        else if (diff < 0)
        {
            float t = 1;
            for (int i = 0; i < diff; i++)
                t *= 10;
            return long.Parse(Math.Floor(timeStamp * t).ToString());
        }
        return timeStamp;
    }

    private static long ToMillSecTimeStamp(long timeStamp)
    {
        int diff = timeStamp.ToString().Length - 13;
        if (diff > 0)
        {
            float t = 1;
            for (int i = 0; i < diff; i++)
                t *= 10;
            return long.Parse(Math.Floor(timeStamp * (1 / t)).ToString());
        }
        else if (diff < 0)
        {
            float t = 1;
            for (int i = 0; i < diff; i++)
                t *= 10;
            return long.Parse(Math.Floor(timeStamp * t).ToString());
        }
        return timeStamp;
    }
}
