using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneRoleController : MonoBehaviour
{

    private TestMainSceneHeroCtrl m_Ctrl;
    public TestMainSceneHeroCtrl MainSceneHeroCtrl { get { return m_Ctrl; } set { m_Ctrl = value; } }

    public void OnMoveStart()
    {
        //Debug.LogError("OnMoveStart");
    }

    public void OnMove(Vector2 vector2)
    {
        if (m_Ctrl == null)
            m_Ctrl = MainSceneManager.Instance.GetMainSceneCamera().GetComponent<TestMainSceneHeroCtrl>();
        if (m_Ctrl != null)
            m_Ctrl.JoystickMove(vector2, Vector2.zero);
    }

    public void OnMoveSpeed(Vector2 vector2)
    {
        //Debug.LogError("OnMoveSpeed vector2:" + vector2);
    }

    public void OnMoveEnd()
    {
        //Debug.LogError("OnMoveEnd");
        if (m_Ctrl != null)
            m_Ctrl.EndJoystickMove();
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
