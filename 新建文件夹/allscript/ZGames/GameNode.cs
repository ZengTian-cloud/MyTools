using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameNode
{
	private static Dictionary<int, GameObject> dictSeq = new Dictionary<int, GameObject>();
	private static List<int> listSeq = new List<int>();
	private static Dictionary<GameObject, int> dictNode = new Dictionary<GameObject, int>();
	private static List<GameObject> listNode = new List<GameObject>();
	private static int totalCnt = 0; // 当前总计数器
	private static int checktag = 0; // 检查间隔标记
	private static int tmpseqid = 0; // 临时Sequeue
	private static GameObject tmpobjid = null; // 临时GameObject

	public static void Launch()
	{
		dictSeq.Clear();
		listSeq.Clear();
		dictNode.Clear();
		listNode.Clear();
		totalCnt = 1;
	}

	public static void UpdateCheck()
	{
		++checktag;
		if (checktag >= 60)
		{
			checktag = 0;
			foreach (var item in dictSeq)
			{
				if (item.Value == null)
				{
					listSeq.Add(item.Key);
					listNode.Add(item.Value);
				}
			}
			for (var i = 0; i < listNode.Count; i++)
			{
				dictSeq.Remove(listSeq[i]);
				dictNode.Remove(listNode[i]);
			}
			listSeq.Clear();
			listNode.Clear();
		}
	}

	public static int CheckId(GameObject temobj)
	{
		tmpseqid = 0;
		if (temobj != null && !dictNode.TryGetValue(temobj, out tmpseqid))
		{
			tmpseqid = totalCnt;
			dictSeq.Add(tmpseqid, temobj);
			dictNode.Add(temobj, tmpseqid);
			++totalCnt;
		}
		return tmpseqid;
	}

	public static GameObject GetChildGameObject(int temid, string tempath)
	{
		var temobj = GetGameObject(temid);
		if (temobj != null && tempath != null)
		{
			var temtrans = temobj.transform.Find(tempath);
			temobj = temtrans != null ? temtrans.gameObject : null;
		}
		return temobj;
	}

	public static GameObject GetGameObject(int temid)
	{
		tmpobjid = null;
		dictSeq.TryGetValue(temid, out tmpobjid);
		return tmpobjid;
	}
}