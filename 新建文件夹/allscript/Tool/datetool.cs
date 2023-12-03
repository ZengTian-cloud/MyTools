using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class datetool
{
    /// <summary>  
    /// ʱ���Timestampת��������  
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
    /// ʱ���ת��ʾ���ڸ�ʽ Y Mo D H Mi S
    /// </summary>
    /// <param name="timeStamp">��Ҫת����ʱ���</param>
    /// <param name="format">���ָ�ʽ:Y Mo D H Mi S Y->�� Mo->�� D->��, H->Сʱ, Mi->��, S->��  ��:Y-Mo-D H:Mi:S ����:x��-x��-x�� xx:xx:xx</param>
    /// <param name="timeStamp">�Ƿ����λ��ʾ</param>
    /// <returns></returns>
    public static string TimeStampToFormatDateString(long timeStamp, string format, bool hasUnit = true)
    {
        DateTime dt = GetDateTime(timeStamp);
        string strHour = dt.Hour >= 10 ? dt.Hour.ToString() : "0" + dt.Hour;
        string strMinute = dt.Minute >= 10 ? dt.Minute.ToString() : "0" + dt.Minute;
        string strSecond = dt.Second >= 10 ? dt.Second.ToString() : "0" + dt.Second;
        //Debug.Log($"{dt.Year}��,{dt.Month}��,{dt.Day}��,{dt.Hour}ʱ,{dt.Minute}��,{dt.Second}��");
        if (hasUnit)
        {
            if (format.Contains("Y"))
                format = format.Replace("Y", $"{dt.Year}��");
            if (format.Contains("Mo"))
                format = format.Replace("Mo", $"{dt.Month}��");
            if (format.Contains("D"))
                format = format.Replace("D", $"{dt.Day}��");
            if (format.Contains("H"))
                format = format.Replace("H", $"{strHour}ʱ");
            if (format.Contains("Mi"))
                format = format.Replace("Mi", $"{strMinute}��");
            if (format.Contains("S"))
                format = format.Replace("S", $"{strSecond}��");
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
    /// ���ʱ��ת��ʾ���ڸ�ʽ D H Mi S
    /// </summary>
    /// <param name="timeStamp">��Ҫת����ʱ���</param>
    /// <param name="format">���ָ�ʽ:D H Mi S D->��, H->Сʱ, Mi->��, S->��  ��:D H:Mi:S ����:x�� xx:xx:xx</param>
    /// <param name="timeStamp">�Ƿ����λ��ʾ</param>
    /// <returns></returns>
    public static string InterValueTimeStampToFormatDateString(long timeStamp, string format, bool hasUnit = true)
    {
        long nowtimeStamp = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
        //Debug.Log(" now:" + nowtimeStamp + " - t:" + timeStamp + " - diff(n-t):" + (nowtimeStamp - timeStamp));
        TimeSpan ts = GetTimeStampIntervalWithNowLocalTimeStamp(timeStamp, nowtimeStamp > timeStamp ? false : true);

        string strHour = ts.Hours >= 10 ? ts.Hours.ToString() : "0" + ts.Hours;
        string strMinute = ts.Minutes >= 10 ? ts.Minutes.ToString() : "0" + ts.Minutes;
        string strSecond = ts.Seconds >= 10 ? ts.Seconds.ToString() : "0" + ts.Seconds;
        //Debug.Log($"{ts.Days}��,{ts.Hours}ʱ,{ts.Minutes}��,{ts.Seconds}��");
        if (hasUnit)
        {
            if (format.Contains("D"))
                format = ts.Days == 0 ? format.Replace("D", "") : format.Replace("D", $"{ts.Days}��");
            if (format.Contains("H"))
                format = ts.Hours == 0 ? format.Replace("H", "") : format.Replace("H", $"{strHour}ʱ");
            if (format.Contains("Mi"))
                format = format.Replace("Mi", $"{strMinute}��");
            if (format.Contains("S"))
                format = format.Replace("S", $"{strSecond}��");
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
    /// ���ʱ��ת��ʾ���ڸ�ʽ D H Mi S
    /// </summary>
    /// <param name="timeStamp">��Ҫת����ʱ���</param>
    /// <param name="format">���ָ�ʽ:D H Mi S D->��, H->Сʱ, Mi->��, S->��  ��:D H:Mi:S ����:x�� xx:xx:xx</param>
    /// <param name="timeStamp">�Ƿ����λ��ʾ</param>
    /// <returns></returns>
    public static string FriendOnlineTimeFormat(long timeStamp, string format, bool hasUnit = true)
    {
        long nowtimeStamp = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
        //Debug.Log(" now:" + nowtimeStamp + " - t:" + timeStamp + " - diff(n-t):" + (nowtimeStamp - timeStamp));
        TimeSpan ts = GetTimeStampIntervalWithNowLocalTimeStamp(timeStamp, nowtimeStamp > timeStamp ? false : true);

        //string strHour = ts.Hours >= 10 ? ts.Hours.ToString() : "0" + ts.Hours;
        //string strMinute = ts.Minutes >= 10 ? ts.Minutes.ToString() : "0" + ts.Minutes;
        string strSecond = ts.Seconds >= 10 ? ts.Seconds.ToString() : "0" + ts.Seconds;
        //Debug.Log($"{ts.Days}��,{ts.Hours}ʱ,{ts.Minutes}��,{ts.Seconds}��");

        int min = ts.Minutes <= 1 ? 1 : ts.Minutes;

        if (ts.Days >= 7)
        {
            return "�ܾ���";
        }
        if (ts.Days >= 3)
        {
            return "3��";
        }
        if (ts.Hours >= 1)
        {
            return $"{ts.Hours}Сʱ";
        }
        if (hasUnit)
        {
            if (format.Contains("H"))
                format = ts.Hours == 0 ? format.Replace("H", "") : format.Replace("H", $"{ts.Hours}Сʱ");
            if (format.Contains("Mi"))
                format = format.Replace("Mi", $"{min}����");
            if (format.Contains("S"))
                format = format.Replace("S", $"{strSecond}��");
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
