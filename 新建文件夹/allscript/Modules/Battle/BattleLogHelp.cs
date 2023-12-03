using UnityEngine;
using Basics;
using System.IO;
using System.Collections;
using Spine;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

public static class BattleLog
{
	private static bool bShowLog = true;

	public static void Log(params object[] param)
	{
		if (bShowLog)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < param.Length; i++)
			{
				builder.Append(param[i].ToString());
            }
			Debug.Log($"<color=yellow>[战斗日志]===>{builder} </color>");
		}
	}


    public static void LogError(string str)
    {
        if (bShowLog)
        {
			Debug.LogError(str);
        }
    }
}

