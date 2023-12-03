using Basics;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Managers
{
	public class PostProcessManager : SingletonOjbect
	{
		//是否开启抗锯齿
		public bool openSmaa = false;
		//当前抗锯齿等级
		public PostProcessLayer.Antialiasing antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
		// 展示当前节点Post-process, 关闭其他节点
		public void ShowComponent(bool justsmaa, params GameObject[] vtemobj)
		{
			HideAllComponent();
			if (vtemobj != null)
			{
				for (var i = 0; i < vtemobj.Length; i++)
				{
					var oneobj = vtemobj[i];
					var camcom = oneobj.GetComponent<Camera>();
					if (camcom != null)
					{
						var temcom = oneobj.GetComponent<PostProcessLayer>();
						if (temcom == null)
						{
							temcom = oneobj.AddComponent<PostProcessLayer>();
							temcom.volumeTrigger = oneobj.transform;

							//temcom.m_Resources = zxload.addsync("zlaunchui", "postprocessresources") as PostProcessResources;

							oneobj.layer = 18;
						}
						temcom.volumeLayer = justsmaa ? 0 : 1 << 18;
						temcom.enabled = true;
						temcom.antialiasingMode = openSmaa ? antialiasingMode : PostProcessLayer.Antialiasing.None;
					}
				}
			}
		}

		public void UnLoadComponent(GameObject temob)
		{
			if (temob != null)
			{
				var temcom = temob.GetComponent<PostProcessLayer>();
				if (temcom != null)
				{
					Destroy(temcom);
				}
			}
		}

		// 关闭所有节点post-process
		public void HideAllComponent()
		{
			var trootui = GameCenter.mIns.m_RootUI;
			if (trootui != null)
			{
				var vpplcom = trootui.GetComponentsInChildren<PostProcessLayer>(true);
				if (vpplcom != null)
				{
					for (var i = 0; i < vpplcom.Length; i++)
					{
						vpplcom[i].enabled = false;
					}
				}
			}
		}

		// 关闭所有节点post-process
		public void HideOneComponent(GameObject temobj, bool isshow)
		{
			var temcom = temobj.GetComponent<PostProcessLayer>();
			if (temcom != null)
			{
				temcom.enabled = isshow;
				if (isshow)
				{
					temcom.antialiasingMode = openSmaa ? antialiasingMode : PostProcessLayer.Antialiasing.None;
				}
			}
		}

		public void ChangeSmaaState(bool open)
		{
			openSmaa = open;
			var trootui = GameCenter.mIns.m_RootUI;
			if (trootui != null)
			{
				var vpplcom = trootui.GetComponentsInChildren<PostProcessLayer>(true);
				if (vpplcom != null)
				{
					for (var i = 0; i < vpplcom.Length; i++)
					{
						vpplcom[i].antialiasingMode = open ? antialiasingMode : PostProcessLayer.Antialiasing.None;
					}
				}
			}
		}

		//设置抗锯齿等级
		public void SetAntialiasingMode(int antialiasing)
		{
			antialiasingMode = (PostProcessLayer.Antialiasing)antialiasing;
			var trootui = GameCenter.mIns.m_RootUI;
			if (trootui != null)
			{
				var vpplcom = trootui.GetComponentsInChildren<PostProcessLayer>(true);
				if (vpplcom != null)
				{
					for (var i = 0; i < vpplcom.Length; i++)
					{
						vpplcom[i].antialiasingMode = openSmaa ? antialiasingMode : PostProcessLayer.Antialiasing.None;
					}
				}
			}
		}
	}
}