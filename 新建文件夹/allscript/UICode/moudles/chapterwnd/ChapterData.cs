
using LitJson;

/// <summary>
/// 章节数据结构
/// </summary>
public class chapterData
{
    // 章节id
    public int chapterId;
    // 章节配置数据
    public ChapterCfgData chapterCfgData;
    // 章节服务端通信数据
    public ChapterCommData chapterCommData;

    public chapterData(int chapterId, ChapterCfgData chapterCfgData, ChapterCommData chapterCommData)
    {
        this.chapterId = chapterId;
        this.chapterCfgData = chapterCfgData;
        this.chapterCommData = chapterCommData;
    }


    /// <summary>
    /// 刷新章节的通信数据
    /// </summary>
    /// <param name="chapterComm"></param>
    public void RefreshCommData(ChapterCommData chapterComm)
    {
        if (chapterComm != null)
        {
            chapterCommData = chapterComm;
        }
    }


   
}

/// <summary>
/// 章节通信数据
/// </summary>
public class ChapterCommData
{
    // 章节id
    public long id = 0;
    // 章节解锁状态 0=未解锁 1=已解锁
    public int lockstate = 0;
    // 集星数
    public int starnum = 0;
    // 1阶奖励 0=未领取 1=已领取
    public int drop1 = 0;
    // 2阶奖励 0=未领取 1=已领取
    public int drop2 = 0;
    // 3阶奖励 0=未领取 1=已领取
    public int drop3 = 0;
}