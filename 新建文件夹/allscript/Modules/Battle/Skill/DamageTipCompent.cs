using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro;

/// <summary>
/// 伤害跳字组件
/// </summary>
public class DamageTipCompent : MonoBehaviour
{
    int maxHeight = 1500;//在该区域内随机 千分位
    int minHeight = 1000;
    int left = -300;
    int right = 300;

    public void ShowTip(float value,int type)
    {
        string damageText = "";
        if (value > 0)
        {
            damageText = commontool.GetArtNumberString(value, type);
        }
        else
        {
            damageText = GameCenter.mIns.m_LanMgr.GetLan("battle_mianyi");
        }

        //高度 y轴
        float randY = Random.Range(maxHeight, minHeight) / 1000f;
        float randX = Random.Range(left, right) / 1000f;
        Vector3 targetPos = this.transform.localPosition + new Vector3(randX, randY, 0);

        this.gameObject.GetComponent<TextMeshPro>().text = damageText;
        this.gameObject.transform.DOLocalMove(targetPos, 0.5f).SetEase(Ease.Linear);
        this.gameObject.GetComponent<TextMeshPro>().DOFade(0, 1f).OnComplete(() => {
            BattlePoolManager.Instance.InPool(ERootType.DamageTMP, this.gameObject);
            this.gameObject.GetComponent<TextMeshPro>().DOFade(1, 0.1f);
        }).SetEase(Ease.InBack);
    }

    private void Update()
    {
        this.transform.rotation = Quaternion.Euler(55, 0, 0);
    }
}

