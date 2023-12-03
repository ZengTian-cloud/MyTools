using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
// 无限滑动列表单个元素
public class TableItem : MonoBehaviour
{
	private RectTransform rect;
	public RectTransform rectTransform
	{
		get
		{
			if (rect == null)
				rect = this.GetComponent<RectTransform>();
			return rect;
		}
	}
	public List<string> names = new List<string>();
	public List<Object> monos = new List<Object>();
	public Dictionary<string, object> savedata = new Dictionary<string, object>();
	private bool m_createtag = true;

	private int t_objid = -1;
	public int m_objid
	{
		get
		{
			if (t_objid == -1)
			{
				t_objid = GameNode.CheckId(gameObject);
			}
			return t_objid;
		}
	}

	public void SetSaveData(string key, object obj)
	{
		if (!savedata.ContainsKey(key))
		{
			savedata.Add(key, obj);
		}
		else
		{
			savedata[key] = obj;
		}
	}

	public object GetSaveData(string key)
	{
		if (savedata.ContainsKey(key))
		{
			return savedata[key];
		}
		return null;
	}

	public Object GetMono(string n)
	{
		int index = names.IndexOf(n);
		if (index == -1)
		{
			return null;
		}
		return GetMono(index + 1);
	}

	public Object GetMono(int index)
	{
		--index;
		if (index >= 0 && index < monos.Count)
		{
			return monos[index];
		}
		return null;
	}

	public int Length
	{
		get
		{
			if (monos != null)
			{
				return monos.Count;
			}
			return 0;
		}
	}

	public bool Createtag { get => m_createtag; set => m_createtag = value; }


	// 适用与聊天
	public bool OpenContentSizeFitter = false;
    private ContentSizeFitter contentSizeFitter;
    public void ForceRefresh()
	{
		if (!OpenContentSizeFitter) return;
		if (contentSizeFitter == null)
			contentSizeFitter = GetComponent<ContentSizeFitter>();
		if (contentSizeFitter == null) return;

		int childCount = transform.childCount;
		for (int i = transform.childCount - 1; i >= 0; i--)
		{
			ContentSizeFitter csf = transform.GetChild(i).GetComponent<ContentSizeFitter>();
			if (csf != null)
			{
                LayoutRebuilder.ForceRebuildLayoutImmediate(csf.GetComponent<RectTransform>());
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }
}