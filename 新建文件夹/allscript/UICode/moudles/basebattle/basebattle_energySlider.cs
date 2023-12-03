using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class basebattle_energySlider //: MonoBehaviour
{
    #region ui components
    private GameObject sliderObj;
    private Image imgBg;
    private RectTransform imgBgRT;
    private Image imgEnergyBg;
    private RectTransform imgEnergyBgRT;
    private Transform framesTran;
    private GameObject oriEnergyBlock;
    private GameObject oriEnergyBlockFrame;
    private TMP_Text txEnergyNum;
    private Image imgNumBg1;

    private GameObject imgLiuGuang;
    #endregion


    // imgbg height: 1->66, interval+48
    // imgbg height: y max=498 || 1->498+66, interval+48
    private int energyGridLimit;    //最大算力格子
    private float energyCurrValue;  //当前算力值
    private float energyValueLimit; //最大算力值
    // 若energyRecoverySpeed==2 则表示: 2秒1点 - > 1秒0.5
    public float energyRecoverySpeed;
    private List<EnergyBlock> energyBlocks = new List<EnergyBlock>();
    private List<GameObject> energyBlockFrames = new List<GameObject>();

    private float imgEnergyBgNextHeight = 99999;//算力条下一次刷新的高度
    private float imgEnergyTotalLen = 0;//算力条总长度
    private Vector2 imgEnergyBgTempVec2 = new Vector2(0, 0);
    private Action<float> currEnergyUpdate;

    private Tweener textDot;
    void Start()
    {
        //energyRecoverySpeed = 0.5f;
        //InitEnergySlider(gameObject, 10, 10, (a) => { });
    }
    public void Clear()
    {
        energyGridLimit = 0;
        energyCurrValue = 0;
        energyValueLimit = 0;
        energyRecoverySpeed = 0;

        foreach (var item in energyBlocks)
            item.Destroy();
        energyBlocks.Clear();
        foreach (var item in energyBlockFrames)
            GameObject.Destroy(item);
        energyBlockFrames.Clear();

        imgEnergyBgNextHeight = 99999;
        imgEnergyTotalLen = 0;
        imgEnergyBgTempVec2 = Vector2.zero;
        currEnergyUpdate = null;

        textDot = null;
    }


    public void DoUpdate()
    {

        //预算高度与ui高度不相等时，刷新ui高度
        if (imgEnergyBgRT.anchoredPosition.y != imgEnergyBgNextHeight && imgEnergyBgNextHeight != 99999)
        {
            imgEnergyBgTempVec2.y = imgEnergyBgNextHeight;
            //大于 增加能量，判断格子高亮
            if (imgEnergyBgNextHeight > imgEnergyBgRT.anchoredPosition.y)
            {
                foreach (var eb in energyBlocks)
                    eb.CheckLight(imgEnergyBgTempVec2.y);
            }
            //小于 减少能量，判断格子变暗
            else if (imgEnergyBgNextHeight < imgEnergyBgRT.anchoredPosition.y)
            {
                foreach (var eb in energyBlocks)
                    eb.CheckNoLight(imgEnergyBgTempVec2.y);
            }
            //刷新ui高度
            imgEnergyBgRT.anchoredPosition = imgEnergyBgTempVec2;
        }

        foreach (var eb in energyBlocks)
        {
            eb.Update(Time.deltaTime);
        }

        //判断算力是否回满
        if (energyCurrValue >= energyGridLimit)
        {
            energyCurrValue = energyGridLimit;
            imgEnergyBgNextHeight = imgEnergyBgRT.anchoredPosition.y;
            if (!imgLiuGuang.activeSelf)
                imgLiuGuang.SetActive(true);
            return;
        }
        else
        {
            if (imgLiuGuang.activeSelf)
                imgLiuGuang.SetActive(false);
        }

        

        //自然回复算力
        if (energyRecoverySpeed > 0)
        {
            //计算单次回复的进度
            float add = energyRecoverySpeed * Time.deltaTime;
            IncreaseEnergy(add);
        }
    }

    /// <summary>
    /// 回复能量条
    /// </summary>
    /// <param name="increaseValue">增加数值</param>
    public void IncreaseEnergy(float increaseValue)
    {
        energyCurrValue += increaseValue;
        if (energyCurrValue > this.energyValueLimit)
        {
            energyCurrValue = energyValueLimit;
        }

        //本次增加的比例
        float increaseProportion = increaseValue / energyValueLimit;
        //计算出本子增加能量后的能量条预计高度
        imgEnergyBgNextHeight = imgEnergyBgNextHeight + imgEnergyTotalLen * increaseProportion;
        if (imgEnergyBgNextHeight >0)
        {
            imgEnergyBgNextHeight = 0;
        }

        int prevValue = int.Parse(txEnergyNum.text);
        currEnergyUpdate?.Invoke(Mathf.Floor(energyCurrValue));
        txEnergyNum.SetTextExt(Mathf.Floor(energyCurrValue).ToString());
        int currValue = int.Parse(Mathf.Floor(energyCurrValue).ToString());
        if (currValue > prevValue)
        {
            if (textDot != null)
            {
                textDot.Kill();
            }
            textDot = txEnergyNum.transform.DOScale(Vector3.one * 1.3f, 0.1f).SetLoops(4, LoopType.Yoyo).OnComplete(() =>
            {
                textDot = txEnergyNum.transform.DOScale(Vector3.one, 0.1f);
            });
        }
    }

    /// <summary>
    /// 消耗算力
    /// </summary>
    /// <param name="reduceValue"></param>
    public void ReduceEnergy(float reduceValue)
    {
        // 立刻移除能量条
        energyCurrValue = energyCurrValue + reduceValue;
        float reduceProportion = Math.Abs(reduceValue / energyValueLimit);
        imgEnergyBgNextHeight = imgEnergyBgNextHeight - imgEnergyTotalLen * reduceProportion;
        if (imgEnergyBgNextHeight > 0)
        {
            imgEnergyBgNextHeight = 0;
        }
        //设置最小值
        if (imgEnergyBgNextHeight <= -480)
            imgEnergyBgNextHeight = -480;

        currEnergyUpdate?.Invoke(Mathf.Floor(energyCurrValue));
        txEnergyNum.SetTextExt(Mathf.Floor(energyCurrValue).ToString());
    }

    public void InitEnergySlider(GameObject sliderObj, int energyGridLimit, float energyValueLimit, Action<float> currEnergyUpdate, float energyCurrValue = 0)
    {
        this.sliderObj = sliderObj;
        this.energyGridLimit = energyGridLimit;
        this.energyValueLimit = energyGridLimit;
        this.energyCurrValue = 0;// energyCurrValue;
        this.currEnergyUpdate = currEnergyUpdate;

        imgBg = sliderObj.transform.Find("imgBg").GetComponent<Image>();
        imgBgRT = imgBg.GetComponent<RectTransform>();
        imgEnergyBg = imgBg.transform.Find("imgEnergyBg").GetComponent<Image>();
        imgEnergyBgRT = imgEnergyBg.GetComponent<RectTransform>();
        txEnergyNum = sliderObj.transform.Find("energyNum/txEnergyNum").GetComponent<TMP_Text>();
        imgNumBg1 = sliderObj.transform.Find("energyNum/imgNumBg1").GetComponent<Image>();
        framesTran = sliderObj.transform.Find("frames");
        oriEnergyBlock = framesTran.FindHideInChild("oriEnergyBlock").gameObject;
        oriEnergyBlockFrame = framesTran.FindHideInChild("oriEnergyBlockFrame").gameObject;
        if (sliderObj.transform.FindHideInChild("imgLiuGuang") != null)
        {
            imgLiuGuang = sliderObj.transform.FindHideInChild("imgLiuGuang").gameObject;
        }
        else
        {
            imgLiuGuang = framesTran.FindHideInChild("imgLiuGuang").gameObject;
        }
        InitImageBgAndEnergyBg();
        InitEnergyBlocks();
        txEnergyNum.text = energyCurrValue.ToString();
        IncreaseEnergy(energyCurrValue);
    }

    private void InitImageBgAndEnergyBg()
    {
        imgBgRT.sizeDelta = new Vector2(imgBgRT.sizeDelta.x, 74 + 48 * (energyGridLimit - 1));
        imgEnergyBgRT.sizeDelta = new Vector2(imgBgRT.sizeDelta.x, 498);
        imgEnergyBgRT.anchoredPosition = new Vector2(0, -480);
        imgEnergyTotalLen = 480;
        imgEnergyBgNextHeight = -480;
    }


    private void InitEnergyBlocks()
    {
        ClearEnergyBlocks();
        for (int i = 0; i < energyGridLimit; i++)
        {
            GameObject energyBlockObj = GameObject.Instantiate(oriEnergyBlock);
            EnergyBlock energyBlock = new EnergyBlock(energyBlockObj, framesTran, i);
            if (energyBlock != null)
            {
                energyBlocks.Add(energyBlock);
            }
        }

        for (int i = 0; i < energyGridLimit; i++)
        {
            GameObject energyBlockFrameObj = GameObject.Instantiate(oriEnergyBlockFrame);
            energyBlockFrameObj.name = "ebframe_" + i;
            energyBlockFrameObj.transform.SetParent(framesTran, true);
            energyBlockFrameObj.transform.localScale = Vector3.one;
            energyBlockFrameObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, i * 48f);
            energyBlockFrameObj.transform.SetAsLastSibling();
            energyBlockFrames.Add(energyBlockFrameObj);
            energyBlocks[i].SetFrame(energyBlockFrameObj);
            energyBlockFrameObj.SetActive(true);
        }
        imgLiuGuang.transform.parent = framesTran;
        // 有2个隐藏的原始对象
        imgLiuGuang.transform.SetSiblingIndex(energyGridLimit + 2);
    }

    /// <summary>
    /// 刷新算力
    /// </summary>
    /// <param name="curEnergy">当前算力</param>
    /// <param name="maxEnergy">最大算力</param>
    /// <param name="changevalue">改变值</param>
    /// <param name="isAdd">是否增加</param>
    public void RefreshEnergy(int curEnergy, int maxEnergy,float changevalue = 0, bool isAdd = false)
    {
        energyGridLimit = maxEnergy;
        energyValueLimit = maxEnergy;
        //算力回复速度
        this.energyRecoverySpeed = float.Parse(GameCenter.mIns.m_CfgMgr.GetConstantValue("base_engrgy_regen")); //energyRecoverySpeed; // 每秒多少点

        // changevalue 整数
        if (changevalue > 0)
        {
            IncreaseEnergy(changevalue);
        }
        else if (changevalue < 0)
        {
            ReduceEnergy(changevalue);
        }
    }

    private void ClearEnergyBlocks()
    {
        if (energyBlocks != null)
        {
            foreach (var eb in energyBlocks)
            {
                eb.Destroy();
            }
            energyBlocks.Clear();
        }
    }

    //单个算力格子
    private class EnergyBlock
    {
        // -->
        private Vector2 startAncPos = Vector2.zero;
        private float interval = 48f;
        // <--
        // index: 0开始
        public int index;
        public GameObject obj;
        public RectTransform rt;
        public Image imgEnergy;
        public RectTransform imgEnergyRT;

        public GameObject frame;
        public SequenceFrameHelper sequenceFrameHelper;
        public GameObject sequenceFrame;

        public float lightHeight;
        public bool isLight = false;

        private Transform curveNode;

        public void CheckLight(float y)
        {
            if (isLight)
                return;

            if (lightHeight <= y)
            {
                isLight = true;
                SetEnergyImageActive(isLight);
            }
        }

        public bool CheckNoLight(float y)
        {
            if (!isLight)
                return false;

            if (lightHeight > y)
            {
                isLight = false;
                SetEnergyImageActive(isLight);
                PlaySequenceFrame();
                return true;
            }
            return false;
        }

        #region 高亮
        // 高亮
        private string propertyColor = "_Color";         // share内部 Color节点的名字
        // private MeshRenderer meshRenderer;
        private Material material;

        private float lightSpeed = 70;
        // 当前亮度
        private float intensity = 1;
        // 最大亮度
        private float intensityLimit = 15;
        private int a_r = 0;
        private Color c;
        // 变亮曲线速度
        private float toLightSpeed = 0.0f;
        // 变暗曲线速度
        private float toDrakSpeed = 0.0f;

        private void StartLight()
        {
            intensity = 1;
            curveNode.localPosition = Vector3.one;
            a_r = 1;
            imgEnergy.DOFade(1, 0.1f).SetEase(Ease.InExpo).OnComplete(() => {
                curveNode.DOLocalMoveX(intensityLimit, 0.1f).SetEase(Ease.OutQuart).OnComplete(() =>
                {
                    curveNode.DOLocalMoveX(1, 0.01f).SetEase(Ease.OutExpo).OnComplete(() =>
                    {
                        a_r = -1;
                    });
                });
            });
        }

        private void EndLight()
        {
            a_r = 0;
            intensity = 1;
            curveNode.localPosition = new Vector3(1, 0, 0);
        }

        public void Update(float dt)
        {
            intensity = curveNode.localPosition.x;
            if (a_r == -1)
            {
                // over
                EndLight();
            }
            else
            {
                
            }
            if (material.GetColor(propertyColor).r == c.r * intensity)
            {
                return;
            }

            Color color = new Color(c.r * intensity, c.g * intensity, c.b * intensity);
            material.SetColor(propertyColor, color);
           
        }
        #endregion

        public EnergyBlock(GameObject obj, Transform parent, int index)
        {
            this.obj = obj;
            this.index = index;
            rt = obj.GetComponent<RectTransform>();
            imgEnergy = obj.transform.GetChild(0).GetComponent<Image>();
            imgEnergyRT = imgEnergy.GetComponent<RectTransform>();
            curveNode = obj.transform.FindHideInChild("curveNode");
            material = imgEnergy.material;
            c = imgEnergy.color;

            obj.name = "eb_" + index;
            obj.transform.SetParent(parent, true);
            obj.transform.localScale = Vector3.one;
            SetPosition();

            material = new Material(imgEnergy.material);
            material.name = material.name + "(Copy)";
            material.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            imgEnergy.material = material;
            imgEnergy.SetMaterialDirty();

            lightHeight = -480 + interval * (index + 1);

            obj.SetActive(true);
        }

        public void SetFrame(GameObject frame)
        {
            this.frame = frame;
            sequenceFrame = frame.transform.FindHideInChild("sequenceFrame").gameObject;
            sequenceFrameHelper = sequenceFrame.GetComponent<SequenceFrameHelper>();
            sequenceFrameHelper.SetFade(0.13f, 3);
            sequenceFrameHelper.overCallback = () =>
            {
                sequenceFrame.SetActive(false);
            };
        }

        public void SetPosition()
        {
            rt.anchoredPosition = new Vector2(0, index * interval);
        }

        public void SetEnergyImageActive(bool bActive)
        {
            if (imgEnergy != null)
            {
                imgEnergy.color = new Color(1, 1, 1, 0);
                imgEnergy.gameObject.SetActive(bActive);
                
                    if (bActive)
                    {
                        //material.SetColor(propertyColor, Color.white);
                        // 亮一下
                        StartLight();
                    }
                
            }
        }

        public void PlaySequenceFrame()
        {
            if (sequenceFrame != null)
            {
                sequenceFrame.SetActive(true);
            }
        }

        public void Destroy()
        {
            if (obj != null)
            {
                GameObject.Destroy(obj);
            }
        }
    }

}

