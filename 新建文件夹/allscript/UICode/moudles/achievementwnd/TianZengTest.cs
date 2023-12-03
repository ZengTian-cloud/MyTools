using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TianZengTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
         
                GameCenter.mIns.m_UIMgr.Open<achievementwnd>();
         
        }
    }
}
