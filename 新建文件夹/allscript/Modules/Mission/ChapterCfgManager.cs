using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LitJson;

/// <summary>
/// 章节管理器
/// </summary>
public class ChapterCfgManager : SingletonNotMono<ChapterCfgManager>
{
    // 所有章节配置数据
    public List<ChapterCfgData> chapterCfgDatas;
    private Dictionary<long, ChapterCfgData> m_ChapterDict = new Dictionary<long, ChapterCfgData>();
    public void InitChapterData()
    {
        if (m_ChapterDict != null)
        {
            m_ChapterDict.Clear();
        }

        // 读取所有章节配置
        chapterCfgDatas = GameCenter.mIns.m_CfgMgr.JsonToListClass<ChapterCfgData>("t_mission_chapter");

        if (chapterCfgDatas != null)
        {
            for (int i = 0; i < chapterCfgDatas.Count; i++)
            {
                m_ChapterDict.Add(chapterCfgDatas[i].chapter, chapterCfgDatas[i]);
            }
        }
    }

    /// <summary>
    /// 获得章节配置表数据
    /// </summary>
    /// <param name="chapterId"></param>
    /// <returns></returns>
    public ChapterCfgData GetChapterCfgDataByChapterId(int chapterId)
    {
        if (m_ChapterDict.ContainsKey(chapterId))
        {
            return m_ChapterDict[chapterId];
        }
        return null;
    }
}