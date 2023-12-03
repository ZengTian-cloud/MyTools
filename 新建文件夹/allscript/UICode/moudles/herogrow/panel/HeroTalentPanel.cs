using System.Collections.Generic;
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static HeroGrow;
using LitJson;

public class HeroTalentPanel : IHeroPanel
{

    public HeroData hero;
    public HeroInfoCfgData heroInfo;

    private Transform parent;
    private float duration;

    private HeroLeftMenu leftMenu = null;

    private GameObject jiexiRoot;//解析界面节点
    private GameObject jiexi_Info_Panel;//解析信息界面
    private TextMeshProUGUI talentName;
    private Image icon;//英雄图片
    private TextMeshProUGUI desc;
    private GameObject costtitle;
    private GameObject expend_item;
    private GameObject expendList;
    private Button btn_upLevel;

    private Image heroIcon;//英雄图片

    private List<GameObject> jiexiList;

    private GameObject curSelect;

    private List<GameObject> expendItems = new List<GameObject>();//升级消耗材料的item预制体列表

    private int curType;

    public HeroTalentPanel(Transform parent, HeroLeftMenu leftMenu, float duration)
    {
        this.parent = parent;
        this.leftMenu = leftMenu;
        this.hero = leftMenu.getSelectHero().data;
        this.heroInfo = leftMenu.getSelectHero().heroInfo;
        this.duration = duration;

        jiexiRoot = parent.Find("root").gameObject;
        heroIcon = parent.Find("root/heroIcon").GetComponent<Image>();
        jiexiList = new List<GameObject>();
        for (int i = 1; i <= 6; i++)
        {
            GameObject item = parent.Find($"root/jiexiList/jiexi_{i}").gameObject;
            int index = i;
            RefreshItem(item, index);

            jiexiList.Add(item);
        }

        jiexi_Info_Panel = parent.Find("jiexi_Info_Panel").gameObject;
        talentName = jiexi_Info_Panel.transform.Find("name/text").GetComponent<TextMeshProUGUI>();
        icon = jiexi_Info_Panel.transform.Find("icon").GetComponent<Image>();
        desc = jiexi_Info_Panel.transform.Find("desc").GetComponent<TextMeshProUGUI>();
        costtitle = jiexi_Info_Panel.transform.Find("costtitle").gameObject;
        expend_item = jiexi_Info_Panel.transform.Find("expend_item").gameObject;
        expendList = jiexi_Info_Panel.transform.Find("expendList").gameObject;
        btn_upLevel = jiexi_Info_Panel.transform.Find("btn_upLevel").GetComponent<Button>();

        btn_upLevel.AddListenerBeforeClear(OnJiexiClick);

        expend_item.SetActive(false);


        leftMenu.setChangeHeroSelect((data, heroInfoData) => {
            this.hero = data;
            this.heroInfo = heroInfoData;

            show();
        });

        if (jiexiRoot.transform.childCount >= 4)
        {
            effect1 = jiexiRoot.transform.GetChild(3).GetComponent<UIParticleEffect>();

            if(effect1==null)
                LoadEffect();
        }
        else
        {
            LoadEffect();
        }
        if(jiexiRoot.transform.childCount >=5 )
        {
            //effect2 = jiexiRoot.transform.GetChild(4).GetComponent<UIParticleEffect>();

            //if(effect2==null)
                //LoadEffect2();
        }
        else
        {
            //LoadEffect2();
        }
    }

    public void close()
    {
        if (curSelect!=null)
        {
            curSelect.SetActive(false);
        }
        parent.gameObject.SetActive(false);
    }

    public async void show()
    {
        parent.gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(heroInfo.profile))
        {
            heroIcon.sprite = await SpriteManager.Instance.GetTextureSpriteSync($"heroIcon_Jiexi/{heroInfo.profile}");
            heroIcon.SetNativeSize();
        }
        else
        {
            Debug.LogError($"未找到英雄{heroInfo.heroid}的剪影资源，请检查！");
        }

        Refreshtree();
    }

    /// <summary>
    /// 刷新天赋树节点
    /// </summary>
    public void Refreshtree()
    {
        int curStar = hero.star;
        for (int i = 0; i < jiexiList.Count; i++)
        {
            jiexiList[i].transform.Find("lock").gameObject.SetActive(i + 1 > curStar);
        }
    }

    private void RefreshItem(GameObject item,int index)
    {
        GameObject _item = item;
        _item.transform.Find("select").gameObject.SetActive(false);

        _item.GetComponent<Button>().AddListenerBeforeClear(() =>
        {
            if (curSelect != null)
            {
                curSelect.SetActive(false);
            }
            int _index = index;
            curSelect = item.transform.Find("select").gameObject;
            curSelect.SetActive(true);
            ShowInfoPanel(_index);

        });
    }

    private void ShowInfoPanel(int type)
    {
        for (int i = 0; i < expendList.transform.childCount; i++)
        {
            GameObject.Destroy(expendList.transform.GetChild(i).gameObject);
        }

        curType = type;
        HeroGrowUtils.backType = BackType.TF;
        leftMenu.hide();
        jiexi_Info_Panel.GetComponent<RectTransform>().DOAnchorPosX(-350f, duration);
        jiexiRoot.GetComponent<RectTransform>().DOAnchorPosX(-289, duration);


        bool block = type <= hero.star;//是否解锁
        HeroTalentCfg talentCfg = HeroDataManager.Instance.GetHeroTalent(hero.heroID, curType);
        BattleSkillTalentCfg battleSkillTalentCfg = BattleCfgManager.Instance.GetTalentCfg(talentCfg.talentid);

        //名字描述
        talentName.text = GameCenter.mIns.m_LanMgr.GetLan(battleSkillTalentCfg.name);
        desc.text = GameCenter.mIns.m_LanMgr.GetLan(battleSkillTalentCfg.note);
        if (block)//已解锁
        {
            costtitle.SetActive(false);
            expendList.SetActive(false);
            btn_upLevel.GetComponentInChildren<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("grow_UpStar_1");
            commontool.SetGary(btn_upLevel.GetComponent<Image>(), true);
            btn_upLevel.interactable = false;
        }
        else//未解锁
        {
            costtitle.SetActive(true);
            expendList.SetActive(true);

            //消耗材料
            List<CostData> costDatas = GameCenter.mIns.m_CfgMgr.GetCostByCostID(talentCfg.costid).getCosts();
            //c材料是否足够
            bool canJiexi1 = true;
            for (int i = 0; i < costDatas.Count; i++)
            {
                GameObject item = GameObject.Instantiate(expend_item, expendList.transform);

                item.SetActive(true);
                int curNum = GameCenter.mIns.userInfo.getPropCount(costDatas[i].propid);
                if (curNum < costDatas[i].count)
                {
                    item.transform.Find("bg_count/text_count").GetComponent<TextMeshProUGUI>().text = $"<color=red>{costDatas[i].count}</color>/{curNum}";
                    canJiexi1 = false;
                }
                else
                {
                    item.transform.Find("bg_count/text_count").GetComponent<TextMeshProUGUI>().text = $"<color=green>{costDatas[i].count}</color>/{curNum}";
                }
                
            }
            //是否下一级
            bool canJiexi2 = type - hero.star == 1;//只能强化下一级
            //先解锁上一级
            if (!canJiexi2)
            {
                btn_upLevel.GetComponentInChildren<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("grow_UpStar_2");
            }
            else if (!canJiexi1)
            {
                btn_upLevel.GetComponentInChildren<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("common_cost");
            }
            else if (canJiexi1 && canJiexi2)
            {
                btn_upLevel.GetComponentInChildren<TextMeshProUGUI>().text = GameCenter.mIns.m_LanMgr.GetLan("grow_UpStar_3");
            }
             btn_upLevel.interactable = canJiexi1 && canJiexi2;
            commontool.SetGary(btn_upLevel.GetComponent<Image>(), !(canJiexi1 && canJiexi2));
        }
        //effect2.gameObject.SetActive(false);
    }

    /// <summary>
    /// 从天赋信息界面返回天赋界面
    /// </summary>
    public void BackTalentByTalentInfo()
    {
        jiexi_Info_Panel.GetComponent<RectTransform>().DOAnchorPosX(400, duration);
        jiexiRoot.GetComponent<RectTransform>().DOAnchorPosX(257, duration);
        //effect2.gameObject.SetActive(true);
        leftMenu.show();
    }

    private void OnJiexiClick()
    {
        JsonData data = GameCenter.mIns.m_NetMgr.GetSendMsgBaseData();
        data["heroid"] = this.hero.heroID;
        GameCenter.mIns.m_NetMgr.SendData(NetCfg.GROW_HERO_TALENT, data, (seqid, code, data) =>
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
                        ShowInfoPanel(curType);
                        Refreshtree();
                    });
                }
            }
        });
    }


    UIParticleEffect effect1;

    private void LoadEffect()
    {
        if (effect1 != null)
        {
            effect1.gameObject.SetActive(true);
            effect1.Play();
            return;
        }

        UIEffectManager.Instance.LoadUIEffect("effs_ui_yh_01", (go) =>
        {
            effect1 = go.AddComponent<UIParticleEffect>();

            effect1.SetUP(new UIParticleEffect.ShowInfo()
            {
                _offset = -5,

                _canvas = jiexiRoot.transform.GetComponentInParent<Canvas>(),
            });
            go.transform.SetParent(jiexiRoot.transform, false);

            go.transform.localPosition = Vector3.zero;
        });
    }

    /*
    //UIParticleEffect effect2;
    private void LoadEffect2()
    {
        
        if (effect2 != null)
        {
            effect2.gameObject.SetActive(true);
            effect2.Play();
            return;
        }

        UIEffectManager.Instance.LoadUIEffect("effs_ui_bjlz_01", (go) =>
        {
            effect2 = go.AddComponent<UIParticleEffect>();

            effect2.SetUP(new UIParticleEffect.ShowInfo()
            {
                _offset = -5,

                _canvas = jiexiRoot.transform.GetComponentInParent<Canvas>(),
            });
            go.transform.SetParent(jiexiRoot.transform, false);

            go.transform.localPosition = Vector3.zero;
        });
    }
    */


}
