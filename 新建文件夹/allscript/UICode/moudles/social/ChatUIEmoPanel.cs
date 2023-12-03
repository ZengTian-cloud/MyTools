using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.TextCore.Text;
using UnityEngine.U2D;

public class ChatUIEmoPanel
{
    private Action<EnumEmoType, int> clickCallbak;
    private GameObject emoPanel;
    private Transform emoPanelTran;
    private EnumEmoType currEmoType = EnumEmoType.Common;

    private GameObject tabsObj;
    private ScrollRect emoSR;
    private GameObject emoSRContent;
    private GameObject emoItem;

    private List<TabBtn> tabBtns = new List<TabBtn>();
    private List<EmoItem> emoItems = new List<EmoItem>();

    private Sprite[] commonSprites = null;

    public ChatUIEmoPanel(GameObject emoPanel, Action<EnumEmoType, int> clickCallbak)
    {
        this.emoPanel = emoPanel;
        emoPanelTran = emoPanel.transform;
        this.clickCallbak = clickCallbak;
        Init();
    }
    public async void Init()
    {
        tabsObj = emoPanelTran.Find("tabs").gameObject;
        emoSR = emoPanelTran.Find("emoSR").GetComponent<ScrollRect>();
        emoSRContent = emoSR.transform.Find("emoSRContent").gameObject;
        emoItem = emoSRContent.transform.Find("emoItem").gameObject;
        emoItem.SetActive(false);
        tabBtns.Clear();
        int tabCount = tabsObj.transform.childCount;
        for (int i = 0; i < tabCount; i++)
        {
            Button tabBtnCom = tabsObj.transform.GetChild(i).GetComponent<Button>();
            TabBtn tabBtn = new TabBtn(tabBtnCom, i, OnClickTab);
            tabBtns.Add(tabBtn);
        }

        foreach (var ei in emoItems)
        {
            ei.OnDestroy();
        }
        emoItems.Clear();

        // 默认初始化通用表情
        currEmoType = EnumEmoType.Common;
        SpriteAtlas emoSpriteAsset = await SpriteManager.Instance.LoadAtlas("emo");
        if (emoSpriteAsset != null)
        {
            Sprite[] sprites = new Sprite[emoSpriteAsset.spriteCount];
            emoSpriteAsset.GetSprites(sprites);
            List<TempSortSpriteObj> spriteList = new List<TempSortSpriteObj>();
            foreach (var sp in sprites)
            {
                spriteList.Add(new TempSortSpriteObj(sp));
            }
            spriteList.Sort(new EmoSpriteSort());

            int i = 0;
            foreach (var sp in spriteList)
            {
                GameObject imgObj = GameObject.Instantiate(emoItem);
                imgObj.name = sp.name;
                imgObj.transform.SetParent(emoItem.transform.parent);
                imgObj.transform.localScale = Vector3.one;
                EmoItem ei = new EmoItem(EnumEmoType.Common, imgObj.GetComponent<Image>(), i++, OnClickEmo);
                emoItems.Add(ei);
                ei.SetSprite(sp.sprite);
            }
        }
    }

    private class TempSortSpriteObj
    {
        public Sprite sprite;
        public string name;
        public int index;
        public TempSortSpriteObj(Sprite sprite)
        {
            if (sprite != null)
            {
                this.sprite = sprite;
                name = sprite.name.Replace("(Clone)", "");
                index = int.Parse(name.Split(new char[] { '_' })[1]);
            }
        }
    }

    private class EmoSpriteSort : IComparer<TempSortSpriteObj>
    {
        public int Compare(TempSortSpriteObj a, TempSortSpriteObj b)
        {
            if (a.index != b.index)
            {
                return a.index.CompareTo(b.index);
            }
            return 0;
        }
    }

    public void SetActive(bool b)
    {
        if (emoPanel != null)
        {
            emoPanel.SetActive(b);
        }
    }

    private void OnClickTab(int index)
    {

    }

    private void OnClickEmo(EnumEmoType enumEmo, int index)
    {
        string emoStr = "";
        if (enumEmo == EnumEmoType.Common)
        {
            emoStr = $"<sprite=\"emo\" name={EnumEmoPrefix.emo_.ToString()}{index}>";
        }
        Debug.LogWarning("on click emo enumEmo:" + enumEmo + " - index:" + index + " - emoStr:" + emoStr);
        GameEventMgr.Distribute(GEKey.Chat_ClickChatEmo, emoStr);
        SetActive(false);
    }

    public void Reset()
    {

    }

    public void Clear()
    {

    }

    private class TabBtn
    {
        public Button btn;
        public int index;
        public Action<int> clickCallback;
        public TabBtn(Button btn, int index, Action<int> clickCallback)
        {
            this.btn = btn;
            this.index = index;
            this.clickCallback = clickCallback;
            btn.AddListenerBeforeClear(OnClick);
        }

        public void OnClick()
        {
            clickCallback?.Invoke(index);
        }
    }

    private class EmoItem
    {
        public EnumEmoType emoType;
        public Image image;
        public int index;
        public Action<EnumEmoType, int> clickCallback;
        public EmoItem(EnumEmoType emoType, Image image, int index, Action<EnumEmoType, int> clickCallback)
        {
            this.emoType = emoType;
            this.image = image;
            this.index = index;
            this.clickCallback = clickCallback;
            image.GetComponent<Button>().AddListenerBeforeClear(OnClick);
        }
        public void SetActive(bool b)
        {
            if (image != null)
                image.gameObject.SetActive(b);
        }

        public void SetSprite(Sprite sprite)
        {
            if (sprite == null)
            {
                SetActive(false);

                return;
            }
            if (image != null)
                image.sprite = sprite;
            SetActive(true);
        }

        public void OnClick()
        {
            clickCallback?.Invoke(emoType, index);
        }

        public void OnDestroy()
        {
            if (image != null)
                GameObject.Destroy(image.gameObject);
        }
    }
}
public enum EnumEmoType
{
    // 通用表情
    Common = 1,
    // 序列帧表情
    Seq = 2,
}

public enum EnumEmoPrefix
{
    // 通用表情前缀
    emo_,
    // 序列帧表情前缀
    seq_
}