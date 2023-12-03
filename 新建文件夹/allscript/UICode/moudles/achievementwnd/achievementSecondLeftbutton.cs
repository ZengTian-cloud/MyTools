using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class achievementSecondLeftbutton : MonoBehaviour
{
    achievementwnd myFather;
    int myIndex;
    UnityAction myAction;
    public void Init(int index,achievementwnd wnd)
    {
        myFather = wnd;
        myIndex = index;
        myAction = ActionFunction;
        GetComponent<ExButton>().onClick.AddListener(myAction);
    }
    public void ActionFunction()
    {
        myFather.SecondLeftbuttoninterface(myIndex);
    }
}
