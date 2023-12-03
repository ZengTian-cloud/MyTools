using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerEx : CanvasScaler
{
    float standard_aspect = 0;
    float dev_aspect = 0;
    protected override void Awake()
	{
		base.Awake();
        if (!Application.isPlaying) return;
		float standard_w = zxconfig.designwidth;//referenceResolution.x;
        float standard_h = zxconfig.designheight;//referenceResolution.y;
        float dev_w = Screen.width;
        float dev_h = Screen.height;

        standard_aspect = standard_w / standard_h;
        dev_aspect = dev_w / dev_h;

        //Debug.Log("standard_w:" + standard_w + " - standard_h:" + standard_h + " - dev_w:" + dev_w + " - dev_h:" + dev_h + " - standard_aspect:" + standard_aspect + " - dev_aspect:" + dev_aspect);
        matchWidthOrHeight = dev_aspect < standard_aspect ? 0 : 1;
        DoRefresh();
	}

	public void DoRefresh()
	{
		float w = zxconfig.designwidth;
        float h = zxconfig.designheight;
        if (matchWidthOrHeight == 0) // Ëø¸ß
            w = zxconfig.designheight * dev_aspect;
        else // Ëø¿í
            h = zxconfig.designwidth / dev_aspect;
        referenceResolution = new Vector2(w, h);
	}
}