using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EditorHeroEffectPopPlane : MonoBehaviour
{
    public Transform root;

    public Button btnClose;
    public Button btnSave;
    public Button btnDel;
    public Button btnRestore;
    public Button btnCopy;
    public Button btnPaste;
    public Button btnSetDelayTime;
    public Button btnPlay;

    public TMP_InputField inputEffectName;
    public Button btnSetEffectRes;
    public Button btnSetEffectPoint;

    public Toggle toggleUseRes;
    public Toggle toggleNoRotation;

    public TMP_InputField inputOffestX;
    public TMP_InputField inputOffestY;
    public TMP_InputField inputOffestZ;

    public TMP_InputField inputRotationX;
    public TMP_InputField inputRotationY;
    public TMP_InputField inputRotationZ;

    public TMP_InputField inputScaleX;
    public TMP_InputField inputScaleY;
    public TMP_InputField inputScaleZ;

    public TMP_InputField inputDuration;
    public TMP_InputField inputDelay;

    public Slider slider;
    public TMP_Text sliderTx;

    public EditorHeroEffectPopData previewData;
    public EditorHeroEffectPopData currData;

    private EditorHero editorHero;
    private string animName;
    private float animLen;
    private float animPre;
    private string animPreName;
    public void Update()
    {
        if (animPre > 0.01f && editorHero != null)
        {
            zanimator.playatpoint(editorHero.curEHD.animator.gameObject, animPreName, animPre);
        }
    }

    public void SetActive(bool b, string animName, EditorHero _editorHero = null, EditorHeroEffectPopData _previewData = null,
        Action<bool, EditorHeroEffectPopData> action = null)
    {
        editorHero = _editorHero;
        this.animName = animName;
        root.gameObject.SetActive(b);
        if (b)
        {
            if (_previewData != null)
            {
                previewData = _previewData;
                currData = EditorHeroHelper.DeepCopy<EditorHeroEffectPopData>(previewData);
                currData.animName = animName;
            }
            else
            {
                currData = new EditorHeroEffectPopData();
                currData.animName = animName;
                currData.uid = new Snowflake().GetId();
            }
            Reset();

            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(() => {
                SetActive(false, "");
            });

            btnSave.onClick.RemoveAllListeners();
            btnSave.onClick.AddListener(() => {
                currData = GetCurrSetData();
                currData.animName = this.animName;
                action?.Invoke(true, currData);
                SetActive(false, "");
                GameCenter.mIns.m_UIMgr.PopMsg("保存成功!");
            });

            btnDel.onClick.RemoveAllListeners();
            btnDel.onClick.AddListener(() => {
                action?.Invoke(false, currData);
                SetActive(false, "");
                GameCenter.mIns.m_UIMgr.PopMsg("删除成功!");
            });

            btnRestore.onClick.RemoveAllListeners();
            btnRestore.onClick.AddListener(() => {
                if (previewData != null)
                {
                    currData = EditorHeroHelper.DeepCopy<EditorHeroEffectPopData>(previewData);
                    currData.animName = animName;
                }
                else
                {
                    currData = new EditorHeroEffectPopData();
                    currData.animName = animName;
                    currData.uid = new Snowflake().GetId();
                }
                Reset();
                GameCenter.mIns.m_UIMgr.PopMsg("还原成功!");
            });

            btnCopy.onClick.RemoveAllListeners();
            btnCopy.onClick.AddListener(() => {
                //_editorHero.copyEffectPopData = GetCurrSetData();
                GameCenter.mIns.m_UIMgr.PopMsg("待完善!");
            });

            btnPaste.onClick.RemoveAllListeners();
            btnPaste.onClick.AddListener(() => {
                //currData = _editorHero.copyEffectPopData;
                //currData.animName = this.animName;
                //InitPlane(currData);
                GameCenter.mIns.m_UIMgr.PopMsg("待完善!");
            });

            btnSetDelayTime.onClick.RemoveAllListeners();
            btnSetDelayTime.onClick.AddListener(() => {

            });

            btnPlay.onClick.RemoveAllListeners();
            btnPlay.onClick.AddListener(() => {
                string playAnim = "";
                List<string> playAnimNames = _editorHero.curEHD.GetTheAnimPlayedAnimNames(animName);
                if (playAnimNames != null && playAnimNames.Count > 0)
                {
                    playAnim = playAnimNames[0];
                }
                editorHero.PlayAnim(animName, playAnim);
            });

            btnSetEffectRes.onClick.RemoveAllListeners();
            btnSetEffectRes.onClick.AddListener(() => {
                editorHero.PopList(true, "effectres", (s) => {
                    if (string.IsNullOrEmpty(s))
                    {
                        return;
                    }
                    btnSetEffectRes.GetComponentInChildren<TMP_Text>().text = s;
                    if (currData != null)
                    {
                        currData.effectResName = s;
                        Debug.Log("effectres sss:" + s);
                    }
                });
            });

            btnSetEffectPoint.onClick.RemoveAllListeners();
            btnSetEffectPoint.onClick.AddListener(() => {
                editorHero.PopList(true, "effectpoint", (s) => {
                    if (string.IsNullOrEmpty(s))
                    {
                        return;
                    }
                    btnSetEffectPoint.GetComponentInChildren<TMP_Text>().text = s;
                    if (currData != null)
                    {
                        currData.effectPoint = s;
                        Debug.Log("effectpoint sss:" + s);
                    }
                });
            });
        }

        if (editorHero != null)
        {
            string playAnim = "";
            List<string> playAnimNames = _editorHero.curEHD.GetTheAnimPlayedAnimNames(animName);
            if (playAnimNames != null && playAnimNames.Count > 0)
            {
                playAnim = playAnimNames[0];
            }

            animLen = zanimator.getlength(editorHero.curEHD.animator.gameObject, playAnim);
            sliderTx.text = "0/" + animLen.ToString();
            slider.onValueChanged.AddListener((v) => {
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
            });
        }
    }

    public void InitPlane(EditorHeroEffectPopData data = null)
    {
        if (data != null)
        {
            inputEffectName.name = data.effectName;
            toggleUseRes.isOn = data.useRes;
            toggleNoRotation.isOn = data.noRotation;
            btnSetEffectRes.GetComponentInChildren<TMP_Text>().text = data.effectResName;
            btnSetEffectPoint.GetComponentInChildren<TMP_Text>().text = data.effectPoint;

            inputOffestX.text = data.offest.x.ToString();
            inputOffestY.text = data.offest.y.ToString();
            inputOffestZ.text = data.offest.z.ToString();

            inputRotationX.text = data.rotation.x.ToString();
            inputRotationY.text = data.rotation.y.ToString();
            inputRotationZ.text = data.rotation.z.ToString();

            inputScaleX.text = data.scale.x.ToString();
            inputScaleY.text = data.scale.y.ToString();
            inputScaleZ.text = data.scale.z.ToString();

            inputDuration.text = data.duration.ToString();
            inputDelay.text = data.delay.ToString();

        }
    }

    public void Reset()
    {
        if (currData != null)
        {
            InitPlane(currData);
        }
        else
        {
            Clear();
        }
    }

    public void Clear()
    {
        inputEffectName.name = "";
        toggleUseRes.isOn = false;
        toggleNoRotation.isOn = false;
        btnSetEffectRes.GetComponentInChildren<TMP_Text>().text = "";
        btnSetEffectPoint.GetComponentInChildren<TMP_Text>().text = "";

        inputOffestX.text = "0";
        inputOffestY.text = "0";
        inputOffestZ.text = "0";

        inputRotationX.text = "0";
        inputRotationY.text = "0";
        inputRotationZ.text = "0";

        inputScaleX.text = "0";
        inputScaleY.text = "0";
        inputScaleZ.text = "0";

        inputDuration.text = "-1";
        inputDelay.text = "0";
    }

    private EditorHeroEffectPopData GetCurrSetData()
    {
        EditorHeroEffectPopData data = null;
        if (currData != null)
        {
            data = currData;
        }
        else
        {
            data = new EditorHeroEffectPopData();
            data.uid = new Snowflake().GetId();
        }
        data.effectName = inputEffectName.name;
        data.useRes = toggleUseRes.isOn;
        data.noRotation = toggleNoRotation.isOn;
        data.effectResName = btnSetEffectRes.GetComponentInChildren<TMP_Text>().text;
        data.effectPoint = btnSetEffectPoint.GetComponentInChildren<TMP_Text>().text;

        data.offest = new Vector3(float.Parse(inputOffestX.text), float.Parse(inputOffestY.text), float.Parse(inputOffestZ.text));
        data.rotation = new Vector3(float.Parse(inputRotationX.text), float.Parse(inputRotationY.text), float.Parse(inputRotationZ.text));
        data.scale = new Vector3(float.Parse(inputScaleX.text), float.Parse(inputScaleY.text), float.Parse(inputScaleZ.text));

        data.duration = float.Parse(inputDuration.text);
        data.delay = float.Parse(inputDelay.text);

        return data;
    }
}

public class EditorHeroEffectPopData
{
    public long uid;
    public string animName;
    public string effectName;
    public bool useRes;
    public bool noRotation;
    public string effectResName;
    public string effectPoint;
    public Vector3 offest;
    public Vector3 rotation;
    public Vector3 scale;
    public float duration;
    public float delay;
}
