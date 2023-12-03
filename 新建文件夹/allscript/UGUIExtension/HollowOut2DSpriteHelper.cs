using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HollowOut2DSpriteHelper : MonoBehaviour
{
    // out:image:501, in:image:231
    public float maxOutScaleSilderPer = 0.6f;
    public float radius = 0.0f;

    private float inOutSizePer = 1f;

    private Material m_Material;
    private Transform m_OutTran;
    private Transform m_InnTran;
    private void Awake()
    {
        m_Material = GetComponent<SpriteRenderer>().material;
        m_OutTran = transform;
        m_InnTran = transform.GetChild(0);
        // maxOutScaleSilderPer = maxOutScaleSilderPer * m_OutTran.localScale.x;
    }

    public void Update()
    {
        if (radius <= 0)
            radius = 0;
        if (radius >= 1)
            radius = 1;

        m_InnTran.localScale = Vector3.one * radius * inOutSizePer;
        m_Material.SetFloat("_Silder", radius * maxOutScaleSilderPer);
    }
}
