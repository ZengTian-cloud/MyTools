using UnityEngine;
using Basics;
using System.IO;
using System.Collections;
using Spine;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.ComponentModel;
using LitJson;
using UnityEditor;
using System.Linq;

namespace Managers
{
    public class BattleManager : SingletonOjbect
    {

        public EBattleState curBattleState { get; private set; }//当前战斗状态
        public MissionData curMissionData { get; private set; }//当前关卡的数据
        public BattleMissionParamCfg curMissionParamCfg { get; private set; }//当前关卡的参数配置
        public MapData curMapData { get; private set; }//当前关卡地图配置

        public Transform battleCameraRoot { get; private set; }//战斗相机节点
        public Camera lookCamer { get; private set; }//布阵相机
        public Camera battleCamer { get; private set; }//战斗相机
        public Camera hpCamera { get; private set; }//血条相机
        public Canvas hpCanvas { get; private set; }//血条画布

        public GameObject battleRoot { get; private set; }//战斗节点
        public GameObject battleScence { get; private set; }//战斗场景对象

        public basebattle BattleUI;//战斗ui
        public Transform battleHpUIRoor;//战斗内hp节点

        public Transform heroListRoot { get; private set; }//英雄节点
        public Transform monsterListRoot { get; private set; }//怪物节点
        public Transform rolePointList { get; private set; }//站位台节点
        public Transform bulletListRoot { get; private set; }//子弹列表

        public GameObject hpSliderTemp { get; private set; }//血条ui模型
        private GameObject rolePointItem;//站台模型
        public Dictionary<V2, GameObject> rolePointObjList { get; private set; }//站位台列表 k-地图格子的坐标 v-对象

        public List<BaseHero> DeployHeroList;//上阵英雄列表（开战前固定，不会随着英雄死亡改变）

        private bool isWin;//战斗是否胜利
        public bool monsterCreatEnd;//怪物生成结束

        public List<Vector3> startPos;//出怪点
        public List<Vector3> endPos;//入怪点

        public BaseObject baseGod;//战斗内上帝角色（无实例）-释放全局的buff或者技能

        public int chapterid;//章节id
        public long missionID;//关卡id

        public int battleType;//战斗类型 1-普通关卡
        /// <summary>
        /// ----------------------------------------------请求战斗，这里主要加载关卡数据、加载场景、加载战斗所需组件----------------------------------------------
        /// </summary>
        /// <param name="missionID">关卡id</param>
        public async void RequstBattle(long missionID, int battleType = 1,Action ab = null)
        {
            
            //TODO:进入loading界面
            //初始化关卡数据
            //this.chapterid = chapterid;
            this.missionID = missionID;
            this.battleType = battleType;
            this.curMissionData = GameCenter.mIns.m_CfgMgr.JsonToSingleClass<MissionData>($"mission{this.missionID}");
            this.curMapData = GameCenter.mIns.m_CfgMgr.JsonToSingleClass<MapData>($"map{this.missionID}");
            this.curMissionParamCfg = BattleCfgManager.Instance.GetMissionParamCfg(this.missionID);

            //战斗管理器激活
            SetBattleManagerEnable(true);
            GameCenter.mIns.m_UIMgr.Open<commonloading_wnd>().ShowLoading(() => {
                GameSceneManager.Instance.LoadScene(curMissionParamCfg.mapid, async (obj) => { //初始化战斗需要的所有组件
                    InitBattleCompent();
                    //初始化战斗节点
                    InitBattleRoot(obj);
                    //初始化上帝
                    InitBaseGod();
                    //初始化入怪点和出怪点
                    InitStartPosAndEndPos();
                    //生成角色站位台
                    CreatRolePoint();
                    //初始化主角
                    LeaderManager.ins.InitLeader(curMissionParamCfg.hp);
                    //切换战斗状态为预备
                    SwitchBattleState(EBattleState.Readly);
                    //获得战斗ui界面
                    BattleUI = GameCenter.mIns.m_UIMgr.Open<basebattle>();
                    BattleUI.OpenBaseBattle();
                    //战斗相机启动
                    battleCameraRoot.gameObject.SetActive(true);
                    // 初始化机关空气墙等
                    BattleTrapManager.Instance.InitObjs();
                    // 初始化高亮模块
                    BattleSelectedHelper.Instance.Init();

                    //判断是否有战前剧情模块
                    BattleInteractCfg interactCfg = BattleCfgManager.Instance.GetBattleInteractByMissionAndTimer(missionID, 1);
                    if (interactCfg != null)
                    {
                        GameCenter.mIns.m_UIMgr.Open<battlestorywnd>(interactCfg);
                    }
                    ab?.Invoke();
                });
            });
            


        }

        /// <summary>
        /// 循环监听战斗场景是否加载完成
        /// </summary>
        private void Update()
        {
            if (curBattleState == EBattleState.Start && ((monsterCreatEnd && BattleMonsterManager.Instance.monsterCount <= 0) || BattleHeroManager.Instance.depolyHeroList.Count <= 0))
            {
                //判断是否有剧情模块
                BattleInteractCfg interactCfg = BattleCfgManager.Instance.GetBattleInteractByMissionAndTimer(missionID, 2);
                if (interactCfg != null)
                {
                    GameCenter.mIns.m_UIMgr.Open<battlestorywnd>(interactCfg);
                }
                BattleEnd();
            }
        }



        /// <summary>
        /// 初始化战斗需要的所有组件
        /// </summary>
        private async void InitBattleCompent()
        {
            //加载战斗对象池
            GameCenter.mIns.gameObject.GetOrAddCompoonet<BattlePoolManager>().InitPool();

            //初始化战斗血条的ui节点
            GameObject go = await ResourcesManager.Instance.LoadUIPrefabSync("battleHP_root", GameCenter.mIns.m_UIMgr.uiRoot.transform);
            battleHpUIRoor = go.transform;
            battleHpUIRoor.localPosition = new Vector3(-2000, -2000, 0);
            hpCamera = battleHpUIRoor.Find("hpCamera").GetComponent<Camera>();
            hpCanvas = battleHpUIRoor.Find("canvas").GetComponent<Canvas>();
            hpSliderTemp = battleHpUIRoor.Find("canvas/hpSlider").gameObject;
            //添加血条相机到baseCamera的stack下
            GameCenter.mIns.m_CamMgr.AddCameraToMainCamera(hpCamera);

            // 初始化机关控制器
            BattleTrapManager.Instance.Init(missionID, curMissionData);
        }

        /// <summary>
        /// 初始初始战斗节点
        /// </summary>
        private async void InitBattleRoot(GameObject obj)
        {
            //战斗场景
            battleScence = obj;
            //战斗相机
            battleCameraRoot = Camera.main.transform.Find("battleCamera");
            lookCamer = Camera.main.transform.Find("battleCamera/lookcamera").GetComponent<Camera>();
            battleCamer = Camera.main.transform.Find("battleCamera/gamecamera").GetComponent<Camera>();
            //战斗节点
            battleRoot = await ResourcesManager.Instance.LoadUIPrefabSync("BattleRoot");
            rolePointList = battleRoot.transform.Find("RolePointList");//站位台节点
            rolePointItem = rolePointList.Find("RolePointItem").gameObject;//站位台对象
            heroListRoot = battleRoot.transform.Find("HeroListRoot");//英雄节点
            monsterListRoot = battleRoot.transform.Find("MonsterListRoot");//怪物节点
            bulletListRoot = battleRoot.transform.Find("BulletListRoot");//子弹节点
        }




        /// <summary>
        ///  ----------------------------------------------开始战斗（点击开战按钮）----------------------------------------------
        /// </summary>
        /// <param name="depolyHeros">本次战斗上阵的英雄</param>
        public void StartBattle(Dictionary<GameObject, BaseHero> depolyHeros)
        {
            BattleMsgManager.Instance.SendChallageMsg(this.missionID);//通信-发起挑战

            this.DeployHeroList = new List<BaseHero>();
            foreach (var item in depolyHeros)
            {
                this.DeployHeroList.Add(item.Value);
            }
            BattleHeroManager.Instance.InitDepolyHeroList(this.DeployHeroList);
            // 开始战斗怪物数量清0
            BattleMonsterManager.Instance.monsterCount = 0;


            //处理机关的添加卡牌机制
            DrawCardMgr.Instance.drawCardDatas.Clear();
            BattleTrapManager.Instance.HandleTrapEffect();
            //初始化英雄技能数据
            for (int i = 0; i < BattleHeroManager.Instance.depolyHeroList.Count; i++)
            {
                BattleHeroManager.Instance.depolyHeroList[i].prefabObj.GetComponent<HeroController>().ChangeCollider();
                BattleHeroManager.Instance.depolyHeroList[i].InitSkillCfg();
            }
            //初始化卡牌列表
            DrawCardMgr.Instance.InitDrawCardDatas(BattleHeroManager.Instance.depolyHeroList);
            CardManager.Instance.InitCurCardData(BattleHeroManager.Instance.depolyHeroList.Count);
            CreatMonster();

            SwitchBattleState(EBattleState.Start);
        }

        /// <summary>
        /// 初始化战斗内上帝对象
        /// </summary>
        private void InitBaseGod()
        {
            baseGod = new BaseObject();
            baseGod.bRecycle = false;
            GameObject god = new GameObject("baseGod");
            god.transform.SetParent(battleRoot.transform);
            baseGod.prefabObj = god;
            baseGod.roleObj = god;
        }

        
        /// <summary>
        /// 设置战斗管理器的激活状态
        /// </summary>
        public void SetBattleManagerEnable(bool bEnable)
        {
            GameCenter.mIns.m_BattleMgr.enabled = bEnable;
        }

        /// <summary>
        /// 切换战斗当前状态
        /// </summary>
        /// <param name="eState"></param>
        public void SwitchBattleState(EBattleState eState)
        {
            this.curBattleState = eState;

            lookCamer.gameObject.SetActive(EBattleState.Readly == eState);
            battleCamer.gameObject.SetActive(EBattleState.Readly != eState);

        }

        /// <summary>
        /// 初始化出怪点和入怪点
        /// </summary>
        private async void InitStartPosAndEndPos()
        {
            startPos = new List<Vector3>();
            endPos = new List<Vector3>();
            List<PointData> pointDatas;
            
            for (int i = 0; i < curMissionData.pathDatas.Count; i++)
            {
                pointDatas = curMissionData.pathDatas[i].pointDatas;
                startPos.Add(new Vector3((float)pointDatas[0].pos.x, 0, (float)pointDatas[0].pos.z));
                endPos.Add(new Vector3((float)pointDatas[pointDatas.Count - 1].pos.x, 0, (float)pointDatas[pointDatas.Count - 1].pos.z));
                GameObject startPoint = await ResourcesManager.Instance.LoadAssetSyncByName("start_point.prefab");
                startPoint.transform.SetParent(battleRoot.transform);
                startPoint.transform.position = new Vector3((float)pointDatas[0].pos.x, 0, (float)pointDatas[0].pos.z);
                startPoint.transform.LookAt(new Vector3((float)pointDatas[1].pos.x, 0, (float)pointDatas[0].pos.z));
                GameObject endPoint = await ResourcesManager.Instance.LoadAssetSyncByName("end_point.prefab");
                endPoint.transform.SetParent(battleRoot.transform);
                endPoint.transform.position = new Vector3((float)pointDatas[pointDatas.Count - 1].pos.x, 0, (float)pointDatas[pointDatas.Count - 1].pos.z);
                endPoint.transform.LookAt(new Vector3((float)pointDatas[pointDatas.Count - 2].pos.x , 0, (float)pointDatas[pointDatas.Count - 1].pos.z));
                //endPoint.transform.localRotation = Quaternion.Euler(0, 180, 0);
            }
        }

        /// <summary>
        /// 生成可放置角色的格子提示
        /// </summary>
        private async void CreatRolePoint()
        {
            rolePointObjList = new Dictionary<V2, GameObject>();
            RolePointData oneData;
            for (int i = 0; i < curMissionData.rolePointList.Count; i++)
            {
                oneData = curMissionData.rolePointList[i];
                GameObject point = await ResourcesManager.Instance.LoadAssetSyncByName("platform.prefab"); //GameObject.Instantiate(battleManager.rolePointItem, battleManager.rolePointList);
                point.transform.SetParent(rolePointList);
                point.SetActive(true);
                point.transform.position = new Vector3((float)oneData.pos.x, 0, (float)oneData.pos.z);
                rolePointObjList.Add(oneData.index, point);
            }
        }

        /// <summary>
        /// 获得展位台坐标
        /// </summary>
        /// <param name="obj"></param>
        public V2 GetRolePointByObj(GameObject obj)
        {
            foreach (var item in rolePointObjList)
            {
                if (item.Value.Equals(obj))
                {
                    return item.Key;
                }
            }
            return null;
        }

        /// <summary>
        /// 生成怪物
        /// </summary>
        private void CreatMonster()
        {
            monsterCreatEnd = false;
            BattleMonsterManager.Instance.CreatMonsterByCfg(curMissionData);
        }

        /// <summary>
        /// 退出战斗场景
        /// </summary>
        public void QuitBattleScene()
        {
            GameCenter.mIns.m_CoroutineMgr.StopAllDelayInvoke();
            //卸载战斗对象池
            BattlePoolManager.Instance.DestroyPool();
            BattleHeroManager.Instance.depolyHeroList.Clear();
            BattleMonsterManager.Instance.dicMonsterList.Clear();
            HpSliderManager.ins.dicSliderList.Clear();
            //Destroy(GameCenter.mIns.gameObject.GetComponent<BattlePoolManager>());

            if (battleRoot != null)
            {
                Destroy(battleRoot);
            }
            if (battleHpUIRoor.gameObject)
            {
                Destroy(battleHpUIRoor.gameObject);
            }

            //卸载战斗场景
            UnloadSceneAsync();

            //todo卸载资源    

            // 关闭雾效
            GameCenter.mIns.m_FogManager.SetSceneSetting("");
            //GameSceneManager.Instance.LeaveBattleScene();

            //战斗相机关闭
            battleCameraRoot.gameObject.SetActive(false);
        }

        public void UnloadSceneAsync()
        {

            GameSceneManager.Instance.UnLoadScene(this.battleScence, () =>
            {
                this.battleScence = null;
            });
            
        }

        /// <summary>
        /// 主角受伤回调
        /// </summary>
        public void OnLeaderHurt()
        {
            BattleUI.RefreshLeaderHPText();
        }

        /// <summary>
        /// 主角死亡回调
        /// </summary>
        public void OnLeaderDie()
        {
            isWin = false;
            BattleEnd();
        }

        /// <summary>
        /// 战斗结束入口
        /// </summary>
        public void BattleEnd()
        {
            if (this.curBattleState != EBattleState.End)
            {
                this.curBattleState = EBattleState.End;
                isWin = BattleMonsterManager.Instance.monsterCount <= 0 && LeaderManager.ins.GetLeaderCurHP() > 0;
                int star = CheckStar();
                BattleEventManager.Dispatch(BattleEventName.battle_end);
                //发起结算
                BattleMsgManager.Instance.SendResultMsg(this.missionID, star, BattleMsgManager.Instance.randomSed, "", () =>
                {
                    if (isWin)//胜利
                    {
                        if (GameCenter.mIns.m_BattleMgr.curMissionParamCfg.aftermapid != -1)//是否存在战后交互场景
                        {

                            BattleDecodeManager.Instance.EnterBattleInteraction(GameCenter.mIns.m_BattleMgr.curMissionParamCfg.aftermapid, GameCenter.mIns.m_BattleMgr.missionID, 2, () => {
                                GameCenter.mIns.m_UIMgr.Open<battlewindup>(true, true);
                                QuitBattle();
                            });
                        }
                        else
                        {
                            GameCenter.mIns.m_UIMgr.Open<battlewindup>(true, false).SetCallBack(QuitBattle);
                        }
                    }
                    else//失败。直接呼出结算界面
                    {
                        GameCenter.mIns.m_UIMgr.Open<battlewindup>(false, false).SetCallBack(QuitBattle);
                    }
                });
                
            }
           
        }


        /// <summary>
        /// 退出战斗
        /// </summary>
        public void QuitBattle()
        {
            BattleUI.OnBattleExit();
            //退出战斗场景
            QuitBattleScene();
        }


        //--------------------------------------------------->>> 布阵 <<<---------------------------------------------------


        /// <summary>
        /// 通过guid获得战斗内对象（英雄、怪物）
        /// </summary>
        public BaseObject GetBaseObjByGuid(long objid)
        {
            BaseObject baseObject = BattleHeroManager.Instance.depolyHeroList.Find(hero => hero.objID == objid);
            if (baseObject != null)
            {
                return baseObject;
            }
            else if (BattleMonsterManager.Instance.dicMonsterList.ContainsKey(objid))
            {
                return BattleMonsterManager.Instance.dicMonsterList[objid];
            }
            return null;
        }





        //-------------------------------------------------
        /// <summary>
        /// 获得离传入坐标最近的可行走格子(不包括自己)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public V2 GetLatelyMapByIndex(V2 index)
        {
            int newX = 0;
            int newY = 0;
            CellData cellData;
            //地图最大宽度为11，最多找十次
            for (int i = 1; i < 10; i++)
            {
                //优先依次 上、左、下、右
                newX = (int)index.x;//上
                newY = (int)index.y + i;
                cellData = curMapData.maplist.Find(data => (int)data.index.x == newX && (int)data.index.y == newY);
                if (cellData != null && cellData.state == 1)
                {
                    return cellData.index;
                }

                newX = (int)index.x-1;//左
                newY = (int)index.y;
                cellData = curMapData.maplist.Find(data => (int)data.index.x == newX && (int)data.index.y == newY);
                if (cellData != null && cellData.state == 1)
                {
                    return cellData.index;
                }

                newX = (int)index.x;//下
                newY = (int)index.y-1;
                cellData = curMapData.maplist.Find(data => (int)data.index.x == newX && (int)data.index.y == newY);
                if (cellData != null && cellData.state == 1)
                {
                    return cellData.index;
                }

                newX = (int)index.x+1;//右
                newY = (int)index.y;
                cellData = curMapData.maplist.Find(data => (int)data.index.x == newX && (int)data.index.y == newY);
                if (cellData != null && cellData.state == 1)
                {
                    return cellData.index;
                }

                //其次依次 左上、左下、右下、右上
                newX = (int)index.x - 1;//左上
                newY = (int)index.y + 1;
                cellData = curMapData.maplist.Find(data => (int)data.index.x == newX && (int)data.index.y == newY);
                if (cellData != null && cellData.state == 1)
                {
                    return cellData.index;
                }

                newX = (int)index.x - 1;//左下
                newY = (int)index.y - 1;
                cellData = curMapData.maplist.Find(data => (int)data.index.x == newX && (int)data.index.y == newY);
                if (cellData != null && cellData.state == 1)
                {
                    return cellData.index;
                }

                newX = (int)index.x + 1;//右下
                newY = (int)index.y - 1;
                cellData = curMapData.maplist.Find(data => (int)data.index.x == newX && (int)data.index.y == newY);
                if (cellData != null && cellData.state == 1)
                {
                    return cellData.index;
                }

                newX = (int)index.x + 1;//右上
                newY = (int)index.y + 1;
                cellData = curMapData.maplist.Find(data => (int)data.index.x == newX && (int)data.index.y == newY);
                if (cellData != null && cellData.state == 1)
                {
                    return cellData.index;
                }
            }
            return null;
        }

        /// <summary>
        /// 通过地块index获得在关卡中的点位
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pathIndex">路线</param>
        /// <param name="targetPos">点位坐标</param>
        public int GetMissionPointByIndex(V2 index,int curPath,out int pathIndex,out V3 targetPos)
        {
            List<PointData> pointDatas;
            PointData curPoint;
            //先判断当前路线
            if (this.curMissionData.pathDatas.Count > curPath)
            {
                pointDatas = this.curMissionData.pathDatas[curPath].pointDatas;
                //当前路线 直接返回
                curPoint = pointDatas.Find(point => point.index.x == index.x && point.index.y == index.y);
                if (curPoint != null)
                {
                    pathIndex = curPath;
                    targetPos = curPoint.pos;
                    return curPoint.point;
                }
                else//寻找所有路线
                {
                    for (int i = 0; i < this.curMissionData.pathDatas.Count; i++)
                    {
                        pointDatas = this.curMissionData.pathDatas[i].pointDatas;
                        curPoint = pointDatas.Find(point => point.index.x == index.x && point.index.y == index.y);
                        if (curPoint != null)
                        {
                            pathIndex = i;
                            targetPos = curPoint.pos;
                            return curPoint.point;
                        }
                    }
                }
            }
            pathIndex = -1;
            targetPos = null;
            return -1;
        }


        

        /// <summary>
        /// 检测关卡条件判断星级
        /// </summary>
        private int CheckStar()
        {
            int start = 0;
            if (CheckOneParma(GameCenter.mIns.m_BattleMgr.curMissionParamCfg.star1, GameCenter.mIns.m_BattleMgr.curMissionParamCfg.star1param))
            {
                start += 1;
            }
            if (CheckOneParma(GameCenter.mIns.m_BattleMgr.curMissionParamCfg.star2, GameCenter.mIns.m_BattleMgr.curMissionParamCfg.star2param))
            {
                start += 1;
            }
            if (CheckOneParma(GameCenter.mIns.m_BattleMgr.curMissionParamCfg.star3, GameCenter.mIns.m_BattleMgr.curMissionParamCfg.star3param))
            {
                start += 1;
            }
            return start;
        }

        private bool CheckOneParma(int type, string param)
        {
            switch (type)
            {
                case 1://主角血量不低于x
                    if (LeaderManager.ins.GetLeaderCurHP() >= int.Parse(param))
                        return true;
                    break;
                case 2://对局内完成某个事件
                    break;
                default:
                    break;
            }
            return false;
        }
    }

    

    /// <summary>
    /// 战斗状态
    /// </summary>
    public enum EBattleState
    {
        Readly = 0,// 准备布阵阶段
        Start,//开始战斗
        Pause,//展厅战斗
        End,//战斗结束
    }


}

