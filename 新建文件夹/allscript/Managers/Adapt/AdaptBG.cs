using UnityEngine;
using UnityEngine.UI;

// 适配背景
public class AdaptBG : RawImage
{
	private RectTransform mRect;

	private bool bsetbase = false;

	protected override void Awake()
	{
		base.Awake();
		DoRefresh();
	}

	private void initBase()
	{
		if (!bsetbase)
		{
			bsetbase = true;
			mRect = GetComponent<RectTransform>();
			mRect.anchorMin = new Vector2(0.5f, 0.5f);
			mRect.anchorMax = new Vector2(0.5f, 0.5f);
			mRect.pivot = new Vector2(0.5f, 0.5f);
		}
	}

	public void DoRefresh()
	{
		initBase();


		if (texture != null)
		{
			var temwidth = texture.width;
			var temheight = texture.height;
			if (temwidth * zxconfig.adaptheight / temheight > zxconfig.adaptwidth)
			{
				mRect.sizeDelta = new Vector2(temwidth * zxconfig.adaptheight / temheight, zxconfig.adaptheight);
			}
			else
			{
				mRect.sizeDelta = new Vector2(zxconfig.adaptwidth, temheight * zxconfig.adaptwidth / temwidth);
			}
		}
	}
}