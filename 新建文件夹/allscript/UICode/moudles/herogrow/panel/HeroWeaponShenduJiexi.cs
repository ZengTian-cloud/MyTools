using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitJson;
/// <summary>
/// 武器深度解析
/// </summary>
public class HeroWeaponShenduJiexi
{
    private HeroWeaponPanel heroWeaponPanel;
    private Transform root;
    public HeroData hero;
    public HeroInfoCfgData heroInfo;
    private WeaponDataCfg weaponData;
    private HeroLeftMenu leftMenu;
    private float duration;
    private bool block;

    private TextMeshProUGUI nameTxt;//武器名字
    private TextMeshProUGUI typeTxt;//武器类型

    private TextMeshProUGUI descTxt;

    private GameObject oldJiexi;//
    private GameObject image;
    private GameObject newJiexi;//

    private GameObject expend_bg;
    private GameObject expendList;
    private GameObject expend_item;

    private Button btn_shengduxiaohao;

    private List<GameObject> expendItems = new List<GameObject>();//深度解析消耗材料的item预制体列表

    public HeroWeaponShenduJiexi(Transform parent, HeroLeftMenu leftMenu, float duration, HeroWeaponPanel weaponPanel)
    {
        this.root = parent;
        this.leftMenu = leftMenu;
        this.duration = duration;
        this.heroWeaponPanel = weaponPanel;

        nameTxt = parent.Find("name/nametext").GetComponent<TextMeshProUGUI>();
        typeTxt = parent.Find("name/type").GetComponent<TextMeshProUGUI>();
        descTxt = parent.Find("desc").GetComponent<TextMeshProUGUI>();

        expend_bg = parent.transform.Find("expend_bg").gameObject;
        expendList = parent.transform.Find("expendList").gameObject;
        expend_item = parent.transform.Find("expend_item").gameObject;

        oldJiexi = parent.transform.Find("oldJiexi").gameObject;
        image = parent.transform.Find("Image").gameObject;
        newJiexi = parent.transform.Find("newJiexi").gameObject;

        btn_shengduxiaohao = parent.Find("btnbox/btn").GetComponent<Button>();

        btn_shengduxiaohao.AddListenerBeforeClear(OnShenduJiexiClick);

        expend_item.SetActive(false);
    }

    public void show(WeaponDataCfg weaponData, HeroLeftMenu leftMenu, bool block,bool isAnimations)
    {
        this.weaponData = weaponData;
        this.block = block;
        this.leftMenu = leftMenu;
        this.hero = leftMenu.getSelectHero().data;
        this.heroInfo = leftMenu.getSelectHero().heroInfo;
        HeroGrowUtils.backType = BackType.WqXh;

        if (isAnimations)
            root.GetComponent<RectTransform>().DOAnchorPosX(-350, duration);
        else
            root.GetComponent<RectTransform>().anchoredPosition = new Vector3(-350, 0);
        root.gameObject.SetActive(true);

        Refresh(weaponData);
    }

    public void hide(bool isAnimations)
    {
        if (isAnimations)
            root.GetComponent<RectTransform>().DOAnchorPosX(400, duration);
        else
            root.GetComponent<RectTransform>().anchoredPosition = new Vector3(400, 0);
        root.gameObject.SetActive(false);
    }

    public void Refresh(WeaponDataCfg weaponData)
    {
        nameTxt.text = GameCenter.mIns.m_LanMgr.GetLan(weaponData.name);
        typeTxt.text = GameCenter.mIns.m_LanMgr.GetLan($"grow_type_{weaponData.type}");

        int curStar = hero.GetWeaponStar(weaponData.weaponid);
        List<CostData> costDatas = weaponData.getStarData(curStar==-1?0:curStar)?.costs;
        bool isCost = (curStar == -1 || (curStar < weaponData.getMaxStar() - 1));


        image.SetActive(false);
        expend_bg.SetActive(false);
        expendList.SetActive(false);
        oldJiexi.transform.Find("bg").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(HeroGrowUtils.getWeaponStarIcon(curStar));
        oldJiexi.transform.Find("bg/text_jiexi").GetComponent<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan($"grow_shendujiexi_{(curStar == -1 ? 0 : curStar)}");
        if (curStar == -1)
        {
            //未解锁
            loadExp(costDatas);
            newJiexi.SetActive(false);
            image.SetActive(false);
            btn_shengduxiaohao.interactable = true;
            btn_shengduxiaohao.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_c_btn_cs"); 
            btn_shengduxiaohao.GetComponentInChildren<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("grow_jiexi");
            oldJiexi.transform.Find("bg/text_jiexi").GetComponent<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan($"grow_shendujiexi_{curStar}");
        } else if (curStar < weaponData.getMaxStar() - 1)
        {
            //未满级
            loadExp(costDatas);
            btn_shengduxiaohao.interactable = true;
            btn_shengduxiaohao.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_c_btn_cs"); ;
            btn_shengduxiaohao.GetComponentInChildren<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("grow_jiexi");
            oldJiexi.transform.Find("bg/text_jiexi").GetComponent<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan($"grow_shendujiexi_{curStar}");
            newJiexi.transform.Find("bg/text_jiexi").GetComponent<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan($"grow_shendujiexi_{curStar + 1}");
            newJiexi.transform.Find("bg").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(HeroGrowUtils.getWeaponStarIcon(curStar + 1));
            newJiexi.SetActive(true);
        } else
        {
            //已满级
            newJiexi.SetActive(false);
            btn_shengduxiaohao.interactable = false;
            btn_shengduxiaohao.GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_c_btn_hs_jy");
            btn_shengduxiaohao.GetComponentInChildren<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("grow_jiexi_1");
        }
    }

    private void loadExp(List<CostData> costDatas)
    {
        image.SetActive(true);
        expend_bg.SetActive(true);
        expendList.SetActive(true);
        if (costDatas!=null&&costDatas.Count > 0)
        {
            for (int i = 0; i < expendItems.Count; i++)
            {

                expendItems[i].SetActive(false);
            }
            for (int i = 0; i < costDatas.Count; i++)
            {
                GameObject item = null;
                if (expendItems.Count > i + 1 && expendItems[i] != null)
                {
                    item = expendItems[i];
                }
                else
                {
                    item = GameObject.Instantiate(expend_item, expendList.transform);
                    expendItems.Add(item);
                }
                item.SetActive(true);
                int curNum = GameCenter.mIns.userInfo.getPropCount(costDatas[i].propid);

                item.transform.Find("countbox/txt").GetComponent<TextMeshProUGUI>().text = HeroGrowUtils.parsePropCountStr(costDatas[i].count, curNum);
                /*if (curNum < costDatas[i].count)
                {
                    canJiexi = false;
                }*/
            }
        }
    }

    private void OnShenduJiexiClick()
    {
        int curStar = hero.GetWeaponStar(weaponData.weaponid);
        if (curStar == -1)
        {
            JsonData data = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
            data["heroid"] = this.hero.heroID;
            data["weaponid"] = this.weaponData.weaponid;
            GameCenter.mIns.m_NetMgr.SendData(NetCfg.GROW_HERO_JIEXIAPON, data, (seqid, code, data) =>
            {
                if (code == 0)
                {
                    JsonData json = JsonMapper.ToObject(new JsonReader(data));
                    JsonData chagedata = json["change"]?["changed"];
                    if (chagedata != null)
                    {
                        GameCenter.mIns.userInfo.onChange(chagedata);
                        GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                        {
                            heroWeaponPanel.RefresCurrHweaponType();
                            Refresh(weaponData);
                        });
                    }
                }
            });
        }
        else
        {
            JsonData data = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
            data["heroid"] = this.hero.heroID;
            data["weaponid"] = weaponData.weaponid;

            GameCenter.mIns.m_NetMgr.SendData(NetCfg.GROW_HERO_WEAPONUPSTAR, data, (seqid, code, data) =>
            {
                if (code == 0)
                {
                    JsonData json = JsonMapper.ToObject(new JsonReader(data));
                    JsonData chagedata = json["change"]?["changed"];
                    if (chagedata != null)
                    {
                        GameCenter.mIns.userInfo.onChange(chagedata);
                        GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                        {
                            Refresh(weaponData);
                        });
                    }
                }
            });
        }
    }
}

