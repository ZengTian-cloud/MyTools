using System;

public class Snowflake
{
    private static long machineId;
    private static long datacenterId = 0L;
    private static long sequence = 0L;

    private static long twepoch = 687888001020L;

    private static long machineIdBits = 5L;
    private static long datacenterIdBits = 5L;
    public static long maxMachineId = -1L ^ -1L << (int)machineIdBits;
    private static long maxDatacenterId = -1L ^ (-1L << (int)datacenterIdBits);

    private static long sequenceBits = 12L;
    private static long machineIdShift = sequenceBits;
    private static long datacenterIdShift = sequenceBits + machineIdBits;
    private static long timestampLeftShift = sequenceBits + machineIdBits + datacenterIdBits;
    public static long sequenceMask = -1L ^ -1L << (int)sequenceBits;
    private static long lastTimestamp = -1L;

    private static object syncRoot = new object();
    static Snowflake snowflake;

    public static Snowflake Instance()
    {
        if (snowflake == null)
            snowflake = new Snowflake();
        return snowflake;
    }

    public Snowflake()
    {
        Snowflakes(0L, -1);
    }

    public Snowflake(long machineId)
    {
        Snowflakes(machineId, -1);
    }

    public Snowflake(long machineId, long datacenterId)
    {
        Snowflakes(machineId, datacenterId);
    }

    private void Snowflakes(long machineId, long datacenterId)
    {
        if (machineId >= 0)
        {
            if (machineId > maxMachineId)
            {
                throw new Exception("机器码ID非法");
            }
            Snowflake.machineId = machineId;
        }
        if (datacenterId >= 0)
        {
            if (datacenterId > maxDatacenterId)
            {
                throw new Exception("数据中心ID非法");
            }
            Snowflake.datacenterId = datacenterId;
        }
    }

    /// <summary>
    /// 生成当前时间戳
    /// </summary>
    /// <returns>毫秒</returns>
    private static long GetTimestamp()
    {
        return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }

    /// <summary>
    /// 获取下一微秒时间戳
    /// </summary>
    /// <param name="lastTimestamp"></param>
    /// <returns></returns>
    private static long GetNextTimestamp(long lastTimestamp)
    {
        long timestamp = GetTimestamp();
        if (timestamp <= lastTimestamp)
        {
            timestamp = GetTimestamp();
        }
        return timestamp;
    }

    /// <summary>
    /// 获取长整形的ID
    /// </summary>
    /// <returns></returns>
    public long GetId()
    {
        lock (syncRoot)
        {
            long timestamp = GetTimestamp();
            if (Snowflake.lastTimestamp == timestamp)
            {
                sequence = (sequence + 1) & sequenceMask;
                if (sequence == 0)
                {
                    timestamp = GetNextTimestamp(Snowflake.lastTimestamp);
                }
            }
            else
            {
                //不同微秒生成ID
                sequence = 0L;
            }
            if (timestamp < lastTimestamp)
            {
                throw new Exception("时间戳比上一次生成ID时时间戳还小，故异常");
            }
            Snowflake.lastTimestamp = timestamp;
            long Id = ((timestamp - twepoch) << (int)timestampLeftShift)
                | (datacenterId << (int)datacenterIdShift)
                | (machineId << (int)machineIdShift)
                | sequence;
            return Id;
        }
    }
}
