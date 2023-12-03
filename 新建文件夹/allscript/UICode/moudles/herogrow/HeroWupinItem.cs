using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroWupinItem 
{
    private TextMeshProUGUI txtCount;
    private Image wupinIcon;
    private GameObject obj;
    private int propcount = 0;
    private int count = 0;

    public HeroWupinItem(Transform parent, GameObject obj, CostData cost)
    {
        this.obj = obj;
        this.count = cost.count;
        obj.GetComponent<RectTransform>().localScale = new Vector3(0.75f, 0.75f, 0.75f);
        txtCount = obj.transform.Find("bg_count/txt").gameObject.GetComponent<TextMeshProUGUI>();
        wupinIcon = obj.transform.GetComponent<Image>(); 
         propcount = GameCenter.mIns.userInfo.getPropCount(cost.propid);
        txtCount.text = HeroGrowUtils.parsePropCountStr(cost.count, propcount);
        wupinIcon.sprite= SpriteManager.Instance.GetSpriteSync("Icon_pack_libao_blue");
        obj.transform.SetParent(parent);
        /**/
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        obj.SetActive(true);
    }

    public bool propIsCount()
    {
        return count < propcount;
    }

    public void OnDestroy()
    {
        if (obj != null)
        {
            GameObject.Destroy(obj);
        }
    }
}
