using UnityEngine;

// 忽略有效区域适配
public class AdaptIgnore : MonoBehaviour
{
	private RectTransform mRect;

	private bool bsetbase = false;

	private void Awake()
	{
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

		mRect.sizeDelta = new Vector2(zxconfig.adaptwidth, zxconfig.adaptheight);
	}
}