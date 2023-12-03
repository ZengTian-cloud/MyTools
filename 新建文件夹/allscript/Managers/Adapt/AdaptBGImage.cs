using UnityEngine;
using UnityEngine.UI;

// 适配背景
public class AdaptBGImage : Image
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
			mRect.ForceUpdateRectTransforms();
		}
	}

	public void DoRefresh()
	{
		initBase();

		if (sprite != null)
		{
			var temwidth = mRect.sizeDelta.x;
			var temheight = mRect.sizeDelta.y;
			//Debug.LogError("temheight:" + temheight + " temwidth:" + temwidth);
			//Debug.LogError("y:" + mRect.sizeDelta.y + " x:" + mRect.sizeDelta.x);
			//Debug.LogError("h:" + zxconfig.adaptheight + " w:" + zxconfig.adaptheight);
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