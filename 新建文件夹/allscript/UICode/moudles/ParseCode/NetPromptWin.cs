using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class NetPromptWin : CommomCustomPrefab
{
	public NetPromptWin(GameObject parentNode, string resName, string abName = "", params object[] param)
	{
		prefabName = resName;
		string _abName = string.IsNullOrEmpty(abName) ? "common" : abName;
		string prompttx = param.Length > 0 ? param[0].ToString() : "";
		InitUI(null,resName, parentNode, prompttx);

		imgEffect = _Root.transform.Find("imgEffect").GetComponent<Image>();
		txPrompt = _Root.transform.Find("txPrompt").GetComponent<Text>();
        if (!string.IsNullOrEmpty(prompttx))
			txPrompt.text = prompttx;

		InitRect();
		RegisterButton();
	}

	public Image imgEffect;
	public Text txPrompt;

	public override async void InitUI(GameObject root, string resName, GameObject parentNode = null, string abName = "")
	{
		string _abName = string.IsNullOrEmpty(abName) ? "common" : abName;
        root = await ResourcesManager.Instance.LoadUIPrefabSync(resName);

		if (parentNode != null)
		{
			root.transform.SetParent(parentNode.transform);
		}
		_Root = root;
	}

    public override void InitRect()
    {
        base.InitRect();
		DoAnim();
	}

	public override void DoAnim()
    {
        base.DoAnim();
		UcsdoTween.playgameobject(imgEffect.gameObject, imgEffect.gameObject.GetInstanceID()).dorotate(0.0f, 0.0f, 360.0f, 3.0f, (int)DG.Tweening.RotateMode.LocalAxisAdd).setloops(-1, (int)DG.Tweening.LoopType.Yoyo);
	}

	public void RefreshContent(string newContent)
    {
		if (!string.IsNullOrEmpty(newContent) && _Root != null && txPrompt !=null)
			txPrompt.text = newContent;
	}

	public void DoDestry()
    {
        if (_Root != null)
        {
			GameObject.Destroy(_Root);
        }
    }
}
