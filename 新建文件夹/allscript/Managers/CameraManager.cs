using System.Collections;
using System.Collections.Generic;
using Basics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Managers
{
    public class CameraManager : SingletonOjbect
    {
        /// <summary>
        /// 主相机
        /// </summary>
        public Camera mainCamera { get {
                if (m_camer == null)
                {
                    m_camer = GameObject.Find("mainCamera").GetComponent<Camera>();
                }
                return m_camer;
        } }

        private Camera m_camer;

        private List<Camera> cameraList;

        private UniversalRenderPipeline urpRP;


        private Camera m_uiCamera;
        public Camera uiCamera
        {
            get
            {
                if (m_uiCamera == null)
                {
                    m_uiCamera = GameObject.Find("wndcam").GetComponent<Camera>();
                }
                return m_uiCamera;
            }
        }
        private void Awake()
        {

            if (mainCamera != null)
            {
                SetCameraRenderType(mainCamera, CameraRenderType.Base);
            }
        }

        public void InitUICamera()
        {
            m_uiCamera = GameObject.Find("wndcam").GetComponent<Camera>(); // uiCamera;
            Debug.Log($"GIANTGAME 初始化UI摄像机 m_uiCamera：{m_uiCamera}");
            //AddCameraToMainCamera(m_uiCamera);
        }

        /// <summary>
        /// 设置相机的模式
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="type"></param>
        public void SetCameraRenderType(Camera camera, CameraRenderType type)
        {
            camera.GetUniversalAdditionalCameraData().renderType = type;
        }


        /// <summary>
        /// 添加一个相机到主相机的stack尾
        /// </summary>
        public void AddCameraToMainCamera(Camera camera)
        {
            SetCameraRenderType(camera, CameraRenderType.Overlay);
            cameraList = mainCamera.GetComponent<UniversalAdditionalCameraData>().cameraStack;
            if (!cameraList.Contains(camera))
            {
                cameraList.Add(camera);
                if (cameraList.Count > 3)
                {
                    Camera temp = cameraList[cameraList.Count - 2];
                    cameraList.RemoveAt(cameraList.Count - 2);
                    cameraList.Add(temp);
                }
            }
        }

        /// <summary>
        /// 从主栈删除一个相机
        /// </summary>
        /// <param name="camera"></param>
        public void RemoveCameraInMainCamera(Camera camera)
        {
            if (cameraList.Contains(camera))
            {
                cameraList.Remove(camera);
            }
        }
    }
}

