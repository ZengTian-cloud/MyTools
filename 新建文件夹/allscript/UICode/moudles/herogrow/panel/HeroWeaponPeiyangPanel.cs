using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static HeroGrow;
using LitJson;

/// <summary>
/// 武器培养界面
/// </summary>
public class HeroWeaponPeiyangPanel
{
    private HeroWeaponPanel heroWeaponPanel;

    //左侧解析按钮列表节点
    private Transform root;
    private WeaponDataCfg weaponData;
    private HeroLeftMenu leftMenu;
    private float duration;

    private bool bLock;

    private Button btn_left_1;//强化
    private Button btn_left_2;//型号

    private GameObject selectItem;

    public HeroData hero;
    public HeroInfoCfgData heroInfo;

    private int curSelectType;//当前选中 1-强化 2-型号

    public HeroWeaponPeiyangPanel(Transform parent, HeroLeftMenu leftMenu, float duration, HeroWeaponPanel weaponPanel)
    {
        this.root = parent;
        this.leftMenu = leftMenu;
        this.duration = duration;
        this.heroWeaponPanel = weaponPanel;

        this.btn_left_1 = parent.transform.Find("itemRoot_1").GetComponent<Button>();
        this.btn_left_2 = parent.transform.Find("itemRoot_2").GetComponent<Button>();

        btn_left_1.AddListenerBeforeClear(OnQingahuaClick);
        btn_left_2.AddListenerBeforeClear(OnXinghaoClick);
        btn_left_1.transform.Find("txName").GetComponent<TextMeshProUGUI>().text=GameCenter.mIns.m_LanMgr.GetLan("grow_WeaponPeiyangPanel_1"); 
        btn_left_2.transform.Find("txName").GetComponent<TextMeshProUGUI>().text=GameCenter.mIns.m_LanMgr.GetLan("grow_WeaponPeiyangPanel_2"); 
    }


    public void refresh(WeaponDataCfg weaponData,HeroLeftMenu leftMenu, bool bLock)
    {
        this.weaponData = weaponData;
        this.bLock = bLock;
        this.leftMenu = leftMenu;
        this.hero = leftMenu.getSelectHero().data;
        this.heroInfo = leftMenu.getSelectHero().heroInfo;

        heroWeaponPanel.SetTypeSwitchCallBack(() => {
            int curType = HeroGrowUtils.getHeroWeaponSelectedType(hero.heroID, heroWeaponPanel.allWeapon, hero.weaponid);
            heroWeaponPanel.ShowPeiyangpanel(heroWeaponPanel.allWeapon[curType].weaponid);
        });
        show();
    }

    private void show()
    {
        leftMenu.hide();
        root.GetComponent<RectTransform>().DOAnchorPosX(262, duration);
        if (curSelectType <= 0)
        {
            curSelectType = 1;//默认选择强化
        }

        if (curSelectType == 1)
        {
            OnQingahuaClick();
        }
        else if (curSelectType == 2)
        {
            OnXinghaoClick();
        }
    }

    //强化
    private void OnQingahuaClick()
    {
        curSelectType = 1;
        heroWeaponPanel.panelState = 2;

        if (selectItem != null)
        {
            selectItem.SetActive(false);
        }
        btn_left_1.transform.Find("txName").GetComponent<TextMeshProUGUI>().color = Color.white;
        btn_left_1.transform.Find("imgIcon").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_d_icon_qianghua_bai");
        btn_left_2.transform.Find("txName").GetComponent<TextMeshProUGUI>().color = Color.HSVToRGB(80,74,72);
        btn_left_2.transform.Find("imgIcon").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_d_icon_xinhao_hei");
        selectItem = btn_left_1.transform.Find("selectItem").gameObject;
        selectItem.SetActive(true);

        //获得武器当前最大等级
        WeaponBreakCfg breakdata = heroInfo.getCurrWeaponBreakData(hero.weaponstate);
        if (hero.weaponLevel < breakdata.level|| breakdata.state>=6)
        {
            //前往升级
            heroWeaponPanel.ShowEnhancePanel(weaponData.weaponid, true);
        }
        else//满级 前往升星界面
        {
            heroWeaponPanel.ShowBreakPanel(weaponData.weaponid,true);
        }
       
    }

    //型号
    private void OnXinghaoClick()
    {
        curSelectType = 2;

        heroWeaponPanel.panelState = 2;
        if (selectItem != null)
        {
            selectItem.SetActive(false);
        }
        selectItem = btn_left_2.transform.Find("selectItem").gameObject;
        selectItem.SetActive(true);

        btn_left_1.transform.Find("txName").GetComponent<TextMeshProUGUI>().color = Color.HSVToRGB(80, 74, 72);
        btn_left_1.transform.Find("imgIcon").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_d_icon_qianghua_hei");
        btn_left_2.transform.Find("txName").GetComponent<TextMeshProUGUI>().color = Color.white;
        btn_left_2.transform.Find("imgIcon").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync("ui_d_icon_xinhao_bai");
        heroWeaponPanel.ShowShendujiexi(weaponData.weaponid);
    }
}

