using UnityEngine;

public class FPSHelper : MonoBehaviour
{ 
    // 上一次更新帧率的时间;
    private float m_LastUpdateShowTime = 0f;   
    // 更新帧率的时间间隔;
    private float m_UpdateShowDeltaTime = 0.2f;
    // 帧数;
    private int m_FrameUpdate = 0;

    private float m_FPS = 0;

    private bool m_Active = true;
    GUIStyle style = new GUIStyle();

    void Awake()
    {
    }

    // Use this for initialization
    void Start()
    {
        style = new GUIStyle();
        style.fontSize = 30;
        style.normal.textColor = Color.white;
        m_LastUpdateShowTime = Time.realtimeSinceStartup;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_Active) return;
        m_FrameUpdate++;
        if (Time.realtimeSinceStartup - m_LastUpdateShowTime >= m_UpdateShowDeltaTime)
        {
            m_FPS = m_FrameUpdate / (Time.realtimeSinceStartup - m_LastUpdateShowTime);
            m_FrameUpdate = 0;
            m_LastUpdateShowTime = Time.realtimeSinceStartup;
        }
    }

    void OnGUI()
    {
        if (!m_Active) return;
        GUI.Label(new Rect(Screen.width / 2, 0, 200, 200), "FPS: " + m_FPS, style);
    }

    public void SetFPSGUIActive(bool b)
    {
        m_Active = b;
    }
}