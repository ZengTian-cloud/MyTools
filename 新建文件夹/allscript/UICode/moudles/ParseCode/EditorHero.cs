using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class EditorHero : BaseWin
{
	public EditorHero() { }
	public override string Prefab => "editorhero";

	public GameObject _Root;
	public Transform uiNode;
	public Button btnClose;
	public Button btnSaveEvent;
	public Button btnSavePoint;

	// anim plane
	public Transform animPlane;
	public Button ap_btnMovePlane;
	public TMP_Dropdown ap_dropdown;
	public TMP_Text ap_tx1;
	public TMP_Text ap_tx2;
	public TMP_InputField ap_inputSkin;
	public Transform ap_list;
	public Transform ap_content;
	public Transform ap_animItem;

	// effctSetPlane
	public Transform animSetPlane;
	public EditorHeroPopPlane monoScript_editorHeroPopPlane;

	// effctSetPlane
	public Transform effctSetPlane;
	public EditorHeroEffectPopPlane monoScript_editorHeroEffectPopPlane;

	// popList
	public Transform popList;
	public EditorHeroPopList monoScript_editorHeroPopList;

	// jumpPlane
	public Transform jumpPlane;
	public EditorHeroJumpPlane monoScript_editorHeroJumpPlane;

	public Transform _3dNode;
	public Transform circleNode;
	public Transform floorNode;
	public Camera camera;
	public Transform heroNode;
	public Transform cacheNode;
	public Transform cameraLook;

	protected override void InitUI()
	{
		btns = new List<Button>();
		_Root = uiRoot;

		uiNode = _Root.transform.Find("uiNode");
		btnClose = uiNode.Find("btnClose").GetComponent<Button>();
		btnSaveEvent = uiNode.Find("btnSaveEvent").GetComponent<Button>();
		btnSavePoint = uiNode.Find("btnSavePoint").GetComponent<Button>();

		btnClose.onClick.RemoveAllListeners();
		btnSaveEvent.onClick.RemoveAllListeners();
		btnSavePoint.onClick.RemoveAllListeners();
		btnClose.onClick.AddListener(OnClickClose);
		btnSaveEvent.onClick.AddListener(OnClickSaveEvent);
		btnSavePoint.onClick.AddListener(OnClickSavePoint);
		btns.Add(btnClose);
		btns.Add(btnSaveEvent);
		btns.Add(btnSavePoint);


		// anim plane
		animPlane = uiNode.Find("animPlane");
		ap_btnMovePlane = animPlane.Find("btnMovePlane").GetComponent<Button>();
		ap_dropdown = animPlane.Find("dropdown").GetComponent<TMP_Dropdown>();
		ap_tx1 = animPlane.Find("tx1").GetComponent<TMP_Text>();
		ap_tx2 = animPlane.Find("tx2").GetComponent<TMP_Text>();
		ap_inputSkin = animPlane.Find("inputSkin").GetComponent<TMP_InputField>();
		ap_list = animPlane.Find("list");
		ap_content = animPlane.Find("list/content");
		ap_animItem = animPlane.Find("list/content/animItem");

		ap_btnMovePlane.onClick.RemoveAllListeners();
		ap_btnMovePlane.onClick.AddListener(OnClickMovePlane);
		btns.Add(ap_btnMovePlane);


		// animSetPlane
		animSetPlane = uiNode.FindHideInChild("animSetPlane");
		monoScript_editorHeroPopPlane = animSetPlane.GetComponent<EditorHeroPopPlane>();

		// ffctSetPlane
		effctSetPlane = uiNode.FindHideInChild("effctSetPlane");
		monoScript_editorHeroEffectPopPlane = effctSetPlane.GetComponent<EditorHeroEffectPopPlane>();

		// popList
		popList = uiNode.FindHideInChild("popList");
		monoScript_editorHeroPopList = popList.GetComponent<EditorHeroPopList>();

		// jumpPlane
		jumpPlane = uiNode.FindHideInChild("jumpPlane");
		monoScript_editorHeroJumpPlane = jumpPlane.GetComponent<EditorHeroJumpPlane>();

		_3dNode = _Root.transform.Find("3dNode");
		circleNode = _3dNode.transform.Find("circle");
		floorNode = _3dNode.transform.Find("floor");
		camera = _3dNode.transform.Find("camera").GetComponent<Camera>();
		heroNode = _3dNode.transform.Find("heroNode");
		cacheNode = _3dNode.transform.FindHideInChild("cacheNode");
		cameraLook = _3dNode.transform.Find("cameraLook");
	}
	partial void OnClickClose();
	partial void OnClickSaveEvent();
	partial void OnClickSavePoint();
	partial void OnClickMovePlane();
}

public class EHListItem
{
	public EditorHero m_editorHero;

	public Transform root;
	public Button btnPlay;
	public TMP_Text txAnimName;
	public Button btnAnimSet;
	public Transform effects;
	public Transform effectContent;
	public Transform effectItem;
	public Button btnVoice;
	public Button btnAddEffect;
	public Button btnSound;

	public int id;
	public string animName;

	public List<EHLIEffectItem> ehliEffectItems = new List<EHLIEffectItem>();

	public EHListItem(Transform _root, string tx, EditorHero editorHero)
    {
		Init(_root);
		txAnimName.text = tx;
		animName = tx;
		m_editorHero = editorHero;

		Reset();

		// btnAnimSet.GetComponentInChildren<TMP_Text>().text = animName;
		SetBtnAnimSetTx();
	}

	public void SetBtnAnimSetTx()
    {
		if (m_editorHero.curEHD != null && m_editorHero.curEHD.GetAnimDataByName(animName) != null)
		{
			if (m_editorHero.curEHD.GetAnimDataByName(animName).editorHeroActionAnimListDatas.Count > 0)
			{
				btnAnimSet.GetComponentInChildren<TMP_Text>().text = m_editorHero.curEHD.GetAnimDataByName(animName).editorHeroActionAnimListDatas[0].animName;
			}
            else
            {
				btnAnimSet.GetComponentInChildren<TMP_Text>().text = "编辑动画";
			}
		}
	}

	public void Init(Transform _root)
    {
		root = _root;
		btnPlay = root.Find("btnPlay").GetComponent<Button>();
		txAnimName = root.Find("txAnimName").GetComponent<TMP_Text>();
		btnAnimSet = root.Find("btnAnimSet").GetComponent<Button>();
		effects = root.Find("effects");
		effectContent = root.Find("effects/effectContent");
		effectItem = root.Find("effects/effectContent/effectItem");
		btnVoice = root.Find("btnVoice").GetComponent<Button>();
		btnAddEffect = root.Find("btnAddEffect").GetComponent<Button>();
		btnSound = root.Find("btnSound").GetComponent<Button>();

		btnPlay.onClick.RemoveAllListeners();
		btnAnimSet.onClick.RemoveAllListeners();
		btnVoice.onClick.RemoveAllListeners();
		btnAddEffect.onClick.RemoveAllListeners();
		btnSound.onClick.RemoveAllListeners();
		btnPlay.onClick.AddListener(OnClickPlayAnim);
		btnAnimSet.onClick.AddListener(OnClickAnimSet);
		btnVoice.onClick.AddListener(OnClickVoice);
		btnAddEffect.onClick.AddListener(OnClickAddEffect);
		btnSound.onClick.AddListener(OnClickSound);
	}

	public void Reset()
    {
		effectItem.gameObject.SetActive(false);

		if (ehliEffectItems != null)
        {
            foreach (var item in ehliEffectItems)
            {
				item.OnDestroy();
			}
        }
		ehliEffectItems = new List<EHLIEffectItem>();

        if (m_editorHero.curEHD != null && m_editorHero.curEHD.actDatas != null)
        {
            foreach (var item in m_editorHero.curEHD.actDatas)
            {
                if (item.animName == animName && item.effectData != null)
                {
                    foreach (var d in item.effectData)
                    {
						OnAddEffect(d, false);
					}
                }
			}
        }
	}

	public void OnAddEffect(EditorHeroEffectPopData editorHeroEffectPopData, bool addToData = true)
    {
		Debug.Log("~~ OnAddEffect.id:" + editorHeroEffectPopData.uid);
		EHLIEffectItem eHLIEffectItem = new EHLIEffectItem();
		GameObject o = GameObject.Instantiate(effectItem.gameObject);
		o.name = animName + "_" + ehliEffectItems.Count;
		o.transform.SetParent(effectItem.parent);
		o.transform.localScale = Vector3.one;
		o.SetActive(true);
		eHLIEffectItem.m_EHListItem = this;
		eHLIEffectItem.id = new Snowflake().GetId();
		eHLIEffectItem.editorHeroEffectPopData = editorHeroEffectPopData;
		eHLIEffectItem.Init(o.transform, ehliEffectItems.Count + 1);
		ehliEffectItems.Add(eHLIEffectItem);
        if (addToData)
        {
			m_editorHero.curEHD.AddEffect(animName, eHLIEffectItem.editorHeroEffectPopData);
		}
		Debug.Log("~~ eHLIEffectItem.id:" + eHLIEffectItem.id + " -d:" + eHLIEffectItem.editorHeroEffectPopData.uid);
	}

	public void OnRmoveEffect(long uid)
	{
		for (int i = ehliEffectItems.Count - 1; i >= 0; i--)
		{
			Debug.Log("~~ OnRmoveEffect has.id:" + ehliEffectItems[i].editorHeroEffectPopData.uid + " -r uid:" + uid);
			if (ehliEffectItems[i].editorHeroEffectPopData != null && ehliEffectItems[i].editorHeroEffectPopData.uid == uid)
			{
				m_editorHero.curEHD.RemoveEffect(animName, ehliEffectItems[i].editorHeroEffectPopData);
				ehliEffectItems[i].OnDestroy();
				ehliEffectItems.RemoveAt(i);
				break;
			}
		}
	}

	private void OnClickPlayAnim()
	{
		if (m_editorHero != null && m_editorHero.curEHD != null && m_editorHero.curEHD.GetAnimDataByName(animName) != null)
		{
			if (m_editorHero.curEHD.GetAnimDataByName(animName).editorHeroActionAnimListDatas.Count > 0)
			{
				m_editorHero.PlayAnim(animName, m_editorHero.curEHD.GetAnimDataByName(animName).editorHeroActionAnimListDatas[0].animName);
			}
			else
			{
				GameCenter.mIns.m_UIMgr.PopMsg("请先绑定动画");
			}
		}
		// m_editorHero.PlayAnim(animName);
	}

	private void OnClickAnimSet()
	{
        if (m_editorHero.monoScript_editorHeroPopPlane != null)
        {
			EditorHeroActionData editorHeroActionData = m_editorHero.curEHD.GetAnimDataByName(animName);
			m_editorHero.monoScript_editorHeroPopPlane.SetActive(true, animName, editorHeroActionData, m_editorHero);
		}
	}

	private void OnClickVoice()
	{

	}

	private void OnClickAddEffect()
	{
		if (m_editorHero.monoScript_editorHeroEffectPopPlane != null)
		{
			m_editorHero.monoScript_editorHeroEffectPopPlane.SetActive(true, animName, m_editorHero,
				null, (b, effectPopData) => {
					if (b)
					{
						OnAddEffect(effectPopData);
					}
				});
		}
	}

	private void OnClickSound()
	{

	}

	public void OnDestroy()
    {
        if (root != null)
        {
			GameObject.Destroy(root.gameObject);
        }
		m_editorHero = null;

	}
}

public class EHLIEffectItem
{
	public long id = 0;
	public Transform root;
	public Button btnEffect;
	public EditorHeroEffectPopData editorHeroEffectPopData;
	public EHListItem m_EHListItem;

	public void Init(Transform _root, int index)
	{
		root = _root;
		btnEffect = root.Find("btnEffect").GetComponent<Button>();
		btnEffect.transform.GetComponentInChildren<TMP_Text>().text = "特效" + index.ToString();
		btnEffect.onClick.RemoveAllListeners();
		btnEffect.onClick.AddListener(OnClickEffect);
	}

	private void OnClickEffect()
	{
		if (m_EHListItem.m_editorHero.monoScript_editorHeroEffectPopPlane != null)
		{
			m_EHListItem.m_editorHero.monoScript_editorHeroEffectPopPlane.SetActive(true, m_EHListItem.animName, m_EHListItem.m_editorHero,
				editorHeroEffectPopData, (b, effectPopData) => {
					if (b)
					{
						editorHeroEffectPopData = effectPopData;
						Debug.Log("~~ OnClickEffect save m_EHListItem.animName: " + m_EHListItem.animName);
						m_EHListItem.m_editorHero.curEHD.GetAnimDataByName(m_EHListItem.animName).UpdateEditorHeroEffectPopData(m_EHListItem.animName, editorHeroEffectPopData);
					}
					else
					{
						Debug.Log("~~ OnClickEffect remove: " + effectPopData.uid);
						m_EHListItem.OnRmoveEffect(effectPopData.uid);
					}
				});
		}
	}

	public void OnDestroy()
	{
		if (root != null)
		{
			GameObject.Destroy(root.gameObject);
		}
	}
}
