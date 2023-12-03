using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 遥杆
/// </summary>
public class InputTouchHelper:MonoBehaviour
{
    public CharacterController characterController;

    public void OnMoveStart()
    {
        //Debug.LogError("OnMoveStart");
    }

    public void OnMove(Vector2 vector2)
    {
        // v2Pos: 上(0,1) 下(0,-1) 左(1,0) 右(-1,0)
    }

    public void OnMoveSpeed(Vector2 vector2)
    {
        //Debug.LogError("OnMoveSpeed vector2:" + vector2);
    }

    public void OnMoveEnd()
    {

    }

    public void OnTouchStart()
    {
        //Debug.LogError("OnTouchStart");
    }

    public void OnTouchUp()
    {
        //Debug.LogError("OnTouchUp");
    }

    public void OnDownUp()
    {
        //Debug.LogError("OnDownUp");
    }

    public void OnDownDown()
    {
        //Debug.LogError("OnDownDown");
    }
    public void OnDownLeft()
    {
        //Debug.LogError("OnDownLeft");

    }

    public void OnDownRight()
    {
        //Debug.LogError("OnDownRight");
    }

    public void OnPressUp()
    {
        //Debug.LogError("OnPressUp");
    }

    public void OnPressDown()
    {
        //Debug.LogError("OnPressDown");
    }
    public void OnPressLeft()
    {
        //Debug.LogError("OnPressLeft");

    }

    public void OnPressRight()
    {
        //Debug.LogError("OnPressRight");
    }
}

