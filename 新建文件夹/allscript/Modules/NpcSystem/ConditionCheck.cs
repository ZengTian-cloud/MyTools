using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 条件检查
/// </summary>
public static class ConditionCheck
{
    /// <summary>
    /// 检查是否满足条件
    /// </summary>
    /// <param name="conditions">条件字符串 类型;参数;判断类型 | 类型;参数;判断类型 ...</param>
    /// <param name="isAnd">true : &&  fasle : || </param>
    /// <returns></returns>
    public static bool Check(string conditions, bool isAnd)
    {
        if (string.IsNullOrEmpty(conditions))

            return true;

        string[] conditionArr = conditions.Split('|');

        return Check(conditionArr, isAnd);
    }
    public static bool Check(string[] conditions, bool isAnd)
    {
        bool isMeetConditions = false;

        for (int i = 0; i < conditions.Length; i++)
        {
            // 类型;参数;判断类型
            string[] condition = conditions[i].Split(";");

            bool checkResult = Check((NpcInteractionConditionType)int.Parse(condition[0]), long.Parse(condition[1]), int.Parse(condition[2]));

            if (isAnd)
            {
                if (!checkResult) return false;

                isMeetConditions = checkResult;
            }
            else
            {
                if (checkResult) return true;

                isMeetConditions = checkResult;
            }
        }
        return isMeetConditions;
    }
    public static bool Check(NpcInteractionConditionType condition, long par, int checkMode)
    {
        return condition switch
        {
            NpcInteractionConditionType.PlayerLevel => CheckLevel((int)par, checkMode),
            NpcInteractionConditionType.InteractionId => CheckInteraction(par, checkMode),
            NpcInteractionConditionType.TaskId => CheckTask(par, checkMode),
            NpcInteractionConditionType.GameLevelId => CheckGameLevel(par, checkMode),
            NpcInteractionConditionType.DecryptionId => CheckDecryption(par, checkMode),
            _ => false,
        };
    }
    /// <summary>
    /// 等级检查
    /// </summary>
    private static bool CheckLevel(int lv, int checkMode)
    {
        int playerLv = (int)GameCenter.mIns.userInfo.Level;

        return checkMode == 1 ? playerLv >= lv : playerLv < lv;
    }
    /// <summary>
    /// 交互事件检查
    /// </summary>
    private static bool CheckInteraction(long interactionId, int checkMode)
    {
        return checkMode == 1 ? NpcInteractionManager.Instance.CheckInteractionIsCompleted(interactionId)
                            : ! NpcInteractionManager.Instance.CheckInteractionIsCompleted(interactionId);
    }
    /// <summary>
    /// 任务检查
    /// </summary>
    private static bool CheckTask(long taskId, int checkMode)
    {
        return true;
    }
    /// <summary>
    /// 关卡检查
    /// </summary>
    private static bool CheckGameLevel(long gameLevelId, int checkMode)
    {
        return checkMode == 1 ? GameCenter.mIns.userInfo.CheckGameLevelIsCompleted(gameLevelId)
                            : ! GameCenter.mIns.userInfo.CheckGameLevelIsCompleted(gameLevelId);
    }
    /// <summary>
    /// 解密检查
    /// </summary>
    private static bool CheckDecryption(long decryptionId, int checkMode)
    {
        return true;
    }

}