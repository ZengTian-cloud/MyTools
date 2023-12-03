using LitJson;
using System.Collections.Generic;
using UnityEngine;
using Basics;

namespace Managers
{
    public class QualityManager : SingletonOjbect
    {
        #region FPS define
        public int targetFrameRate { get; private set; }
        // 目前定30帧
        public const int TargetFrameRate = 120;
        public bool IsDisplayFrameRate { get; set; } = false;
        private FPSHelper m_FPSHelper;
        #endregion

        private void OnEnable()
        {
            Init();
        }

        public void Init()
        {
            #region FPS
            targetFrameRate = TargetFrameRate;
            Application.targetFrameRate = targetFrameRate;

#if UNITY_EDITOR
            IsDisplayFrameRate = true;
#else
    IsDisplayFrameRate = true;
#endif

            if (Application.isPlaying && IsDisplayFrameRate)
            {
                m_FPSHelper = gameObject.GetOrAddCompoonet<FPSHelper>();
            }
            #endregion
        }

        public void SetFPSGUIActive(bool b)
        {
            if (m_FPSHelper != null)
            {
                m_FPSHelper.SetFPSGUIActive(b);
            }
        }
    }
}
