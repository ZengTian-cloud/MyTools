using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderManager
{
    private static LeaderManager Ins;
    public static LeaderManager ins
    {
        get
        {
            if (Ins == null)
            {
                Ins = new LeaderManager();
            }
            return Ins;
        }
    }

    private int MaxHP;//最大血量

    private int CurHP;//当前血量

    private bool bDie;

    /// <summary>
    /// 初始化主角
    /// </summary>
    public void InitLeader(int hp)
    {
        bDie = false;
        MaxHP = hp;
        CurHP = MaxHP;
    }

    /// <summary>
    /// 主角受伤
    /// </summary>
    public void LeaderHurt(int hurtNum)
    {
        this.CurHP = Mathf.Clamp(CurHP- hurtNum, 0,MaxHP);    
        GameCenter.mIns.m_BattleMgr.OnLeaderHurt();
        if (CurHP <= 0 && !bDie)
        {
            bDie = true;
            GameCenter.mIns.m_BattleMgr.OnLeaderDie();
        }
        
    }

    /// <summary>
    /// 返回主角当前血量
    /// </summary>
    /// <returns></returns>
    public int GetLeaderCurHP()
    {
        return this.CurHP;
    }

    /// <summary>
    /// 返回主角最大血量
    /// </summary>
    /// <returns></returns>
    public int GetLeaderMaxHP()
    {
        return this.MaxHP;
    }
}
