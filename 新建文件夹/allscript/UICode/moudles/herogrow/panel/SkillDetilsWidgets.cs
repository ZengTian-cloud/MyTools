using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillDetilsWidgets
{
    private enum skilltypeEnum //后面改到语言包里去
    {
        普攻 = 1,
        战技 = 2,
        秘技 = 3,
        终结技 = 4
    }

    Transform mTransform;
    TextMeshProUGUI skillName;
    TextMeshProUGUI skillCostNum;
    TextMeshProUGUI skillLevel;
    TextMeshProUGUI skillType;
    TextMeshProUGUI skillNote;
    TextMeshProUGUI skillExplain;
    Transform talentInfoContent;
    GameObject talentInfoPrefab;

    private long currentSkillID;
    private float duration;
    private Action<BackType> onBack;
    private Transform parent;

    public SkillDetilsWidgets(Transform parent, float duration, Action<BackType> onBack)
    {
        this.parent = parent;
        this.duration = duration;
        this.onBack = onBack;

        mTransform = parent.Find("SkillDetilsWidgets");
        skillName = Utils.Find<TextMeshProUGUI>(mTransform, "title/name");
        skillCostNum = Utils.Find<TextMeshProUGUI>(mTransform, "title/cost/num");
        skillLevel = Utils.Find<TextMeshProUGUI>(mTransform, "level");
        skillType = Utils.Find<TextMeshProUGUI>(mTransform, "type");
        skillNote = Utils.Find<TextMeshProUGUI>(mTransform, "note");
        skillExplain = Utils.Find<TextMeshProUGUI>(mTransform, "explain");
        talentInfoContent = mTransform.Find("talentEffect/Scroll View/Viewport/Content");
        talentInfoPrefab = mTransform.Find("talentEffect/talentInfoPrefab").gameObject;
        Utils.Find<Button>(mTransform, "mask").AddListenerBeforeClear(() => hide());
    }
    public void show(HeroData hero, bool isAnimations, long skillid)
    {
        if (currentSkillID == skillid)
        {
            hide(); 
            return;
        }
        currentSkillID = skillid;
        mTransform.gameObject.SetActive(true);
        if (isAnimations)
            mTransform.GetComponent<RectTransform>().DOAnchorPosX(240, duration);
        //else
        //mTransform.GetComponent<RectTransform>().anchoredPosition = new Vector3(240, 0);
        
        BattleSkillCfg battleSkillCfg = BattleCfgManager.Instance.GetSkillCfgBySkillID(skillid);
        skillName.SetText(GameCenter.mIns.m_LanMgr.GetLan(battleSkillCfg.name));
        skillCostNum.SetText(battleSkillCfg.energycost.ToString());
        int level = hero.GetSkillLvBySkillID(skillid);
        skillLevel.SetText(level.ToString());
        skillType.SetText(((skilltypeEnum)battleSkillCfg.skilltype).ToString());
        skillNote.SetText(string.Format("【{0}】", GameCenter.mIns.m_LanMgr.GetLan(battleSkillCfg.note1)));
        skillExplain.SetText(BattleCfgManager.Instance.GetSkillCfgNoteValue(battleSkillCfg, level));

        SetTalentContent(battleSkillCfg);
    }

    public void hide(bool isAnimations = false)
    {
        if (isAnimations)
            mTransform.GetComponent<RectTransform>().DOAnchorPosX(1600, duration).OnComplete(() =>
            mTransform.gameObject.SetActive(false));
        else
        {
            mTransform.GetComponent<RectTransform>().anchoredPosition = new Vector3(1600, 0);

            mTransform.gameObject.SetActive(false);
        }
        this.currentSkillID = -1;

        onBack?.Invoke(BackType.Info);
    }
    /// <summary>
    /// 设置天赋效果加成
    /// </summary>
    /// <param name="skillCfg"></param>
    void SetTalentContent(BattleSkillCfg skillCfg)
    {
        string relationskills = skillCfg.relationskills; // "1111|2222"
        string[] relationskillssplt = relationskills.Split('|');
        Debug.Log("relationskillssplt:" + relationskillssplt.Length + "--------" + relationskillssplt[0]);
        for (int i = 0; i < relationskillssplt.Length; i++)
        {
            if (string.IsNullOrEmpty(relationskillssplt[i]))
            {
                talentInfoContent.GetChild(i).gameObject.SetActive(false);
                continue;
            }
            GameObject talentInfoObj;
            if (talentInfoContent.childCount <= i)
            {
                talentInfoObj = UnityEngine.Object.Instantiate(talentInfoPrefab);
                talentInfoObj.transform.SetParent(talentInfoContent, false);
            }
            else
            {
                talentInfoObj = talentInfoContent.GetChild(i).gameObject;
            }
            BattleSkillTalentCfg talentCfg = BattleCfgManager.Instance.GetTalentCfg(long.Parse(relationskillssplt[i]));//天赋技能表
            talentInfoObj.SetActive(true);
            talentInfoObj.transform.Find("explain").GetComponent<TextMeshProUGUI>().SetText(GameCenter.mIns.m_LanMgr.GetLan(talentCfg.note));
            talentInfoObj.transform.Find("name").GetComponent<TextMeshProUGUI>().SetText(GameCenter.mIns.m_LanMgr.GetLan(talentCfg.name));
            LayoutRebuilder.ForceRebuildLayoutImmediate(talentInfoObj.transform.GetComponent<RectTransform>());
        }
        if (talentInfoContent.childCount > relationskillssplt.Length)
        {
            for (int i = relationskillssplt.Length; i < talentInfoContent.childCount; i++)
            {
                talentInfoContent.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

}
