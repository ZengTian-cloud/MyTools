using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CommomCustomPrefab : PrefabUIBase
{
	public override string PrefabType { get => "CustomPrefab"; }

	public string prefabName = "";

	public async void Init(GameObject parentNode, string resName, string abName = "", params object[] param)
    {
        prefabName = resName;
        string _abName = string.IsNullOrEmpty(abName) ? resName : abName;
        GameObject root = await ResourcesManager.Instance.LoadUIPrefabSync(resName);
        InitUI(root, resName, parentNode);
    }

	public virtual async void InitUI(GameObject root, string resName, GameObject parentNode = null, string abName = "")
	{
		prefabName = resName;
		if (root == null)
		{
			string _abName = string.IsNullOrEmpty(abName) ? resName : abName;
			root = await ResourcesManager.Instance.LoadUIPrefabSync(resName);
        }
		if (parentNode != null)
		{
			root.transform.SetParent(parentNode.transform);
		}
		_Root = root;
	
		InitRect();
		RegisterButton();
	}

	public override void InitRect()
	{
		_Root.transform.localPosition = Vector3.zero;
		_Root.transform.localScale = Vector3.one;
		RectTransform rect = _Root.GetComponent<RectTransform>();
		rect.anchorMin = new Vector2(0, 0);
		rect.anchorMax = new Vector2(1, 1);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.offsetMin = new Vector2(0, 0);
		rect.offsetMax = new Vector2(0, 0);

		DoAnim();
	}

	public override void DoAnim()
	{
	
	}

	public override void RegisterButton()
	{
	}

	public override void UnRegisterButton()
	{
	}

	public void OnDestroy()
	{
		UnRegisterButton();
        if (_Root != null)
        {
			GameObject.Destroy(_Root);
        }
	}
}
