using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SequenceFrameHelper : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Image image;
    public Sprite[] sprites;
    public float limit = 0.033f;
    public float speed = 1;
    private int index = 0;
    private bool isOver = false;
    private float timer = 0.0f;

    public Action overCallback;
    public bool bOverActive = true;

    public bool isStartFade = false;
    public float fadeRate = 0;
    public int fateFrame = 0;
    private void OnEnable()
    {
        if (image == null)
            image = GetComponent<Image>();
        if (sprites != null  && sprites.Length > 0)
            image.sprite = sprites[0];
        isOver = false;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
        }
        isStartFade = false;
        SetFade(0.13f, 3);
    }

    public void Update()
    {
        if (sprites == null && sprites.Length <= 0) return;
        if (isOver) return;
        timer += Time.deltaTime * speed;
        if (timer >= limit)
        {
            if (isStartFade && canvasGroup != null)
            {
                canvasGroup.alpha = canvasGroup.alpha - fadeRate;
                Debug.LogError("index:" + index + " - canvasGroup.alpha:" + canvasGroup.alpha);
            }

            if (sprites != null && sprites.Length > 0 && index < sprites.Length)
            {
                image.sprite = sprites[index];
                index++;
            }
            if (index == sprites.Length)
            {
                // 最后停顿一哈
                index++;
            }
            if (index == sprites.Length + 1)
            {
                timer = 0;
                index = 0;
                isOver = true;
                if (bOverActive)
                {
                    gameObject.SetActive(false);
                }
                overCallback?.Invoke();
            }
            if (fateFrame != 0 && index >= fateFrame && !isStartFade)
            {
                isStartFade = true;
            }
            timer = 0;
        }
    }

    public void SetFade(float fadeRate, int fateFrame)
    {
        this.fadeRate = fadeRate;
        this.fateFrame = fateFrame;
    }

    public void SetOverActive(bool bOverActive = true)
    {
        this.bOverActive = bOverActive;
    }

    public void Init(string seqName, int startIndex, int endIndex, bool tenZero = false)
    {
        if (endIndex <= 0 || startIndex <= 0 || startIndex >= endIndex)
            return;

        sprites = new Sprite[endIndex - startIndex];
        for (int i = startIndex; i <= endIndex; i++)
        {
            string resName = "";
            string strnum = "";
            if (i < 10)
            {
                strnum = tenZero ? "0" + i : i.ToString();
            }
            else
            {
                strnum = i.ToString();
            }
            resName = $"{seqName}{strnum}";
            Sprite sprite = SpriteManager.Instance.GetSpriteSync(resName);
            if (sprite != null)
            {
                // sprites
                sprites[i - startIndex] = sprite;
            }
        }
    }
}
