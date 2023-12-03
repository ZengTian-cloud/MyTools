using DG.Tweening;
using System;
using UnityEngine;
using TMPro;

public class BattleSkillIcon : CommonSkillIcon
{
    #region ui components
    // ���ڵ�
    public Transform root;
    // rt
    public RectTransform rt;
    #endregion

    #region node
    // �϶�ǰ��λ��
    public Vector2 dragBeforePos;
    // ��ʼy�߶�
    private float initAncPosY = -32f;
    // ���
    private float interval = 160;
    // ˮƽ������ƶ��ٶ�
    private float moveHorizontalSpeed = 500;
    // ��ֱ������ƶ��ٶ�
    private float moveVerticalSpeed = 500;
    // ����ԭλ���ƶ��ٶ�
    private float moveReturnSpeed = 2000;
    // temp vec2
    private Vector2 tempVec2 = Vector2.zero;

    // ǰһ���ڵ����(������)
    public BattleSkillIcon prevBSI_Data;
    // ��һ���ڵ����(������)
    public BattleSkillIcon nextBSI_Data;
    // ǰһ���ڵ����(������)
    public BattleSkillIcon prevBSI_View;
    // ��һ���ڵ����(������)
    public BattleSkillIcon nextBSI_View;


    public bool bGary = false;

    /// <summary>
    /// ����ǰһ�����ݽڵ�
    /// </summary>
    /// <param name="battleSkillIcon"></param>
    public void SetPrevBSI_Data(BattleSkillIcon battleSkillIcon)
    {
        prevBSI_Data = battleSkillIcon;
    }

    /// <summary>
    /// ���ú�һ�����ݽڵ�
    /// </summary>
    /// <param name="battleSkillIcon"></param>
    public void SetNextBSI_Data(BattleSkillIcon battleSkillIcon)
    {
        nextBSI_Data = battleSkillIcon;
    }

    /// <summary>
    /// ����ǰһ�����ֽڵ�
    /// </summary>
    /// <param name="battleSkillIcon"></param>
    public void SetPrevBSI_View(BattleSkillIcon battleSkillIcon)
    {
        prevBSI_View = battleSkillIcon;
    }

    /// <summary>
    /// ���ú�һ�����ֽڵ�
    /// </summary>
    /// <param name="battleSkillIcon"></param>
    public void SetNextBSI_View(BattleSkillIcon battleSkillIcon)
    {
        nextBSI_View = battleSkillIcon;
    }

    // �Ƿ���ˮƽ�ƶ���
    public bool isMoveXAnim { get; private set; }
    // �Ƿ�����ֱ�ƶ���
    public bool isMoveYAnim { get; private set; }
    // �Ƿ��ڷ���ԭλ�ƶ���
    public bool isReturnAnim { get; private set; }
    // �Ƿ��ڳ鿨�����ƶ���
    public bool isOutCardAnim { get; private set; }
    // �Ƿ������⶯����
    public bool IsAniming { get { return isMoveXAnim || isMoveYAnim || isReturnAnim || isOutCardAnim; } }
    // �Ƿ�drag��������
    public bool IsDragLimitAniming { get { return isReturnAnim || isOutCardAnim; } }

    // ̧��Ķ������ܱ������������
    public Tweener clickUpTweener = null;
    // �Ƿ���̧�𶯻���
    public bool isClickUpAnim { get; private set; }

    // �Ƿ����������㶯����(���������ƶ��������)
    public bool isEnergyNotEnough = false;
    // �������㶯����¡����
    public GameObject energyNotEnoughCloneObj;
    /// <summary>
    /// tostring
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        int prev_data_index = prevBSI_Data == null ? -1 : prevBSI_Data.index;
        int next_data_index = nextBSI_Data == null ? -1 : nextBSI_Data.index;
        int prev_view_index = prevBSI_View == null ? -1 : prevBSI_View.index;
        int next_view_index = nextBSI_View == null ? -1 : nextBSI_View.index;
        return $"self index:{index} - prev_data_index:{prev_data_index} - next_data_index:{next_data_index} - prev_view_index:{prev_view_index} - next_view_index:{next_view_index}";
    }

    #endregion

    #region data
    // ��������
    public DrawCardData drawCardData { get; set; }
    // Ψһid
    public long uid { get; private set; }
    // ����id
    public long skillId { get; private set; }
    // Ӣ��id
    public long heroId { get; private set; }
    // ����ص�
    public Action<BattleSkillCfg> cliclCallback { get; private set; }
    // ���б�������
    public int index { get; set; }


    // ������icon
    private BattleSkillIcon tempBSI;
    // �ڸ�������ק����? --> �ò����ڿ��Ʒ���ԭλ�����Ϊfalse
    private bool bFloat = false;
    // �ڸ�������ק����? --> ֻҪ��ָ�ſ��ˣ��ͻ���Ϊfalse
    private bool bFingerDrag = false;
    /// <summary>
    /// �ڸ�������ק����?
    /// </summary>
    /// <returns></returns>
    public bool IsFloat()
    {
        return bFloat;
    }


    #endregion

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="o"></param>
    /// <param name="skillId"></param>
    /// <param name="heroId"></param>
    /// <param name="cliclCallback"></param>
    public BattleSkillIcon(GameObject o, long skillId, long heroId, Action<BattleSkillCfg> cliclCallback) : base(o, skillId, heroId, cliclCallback)
    {
    }

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="o"></param>
    /// <param name="skillId"></param>
    /// <param name="heroId"></param>
    /// <param name="cliclCallback"></param>
    public BattleSkillIcon(DrawCardData drawCardData, GameObject o, long skillId, long heroId, Action<BattleSkillCfg> cliclCallback, int index, bool isInit, Action<long> initCallback = null) : base(o, skillId, heroId, cliclCallback, isInit)
    {
        this.drawCardData = drawCardData;
        uid = drawCardData.uid;
        this.skillId = skillId;
        this.heroId = heroId;
        this.cliclCallback = cliclCallback;
        this.index = index;
        this.initCallback = initCallback;
        BindUI();
        if (isInit)
        {
            // ��ʼ����Ƭ����
        }
        else
        {
            // �³鿨����
        }
    }

    /// <summary>
    /// 刷新卡牌算力-算力改变时
    /// </summary>
    private void RefresExpend()
    {
        BaseHero baseHero = BattleHeroManager.Instance.GetBaseHeroByHeroID(heroId);
        if (baseHero != null && !baseHero.bRecycle)
        {
            if (baseHero.buffStackCompent.dicCurBuff.ContainsKey(11))
            {
                for (int i = 0; i < baseHero.buffStackCompent.dicCurBuff[11].Count; i++)
                {
                    BaseBuff baseBuff = baseHero.buffStackCompent.dicCurBuff[11][i];
                    if (baseBuff.mainBuff.buffCfg.functiontype == 11 && !string.IsNullOrEmpty(baseBuff.mainBuff.buffCfg.functionpm))
                    {
                        string[] pm = baseBuff.mainBuff.buffCfg.functionpm.Split(';');
                        if ((long.Parse(pm[0]) == heroId || long.Parse(pm[0]) == -1) && (m_SkillCfg.skilltype == int.Parse(pm[1])|| int.Parse(pm[1]) == -1))
                        {
                            if (txExpend != null)
                            {
                                drawCardData.energy = m_SkillCfg.energycost + int.Parse(baseBuff.mainBuff.buffCfg.value);
                                txExpend.GetComponent<TextMeshProUGUI>().text = drawCardData.energy.ToString();
                            }
                            return;
                        }
                    }
                }
            }
            if (txExpend!= null)
            {
                drawCardData.energy = m_SkillCfg.energycost;
                txExpend.GetComponent<TextMeshProUGUI>().text = drawCardData.energy.ToString();
            }
        }
    }

    /// <summary>
    /// BindUI
    /// </summary>
    protected override void BindUI()
    {
        BattleEventManager.RegisterEvent(BattleEventName.battle_cardCostChange, RefresExpend, true);
        base.BindUI();
        RefresExpend();
        root = obj.transform;
        rt = obj.GetComponent<RectTransform>();
        obj.name = "skillicon_" + index + "_" + uid;
        obj.SetActive(true);
        // rt.anchoredPosition = new Vector2(-index * interval, initAncPosY);
        // DoDrawAnim(new Vector2(-index * interval, initAncPosY));
        InitDrawAnim(new Vector2(-index * interval - 67, initAncPosY));
        //InitHighLightParam();
    }

    /// <summary>
    /// update
    /// </summary>
    public void Update()
    {
        if (rt == null)
        {
            return;
        }
        if (!isDoOutCard) return;

        if (bFloat)
        {
            // �Ƿ��view�б��а���
            float x_dis = Math.Abs(dragBeforePos.x - rt.anchoredPosition.x);
            float y_dis = Math.Abs(dragBeforePos.y - rt.anchoredPosition.y);
            // x:160 -- y:200
            //if (prevBSI_View == null && nextBSI_View == null && isReturnAnim && !UnityEditor.EditorApplication.isPaused)
            //{
            //    UnityEditor.EditorApplication.isPaused = true;
            //    //Debug.LogError("dragBeforePos:" + dragBeforePos + " - x_dis:" + x_dis + " - y_dis:" + y_dis);
            //}
            if (x_dis >= 160 && !isReturnAnim)
            {
                if (prevBSI_View == null && nextBSI_View == null)
                {
                    // //Debug.LogError("�Ѿ������� dragBeforePos:" + dragBeforePos + " - x_dis:" + x_dis + " - y_dis:" + y_dis);
                    return;
                }
                peelOrBackCallback?.Invoke(1, this);
            }
            else if ((prevBSI_View == null && nextBSI_View == null && (isReturnAnim || bFingerDrag) && x_dis < 160)) 
            {
                //Debug.LogError("isReturnAnim:" + isReturnAnim);
                // �ػ�view�б�
                peelOrBackCallback?.Invoke(0, this);
            }

            if (isReturnAnim)
            {
                fadeRightDownNode?.Invoke(this);
            }
            return;
        }

        //if (prevBSI_View != null && prevBSI_View.IsFloat() && prevBSI_View.prevBSI_View != null)
        //{
        //    tempBSI = prevBSI_View.prevBSI_View;
        //}
        //else
        //{
        tempBSI = prevBSI_View;
        //}

        if (tempBSI != null && tempBSI.rt != null && rt != null)
        {
            // ���ǰһ��bsi��float item�����Ƿ����Ҽ�ѹ
            if (tempBSI.IsFloat())
            {
                // ������item �����󻬶���
                if (tempBSI.dragBeforePos.x > tempBSI.rt.anchoredPosition.x)
                {
                    // ��ǰ�������ҿ�£
                    float _dis = Math.Abs(tempBSI.rt.anchoredPosition.x - tempBSI.dragBeforePos.x);
                    float wantToX = tempBSI.dragBeforePos.x - 160 + _dis;
                    ////Debug.LogError("tempBSI dragBeforePos:" + tempBSI.dragBeforePos + " - _dis:" + _dis);
                    // ��ק�в�ƥ���б�λ��
                    if (bFloat) return;
                    MoveToAnchorX(wantToX);
                    return;
                }
            }


            float dis = Math.Abs(tempBSI.rt.anchoredPosition.x - rt.anchoredPosition.x);
            if (dis != interval)
            {
                float wantToX = tempBSI.rt.anchoredPosition.x - interval;
                // ��ק�в�ƥ���б�λ��
                if (bFloat) return;
                MoveToAnchorX(wantToX);
            }
        }
        else if (index == 0)
        {
            if (bFloat) return;
            MoveToAnchorX(-67);
        }
    }

    // ���룬���ر����б��ص�
    Action<int, BattleSkillIcon> peelOrBackCallback;
    /// <summary>
    /// ���ð��룬���ر����б��ص�
    /// </summary>
    /// <param name="peelOrBackCallback"></param>
    public void SetPeelOrBackCallback(Action<int, BattleSkillIcon> peelOrBackCallback)
    {
        this.peelOrBackCallback = peelOrBackCallback;
    }

    // �����б������ص�
    Action<BattleSkillIcon> fadeRightDownNode;
    /// <summary>
    /// �����б������ص�
    /// </summary>
    /// <param name="fadeRightDownNode"></param>
    public void SetFadeRightDownNodeCallback(Action<BattleSkillIcon> fadeRightDownNode)
    {
        this.fadeRightDownNode = fadeRightDownNode;
    }

    /// <summary>
    /// ����
    /// </summary>
    public void Destroy()
    {

    }

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="bFloat"></param>
    public void OnFloat(bool bFloat)
    {
        bFingerDrag = bFloat;
        if (this.bFloat && bFloat) return;
        if (bFloat)
        {
            // ��ʼ��ק
            CheckClickUpAnim();
            if(dragBeforePos == Vector2.zero)
                dragBeforePos = rt.anchoredPosition;
            root.SetAsLastSibling();
            //Debug.LogError("��ʼ��ק");
            this.bFloat = true;
        }
        else
        {
            MoveReturn(dragBeforePos);
            root.SetSiblingIndex(index);
            //Debug.LogError("������ק");
            // peelOrBackCallback?.Invoke(0, this);
        }
        //this.bFloat = bFloat;
    }

    public void CancelOnFloatData()
    {
        bFingerDrag = false;
        bFloat = false;
        root.SetSiblingIndex(index);
    }

    /// <summary>
    /// ��ѡ��
    /// </summary>
    public void OnSelected()
    {
        selected.SetActive(true);
    }

    /// <summary>
    /// ȡ��ѡ��
    /// </summary>
    public void OnCancelSelected()
    {
        selected.SetActive(false);
    }

    /// <summary>
    /// �����ı�͸����
    /// </summary>
    /// <param name="fadeValue"></param>

    public void DoFadeImmediate(float fadeValue)
    {
        if (canvasGroup.alpha != fadeValue)
            canvasGroup.alpha = fadeValue;
    }

    /// <summary>
    /// �����ı�͸����
    /// </summary>
    /// <param name="fadeValue"></param>
    /// <param name="duration"></param>
    /// <param name="overCallback"></param>
    public void DoFadeDuration(float fadeValue, float duration, Action overCallback = null)
    {
        canvasGroup.DOFade(fadeValue, duration).OnComplete(() => {
            overCallback?.Invoke();
        });
    }

    /// <summary>
    /// ��ȡִ��ĳ���ƶ��ĳ���ʱ��
    /// </summary>
    /// <param name="toPos"></param>
    /// <param name="speed"></param>
    /// <returns></returns>
    private float GetMoveDuration(Vector2 toPos, float speed)
    {
        float dis = Vector2.Distance(toPos, rt.anchoredPosition);
        return dis / speed;
    }

    /// <summary>
    /// ˮƽ�����ƶ���ĳ��λ��
    /// </summary>
    /// <param name="x"></param>
    public void MoveToAnchorX(float x)
    {
        if (IsAniming || Math.Abs(rt.anchoredPosition.x - x) <= 0.05f) return;
        tempVec2.x = x;
        tempVec2.y = rt.anchoredPosition.y;
        isMoveXAnim = true;
        CheckClickUpAnim();
        rt.DOAnchorPosX(x, GetMoveDuration(tempVec2, moveHorizontalSpeed)).OnComplete(() => {
            isMoveXAnim = false;
        }).SetEase(Ease.Linear);
    }

    /// <summary>
    /// ��ֱ�����ƶ���ĳ��λ��
    /// </summary>
    /// <param name="y"></param>
    public void MoveToAnchorY(float y)
    {
        if (IsAniming || Math.Abs(rt.anchoredPosition.y - y) <= 0.05f) return;
        tempVec2.x = rt.anchoredPosition.x;
        tempVec2.y = y;
        isMoveYAnim = true;
        CheckClickUpAnim();
        rt.DOAnchorPosY(y, GetMoveDuration(tempVec2, moveVerticalSpeed)).OnComplete(() => {
            isMoveYAnim = false;
        }).SetEase(Ease.Linear);
    }

    /// <summary>
    /// ���ص�ĳ��λ��
    /// </summary>
    /// <param name="pos"></param>
    public void MoveReturn(Vector2 pos)
    {
        if (IsAniming || (Vector2.Distance(rt.anchoredPosition, pos) <= 0.05f)) return;
        tempVec2 = pos;
        isReturnAnim = true;
        //Debug.LogError("MoveReturn:" + pos);
        CheckClickUpAnim();
        rt.DOAnchorPos(pos, GetMoveDuration(tempVec2, moveReturnSpeed)).OnComplete(() => {
            isReturnAnim = false;
            bFloat = false;
            dragBeforePos = Vector2.zero;
        });
    }

    /// <summary>
    /// ���̧��
    /// </summary>
    public void ClickUp(bool bUp)
    {
        if (IsAniming) return;
        rt.DOKill();
        if (bUp)
        {
            isClickUpAnim = true;
            if (dragBeforePos == Vector2.zero)
                dragBeforePos = rt.anchoredPosition;
            clickUpTweener = rt.DOAnchorPos(new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y + 30), 0.2f).OnComplete(() => {
                isClickUpAnim = false;
                clickUpTweener = null;
            });
        }
        else if (dragBeforePos != Vector2.zero)
        { 
            MoveReturn(dragBeforePos);
        }
    }

    /// <summary>
    /// ���������������ʱ�� ���ڲ���̧�𶯻��� ֱ���ж�
    /// </summary>
    private void CheckClickUpAnim()
    {
        if (clickUpTweener != null)
        {
            clickUpTweener.Kill();
            clickUpTweener = null;
            // Debug.LogError("̧�𶯻��ж���!");
        }
    }

    /// <summary>
    /// ����������㶯��
    /// </summary>
    public void ClickEnergyNotEnough()
    {
        if (isEnergyNotEnough) return;
        // expend
        energyNotEnoughCloneObj = GameObject.Instantiate(expend);
        energyNotEnoughCloneObj.transform.parent = expend.transform.parent;
        energyNotEnoughCloneObj.transform.localPosition = expend.transform.localPosition;
        energyNotEnoughCloneObj.transform.localScale = expend.transform.localScale;
        energyNotEnoughCloneObj.transform.parent = expend.transform.parent.parent;
        energyNotEnoughCloneObj.name = "expendclone";
        expend.gameObject.SetActive(false);
        isEnergyNotEnough = true;
        energyNotEnoughCloneObj.transform.DOShakePosition(1, 5).OnComplete(() => {
            isEnergyNotEnough = false;
            GameObject.Destroy(energyNotEnoughCloneObj);
            expend.gameObject.SetActive(true);
        });
    }

    #region DrawAnim
    public bool isDoOutCard = false;
    private Vector2 wantToAncPos;
    private Action<long> initCallback;
    // ����λ��
    private Vector2 cradOutAncPos = new Vector2(93, -32);
    // ����λ�õ���λ����ٶ�
    private float cradOutToHighSpeed = 2800; //700f;
    // ��λ��
    private Vector2 cradHighAncPos = new Vector2(-67, 177);
    // ��λ�㵽��Ƭ�б��е��ٶ�(��̬�����ݾ���, �������ٶȻ���)
    private float cradHighToListSpeed = 4000;// 1400f;  //700f;

    /// <summary>
    /// ÿ�ſ��ƴӳ�ʼλ�õȲ�������
    /// </summary>
    public void InitDrawAnim(Vector2 wantToAncPos)
    {
        this.wantToAncPos = wantToAncPos;
        rt.anchoredPosition = cradOutAncPos;
        canvasGroup.alpha = 1f; // ����
        isDoOutCard = false;
    }

    /// <summary>
    /// �鿨����
    /// </summary>
    public void DoDrawAnim(Vector2 _wantToAncPos = default)
    {
        Vector2 toAncPos = _wantToAncPos == default ? wantToAncPos : _wantToAncPos;
        isOutCardAnim = true;
        if (rt == null)
        {
            return;
        }

        CheckClickUpAnim();
        canvasGroup.alpha = 1f;
        bool cdOldState = cd.activeSelf;
        bool expendOldState = expend.activeSelf;
        bool selectedOldState = selected.activeSelf;
        cd.SetActive(false);
        expend.SetActive(false);
        selected.SetActive(false);
        float time_oth = Vector2.Distance(cradOutAncPos, cradHighAncPos) / cradOutToHighSpeed;
        rt.DOAnchorPos(cradHighAncPos, time_oth).SetEase(Ease.OutExpo).OnComplete(() =>
        {
            float time_htl = Vector2.Distance(cradHighAncPos, toAncPos) / cradHighToListSpeed;
            GameCenter.mIns.RunWaitCoroutine(() => {
                rt.DOAnchorPos(toAncPos, time_htl).SetEase(Ease.InExpo).OnComplete(() =>
                {
                    cd.SetActive(cdOldState);
                    expend.SetActive(expendOldState);
                    selected.SetActive(selectedOldState);

                    isDoOutCard = true;
                    isOutCardAnim = false;
                    initCallback?.Invoke(uid);
                });
            }, 0.1f);
      
        });
        //canvasGroup.DOFade(1, time_oth).OnComplete(() =>
        //{
        //});
}
    #endregion


    #region high light

    private Material cloneMaterial;
    private Color oriColor;
    private float maxIntensity = 10;
    public string propertyColor = "_Color";
    public void InitHighLightParam()
    {
        Material _material = new Material(imgIcon.material);
        _material.name = _material.name + "(Copy)";
        _material.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
        imgIcon.material = _material;
        imgIcon.SetMaterialDirty();
        cloneMaterial = imgIcon.material;
        oriColor = cloneMaterial.GetColor(propertyColor);
    }

    public void DoHighLight()
    {
        Color top_c = new Color(oriColor.r * maxIntensity, oriColor.g * maxIntensity, oriColor.b * maxIntensity);
        cloneMaterial.DOColor(top_c, 0.1f).OnComplete(() =>
        {
            cloneMaterial.DOColor(oriColor, 0.1f).OnComplete(() =>
            {
                cloneMaterial.SetColor(propertyColor, oriColor);
            });
        });
    }

    #endregion
}
