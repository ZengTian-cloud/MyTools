using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MonsterPanelCompute : MonoBehaviour
{
    InputField transform_StartInput;
    InputField transform_IntervalInput;
    InputField transform_CountInput;
    // Start is called before the first frame update
    void Start()
    {
        transform_StartInput = transform.parent.Find("StartInput").GetComponent<InputField>();
        transform_IntervalInput = transform.parent.Find("IntervalInput").GetComponent<InputField>();
        transform_CountInput = transform.parent.Find("CountInput").GetComponent<InputField>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!string.IsNullOrEmpty(transform_StartInput.text) && !string.IsNullOrEmpty(transform_IntervalInput.text) && !string.IsNullOrEmpty(transform_CountInput.text))
        {
            GetComponent<Text>().text = (int.Parse(transform_StartInput.text) + int.Parse(transform_IntervalInput.text) * (int.Parse(transform_CountInput.text) - 1)).ToString();
        }
        else
        {
            GetComponent<Text>().text = "";
        }
    }
}
