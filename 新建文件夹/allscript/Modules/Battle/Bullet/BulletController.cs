using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Security.Cryptography;
using DG.Tweening;

/// <summary>
/// 子弹控制器
/// </summary>
public class BulletController : MonoBehaviour
{
    public BaseBullet bulletData;

    public bool isMove;//是否运动

    private BaseObject target;//目标对象

    private Vector3 targetPOS;

    private Vector3 moveNor;//运动方向

    private float distance;//目标距离


    private float totalTime;

    private List<BattleSkillLogicCfg> battleSkillLogicCfgs;

    private Transform targetRoot;

    private bool isLogic;

    private bool isSkill;

    private BattleSkillCfg skillCfg;

    private Vector3 curPos;
    private bool endmove;

    private string endPoint;

    private bool isinitiative;

    private BaseObject computeBase;

    public void OnBulletInit(BaseObject computeBase,BaseBullet bulletData, int delay, bool isLogic, bool isSkill, BattleSkillCfg battleSkillCfg,bool isinitiative, float taotalTime = 0)
    {
        this.computeBase = computeBase;
        this.totalTime = taotalTime;
        this.bulletData = bulletData;
        this.transform.position = bulletData.holder.pointHelper.GetBoneByString(bulletData.battleSkillBulletCfg.flyrespoint).position;
        //this.transform.localRotation = bulletData.holder.pointHelper.transform.localRotation;
        this.battleSkillLogicCfgs = SkillManager.ins.GetAllLogicCfgByBullet(bulletData.battleSkillBulletCfg);
        this.isLogic = isLogic;
        this.isSkill = isSkill;
        this.skillCfg = battleSkillCfg;
        this.endPoint = bulletData.battleSkillBulletCfg.endpoint;
        endmove = false;
        this.isinitiative = isinitiative;

        this.curPos = this.transform.position;

        if (bulletData.targetObj != null)
        {
            target = bulletData.targetObj[0];
        }
        if (bulletData.targetPos != null)
        {
            targetPOS = bulletData.targetPos;
            //偏移值
            string[] trackoffest = bulletData.battleSkillBulletCfg.trackoffset.Split(';');
            targetPOS.x += float.Parse(trackoffest[0]) / 100f;
            targetPOS.z += float.Parse(trackoffest[1]) / 100f;
        }

        if (delay > 0)
        {
            GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke(delay, EnableButton);
        }
        else
        {
            EnableButton();
        }   
    }

    //激活子弹
    private void EnableButton()
    {
        this.gameObject.SetActive(true);
        isMove = true;
    }

    private void Update()
    {
        if (!bulletData.holder.bRecycle)
        {
            if (bulletData != null && isMove == true)
            {
                if (bulletData.bulletType == 1)//锁定目标
                {
                    if (target != null && !target.bRecycle)//目标已经回收
                    {
                        this.transform.LookAt(target.pointHelper.GetBoneByString(endPoint));
                        if (bulletData.battleSkillBulletCfg.tracktype == 7)//瞄准
                        {
                            bool doEnd = false;
                            distance = Vector3.Distance(target.pointHelper.GetBoneByString(endPoint).position, this.transform.position);
                            if (!endmove)
                            {
                                Vector3 zero = Vector3.zero;
                                DOTween.To(() => zero, x => zero = x, new Vector3(0, 0, distance), bulletData.battleSkillBulletCfg.speed / 1000f).SetEase(Ease.Linear).OnUpdate(() =>
                                {
                                    this.transform.Find("Line").GetComponent<LineRenderer>().SetPosition(1, zero);
                                }).OnComplete(() =>
                                {
                                    if (this.isLogic)
                                    {
                                        if (this.battleSkillLogicCfgs != null)
                                        {
                                            BuffManager.ins.ExecuteBuff(this.computeBase,this.bulletData.targetObj, this.bulletData.holder, this.battleSkillLogicCfgs, this.skillCfg, isinitiative, isSkill);
                                        }
                                    }
                                    doEnd = true;
                                });
                                GameCenter.mIns.m_CoroutineMgr.DoDelayInvoke(this.bulletData.battleSkillBulletCfg.disappeardelay, () =>
                                {
                                    this.transform.Find("Line").GetComponent<LineRenderer>().SetPosition(1, new Vector3(0, 0, 0));
                                    BattlePoolManager.Instance.InPool(ERootType.Bullet, this.gameObject, bulletData.bulletID.ToString());
                                });
                                endmove = true;
                            }
                            if (doEnd)
                            {
                                this.transform.Find("Line").GetComponent<LineRenderer>().SetPosition(1, new Vector3(0, 0, distance));
                            }
                        }
                        else
                        {
                            moveNor = target.pointHelper.GetBoneByString(endPoint).position - this.transform.position;
                            distance = Vector3.Distance(target.pointHelper.GetBoneByString(endPoint).position, this.transform.position);
                            if (distance <= 0.5f)
                            {
                                isMove = false;
                                BattlePoolManager.Instance.InPool(ERootType.Bullet, this.gameObject, bulletData.bulletID.ToString());
                                if (this.isLogic)
                                {
                                    if (this.battleSkillLogicCfgs != null)
                                    {
                                        BuffManager.ins.ExecuteBuff(this.computeBase,this.bulletData.targetObj, this.bulletData.holder, this.battleSkillLogicCfgs, this.skillCfg, isinitiative, isSkill);
                                    }
                                }

                                return;
                            }
                            this.transform.Translate(moveNor.normalized * bulletData.battleSkillBulletCfg.speed / 100f * Time.deltaTime, Space.World);
                        }
                    }
                    else
                    {
                        isMove = false;
                        bulletData.holder.isOnSkill = false;
                        BattlePoolManager.Instance.InPool(ERootType.Bullet, this.gameObject, bulletData.bulletID.ToString());
                    }
                }
                else if (bulletData.bulletType == 2)//指定范围
                {
                    this.transform.LookAt(targetPOS);
                    if (totalTime > 0)
                    {
                        if (bulletData.battleSkillBulletCfg.tracktype == 1 || bulletData.battleSkillBulletCfg.tracktype == 4)
                        {
                            Vector3 start = this.transform.position;
                            DOTween.To(setter: value =>
                            {
                                this.transform.position = BulletManager.ins.Parabola(start, targetPOS, 3, value);
                            }, startValue: 0, endValue: 1, duration: totalTime).SetEase(Ease.Linear).OnComplete(() =>
                            {
                                isMove = false;
                                BattlePoolManager.Instance.InPool(ERootType.Bullet, this.gameObject, bulletData.bulletID.ToString());
                                if (this.isLogic)
                                {
                                    if (this.battleSkillLogicCfgs != null)
                                    {
                                        BuffManager.ins.ExecuteBuff(this.computeBase,this.bulletData.targetPos, this.bulletData.holder, this.battleSkillLogicCfgs, this.skillCfg, isinitiative, isSkill);
                                    }
                                }

                            });
                        }
                        else
                        {
                            this.transform.DOMove(targetPOS, totalTime).SetEase(Ease.Linear).OnComplete(() =>
                            {
                                isMove = false;
                                BattlePoolManager.Instance.InPool(ERootType.Bullet, this.gameObject, bulletData.bulletID.ToString());
                                if (this.isLogic)
                                {
                                    if (this.battleSkillLogicCfgs != null)
                                    {
                                        BuffManager.ins.ExecuteBuff(this.computeBase,this.bulletData.targetPos, this.bulletData.holder, this.battleSkillLogicCfgs, this.skillCfg, isinitiative, isSkill);
                                    }
                                }

                            });
                        }
                        totalTime = 0;
                    }
                    /*moveNor = targetPOS - this.transform.position;
                    distance = Vector3.Distance(targetPOS, this.transform.position);
                    if (distance <= 0.5f)
                    {
                        isMove = false;
                        BattlePoolManager.Instance.InPool(ERootType.Bullet, this.gameObject, bulletData.bulletID.ToString());
                        desCB?.Invoke();
                        return;
                    }
                    this.transform.Translate(moveNor.normalized * 13f * Time.deltaTime);*/
                }
            }
        }
        else
        {
            isMove = false;
            BattlePoolManager.Instance.InPool(ERootType.Bullet, this.gameObject, bulletData.bulletID.ToString());
        }
        
    }
}
