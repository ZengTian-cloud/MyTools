using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
using Cinemachine;

public class BattleCloseUpHelper : MonoBehaviour
{
    private Camera m_Camera;
    private GameObject m_Target;

    private bool m_RunFlag;
    private float m_Duration;
    private float m_Timer;

    private GameObject m_TimeLineObj;
    private PlayableDirector m_Director;

    private CloseUpObject m_CloseUpObject;

    private void OnEnable()
    {
        m_Camera = transform.Find("CloseUpCamera").GetComponent<Camera>();
    }

    private void Update()
    {
        if (m_RunFlag)
        {
            m_Timer += Time.deltaTime;
            if (m_Timer >= m_Duration)
            {
                Over();
            }
        }
    }

    public void Begin(GameObject target, CloseUpObject closeUpObject)
    {
        m_CloseUpObject = closeUpObject;
        m_Target = target;
        m_Timer = 0.0f;
        m_TimeLineObj = transform.GetChild(0).gameObject;
        m_Director = m_TimeLineObj.GetComponent<PlayableDirector>();
        if (m_Director == null)
        {
            Over();
            return;
        }
        // Debug.LogError("playableAsset.name:" + m_Director.playableAsset.name + " - playableAsset.duration:" + m_Director.playableAsset.duration);
        m_Duration = (float)m_Director.playableAsset.duration;
        CinemachineVirtualCamera[] cinemachineVirtualCamera = closeUpObject.closeUpObj.transform.GetComponentsInChildren<CinemachineVirtualCamera>();
        foreach (var cvc in cinemachineVirtualCamera)
        {
            cvc.LookAt = target.transform;
        }
        //m_Director.stopped += OnStopped();
        //m_Director.paused += OnPaused();
        //m_Director.played += OnPlayed();
        m_TimeLineObj.SetActive(true);
        m_RunFlag = true;
    }

    public void Over()
    {
        m_TimeLineObj.SetActive(false);
        m_Director = null;
        m_Target = null;
        m_RunFlag = false;
        // Debug.LogError("~~~~~~~ Over m_CloseUpObject:" + m_CloseUpObject);
        BattleCloseUpMgr.Instance.OnExitCloseUp(m_CloseUpObject);
    }

    private Action<PlayableDirector> OnStopped()
    {
        // Debug.LogError("OnStopped");
        return null;
    }

    private Action<PlayableDirector> OnPaused()
    {
        // Debug.LogError("OnPaused");
        return null;
    }

    private Action<PlayableDirector> OnPlayed()
    {
        // Debug.LogError("OnPlayed");
        return null;
    }
}
