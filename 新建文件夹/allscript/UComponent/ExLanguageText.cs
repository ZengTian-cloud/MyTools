using TMPro;
using UnityEngine;

[AddComponentMenu("UI/ExLanguageText - Text (TiJi)", 11)]
public class ExLanguageText:MonoBehaviour
{
    [SerializeField]
    private string lankey;
    public void Awake()
    {
        if (!string.IsNullOrEmpty(lankey)) {
            string txt = GameCenter.mIns.m_LanMgr.GetLan(lankey);
            if (!string.IsNullOrEmpty(txt))
            {
                TextMeshProUGUI txtUI = this.GetComponent<TextMeshProUGUI>();
                if(txtUI!=null)
                    txtUI.text = txt;
            }
            else {
                Debug.LogError($"语言没有找到 lankey:{lankey}");
            }
        }
    }
}

