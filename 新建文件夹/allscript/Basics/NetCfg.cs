using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameTiJi;

public class NetCfg
{
    /* old
       "http://gta.localcenter.qilegames.com";
   */
    //HTTP接口
    // 2023-9-5: 修改为中心服测试服地址
    private static string URL_BASIC = string.IsNullOrEmpty(GameTiJi.TijiConfig.masterURL)?"http://gta.testcenter.aitij.com": GameTiJi.TijiConfig.masterURL;//"http://gta.testcenter.qilegames.com";

    public static string URL_UPDATE_CHECK = URL_BASIC + "/check/update";
    public static string URL_LOGIN = URL_BASIC + "/login";
    public static string URL_LOGIN_SERVER = URL_BASIC + "/login/server";

    public static void SetUrlBasic(string url)
    {
        URL_BASIC = url;
    }

    //SOCKET接口
    public static int SOCKET_GATEWAY_CHECK = 0;
    public static int SOCKET_GET_KEY = 1;
    public static int SOCKET_SEND_KEY = 2;
    public static int SOCKET_HEART = 3;
    public static int SOCKET_COMMENTID_LOGIN = 1001;
    public static int SOCKET_COMMENTID_OFFLINE_PUSH = 1002;

    //gm接口
    public static int GM_CHANGEPROP = 3000;


    // push 消息接口
    public static int PUSH_FRIEND_ADD_RESULT_NOTIF = 102;  // 好友申请结果通知
    public static int PUSH_FRIEND_MSG_NOTIF = 103;  // 好友消息通知
    public static int PUSH_FRIEND_ADD_NOTIF = 104;  // 好友申请通知
    public static int PUSH_FRIEND_DELETE_NOTIF = 105;  // 删除好友通知
    public static int PUSH_FRIEND_BLACK_NOTIF = 106;  // 拉黑好友通知

    public static int PUSH_TASK_END_NOTIF = 109;//任务完成通知

    // 好友聊天
    public static int FRIEND_LIST = 5000;  // 获取好友列表
    public static int FRIEND_CHAT_TX = 5001;  // 获取好友私聊
    public static int FRIEND_CHAT_TX_SEND = 5002;  // 发送好友私聊
    public static int FRIEND_MARKCHAT_TX_READED = 5003;  // 标记已读私信
    public static int FRIEND_GET_APPLYLIST = 5004;  // 好友申请结果通知
    public static int FRIEND_APPLY_ADD = 5005;  // 申请添加好友
    public static int FRIEND_DISPOSE_APPLY = 5006;  // 处理好友申请
    public static int FRIEND_DISPOSE_BLACK = 5008;  // 处理拉黑好友
    public static int FRIEND_SEARCH = 5009;  // 查找好友
    public static int FRIEND_ELECT = 5010;  // 好友推荐
    public static int FRIEND_DELETE = 5011;  // 删除好友
    public static int FRIEND_DELETE_BLACK = 5012;  // 删除黑名单
    public static int FRIEND_ONETIME_DISPOSE_APPLY = 5013;  // 一键处理好友申请
    public static int FRIEND_REPORT_ROLE = 5014;  // 举报玩家
    public static int FRIEND_BLACK_LIST = 5015;  // 黑名单列表

    // 章节关卡
    public static int CHAPTER_GET_ALL = 8100;  // 获取章节信息
    public static int CHAPTER_GET_REWARD = 8101;  // 关卡章节奖励
    public static int CHAPTER_GET_MISSIONS = 8102;  // 关卡信息
    public static int CHAPTER_MISSION_UNLOCK = 8103;  // 关卡解锁(手动)
    public static int CHAPTER_MISSION_LOAD = 8104;  // 关卡加载
    public static int CHAPTER_MISSION_CHALLAGE = 8105;  // 关卡挑战
    public static int CHAPTER_MISSION_RESULT = 8106;  // 关卡结算
    public static int GetAllCompletedGameLevelInfo = 8107;

    //英雄养成
    public static int GROW_GET_PROP_GO = 8009;  // 获取英雄升级或突破的数据预览
    public static int GROW_HERO_UPGRADES = 8000;  // 英雄升级
    public static int GROW_HERO_BREAK = 8001;  // 英雄突破
    public static int GROW_HERO_SKILLUP = 8003; //英雄技能升级
    public static int GROW_HERO_WEAPONLEVELUP = 8004;//升级武器
    public static int GROW_HERO_WEAPONBREAK = 8005;//突破武器
    public static int GROW_HERO_WEAPONUPSTAR = 8006;//升星（解析）武器
    public static int GROW_HERO_SWITCHWEAPON = 8007;//切换武器
    public static int GROW_HERO_JIEXIAPON = 8008;//解析武器
    public static int GROW_HERO_TALENT = 8010;//升星
    public static int GROW_GET_WEAPON_PROP_GO = 8011;  // 获取英雄升级或突破的数据预览

    //NPC模块
    public static int NPC_GET_INTERACT_LIST = 8300;//获得交互列表
    public static int NPC_SAVE_INTERACT_ID = 8301;//保存交互id
    public static int NPC_GET_INTERACT_REWORD = 8302;//领取交互奖励

   
    //任务
    public static int TASK_GET_TASKLIST = 8200;//获得任务数据
    public static int TASK_GET_TASKREWARD = 8201;//领取任务奖励
    public static int TASK_ACCESS_TASK = 8202;//接取任务

    public static int LOGBOOK_GET_CURRENT = 8203;//获得里程碑当前数据
    public static int LOGBOOK_GET_FUNCREWARD = 8204;//获得功能任务奖励

    //大厅信息同步
    public static int GameLobby_Join = 1100;//加入大厅
    public static int GameLobby_Quit = 1103;//退出大厅
    public static int GameLobby_GetInfo = 1101;//获取大厅信息
    public static int GameLobby_UploadInfo = 1102;//上传大厅信息

}
