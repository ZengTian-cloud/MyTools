using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static HeroGrow;
using LitJson;

public class HeroWeaponPanel
{
    public HeroData hero;
    public HeroInfoCfgData heroInfo;

    private Transform parent;
    private float duration;

    private HeroLeftMenu leftMenu = null;

    private List<GameObject> weaponTypeList;

    private GameObject weaponTypes;//武器类型
    private GameObject panel_weapon_info;//武器信息面板

    private Button btn_left;
    private Button btn_right;
    private Image weaponicon;//武器图片

    public GameObject left_jiexi;//左侧解析tag

    public GameObject panel_weapon_enhance;//武器强化面板
    public GameObject panel_weapon_shendujiexi;//武器深度解析面板
    public GameObject panel_weapon_upSatr;//武器升星面板


    public long curWeapon;//当前装备武器
    public Dictionary<int, WeaponDataCfg> allWeapon;//该角色的所有武器
    public Dictionary<int, GameObject> dicWeaponType;//武器类型

    public Action onTypeSwith;//武器类型切换的回调

    //当前选中武器
    //public int curType;

    private HeroWeaponInfoPanel curInfoPanel;//信息界面

    private HeroWeaponPeiyangPanel peiyangPanel;

    private HeroWeaponEnhancePanel curEnhancePanel;//升级界面

    private HeroWeaponBreakPanel curBreakPanel;//突破界面

    private HeroWeaponShenduJiexi shenduJiexi;//深度解析界面

    public int panelState;//面板状态 1-信息面板 2-培养面板

    public HeroWeaponPanel(Transform parent, HeroLeftMenu leftMenu, float duration)
    {
        this.parent = parent;
        this.leftMenu = leftMenu;
        this.hero = leftMenu.getSelectHero().data;
        this.heroInfo = leftMenu.getSelectHero().heroInfo;
        this.duration = duration;
        panelState = 1;

        weaponTypes = parent.Find("weaponTypes").gameObject;
        panel_weapon_info = parent.Find("panel_weapon_info").gameObject;
        btn_left = parent.Find("btn_left").GetComponent<Button>();
        btn_right = parent.Find("btn_right").GetComponent<Button>();
        weaponicon = parent.Find("weaponicon").GetComponent<Image>();
        left_jiexi = parent.Find("left_jiexi").gameObject;
        panel_weapon_enhance = parent.Find("panel_weapon_enhance").gameObject;
        panel_weapon_shendujiexi = parent.Find("panel_weapon_shendujiexi").gameObject;
        panel_weapon_upSatr = parent.Find("panel_weapon_upSatr").gameObject;

        panel_weapon_info.SetActive(false);
        panel_weapon_info.GetComponent<RectTransform>().anchoredPosition = new Vector3(400, 0);
        panel_weapon_enhance.SetActive(false);
        panel_weapon_enhance.GetComponent<RectTransform>().anchoredPosition = new Vector3(400, 0);
        panel_weapon_shendujiexi.SetActive(false);
        panel_weapon_shendujiexi.GetComponent<RectTransform>().anchoredPosition = new Vector3(400, 0);
        panel_weapon_upSatr.SetActive(false);
        panel_weapon_upSatr.GetComponent<RectTransform>().anchoredPosition = new Vector3(400, 0);

        dicWeaponType = new Dictionary<int, GameObject>();
        for (int i = 1; i <= 7; i++)
        {
            GameObject type = weaponTypes.transform.Find($"weapon_{i}").gameObject;
            int typeindex = i;
            type.GetComponent<Button>().AddListenerBeforeClear(() =>
            {
                OnTypeItemClick(typeindex);
            });
            if (type!= null)
            {
                dicWeaponType.Add(i, type);
            }
        }

        leftMenu.setChangeHeroSelect((hero, heroInfoData) => {
            onMenuChange(hero, heroInfoData);
        });
    }
    
    public void show()
    {
        parent.gameObject.SetActive(true);
        curWeapon = hero.weaponid;
        allWeapon = GameCenter.mIns.m_CfgMgr.GetHeroAllWeapon(hero.heroID);
        for (int i = 1; i <= 7; i++)
        {
            GameObject type = weaponTypes.transform.Find($"weapon_{i}").gameObject;
            type.SetActive(i <= allWeapon.Count);
        }
        //当前装备的武器
        WeaponDataCfg weaponDataCfg = GameCenter.mIns.m_CfgMgr.GetWeaponCfgByWeaponID(curWeapon);
        if (weaponDataCfg == null)
        {
            Debug.LogError("武器配置没有找到。。。。。。");
            return ;
        }
        OnTypeItemClick(weaponDataCfg.type);

        showInfoPanel(curWeapon);

        LoadEffect();
    }
    public void onMenuChange(HeroData hero, HeroInfoCfgData heroInfoData)
    {
        this.hero = hero;
        this.heroInfo = heroInfoData;
        curWeapon = hero.weaponid;
        allWeapon = GameCenter.mIns.m_CfgMgr.GetHeroAllWeapon(hero.heroID);
        parent.gameObject.SetActive(true);
        for (int i = 1; i <= 7; i++)
        {
            GameObject type = weaponTypes.transform.Find($"weapon_{i}").gameObject;
            type.SetActive(i <= allWeapon.Count);
        }
        //默认选中当前装备的武器
        int curType = HeroGrowUtils.getHeroWeaponSelectedType(hero.heroID, allWeapon, hero.weaponid);
        OnTypeItemClick(curType);
        showInfoPanel(allWeapon[curType].weaponid);
    }
    public void Refresh(HeroLeftMenu leftMenu = null)
    {
        if (leftMenu!= null)
        {
            this.leftMenu = leftMenu;
            leftMenu.setChangeHeroSelect((hero, heroInfoData) => {
                onMenuChange(hero, heroInfoData);
            });
        }
        
        curWeapon = hero.weaponid;
        allWeapon = GameCenter.mIns.m_CfgMgr.GetHeroAllWeapon(hero.heroID);
        parent.gameObject.SetActive(true);

        //当前选中的武器
        int curType = HeroGrowUtils.getHeroWeaponSelectedType(hero.heroID, allWeapon, hero.weaponid);
        OnTypeItemClick(curType);
    }


    public void close()
    {
        parent.gameObject.SetActive(false);
    }

    public void RefresCurrHweaponType()
    {
        int curType = HeroGrowUtils.getHeroWeaponSelectedType(hero.heroID, allWeapon, hero.weaponid);
        RefreshOneWeaponType(allWeapon[curType]);
    }

    public void RefreshweaponType()
    {
        //刷新武器类型
        foreach (var item in allWeapon)
        {
            RefreshOneWeaponType(item.Value);
        }
    }

    /// <summary>
    /// 刷新单个item
    /// </summary>
    /// <param name="weaponCfg"></param>
    private void RefreshOneWeaponType(WeaponDataCfg weaponCfg)
    {
        //是否解锁
        bool bLock = hero.CheckWeaponUnLock(weaponCfg.weaponid);
        if (dicWeaponType.ContainsKey(weaponCfg.type))
        {
            GameObject typeItem = dicWeaponType[weaponCfg.type];
            int curType = HeroGrowUtils.getHeroWeaponSelectedType(hero.heroID, allWeapon, hero.weaponid);
            if (bLock && weaponCfg.type == curType)
            {
               
                typeItem.transform.Find("mask").GetComponent<RectTransform>().DOSizeDelta(new Vector2(140, 140), 0.1f).OnComplete(() => {

                    typeItem.transform.Find("mask/select").GetComponent<RectTransform>().DOSizeDelta(new Vector2(136, 136), 0.1f);
                });
            }
            else
            {
                typeItem.GetComponent<RectTransform>().localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                typeItem.GetComponent<RectTransform>().sizeDelta = new Vector2(104, 105);
                typeItem.transform.Find("mask").GetComponent<RectTransform>().sizeDelta = new Vector2(140, 0);
                typeItem.transform.Find("mask/select").GetComponent<RectTransform>().sizeDelta = new Vector2(104, 105);
            }

            typeItem.transform.Find("lock").gameObject.SetActive(!bLock);
            //是否被选中
            typeItem.transform.Find("mask/select").GetComponent<Image>().sprite = SpriteManager.Instance.GetSpriteSync(GetSelectByWeaponQuality(weaponCfg.quality));
            typeItem.transform.Find("mask/select").gameObject.SetActive(bLock && weaponCfg.type == curType);
            //未解锁但选中
            typeItem.transform.Find("lock/select").gameObject.SetActive(!bLock && weaponCfg.type == curType);
            //未选中但装备
            typeItem.transform.Find("inEquip").gameObject.SetActive(weaponCfg.type != curType && hero.weaponid == weaponCfg.weaponid);
            //被选中且装备
            typeItem.transform.Find("mask/select/inEquip").gameObject.SetActive(weaponCfg.type == curType && hero.weaponid == weaponCfg.weaponid);
        }
    }

    /// <summary>
    /// 获得品质框
    /// </summary>
    /// <param name="quality"></param>
    private string GetSelectByWeaponQuality(int quality)
    {
        switch (quality)
        {
            case 3://蓝
                return "ui_d_btn_xinhao_lan";
            case 4://紫
                return "ui_d_btn_xinhao_zi";
            case 5://橙
                return "ui_d_btn_xinhao_cheng";
            case 6://红
                return "ui_d_btn_xinhao_hong";
            default:
                return null;
        }
    }

    public void SetTypeSwitchCallBack(Action cb)
    {
        onTypeSwith = cb;
    }

    /// <summary>
    /// 类型按钮回调
    /// </summary>
    /// <param name="typeIndex"></param>
    private async void OnTypeItemClick(int typeIndex)
    {
        HeroGrowUtils.setHeroWeaponSelected(hero.heroID,typeIndex);
        weaponicon.sprite = await SpriteManager.Instance.GetTextureSpriteSync("equipment/" + allWeapon[typeIndex].icon);
        weaponicon.SetNativeSize();
        RefreshweaponType();

        onTypeSwith?.Invoke();
    }

    /// <summary>
    /// 打开武器信息界面
    /// </summary>
    /// <param name="weaponid"></param>
    public void showInfoPanel(long weaponid)
    {
        WeaponDataCfg weaponDataCfg = GameCenter.mIns.m_CfgMgr.GetWeaponCfgByWeaponID(weaponid);
        if (curInfoPanel == null)
        {
            curInfoPanel = new HeroWeaponInfoPanel(panel_weapon_info.transform, leftMenu, duration, this);
        }
        curInfoPanel.show(weaponDataCfg, leftMenu, hero.CheckWeaponUnLock(weaponid),true);
    }

    /// <summary>
    /// 打开武器培养界面
    /// </summary>
    /// <param name="weaponid"></param>
    public void ShowPeiyangpanel(long weaponid)
    {
        WeaponDataCfg weaponDataCfg = GameCenter.mIns.m_CfgMgr.GetWeaponCfgByWeaponID(weaponid);
        if (peiyangPanel == null)
        {
            peiyangPanel = new HeroWeaponPeiyangPanel(left_jiexi.transform, leftMenu, duration, this);
        }
        peiyangPanel.refresh(weaponDataCfg, this.leftMenu, hero.CheckWeaponUnLock(weaponid));
    }

    /// <summary>
    /// 打开武器升级界面
    /// </summary>
    /// <param name="weaponid"></param>
    public void ShowEnhancePanel(long weaponid,bool isAn)
    {
        WeaponDataCfg weaponDataCfg = GameCenter.mIns.m_CfgMgr.GetWeaponCfgByWeaponID(weaponid);
        if (curEnhancePanel == null)
        {
            curEnhancePanel = new HeroWeaponEnhancePanel(panel_weapon_enhance.transform, duration,  (backtype) => {
                ShowBreakPanel(hero.weaponid,false);
            });
        }
        curEnhancePanel.show(weaponDataCfg, hero, heroInfo, hero.CheckWeaponUnLock(weaponid), isAn);
        if (curBreakPanel != null)
        {
            curBreakPanel.hide(true);
        }
        if (shenduJiexi != null)
        {
            shenduJiexi.hide(true);
        }
    }

    /// <summary>
    /// 打开武器突破界面
    /// </summary>
    /// <param name="weaponDataCfg"></param>
    public void ShowBreakPanel(long weaponid, bool isAn)
    {
        WeaponDataCfg weaponDataCfg = GameCenter.mIns.m_CfgMgr.GetWeaponCfgByWeaponID(weaponid);
        if (curBreakPanel == null)
        {
            curBreakPanel = new HeroWeaponBreakPanel(panel_weapon_upSatr.transform, duration, this);
        }
        curBreakPanel.show(weaponDataCfg, hero, heroInfo, hero.CheckWeaponUnLock(weaponid), isAn);
        if (curEnhancePanel!= null)
        {
            curEnhancePanel.hide(true);
        }
        if (shenduJiexi!= null)
        {
            shenduJiexi.hide(true);
        }
    }

    /// <summary>
    /// 打开深度解析界面
    /// </summary>
    /// <param name="weaponid"></param>
    public void ShowShendujiexi(long weaponid)
    {
        WeaponDataCfg weaponDataCfg = GameCenter.mIns.m_CfgMgr.GetWeaponCfgByWeaponID(weaponid);
        if (shenduJiexi == null)
        {
            shenduJiexi = new HeroWeaponShenduJiexi(panel_weapon_shendujiexi.transform, leftMenu, duration, this);
        }
        shenduJiexi.show(weaponDataCfg, leftMenu, hero.CheckWeaponUnLock(weaponid),true);
        if (curEnhancePanel!= null)
        {
            curEnhancePanel.hide(true);
        }
        if (curBreakPanel != null)
        {
            curBreakPanel.hide(true);
        }
    }

    /// <summary>
    /// 从武器升级界面返回到武器界面
    /// </summary>
    public void BackWeaponPanelByUpLevel()
    {
        panelState = 1;
        if (curEnhancePanel != null)
            curEnhancePanel.hide(true);

        if (shenduJiexi != null)
            shenduJiexi.hide(true);    

        if (curBreakPanel != null)
            curBreakPanel.hide(true);
        int curType = HeroGrowUtils.getHeroWeaponSelectedType(hero.heroID, allWeapon, hero.weaponid);
        showInfoPanel(allWeapon[curType].weaponid);
        left_jiexi.GetComponent<RectTransform>().DOAnchorPosX(-262, duration);
        leftMenu.show();
    }

    UIParticleEffect effect;

    private void LoadEffect()
    {
        if(effect!=null)
        {
            effect.gameObject.SetActive(true);
            effect.Play();
            return;
        }

        UIEffectManager.Instance.LoadUIEffect("effs_ui_ghlz_01", (go) =>
        {
            effect = go.AddComponent<UIParticleEffect>();

            effect.SetUP(new UIParticleEffect.ShowInfo()
            {
                _offset = -10,

                _canvas = parent.GetComponentInParent<Canvas>(),
            });
            go.transform.SetParent(weaponicon.transform, false);

            go.transform.localPosition = Vector3.zero;
        });
    }
   
}
