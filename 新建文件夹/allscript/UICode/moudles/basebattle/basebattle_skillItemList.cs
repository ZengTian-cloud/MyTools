using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Managers;

public class basebattle_skillItemList// : MonoBehaviour
{
    // �б�����ê������
    private float rightDownCanvasGroupToAlphaZeroAncY = 60;
    // ���������б�����
    public CanvasGroup rightDownCanvasGroup;
    // �������
    private GameObject backPanel;
    // �������CanvasGroup
    private CanvasGroup backPanelCanvasGroup;
    // �������ѡ���Ӷ���
    private GameObject backPanelChildSelect;

    // infoPanel
    private GameObject infoPanel;
    // infoPanel CanvasGroup
    private CanvasGroup infoPanelCanvasGroup;
    // infoPanel child bg rt
    private RectTransform infoPanelChildRt;
    // ���ܿ����б�
    private List<BattleSkillIcon> battleSkillIcons = new List<BattleSkillIcon>();
    public int BattleSkillIconsCount { get { return battleSkillIcons.Count; } }
    // 当前抽卡动画
    private BattleSkillIcon currDrawAniming = null;
    // �³�ȡ�Ŀ��ƶ��У���Ҫһ���Ų��ų鿨����
    private Queue<BattleSkillIcon> drawCardQueue = new Queue<BattleSkillIcon>();
    // �Ƿ��п������������ƶ���
    public bool HasCradDoAniming = false;
    // ���������ƶ����Ŀ����б�
    public List<BattleSkillIcon> currAnimingIcons = new List<BattleSkillIcon>();
    // ui���
    private Camera uiCamera = null;
    // ����ui��ȡ��ǰ�����Ļص�
    private Action getCurrEnergyCallback;
    // ��ǰ����
    public int CurrEnergy { get; set; }

    // �Ƿ��Ѿ���ʾ���ܷ�Χͼ�꣬���Ѿ���ʾ����ָ���ػ�������ʱ�������б�����ʾ�����������ɫ��ȷ�����պ󣬿�����ԭ����λ���Ϸ�����
    private bool isDisplaySkillRange = false;

    // ������
    private GameObject cardAdd = null;
    private RectTransform cardAddRT = null;
    private BuyCardItem buyCardItem = null;
    private basebattle _basebattle;

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="rightDownCanvasGroup"></param>
    /// <param name="backPanel"></param>
    /// <param name="infoPanel"></param>
    /// <param name="getCurrEnergyCallback"></param>
    public basebattle_skillItemList(CanvasGroup rightDownCanvasGroup, GameObject backPanel, GameObject infoPanel, Action getCurrEnergyCallback, basebattle _basebattle = null)
    {
        battleSkillIcons.Clear();
        uiCamera = GameCenter.mIns.m_UIMgr.UICamera;
        this.rightDownCanvasGroup = rightDownCanvasGroup;
        this.backPanel = backPanel;
        this._basebattle = _basebattle;
        backPanelCanvasGroup = backPanel.GetComponent<CanvasGroup>();
        backPanelChildSelect = backPanel.transform.Find("select").gameObject;
        this.infoPanel = infoPanel;
        infoPanelCanvasGroup = infoPanel.GetComponent<CanvasGroup>();
        infoPanelChildRt = infoPanel.transform.GetChild(0).GetComponent<RectTransform>();
        this.getCurrEnergyCallback = getCurrEnergyCallback;

        Transform listadd = rightDownCanvasGroup.transform.Find("cardList_add");
        cardAdd = listadd.FindHideInChild("cardAdd").gameObject;
        cardAddRT = cardAdd.GetComponent<RectTransform>();
        cardAddRT.localPosition = new Vector3(-60, -32, 0);
        buyCardItem = new BuyCardItem(cardAdd, (need) => {
            // ��ȡ��������
            getCurrEnergyCallback?.Invoke();
            //Debug.LogError(" cur:" + CurrEnergy + " - need:" + need);
            if (need > CurrEnergy)
            {

                GameCenter.mIns.m_UIMgr.PopMsg(GameCenter.mIns.m_LanMgr.GetLan("battle_buy"));
                return;
            }

            buyCardItem.CurrBuyCountAddOne();
            if (_basebattle != null)
            {
                _basebattle.OnBuyCardSucc(need);
            }
            /*
                ����ť����ʾ����Ҫ���ĵ�����			
                UI���Ż�����			
                ս���ؿ�������t_mission_battle.buyhandcost�ֶθ�Ϊramcost�ֶΣ���ʾ��������ʱ����������			
                ����ֵ4|8��ʾ������2�ε�һ�ι�����4���ڶ��ι�����8			
                ����-1��ʾ�ùؿ����ɹ���			
                �޷�����ʱ�������ƵļӺŰ�ť��ʧ			
			
                ͼ��0/2��ʾ�ɹ��򼸴Ρ�����1�κ��Ϊ1/2������2�κ�ð�ť��ʧ			
			
                �����ƺ�������һ����			
             
             */

        });
        InitCardLib();
    }

    public void Clear()
    {
        battleSkillIcons.Clear();
        currDrawAniming = null;
        drawCardQueue.Clear();
        HasCradDoAniming = false;
        uiCamera = null;
        getCurrEnergyCallback = null;
        CurrEnergy = 0;
        ClearCardLib();
    }

    /// <summary>
    /// 添加一张技能卡牌
    /// </summary>
    /// <param name="drawCardData"></param>
    /// <param name="o"></param>
    /// <param name="skillId"></param>
    /// <param name="heroId"></param>
    /// <param name="index"></param>
    /// <param name="isInit"></param>

    public void AddSkillIcon(DrawCardData drawCardData, GameObject o, long skillId, long heroId, int index = -1, bool isInit = false)
    {

        int toindex = index == -1 ? battleSkillIcons.Count : index;
        //Debug.LogError("battleSkillIcons.Count:" + battleSkillIcons.Count);
        BattleSkillIcon battleSkillIcon = new BattleSkillIcon(drawCardData, o, skillId, heroId, (cliclCallback)=> { 
        
        }, toindex, isInit, (uid) =>
        {
            // 
            if (currDrawAniming != null && uid == currDrawAniming.uid)
            {
                currDrawAniming = null;
            }
            if (drawCardQueue.Count > 0)
            {
                currDrawAniming = drawCardQueue.Dequeue();
                currDrawAniming.DoDrawAnim();
            }
            
        });

        battleSkillIcon.SetPeelOrBackCallback((state, icon) => {
            if (state == 1)
            {
                PeelFromView(icon);
            }
            else
            {
                BackToView(icon);
            }
        });

        battleSkillIcon.SetFadeRightDownNodeCallback((icon) => {
            DoRightDownCanvasGroupValue(icon);
        });
        

        //Debug.LogError("battleSkillIcon:" + battleSkillIcon);
        if (battleSkillIcon != null)
        {
            // ����鿨���У�����鿨�������Ƴ�
            if (currDrawAniming != null)
                drawCardQueue.Enqueue(battleSkillIcon);
            else
            {
                currDrawAniming = battleSkillIcon;
                currDrawAniming.DoDrawAnim();
            }
            if (index == -1)
            {
                // ˳�����
                BattleSkillIcon prev = GetBattleSkillIcon(battleSkillIcons.Count - 1);
                if (prev != null)
                {
                    battleSkillIcon.SetPrevBSI_Data(prev);
                    battleSkillIcon.SetPrevBSI_View(prev);
                    prev.SetNextBSI_Data(battleSkillIcon);
                    prev.SetNextBSI_View(battleSkillIcon);
                    battleSkillIcons.Add(battleSkillIcon);
                    //Debug.LogError("add sss prev:" + prev.index);
                }
                else
                {
                    battleSkillIcons.Add(battleSkillIcon);
                    //Debug.LogError("add sss prev:" + "null");
                }
            }
            else
            {
                // index����
                BattleSkillIcon prev = GetBattleSkillIcon(index - 1);
                BattleSkillIcon next = GetBattleSkillIcon(index + 1);
                if (prev != null)
                {
                    battleSkillIcon.SetPrevBSI_Data(prev);
                    battleSkillIcon.SetPrevBSI_View(prev);
                    prev.SetNextBSI_Data(battleSkillIcon);
                    prev.SetNextBSI_View(battleSkillIcon);
                    battleSkillIcons.Add(battleSkillIcon);
                }
                if (next != null)
                {
                    battleSkillIcon.SetNextBSI_Data(next);
                    battleSkillIcon.SetNextBSI_View(next);
                    next.SetPrevBSI_Data(battleSkillIcon);
                    next.SetPrevBSI_View(battleSkillIcon);
                    battleSkillIcons.Insert(index, battleSkillIcon);
                }
            }
        }

        foreach (var item in battleSkillIcons)
        {
            //Debug.LogError("init item:" + item.ToString());
        }
    }

    /// <summary>
    /// �Ƴ�һ������
    /// </summary>
    /// <param name="uid"></param>
    public void RemoveSkillIcon(long uid)
    {
        //Debug.LogError("RemoveSkillIcon item uid:" + uid);
        foreach (var item in battleSkillIcons)
        {
            //Debug.LogError("1 RemoveSkillIcon item:" + item.ToString());
        }
        for (int i = battleSkillIcons.Count - 1; i >= 0; i--)
        {
            //Debug.LogError("aaa RemoveSkillIcon uid:" + battleSkillIcons[i].uid);
            if (battleSkillIcons[i].uid == uid)
            {
                //Debug.LogError("bbb RemoveSkillIcon uid:" + battleSkillIcons[i].uid);
                BattleSkillIcon prev = battleSkillIcons[i].prevBSI_Data;
                BattleSkillIcon next = battleSkillIcons[i].nextBSI_Data;

                if (prev != null && next != null)
                {
                    prev.SetNextBSI_Data(next);
                    prev.SetNextBSI_View(next);
                    next.SetPrevBSI_Data(prev);
                    next.SetPrevBSI_View(prev);
                }
                else if (prev != null && next == null)
                {
                    prev.SetNextBSI_Data(null);
                    prev.SetNextBSI_View(null);
                }
                else if (prev == null && next != null)
                {
                    next.SetPrevBSI_Data(null);
                    next.SetPrevBSI_View(null);
                }

                battleSkillIcons[i].Destroy();
                battleSkillIcons.RemoveAt(i);
            }
        }
        rightDownCanvasGroup.alpha = 1;
        // ��������index
        int index = 0;
        foreach (var item in battleSkillIcons)
        {
            item.index = index++;
            item.obj.name = "skillicon_" + item.index + "_" + item.uid;
            //Debug.LogError("2 RemoveSkillIcon item:" + item.ToString());
        }
    }

    /// <summary>
    /// ��ȡ���ƣ��������б��е�����
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public BattleSkillIcon GetBattleSkillIcon(int index = -1)
    {
        if (index != -1 && index <= battleSkillIcons.Count - 1)
        {
            return battleSkillIcons[index];
        }
        else
        {
            // �����б����һ��
            return battleSkillIcons.Count <= 0 ? null : battleSkillIcons[battleSkillIcons.Count - 1];
        }
    }

    /// <summary>
    /// ��ȡ���ƣ����ݿ��ƶ���
    /// </summary>
    /// <param name="cardData"></param>
    /// <returns></returns>
    public BattleSkillIcon GetBattleSkillIcon(DrawCardData cardData)
    {
        foreach (var item in battleSkillIcons)
        {
            if (item.uid == cardData.uid)
            {
                return item;
            }
        }
        return null;
    }

    /// <summary>
    /// ���������
    /// </summary>
    /// <param name="cardData"></param>
    public void OnPointerDown(DrawCardData cardData, BaseHero baseHero = null)
    {
        BattleSkillIcon bsIcon = GetBattleSkillIcon(cardData);
        if (bsIcon != null)
        {
            getCurrEnergyCallback?.Invoke();
            if (bsIcon.drawCardData.skillCfgData.energycost > CurrEnergy)
            {
                // ��������
                bsIcon.ClickEnergyNotEnough();
            }
            else
            {
                // ̧��
                bsIcon.ClickUp(true);
            }
            bsIcon.OnSelected();
            if (CurrEnergy >= cardData.skillCfgData.energycost && baseHero != null)
            {
                SetHeroHeadPointerActive(true, baseHero.roleObj.transform.position, cardData);
            }
        }
        //Debug.LogError("`` OnPointerDown bsIcon:" + bsIcon.index);
    }

    /// <summary>
    /// �����̧��
    /// </summary>
    /// <param name="cardData"></param>
    public void OnPointerUp(DrawCardData cardData)
    {
        BattleSkillIcon bsIcon = GetBattleSkillIcon(cardData);
        if (bsIcon != null)
        {
            bsIcon.OnCancelSelected();
            if (!isDisplaySkillRange)
            {
                bsIcon.ClickUp(false);
            }
            SetHeroHeadPointerActive(false, Vector3.zero, cardData);
        }
        if (backPanel.activeSelf)
        {
            backPanel.SetActive(false);
            backPanelCanvasGroup.alpha = 1f;
        }
        //Debug.LogError("`` OnPointerUp bsIcon:" + bsIcon.index);
    }

    /// <summary>
    /// ��ʼ��ק����
    /// </summary>
    /// <param name="cardData"></param>
    public void OnBeginDrag(DrawCardData cardData)
    {
        BattleSkillIcon bsIcon = GetBattleSkillIcon(cardData);
        if (bsIcon != null)
        {
            bsIcon.OnFloat(true);
            bsIcon.OnCancelSelected();
        }
        if (!backPanel.activeSelf)
        {
            backPanel.SetActive(true);
            backPanel.transform.FindHideInChild("text").GetComponent<TMP_Text>().SetText(GameCenter.mIns.m_LanMgr.GetLan("battle_back"));
            backPanelCanvasGroup.alpha = 0f;
        }
        //Debug.LogError("`` OnBeginDrag bsIcon:" + bsIcon.index);
    }

    /// <summary>
    /// ��ק������
    /// </summary>
    /// <param name="cardData"></param>
    public void OnDrag(DrawCardData cardData)
    {
        //if (!IconCanDrag(cardData))
        //{
        //    Debug.Log("�ȴ��������ƶ������!");
        //    return;
        //}

        BattleSkillIcon bsIcon = GetBattleSkillIcon(cardData);
        if (bsIcon != null)
        {
            bsIcon.root.position = ScreenToUIWorldPos(bsIcon.root as RectTransform, Input.mousePosition, uiCamera);

            if (isDisplaySkillRange && backPanel.GetComponent<skillHander>().onPoint)
            {
                if (!backPanelChildSelect.activeSelf)
                {
                    backPanelChildSelect.SetActive(true);
                }
                return;
            }

            if (!backPanel.GetComponent<skillHander>().onPoint)
            {
                isDisplaySkillRange = true;
                rightDownCanvasGroup.alpha = 0f;
                backPanelCanvasGroup.alpha = 1f;
                if (backPanelChildSelect.activeSelf)
                {
                    backPanelChildSelect.SetActive(false);
                }
                return;
            }

            /*if (isDisplaySkillRange && bsIcon.rt.anchoredPosition.y < rightDownCanvasGroupToAlphaZeroAncY)
            {
                if (!backPanelChildSelect.activeSelf)
                {
                    backPanelChildSelect.SetActive(true);
                }
                return;
            }*/

            /*if (bsIcon.rt.anchoredPosition.y >= rightDownCanvasGroupToAlphaZeroAncY)
            {
                isDisplaySkillRange = true;
                rightDownCanvasGroup.alpha = 0f;
                backPanelCanvasGroup.alpha = 1f;
                if (backPanelChildSelect.activeSelf)
                {
                    backPanelChildSelect.SetActive(false);
                }
                return;
            }*/
            DoRightDownCanvasGroupValue(bsIcon);
        }
        ////Debug.LogError("`` OnDrag bsIcon:" + bsIcon.index);
    }

    /// <summary>
    /// ������ק����
    /// </summary>
    /// <param name="cardData"></param>
    public void OnCancelDrag(DrawCardData cardData)
    {
        if (backPanel.activeSelf)
        {
            backPanel.SetActive(false);
            backPanelCanvasGroup.alpha = 1f;
        }

        BattleSkillIcon bsIcon = GetBattleSkillIcon(cardData);
        if (isDisplaySkillRange)
        {
            // ����
            bsIcon.rt.anchoredPosition = bsIcon.dragBeforePos + new Vector2(0, 100);
            bsIcon.MoveReturn(bsIcon.dragBeforePos);
            rightDownCanvasGroup.alpha = 1f;
            isDisplaySkillRange = false;
            //// ��һִ֡��
            //GameCenter.mIns.RunWaitCoroutine(() =>
            //{
            //    Debug.LogError("������??? 2:" + bsIcon.rt.anchoredPosition);
            //    //bsIcon.rt.DOKill();
            //    bsIcon.MoveReturn(bsIcon.dragBeforePos);
            //});
            bsIcon.CancelOnFloatData();
            BackToView(bsIcon);
            return;
        }

        if (bsIcon != null)
        {
            bsIcon.OnFloat(false);
        }
        //Debug.LogError("`` OnCancelDrag bsIcon:" + bsIcon.index);
    }

    /// <summary>
    /// ִ�������б�������
    /// </summary>
    /// <param name="bsIcon"></param>

    public void DoRightDownCanvasGroupValue(BattleSkillIcon bsIcon)
    {
        if (bsIcon == null) { return; }
        // 0~160 fade
        if (bsIcon.rt.anchoredPosition.y >= 0)
        {
            float prop = bsIcon.rt.anchoredPosition.y / rightDownCanvasGroupToAlphaZeroAncY;
           //  Debug.LogError("~~ bsIcon.rt.anchoredPosition.y:" + bsIcon.rt.anchoredPosition.y + " - prop:" + prop);
            float fadeValue = 1 - prop;
            rightDownCanvasGroup.alpha = fadeValue;
            DoBackPanelCanvasGroupValue(1 - fadeValue);
            DoInfoPanelCanvasGroupValue(prop, 1 - fadeValue);
        }
    }

    /// <summary>
    /// �������CanvasGroupValue
    /// </summary>
    public void DoBackPanelCanvasGroupValue(float prop)
    {
        if (backPanelCanvasGroup != null)
        {
            backPanelCanvasGroup.alpha = prop;
        }
    }


    /// <summary>
    /// infoPanel(����ʱ�ı���)
    /// </summary>
    public void DoInfoPanelCanvasGroupValue(float fadeValue, float prop)
    {
        //if (infoPanelCanvasGroup != null)
        //{
        //    infoPanelCanvasGroup.alpha = prop;

        //    //RectTransform infoBgRt = infoPanel.transform.GetChild(0).GetComponent<RectTransform>();
        //    //CanvasGroup cg = infoPanel.GetComponent<CanvasGroup>();
        //    //infoBgRt.anchoredPosition = infoBgRt.anchoredPosition.x > 0 ? new Vector2(-500, infoBgRt.anchoredPosition.y) : new Vector2(500, infoBgRt.anchoredPosition.y);
        //    float moveAllLen = 500 + 344.5f;
        //    float lenProp = moveAllLen * prop;
        //    float wantToX = 0;
        //    if (infoPanelChildRt.anchoredPosition.x > 0)
        //    {
        //        // Ŀǰ�����
        //        wantToX = 344.5f - lenProp;
        //    }
        //    else
        //    {
        //        wantToX = -344.5f + lenProp;
        //    }
        //    Debug.LogError("~~~ infoPanelChildRt.anchoredPosition:" + infoPanelChildRt.anchoredPosition + " - prop:" + prop + " - wantToX:" + wantToX);

        //    infoPanelChildRt.anchoredPosition = new Vector2(wantToX, infoPanelChildRt.anchoredPosition.y);
        //    backPanelCanvasGroup.alpha = fadeValue;
        //}
    }

    bool isCardAddRT = true;
    /// <summary>
    /// update
    /// </summary>
    public void DoUpdate()
    {
        bool hasCradDoAniming = false;
        currAnimingIcons.Clear();
        int tempIndex = -1;
        BattleSkillIcon listLastIcon = null;
        foreach (var icon in battleSkillIcons)
        {
            icon.Update();
            if (icon.IsAniming)
            {
                hasCradDoAniming = true;
            }
            if (icon.IsDragLimitAniming)
            {
                currAnimingIcons.Add(icon);
            }
            if ((tempIndex == -1 || tempIndex < icon.index)  && icon.isDoOutCard)
            {
                tempIndex = icon.index;
                listLastIcon = icon;
            }
        }
        HasCradDoAniming = hasCradDoAniming;

        if (listLastIcon != null && cardAdd.activeSelf && listLastIcon.rt != null)
        {

            float dis = Math.Abs(listLastIcon.rt.anchoredPosition.x - cardAddRT.anchoredPosition.x);
            if ((dis != 160 && !listLastIcon.IsAniming) || (dis < 180))
            {
                //isCardAddRT = true;
                float wantToX = listLastIcon.rt.anchoredPosition.x - 160;
                //cardAddRT.DOAnchorPosX(wantToX, 0.5f).OnComplete(() => {
                //    //isCardAddRT = false;
                //});
                cardAddRT.anchoredPosition = new Vector2(wantToX, cardAddRT.anchoredPosition.y);
            }
        }

    }

    public void RestCardAddObjPosition()
    {
        if (cardAddRT != null)
        {
            cardAddRT.anchoredPosition = new Vector2(-65.8f, cardAddRT.anchoredPosition.y);
        }
    }

    /// <summary>
    /// �����Ƿ�����Ϸ�
    /// </summary>
    /// <param name="cardData"></param>
    /// <returns></returns>
    public bool IconCanDrag(DrawCardData cardData)
    {
        bool isInCurrAnimingIcons = false;
        foreach (var icon in currAnimingIcons) {
            if (icon.drawCardData.uid == cardData.uid)
            {
                isInCurrAnimingIcons = true;
                break;
            }
        }
        // cardDataû��currAnimingIcons�����������ڲ�����(Count > 0) �� cardData��currAnimingIcons�������������ڲ�����(Count > 1)
        if ((!isInCurrAnimingIcons && currAnimingIcons.Count > 0) || (isInCurrAnimingIcons && currAnimingIcons.Count > 1))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// ����ק�Ķ��󱻴�view�а���
    /// </summary>
    /// <param name="battleSkillIcon"></param>
    public void PeelFromView(BattleSkillIcon battleSkillIcon)
    {
        foreach (var item in battleSkillIcons)
        {
            //Debug.LogError("1` PeelFromView item:" + item.ToString());
        }

        if (battleSkillIcon == null) return;
        BattleSkillIcon prev = battleSkillIcon.prevBSI_View;
        BattleSkillIcon next = battleSkillIcon.nextBSI_View;
        battleSkillIcon.SetPrevBSI_View(null);
        battleSkillIcon.SetNextBSI_View(null);
        ////Debug.LogError("------- PeelFromView prev:" + prev.ToString() + " - next:" + next.ToString());
        if (prev != null && next != null)
        {
            prev.SetNextBSI_View(next);
            next.SetPrevBSI_View(prev);
        }
        else if (prev != null && next == null)
        {
            prev.SetNextBSI_View(null);
        }
        else if (prev == null && next != null)
        {
            next.SetPrevBSI_View(null);
        }
        foreach (var item in battleSkillIcons)
        {
            //Debug.LogError("2` PeelFromView item:" + item.ToString());
        }
    }

    /// <summary>
    /// ����ק�Ķ����������ӵ�view��
    /// </summary>
    /// <param name="battleSkillIcon"></param>
    public void BackToView(BattleSkillIcon battleSkillIcon)
    {
        foreach (var item in battleSkillIcons)
        {
            //Debug.LogError("1` BackToView item:" + item.ToString());
        }
        if (battleSkillIcon == null) return;
        BattleSkillIcon prev = battleSkillIcon.prevBSI_Data;
        BattleSkillIcon next = battleSkillIcon.nextBSI_Data;
        ////Debug.LogError("------- PeelFromView prev:" + prev.ToString() + " - next:" + next.ToString());
        if (prev != null && next != null)
        {
            battleSkillIcon.SetPrevBSI_View(prev);
            battleSkillIcon.SetNextBSI_View(next);
            prev.SetNextBSI_View(battleSkillIcon);
            next.SetPrevBSI_View(battleSkillIcon);
        }
        else if (prev != null && next == null)
        {
            battleSkillIcon.SetPrevBSI_View(prev);
            prev.SetNextBSI_View(battleSkillIcon);
        }
        else if (prev == null && next != null)
        {
            battleSkillIcon.SetNextBSI_View(next);
            next.SetPrevBSI_View(battleSkillIcon);
        }
        foreach (var item in battleSkillIcons)
        {
            //Debug.LogError("2` BackToView item:" + item.ToString());
        }
    }

    /// <summary>
    /// ��Ļ����ת���� UI ����
    /// </summary>
    /// <param name="targetRect"> Ŀ�� UI ����� RectTransform </param>
    /// <param name="mousePos"> ���λ�� </param>
    /// <param name="canvasCam"> ���Canvas����ȾģʽΪ: Screen Space - Overlay, Camera ����Ϊ null;
    /// Screen Space-Camera or World Space, Camera ����Ϊ Camera.main></param>
    /// <returns> UI ������ </returns>
    private Vector3 ScreenToUIWorldPos(RectTransform targetRect, Vector2 mousePos, Camera canvasCam = null)
    {
        //UI �ľֲ�����
        Vector3 worldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(targetRect, mousePos, canvasCam, out worldPos);
        return worldPos;
    }

    #region ������

    public void InitBuyCard()
    {

    }

    public void UpdateBuyCard()
    {

    }



    private class BuyCardItem
    {
        public GameObject buyCardObj;
        public Button btnBuy;
        public int maxBuyCount = 2;
        public int currBuyCount = 0;

        public TMP_Text txExpend;
        public TMP_Text txNum;

        public Action<int> buyCallback;

        public int[] needExpend;
        public BuyCardItem(GameObject buyCardObj, Action<int> buyCallback)
        {
            this.buyCardObj = buyCardObj;
            this.buyCallback = buyCallback;
            btnBuy = buyCardObj.GetComponent<Button>();
            txExpend = buyCardObj.transform.Find("bg/expend/text").GetComponent<TMP_Text>();
            txNum = buyCardObj.transform.Find("bg/num").GetComponent<TMP_Text>();

            btnBuy.AddListenerBeforeClear(() => {
                // �������
                buyCallback?.Invoke(needExpend[currBuyCount]);
            });

            BattleMissionParamCfg battleMissionParamCfg = BattleCfgManager.Instance.GetMissionParamCfg(GameCenter.mIns.m_BattleMgr.missionID);
            if (battleMissionParamCfg.ramcost == "-1")
            {
                buyCardObj.SetActive(false);
                return;
            }
            else
            {
                buyCardObj.SetActive(true);
            }

            string[] strings = battleMissionParamCfg.ramcost.Split('|');
            needExpend = new int[strings.Length];
            for (int i = 0; i < strings.Length; i++)
            {
                needExpend[i] = int.Parse(strings[i]);
            }
            maxBuyCount = strings.Length;
            currBuyCount = 0;
            txNum.SetText($"{currBuyCount}/{maxBuyCount}");

            UpdateEnergy(needExpend[currBuyCount]);
        }

        public void UpdateEnergy(int value)
        {
            txExpend.SetText(value.ToString());
        }

        public void CurrBuyCountAddOne()
        {
            currBuyCount++;
            if (currBuyCount == maxBuyCount)
            {
                buyCardObj.SetActive(false);
                return;
            }
            UpdateEnergy(needExpend[currBuyCount]);
            txNum.SetText($"{currBuyCount}/{maxBuyCount}");
        }
    }

    #endregion

    #region Hero Head Pointer

    public void SetHeroHeadPointerActive(bool bActive, Vector3 worldPosition, DrawCardData cardData)
    {
        Transform headPointer = _basebattle.heroHeadPointer;
        if (!bActive)
        {
            if (headPointer.gameObject.activeSelf)
                headPointer.gameObject.SetActive(false);
            return;
        }

        BattleSkillCfg SkillCfg = BattleCfgManager.Instance.GetSkillCfgBySkillID(cardData.skillid);
        headPointer.Find("imgIcon").GetComponent<Image>().sprite =  SpriteManager.Instance.GetSpriteSync(SkillCfg.icon);

        Vector2 screenPos = GameCenter.mIns.m_BattleMgr.battleCamer.WorldToScreenPoint(worldPosition);
        RectTransformUtility.ScreenPointToWorldPointInRectangle(headPointer.parent.GetComponent<RectTransform>(), screenPos, uiCamera, out Vector3 beginPos);

        headPointer.position = beginPos + new Vector3(0, 100,0);
        headPointer.gameObject.SetActive(bActive);
        basebattle_heroHeadPointer heroHeadPointerAnim = headPointer.GetComponent<basebattle_heroHeadPointer>();
        if (heroHeadPointerAnim == null)
        {
            heroHeadPointerAnim = headPointer.gameObject.AddComponent<basebattle_heroHeadPointer>();
        }

        heroHeadPointerAnim.DoAnim(headPointer.position);
    }

    #endregion

    #region ����
    // 初始化卡库
    private GameObject cardbag = null;
    private CardLibItem cardLibItem = null;
    private basebattle_cardLibrary m_basebattle_CardLibrary = null;
    private void InitCardLib()
    {
        cardbag = rightDownCanvasGroup.transform.Find("cardbag").gameObject;
        cardLibItem = new CardLibItem(cardbag, () => {
            List<BaseHero> heroDatas = BattleHeroManager.Instance.depolyHeroList;
            List<long> heroIds = new List<long>();
            foreach (var hd in heroDatas)
            {
                heroIds.Add(hd.heroData.heroID);
            }
            long missionId = GameCenter.mIns.m_BattleMgr.missionID;
            // ��ʾ����
            if (m_basebattle_CardLibrary == null)
            {
                m_basebattle_CardLibrary = new basebattle_cardLibrary(rightDownCanvasGroup.transform.parent.parent, heroIds, missionId, _basebattle, _basebattle.basebattle_energySlider.energyRecoverySpeed);
            }
            else
            {
                m_basebattle_CardLibrary.Display(heroIds, missionId, _basebattle, _basebattle.basebattle_energySlider.energyRecoverySpeed);
            }
        });
    }

    private void ClearCardLib()
    {
        if (m_basebattle_CardLibrary != null)
        {
            m_basebattle_CardLibrary.OnDestroy();
            m_basebattle_CardLibrary = null;
        }
    }

    private class CardLibItem
    {
        public GameObject o;

        public Button btnClick;
        public TMP_Text txNum;

        public CardLibItem(GameObject o, Action clickCallback)
        {
            txNum = o.transform.Find("bg/txNum").GetComponent<TMP_Text>();
            btnClick = o.GetComponent<Button>();

            btnClick.AddListenerBeforeClear(() => {
                clickCallback?.Invoke();
            });
        }

        public void SetNum(int num)
        {
            txNum.SetText(num.ToString());
        }
    }

    public bool IsOpnCardLib()
    {
        return cardbag != null && cardbag.activeSelf;
    }

    #endregion

}