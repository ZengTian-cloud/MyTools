using Cinemachine.Utility;
using UnityEngine;
/// <summary>
/// npc地图表
/// </summary>
public class NpcMapConfig : Config
{
	public string mapnote;//地图说明

	public string name;//地图名字

	public int cameratype;//摄像机类型 1-可调整角度 2-固定角度

	public string mapasset;//地图资源

	public string npclist;//npc列表

	public int npcuidistance;//UI隐藏距离（万分位）

	public int npcmodeldistance;//NPC模型隐藏距离（万分位）

	public int issaveposition;//是否记录最新坐标

    public string size;

    public string texturePath;

    private long[] _npcList;

	public long[] NpcList
	{
		get
		{
			if (_npcList==null)
			{
				string[] strArr = npclist.Split(';');

				_npcList = new long[strArr.Length];

				for (int i = 0; i < strArr.Length; i++)
				{
					_npcList[i] = long.Parse(strArr[i]);
				}
			}
			return _npcList;
		}
	}
    private Vector2 _size;

    public Vector2 Size
    {
        get
        {
            if (_size.Equals(default))
            {
                string[] strs = size.Split(';');

                _size = new Vector2(float.Parse(strs[0]), float.Parse(strs[1]));
            }
            return _size;
        }
    }
}

