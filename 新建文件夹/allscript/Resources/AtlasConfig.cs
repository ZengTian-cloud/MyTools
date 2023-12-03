/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 * 图集配置
 *	最终转二进制于游戏中加载使用
 *	由于手动管理图集，可通过配置用图片名和对应的路径crc找到该图片所在图集加载之
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;


[System.Serializable]
public class AtlasConfigInfo
{
	public SpriteAtlas atlas;
	public uint AtlasCRC;
	public int AtlasID;
	// 0:不常驻内存，1：常驻内存
	public int AtlasType = 0;
	public string AtlasName;
	public string AtlasPath;
	public List<AtlasSpriteInfo> Sprites;

	[System.Serializable]
	public class AtlasSpriteInfo
    {
		public string name;
		public string inAtlasName;
		public int idOutAtlas;
		public uint crc;
		public override string ToString()
		{
			return string.Format("AtlasSpriteInfo name:{0}, inAtlasName:{1}, idOutAtlas:{2}, crc:{3}",
				name, inAtlasName, idOutAtlas, crc);
		}
	}

    public override string ToString()
    {
        return string.Format("AtlasConfigInfo atlas:{0}, AtlasCRC:{1}, AtlasID:{2}, AtlasName:{3}, AtlasPath:{4}, AtlasType{5}, Sprites.Length:{6}",
			atlas, AtlasCRC, AtlasID, AtlasName, AtlasPath, AtlasType, Sprites.Count);
    }
}

[CreateAssetMenu(fileName = "AtlasConfig", menuName = "CreatAtlasConfig", order = 0)]
public class AtlasConfig : ScriptableObject
{
	[SerializeField]
	public List<AtlasConfigInfo> m_CommonAtlasList = new List<AtlasConfigInfo>();
	[SerializeField]
	public List<AtlasConfigInfo> m_NormalAtlasList = new List<AtlasConfigInfo>();
#if UNITY_EDITOR
    //序列化对象
    public SerializedObject m_SerializedObject;
    //序列化属性
    public SerializedProperty m_CommonAtlasListProperty;
    public SerializedProperty m_NormalAtlasListProperty;
    public static AtlasConfig Asset()
	{
		AtlasConfig config = UnityEditor.AssetDatabase.LoadAssetAtPath<AtlasConfig>(@"Assets\GameResources\Atlas\AtlasConfig.asset");
		return config;
	}
#endif
}


[System.Serializable]
public class AtlasConfigBytes 
{
	[SerializeField]
	public List<AtlasConfigInfoForBytes> m_CommonAtlasList { get; set; }
	[SerializeField]
	public List<AtlasConfigInfoForBytes> m_NormalAtlasList { get; set; }

	public void SetCom(List<AtlasConfigInfo> list)
    {
		foreach(AtlasConfigInfo atlasConfigInfo in list)
        {
			AtlasConfigInfoForBytes atlasConfigInfoForBytes = TypeInfoChanged(atlasConfigInfo);
			if(atlasConfigInfoForBytes != null)
            {
				if (m_CommonAtlasList == null)
					m_CommonAtlasList = new List<AtlasConfigInfoForBytes>();
				m_CommonAtlasList.Add(atlasConfigInfoForBytes);

			}
		}
    }
	public void SetNor(List<AtlasConfigInfo> list)
	{
		foreach (AtlasConfigInfo atlasConfigInfo in list)
		{
			AtlasConfigInfoForBytes atlasConfigInfoForBytes = TypeInfoChanged(atlasConfigInfo);
			if (atlasConfigInfoForBytes != null)
			{
				if (m_NormalAtlasList == null)
					m_NormalAtlasList = new List<AtlasConfigInfoForBytes>();
				m_NormalAtlasList.Add(atlasConfigInfoForBytes);
			}
		}
	}

	private AtlasConfigInfoForBytes TypeInfoChanged(AtlasConfigInfo atlasConfigInfo)
    {
		AtlasConfigInfoForBytes atlasConfigInfoForBytes = new AtlasConfigInfoForBytes();
		if (atlasConfigInfo != null)
        {
			atlasConfigInfoForBytes.AtlasCRC = atlasConfigInfo.AtlasCRC;
			atlasConfigInfoForBytes.AtlasID = atlasConfigInfo.AtlasID;
			atlasConfigInfoForBytes.AtlasName = atlasConfigInfo.AtlasName;
			atlasConfigInfoForBytes.AtlasPath = atlasConfigInfo.AtlasPath;
			atlasConfigInfoForBytes.Sprites = atlasConfigInfo.Sprites;
			atlasConfigInfoForBytes.AtlasType = atlasConfigInfo.AtlasType;
			return atlasConfigInfoForBytes;
		}
		return null;
    }
}

[System.Serializable]
public class AtlasConfigInfoForBytes
{
	public uint AtlasCRC;
	public int AtlasID;
	public int AtlasType;
	public string AtlasName;
	public string AtlasPath;
	public List<AtlasConfigInfo.AtlasSpriteInfo> Sprites;
	public override string ToString()
	{
		return string.Format("AtlasConfigInfo atlas:{0}, AtlasCRC:{1}, AtlasID:{2}, AtlasName:{3}, AtlasPath:{4}, Sprites.Length:{5}",
			null, AtlasCRC, AtlasID, AtlasName, AtlasPath, Sprites.Count);
	}
}
