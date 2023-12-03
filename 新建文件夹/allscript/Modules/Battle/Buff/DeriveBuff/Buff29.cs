using System;

/// <summary>
// 传送到离施法者距离最近的格子
/// </summary>
public class Buff29 : BaseBuff
{
    private int pathIndex;//目标当前的路线

    private int targrtPointIndex;
    private int targetPathIndex;
    private V3 targetPos;

    public override bool OnChlidStart(params object[] parm)
    {
        if (target != null && !target.bRecycle)
        {
            if (target.objType == 2)//怪物
            {
                BaseMonster baseMonster = (BaseMonster)target;
                pathIndex = baseMonster.Controller.curPath;
                GetTargetMapPoint();
                baseMonster.Controller.ForcedDisplacement(targrtPointIndex, targetPathIndex, targetPos,0);

                return true;
            }
        }
        return false;
    }

    //获得目标格子（从释放者逆时针方向开始取，最近的第一个格子） 先取正方 再取斜方
    public void GetTargetMapPoint()
    {
        //当前角色点位
        V2 curPoint = mainBuff.holder.rolePoint;
        //获取最近的可用点位

        V2 newPoint = GameCenter.mIns.m_BattleMgr.GetLatelyMapByIndex(curPoint);
        //根据点位在关卡表中获取对应的point序号
        int newPathIndex = -1;
        V3 newTargetPos = null;
        targrtPointIndex =  GameCenter.mIns.m_BattleMgr.GetMissionPointByIndex(newPoint, pathIndex, out newPathIndex, out newTargetPos);
        targetPathIndex = newPathIndex;
        targetPos = newTargetPos;
        //GameCenter.mIns.m_BattleMgr.GetDeployHeroList()
    }
}

