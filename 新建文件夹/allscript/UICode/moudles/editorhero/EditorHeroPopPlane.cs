using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EditorHeroPopPlane : MonoBehaviour
{
    public Transform root;

    public Button btnClose;
    public TMP_Text txTitle;
    public GameObject animList;
    public GameObject animListContent;
    public GameObject animListItem;
    public Button btnDelEvent;
    public Button btnPlay;
    public Button btnAdd;
    public Button btnSave;
    public Button btnRestore;
    public Button btnSetEvent;
    public TMP_Dropdown dropdown;
    public TMP_InputField inputSetTime;
    public TMP_InputField inputTimeScale;
    public Slider slider;
    public TMP_Text sliderTx;
    public TMP_Text eventTx;

    private int heroId;
    private string animName;
    private float animLen;
    private float animPre;
    private string animPreName;
    private int currFrame;
    private EditorHero editorHero;
    private EditorHeroActionData previewData;

    private EditorHeroActionData currSetData;

    private List<ListItem> listItems = new List<ListItem>();

    public void Update()
    {
        if (animPre > 0.01f && editorHero != null)
        {
            zanimator.playatpoint(editorHero.curEHD.animator.gameObject, animPreName, animPre);
        }
    }

    public void SetActive(bool b, string animName, EditorHeroActionData previewData, EditorHero _editorHero = null)
    {
        this.previewData = previewData;
        this.currSetData = previewData;
        //if (previewData != null)
        //{
        //    currSetData = EditorHeroHelper.DeepCopy<EditorHeroActionData>(previewData);
        //    Debug.Log("SetActive previewData: " + previewData.animName + " - currSetData:" + currSetData.animName);
        //    Debug.Log("Count: " + currSetData.events.Count);
        //    if (currSetData.editorHeroActionAnimListDatas == null)
        //        currSetData.editorHeroActionAnimListDatas = new List<EditorHeroActionAnimListData>();
        //    if (currSetData.events == null)
        //        currSetData.events = new List<int>();
        //    if (currSetData.effectData == null)
        //        currSetData.effectData = new List<EditorHeroEffectPopData>();
        //}
        this.animName = animName;
        root.gameObject.SetActive(b);
        if (b)
        {
            btnClose.onClick.AddListener(() => {
                SetActive(false, "", null);
            });
        
            btnPlay.onClick.AddListener(() => {
                if (listItems != null && listItems.Count > 0)
                {
                    // 先只播放一个
                    foreach (var li in listItems)
                    {
                        if (li.editorHeroActionAnimListData != null)
                        {
                            _editorHero.PlayAnim(animName, li.editorHeroActionAnimListData.animName);
                            break;
                        }
                    }
                    
                }
                else{
                    GameCenter.mIns.m_UIMgr.PopMsg("请先绑定动画");
                }
                //_editorHero.PlayAnim(animName);
            });
            btnAdd.onClick.AddListener(() => {
                _editorHero.PopList(true, "animlist", (clipName) => {
                    Debug.Log("clipName:" + clipName);
                    if (string.IsNullOrEmpty(clipName))
                    {
                        return;
                    }
                    AddAnim(_editorHero.curEHD.animator, clipName);
                });
            });
            btnSave.onClick.AddListener(() => {
                // Init(editorHero, previewData);
                if (_editorHero != null)
                {
                    EditorHeroActionData editorHeroActionData = _editorHero.curEHD.GetAnimDataByName(animName);
                    if (editorHeroActionData != null)
                    {
                        editorHeroActionData = currSetData;
                        GameCenter.mIns.m_UIMgr.PopMsg("保存成功!");
                        _editorHero.UpdateEHListItem(animName);
                        SetActive(false, "", null);
                    }
                }
            });
            btnRestore.onClick.AddListener(() => {
                Init(editorHero, previewData);
                GameCenter.mIns.m_UIMgr.PopMsg("还原成功!");
            });

            btnDelEvent.onClick.AddListener(() => {
                if (currSetData != null)
                {
                    string s = currSetData.RemoveEvent();
                    eventTx.text = "events:" + s;
                }
                else if (_editorHero != null)
                {
                    EditorHeroActionData editorHeroActionData = _editorHero.curEHD.GetAnimDataByName(animName);
                    if (editorHeroActionData != null)
                    {
                        string s = editorHeroActionData.RemoveEvent();
                        eventTx.text = "events:" + s;
                    }
                }
            });
            btnSetEvent.onClick.AddListener(() => {
                if (currSetData != null)
                {
                    Debug.Log("Count: " + currSetData.events.Count);
                    string s = currSetData.AddEvent(currFrame);
                    eventTx.text = "events:" + s;
                }
                else if(_editorHero != null)
                {
                    EditorHeroActionData editorHeroActionData = _editorHero.curEHD.GetAnimDataByName(animName);
                    if (editorHeroActionData != null)
                    {
                        string s = editorHeroActionData.AddEvent(currFrame);
                        eventTx.text = "events:" + s;
                    }
                }
            });

            dropdown.ClearOptions();
            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
            optionData.text = "请选择一个Key";
            dropdown.options.Add(optionData);
            Init(_editorHero, currSetData, false);
        }
        else
        {
            btnClose.onClick.RemoveAllListeners();
            btnDelEvent.onClick.RemoveAllListeners();
            btnPlay.onClick.RemoveAllListeners();
            btnAdd.onClick.RemoveAllListeners();
            btnSave.onClick.RemoveAllListeners();
            btnRestore.onClick.RemoveAllListeners();
            btnSetEvent.onClick.RemoveAllListeners();
        }
    }

    public void Init(EditorHero _editorHero, EditorHeroActionData previewData, bool needCopy = true)
    {
        animListItem.SetActive(false);
        editorHero = _editorHero;
        //if (previewData != null && needCopy)
        //{
        //    this.previewData = previewData;
        //    currSetData = EditorHeroHelper.DeepCopy<EditorHeroActionData>(previewData);
        //    if (currSetData.editorHeroActionAnimListDatas == null)
        //        currSetData.editorHeroActionAnimListDatas = new List<EditorHeroActionAnimListData>();
        //    if (currSetData.events == null)
        //        currSetData.events = new List<int>();
        //    if (currSetData.effectData == null)
        //        currSetData.effectData = new List<EditorHeroEffectPopData>();
        //    Debug.Log("Init previewData: " + previewData.animName + " - currSetData:" + currSetData.animName);
        //}
        CreateList();

        if (editorHero != null)
        {
            // Debug.LogError("~~animName:" + animName);
            EditorHeroActionData editorHeroActionData = _editorHero.curEHD.GetAnimDataByName(animName);
            if (editorHeroActionData != null && editorHeroActionData.editorHeroActionAnimListDatas.Count > 0)
            {
                animLen = editorHeroActionData.editorHeroActionAnimListDatas[0].len;
            }
           // animLen = zanimator.getlength(editorHero.curEHD.animator.gameObject, animName);
            sliderTx.text = "0/" + animLen.ToString();
            slider.onValueChanged.AddListener((v) =>
            {
                int curr = (int)Mathf.Floor(v * animLen + 0.5f);
                sliderTx.text = curr.ToString() + "/" + animLen.ToString();

                string playAnimName = animName;
                EditorHeroActionData editorHeroActionData = _editorHero.curEHD.GetAnimDataByName(animName);
                if (editorHeroActionData != null && editorHeroActionData.editorHeroActionAnimListDatas.Count > 0)
                {
                    playAnimName = editorHeroActionData.editorHeroActionAnimListDatas[0].animName;
                }
                zanimator.playatpoint(editorHero.curEHD.animator.gameObject, playAnimName, v);
                animPreName = playAnimName;
                animPre = v;
                currFrame = curr;
            });
        }
    }

    private void CreateList()
    {
        if (listItems != null)
        {
            foreach (var item in listItems)
            {
                item.OnDestroy();
            }
        }
        listItems = new List<ListItem>();
        if (currSetData != null)
        {
            for (int i = 0; i < currSetData.editorHeroActionAnimListDatas.Count; i++)
            {
                var item = currSetData.editorHeroActionAnimListDatas[i];
                GameObject o = GetCloneListItem(item);
                ListItem listItem = new ListItem();
                listItem.Init(item, o);
                listItems.Add(listItem);
                o.SetActive(true);
            }
      
            EditorHeroActionData editorHeroActionData = editorHero.curEHD.GetAnimDataByName(animName);
            if (editorHeroActionData != null)
            {
                eventTx.text = "events:" + editorHeroActionData.GetEventString();
            }
        }
    }

    private GameObject GetCloneListItem(EditorHeroActionAnimListData data)
    {
        GameObject o = GameObject.Instantiate(animListItem);

        o.transform.SetParent(animListItem.transform.parent);
        o.transform.localScale = Vector3.one;
        o.transform.Find("txName").GetComponent<TMP_Text>().text = "动画" + listItems.Count;
        o.transform.Find("input1").GetComponent<TMP_InputField>().text = data.animName;
        o.transform.Find("input2").GetComponent<TMP_InputField>().text = "长度:" + data.len.ToString();
        o.transform.Find("input3").GetComponent<TMP_InputField>().text = data.len.ToString();
        o.transform.Find("input4").GetComponent<TMP_InputField>().text = "0";
        o.transform.Find("btnDel").GetComponent<Button>().onClick.AddListener(() => {
            // delect
            RemoveAnim(data);
            editorHero.UpdateEHListItem(data.animName);
        });
        return o;
    }

    private void AddAnim(Animator animator, string clipName)
    {
        EditorHeroActionAnimListData editorHeroActionAnimListData = new EditorHeroActionAnimListData();
        editorHeroActionAnimListData.uid = new Snowflake().GetId();
        foreach (var item in animator.runtimeAnimatorController.animationClips)
        {
            if (item.name == clipName)
            {
                Debug.Log("item.name:" + item.name);
                editorHeroActionAnimListData.animName = clipName;
                editorHeroActionAnimListData.len = (int)(item.length * 1000);
                editorHeroActionAnimListData.param1 = (int)(item.length * 1000);
                editorHeroActionAnimListData.param2 = 0;
                break;
            }
        }
        GameObject o = GetCloneListItem(editorHeroActionAnimListData);
        o.SetActive(true);
        Debug.Log("o:" + o.name);
        ListItem listItem = new ListItem();
        listItem.Init(editorHeroActionAnimListData, o);
        listItems.Add(listItem);
        editorHero.curEHD.GetAnimDataByName(animName).AddHeroActionAnim(animName, editorHeroActionAnimListData);
        editorHero.UpdateEHListItem(animName);
    }

    private void RemoveAnim(EditorHeroActionAnimListData data)
    {
        for (int i = listItems.Count - 1; i >= 0; i--)
        {
            if (listItems[i].editorHeroActionAnimListData.uid == data.uid)
            {
                listItems[i].OnDestroy();
                listItems.RemoveAt(i);
                editorHero.curEHD.GetAnimDataByName(animName).RemoveHeroActionAnim(animName, data);
            }
        }
    }

    private class ListItem
    {
        public EditorHeroActionAnimListData editorHeroActionAnimListData;
        public GameObject o;
        public void Init(EditorHeroActionAnimListData _editorHeroActionAnimListData, GameObject _o)
        {
            editorHeroActionAnimListData = _editorHeroActionAnimListData;
            o = _o;
        }

        public void OnDestroy()
        {
            if (o != null)
            {
                GameObject.Destroy(o);
            }
        }
    }
}

// 英雄动作编辑数据
public class EditorHeroActionData
{
    public string animName;

    public List<EditorHeroActionAnimListData> editorHeroActionAnimListDatas = new List<EditorHeroActionAnimListData>();

    public List<int> events = new List<int>();

    public List<EditorHeroEffectPopData> effectData = new List<EditorHeroEffectPopData>();

    public EditorHeroJumpData jumpData;

    public float timeScale;

    public void Changed(EditorHeroActionData editorHeroActionData)
    {
        editorHeroActionAnimListDatas = editorHeroActionData.editorHeroActionAnimListDatas;
        events = editorHeroActionData.events;
        timeScale = editorHeroActionData.timeScale;
    }

    public string AddEvent(int frame)
    {
        events.Add(frame);
        return GetEventString();
    }

    public string RemoveEvent()
    {
        if (events.Count > 0)
        {
            events.RemoveAt(events.Count - 1);
        }
        return GetEventString();
    }

    public string GetEventString()
    {
        string s = "";
        for (int i = 0; i < events.Count; i++)
        {
            s = i != events.Count -1 ? s + events[i] + "-" : s + events[i];
        }
        return s;
    }

    public void AddHeroActionAnim(string animName, EditorHeroActionAnimListData heroActionAnimData)
    {
        if (this.animName == animName)
        {
            editorHeroActionAnimListDatas.Add(heroActionAnimData);
        }
    }

    public void RemoveHeroActionAnim(string animName, EditorHeroActionAnimListData heroActionAnimData)
    {
        if (this.animName == animName)
        {
            for (int i = editorHeroActionAnimListDatas.Count - 1; i >= 0; i--)
            {
                if (editorHeroActionAnimListDatas[i].uid == heroActionAnimData.uid)
                {
                    editorHeroActionAnimListDatas.RemoveAt(i);
                }
            }
        }
    }

    public void UpdateHeroActionAnim(string animName, EditorHeroActionAnimListData heroActionAnimData)
    {
        if (this.animName == animName)
        {
            for (int i = editorHeroActionAnimListDatas.Count - 1; i >= 0; i--)
            {
                if (editorHeroActionAnimListDatas[i].uid == heroActionAnimData.uid)
                {
                    editorHeroActionAnimListDatas[i] = heroActionAnimData;
                }
            }
        }
    }


    public void UpdateEditorHeroEffectPopData(string animName, EditorHeroEffectPopData editorHeroEffectPopData)
    {
        if (this.animName == animName)
        {
            for (int i = 0; i < effectData.Count; i++)
            {
                if (effectData[i].uid == editorHeroEffectPopData.uid)
                {
                    effectData[i] = editorHeroEffectPopData;
                }
            }
        }
    }

    public void AddEditorHeroEffectPopData(string animName, EditorHeroEffectPopData editorHeroEffectPopData)
    {
        if (this.animName == animName)
        {
            effectData.Add(editorHeroEffectPopData);
        }
    }

    public void RemoveEditorHeroEffectPopData(string animName, EditorHeroEffectPopData editorHeroEffectPopData)
    {
        if (this.animName == animName)
        {
            for (int i = effectData.Count - 1; i >= 0; i--)
            {
                if (effectData[i].uid == editorHeroEffectPopData.uid)
                {
                    effectData.RemoveAt(i);
                }
            }
        }
    }

    public void UpdateJumpData(string animName, EditorHeroJumpData jumpData)
    {
        if (this.animName == animName)
        {
            // jumpData == null == remove
            this.jumpData = jumpData;
        }
    }
}

public class EditorHeroActionAnimListData
{
    public long uid;
    public string animName; // name death
    public int len; // anilen
    public int param1; // logiclen
    public int param2; // offset
    //  {anilen=2200,logiclen=2200,name="death",offset=0,tname="death"}
    public EditorHeroActionAnimListData()
    {

    }

    public EditorHeroActionAnimListData(string animName, int len, int param1, int param2)
    {
        this.animName = animName;
        this.len = len;
        this.param1 = param1;
        this.param2 = param2;
    }
}

