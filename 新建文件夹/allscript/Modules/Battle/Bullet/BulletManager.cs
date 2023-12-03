using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子弹管理类
/// </summary>
public class BulletManager
{
    private static BulletManager Ins;
    public static BulletManager ins
    {
        get
        {
            if (Ins == null)
            {
                Ins = new BulletManager();
            }
            return Ins;
        }
    }

    public long bulletUidTimer = 9000;

    public Dictionary<long, BaseBullet> dicBulletList = new Dictionary<long, BaseBullet>();

    /// <summary>
    /// 生成射线索敌(101002英雄特殊类型)
    /// </summary>
    /// <param name="computeBase">快照数据</param>
    /// <param name="bulletCfg"></param>
    /// <param name="holder"></param>
    /// <param name="isSkill"></param>
    /// <param name="battleSkillCfg"></param>
    /// <param name="isinitiative">是否主动释放</param>
    public void ShowRayButtle(BaseObject computeBase,BattleSkillBulletCfg bulletCfg, BaseObject holder,bool isSkill, BattleSkillCfg battleSkillCfg,bool isinitiative)
    {
        List<BaseObject> targetList = new List<BaseObject>();
        GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke(int.Parse(bulletCfg.delay), () =>
        {
            RaycastHit[] _hit = Physics.RaycastAll(holder.roleObj.transform.position, GetRotatePosition(holder.roleObj.transform.forward, Vector3.zero, float.Parse(bulletCfg.trackoffset)),1.95f, 1 << LayerMask.NameToLayer("MonsterItem"));
            if (_hit.Length > 0)
            {
                for (int i = 0; i < _hit.Length; i++)
                {
                    long guid = long.Parse(_hit[i].collider.name.Split('_')[1]);
                    targetList.Add(GameCenter.mIns.m_BattleMgr.GetBaseObjByGuid(guid));
                }
            }               
            //Debug.DrawRay(holder.roleObj.transform.position, GetRotatePosition(holder.roleObj.transform.forward, Vector3.zero, float.Parse(bulletCfg.trackoffset))*1.95f, Color.yellow,1f);
            List<BattleSkillLogicCfg> battleSkillLogicCfgs = SkillManager.ins.GetAllLogicCfgByBullet(bulletCfg);
            if (battleSkillLogicCfgs!= null)
            {
                BuffManager.ins.ExecuteBuff(computeBase,targetList, holder, battleSkillLogicCfgs, battleSkillCfg, isinitiative, isSkill);
            }
        });
    }

    public static Vector3 GetRotatePosition(Vector3 tp, Vector3 cp, float angele)
    {
        angele = angele * -1;
        float ex = (tp.x - cp.x) * Mathf.Cos(angele * Mathf.Deg2Rad) -
                     (tp.z - cp.z) * Mathf.Sin(angele * Mathf.Deg2Rad) + cp.x;
        float ey = (tp.z - cp.z) * Mathf.Cos(angele * Mathf.Deg2Rad) +
                     (tp.x - cp.x) * Mathf.Sin(angele * Mathf.Deg2Rad) + cp.z;
        return new Vector3(ex, 0, ey);
    }

    /// <summary>
    /// 生成子弹(索敌)
    /// </summary>
    /// <param name="computeBase">快照数据</param>
    /// <param name="battleSkillBulletCfg"></param>
    /// <param name="target"></param>
    /// <param name="_holder"></param>
    /// <param name="isSkill"></param>
    /// <param name="battleSkillCfg"></param>
    /// <param name="isinitiative"></param>
    public async void CreatOneBullet(BaseObject computeBase,BattleSkillBulletCfg battleSkillBulletCfg, List<BaseObject> target,BaseObject _holder,bool isSkill, BattleSkillCfg battleSkillCfg, bool isinitiative)
    {
        if (target ==null || target.Count < 1)
        {
            return;
        }

       bulletUidTimer = bulletUidTimer + 1;
        //子弹个数
        int count = battleSkillBulletCfg.count;
        //延时
        string[] delaytimes = battleSkillBulletCfg.delay.Split('|');

        GameObject bullet;
        BulletController bulletController;
        for (int i = 0; i < count; i++)
        {
            bullet = BattlePoolManager.Instance.OutPool(ERootType.Bullet, battleSkillBulletCfg.bulletid.ToString());
            if (bullet == null)
            {
                bullet = await ResourcesManager.Instance.LoadAssetSyncByName($"{battleSkillBulletCfg.flyres}.prefab") as GameObject; // ResourcesManager.Instance.LoadPrefabSync($"effect/{_holder.objID}.prefab", battleSkillBulletCfg.flyres); 
            }
            bullet.transform.SetParent(GameCenter.mIns.m_BattleMgr.bulletListRoot);
            bullet.name = $"{battleSkillBulletCfg.bulletid}_{bulletUidTimer}";

            bullet.SetActive(false);
            BaseBullet baseBullet = new BaseBullet
            {
                bulletUID = bulletUidTimer,
                bulletID = battleSkillBulletCfg.bulletid,
                bulletObj = bullet,
                bulletType = 1,
                holder = _holder,
                targetObj = target,
                battleSkillBulletCfg = battleSkillBulletCfg
            };

            bulletController = bullet.GetOrAddCompoonet<BulletController>();
            if (i == 0)
            {
                //只有第一颗子弹触发伤害逻辑
                bulletController.OnBulletInit(computeBase,baseBullet, int.Parse(delaytimes[i]), true, isSkill, battleSkillCfg, isinitiative);
            }
            else
            {
                bulletController.OnBulletInit(computeBase,baseBullet, int.Parse(delaytimes[i]), false, isSkill, battleSkillCfg, isinitiative);
            }
        }
    }

    /// <summary>
    /// 生成子弹(范围)
    /// </summary>
    /// <param name="computeBase">快照数据</param>
    /// <param name="battleSkillBulletCfg"></param>
    /// <param name="targetPos"></param>
    /// <param name="_holder"></param>
    /// <param name="totaltime"></param>
    /// <param name="isSkill"></param>
    /// <param name="battleSkillCfg"></param>
    /// <param name="isinitiative"></param>
    public async void CreatOneBullet(BaseObject computeBase,BattleSkillBulletCfg battleSkillBulletCfg, Vector3 targetPos, BaseObject _holder,float totaltime,bool isSkill, BattleSkillCfg battleSkillCfg, bool isinitiative)
    {
        bulletUidTimer = bulletUidTimer + 1;
        //子弹个数
        int count = battleSkillBulletCfg.count;
        //延时
        string[] delaytimes = battleSkillBulletCfg.delay.Split('|');
        GameObject bullet;
        BulletController bulletController;

        for (int i = 0; i < count; i++)
        {
            bullet = BattlePoolManager.Instance.OutPool(ERootType.Bullet, battleSkillBulletCfg.bulletid.ToString());
            if (bullet == null)
            {
                bullet = await ResourcesManager.Instance.LoadAssetSyncByName($"{battleSkillBulletCfg.flyres}.prefab") as GameObject;
            }
            bullet.transform.SetParent(GameCenter.mIns.m_BattleMgr.bulletListRoot);
            bullet.name = $"{battleSkillBulletCfg.bulletid}_{bulletUidTimer}";

            bullet.SetActive(false);
            BaseBullet baseBullet = new BaseBullet
            {
                bulletUID = bulletUidTimer,
                bulletID = battleSkillBulletCfg.bulletid,
                bulletObj = bullet,
                bulletType = 2,
                holder = _holder,
                targetPos = targetPos,
                battleSkillBulletCfg = battleSkillBulletCfg
            };

            bulletController = bullet.GetOrAddCompoonet<BulletController>();
            if (i == 0)
            {
                //只有第一颗子弹触发伤害逻辑
                bulletController.OnBulletInit(computeBase,baseBullet, int.Parse(delaytimes[i]),true, isSkill, battleSkillCfg, isinitiative,totaltime);
            }
            else
            {
                bulletController.OnBulletInit(computeBase,baseBullet, int.Parse(delaytimes[i]),false, isSkill, battleSkillCfg, isinitiative,totaltime);
            }
        }

    }

    public Vector3 Parabola(Vector3 start,Vector3 end,float height,float t)
    {
        float Func(float x) => 4 * (-height * x * x + height * x);

        var mid = Vector3.Lerp(start, end, t);

        return new Vector3(mid.x, Func(t) + Mathf.Lerp(start.y, end.y, t), mid.z);
    }
}
