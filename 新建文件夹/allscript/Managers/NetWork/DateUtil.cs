using UnityEngine;
using System.Collections;
using System;

public static class DateUtil 
{

   public static long CurrentMillTime()
    {
        TimeSpan tss = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        long millTime = Convert.ToInt64(tss.TotalMilliseconds);
        return millTime*1000;

    }
    /// <summary>
    /// 将秒转化为时分秒的格式
    /// </summary>
    /// <param name="second"></param>
    /// <returns></returns>
    public static string SecondForHHMMSS(long second)
    {
        TimeSpan ts = new TimeSpan(0, 0, Convert.ToInt32(second));
        string str = String.Format("{0:00}", ts.Hours) + ":" + String.Format("{0:00}", ts.Minutes) + ":" + String.Format("{0:00}", ts.Seconds);

      
        return str;
    }
    public static long CurrentSecond()
    {
        return CurrentMillTime() / 1000;
    }

    public static byte[] int2bytes(int n)
    {
        byte[] b = new byte[4];
        for(int i=0;i<4;i++){
            b[i] = (byte)(n>>8*(3-i) & 0xff);
        }
        return b;
    }


}
