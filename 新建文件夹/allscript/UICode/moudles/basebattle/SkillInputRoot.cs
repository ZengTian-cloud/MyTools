using UnityEngine;
using System.Collections;

/// <summary>
/// 技能指示器 十字形
/// </summary>
public class SkillInputRoot : MonoBehaviour
{
	public GameObject sprite_0;//中心点

	public GameObject right;//右边条

    public GameObject right_1;//右边条端点

    public GameObject up;//右边条

    public GameObject up_1;//右边条端点

    public GameObject left;//右边条

    public GameObject left_1;//右边条端点

    public GameObject down;//右边条

    public GameObject down_1;//右边条端点

    private float endPointSize = 0.3f;

    private float oneSize = 1.3f;

    private float hight = 0.05f;
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="v">半径</param>
    public void InitRoot(float v)
    {
        float value = v / 100 - endPointSize;
        right.GetComponent<SpriteRenderer>().size = new Vector2(value, oneSize);
        up.GetComponent<SpriteRenderer>().size = new Vector2(value, oneSize);
        left.GetComponent<SpriteRenderer>().size = new Vector2(value, oneSize);
        down.GetComponent<SpriteRenderer>().size = new Vector2(value, oneSize);

        
        right.transform.localPosition = new Vector3((value+ oneSize) / 2 * 1.3f, hight, 0);
        up.transform.localPosition = new Vector3(0, hight, (value + oneSize) / 2 * 1.3f);
        left.transform.localPosition = new Vector3(-((value + oneSize) / 2 * 1.3f), hight, 0);
        down.transform.localPosition = new Vector3(0, hight, -((value + oneSize) / 2 * 1.3f));

        right_1.transform.localPosition = new Vector3((oneSize + endPointSize) / 2 * 1.3f + value * 1.3f, hight, 0);
        up_1.transform.localPosition = new Vector3(0, hight, (oneSize + endPointSize) / 2 * 1.3f + value * 1.3f);
        left_1.transform.localPosition = new Vector3(-((oneSize + endPointSize) / 2 * 1.3f + value * 1.3f), hight, 0);
        down_1.transform.localPosition = new Vector3(0, hight, -((oneSize + endPointSize) / 2 * 1.3f + value * 1.3f));
    }
}

