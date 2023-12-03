using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class HeroGrow:BaseWin
{

	public GameObject _Root;
	public Transform heroLeft;
    
    public Transform panelRight;
    public Transform panelHeroGrow;
    public Transform panelHeroSkill;
    public Transform panelHeroWeapon;
    public Transform panelHeroTalent;
    public Transform panelHeroProfile;

    public override string Prefab => "herogrow";

	public override UILayerType uiLayerType => UILayerType.Normal;


    protected override void InitUI()
    {
		_Root = uiRoot;
        heroLeft = _Root.transform.Find("heroLeft");
        
        panelRight = _Root.transform.Find("panelRight");
        panelHeroGrow = panelRight.transform.Find("panel_hero_grow");
        panelHeroSkill = panelRight.transform.Find("panel_hero_skill");
        panelHeroWeapon = panelRight.transform.Find("panel_hero_weapon");
        panelHeroTalent = panelRight.transform.Find("panel_hero_talent");
        panelHeroProfile = panelRight.transform.Find("panel_hero_profile");

    }

  
}
