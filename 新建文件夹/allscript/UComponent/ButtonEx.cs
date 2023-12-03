using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 自定义按钮
public class ButtonEx : Button
{
	[Range(500, 10000)]
	public int m_longPressNeedTime = 10000; // 标记长按所需要的时间
	public int m_intervalTime = 300; // 点击间隔时间
	public bool m_bPassEvent = false; // 是否穿透事件
	public bool m_bReceivePass = false; // 是否接受穿透
	public bool m_btnAnim = true; //播放动画

	private long m_lastClickTime = 0; // 上次点击的时间
	private int m_shortPressTag = 0; // 按钮标记 0.触发按下 1.触发抬起 2.触发长按
	private ButtonClickedEvent m_onClick = null;
	public ButtonClickedEvent onExClick
	{
		get
		{
			if (m_onClick == null)
			{
				m_onClick = new ButtonClickedEvent();
			}
			return m_onClick;
		}
		set { m_onClick = value; }
	}

	private ButtonClickedEvent m_onPress = null;
	public ButtonClickedEvent onPress
	{
		get
		{
			if (m_onPress == null)
			{
				m_onPress = new ButtonClickedEvent();
			}
			return m_onPress;
		}
		set { m_onPress = value; }
	}

	private ButtonClickedEvent m_onLongPress = null;
	public ButtonClickedEvent onLongPress
	{
		get
		{
			if (m_onLongPress == null)
			{
				m_onLongPress = new ButtonClickedEvent();
			}
			return m_onLongPress;
		}
		set { m_onLongPress = value; }
	}

	private ButtonClickedEvent m_onUp = null;
	public ButtonClickedEvent onUp
	{
		get
		{
			if (m_onUp == null)
			{
				m_onUp = new ButtonClickedEvent();
			}
			return m_onUp;
		}
		set { m_onUp = value; }
	}

	public void RemoveAllListeners()
	{
		m_onPress?.RemoveAllListeners();
		m_onLongPress?.RemoveAllListeners();
		m_onUp?.RemoveAllListeners();
		m_onClick?.RemoveAllListeners();
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		m_shortPressTag = 0;
		m_onPress?.Invoke();
		Invoke("OnPointLongPress", m_longPressNeedTime * 0.001f * Mathf.Max(1, Time.timeScale));
		if (m_btnAnim)
		{
			UcsdoTween.playgameobject(gameObject, 1).doscale(0.97f, 0.97f, 0.97f, 0.1f);
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		if (m_shortPressTag == 0)
		{
			m_shortPressTag = 1;
			CancelInvoke("OnPointLongPress");
		}
		if (m_btnAnim)
		{
			UcsdoTween.playgameobject(gameObject, 1).doscale(1f, 1f, 1f, 0.2f);
		}
		base.OnPointerUp(eventData);
		m_onUp?.Invoke();
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (interactable && m_shortPressTag <= 1)
		{
			base.OnPointerClick(eventData);
			if (m_lastClickTime + m_intervalTime < nettool.timestamp)
			{
				m_lastClickTime = nettool.timestamp;
				if (m_bPassEvent)
				{
					var listButton = CheckPassEvent(eventData);
					m_onClick?.Invoke();
					for (var i = 0; i < listButton.Count; i++)
					{
						var temPass = listButton[i].m_bPassEvent;
						listButton[i].m_bPassEvent = false;
						ExecuteEvents.Execute(listButton[i].gameObject, eventData, ExecuteEvents.pointerClickHandler);
						listButton[i].m_bPassEvent = temPass;
					}
				}
				else
				{
					m_onClick?.Invoke();
				}
			}
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (m_shortPressTag == 0)
		{
			m_shortPressTag = 1;
			CancelInvoke("OnPointLongPress");
		}
		base.OnPointerExit(eventData);
	}

	private void OnPointLongPress()
	{
		m_shortPressTag = 2;
		onLongPress?.Invoke();
	}

	private List<ButtonEx> CheckPassEvent(PointerEventData data)
	{
		var listButton = new List<ButtonEx>();
		var results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(data, results);
		for (int i = 1; i < results.Count; i++)
		{
			var passObj = results[i].gameObject;
			if (passObj != null)
			{
				var passCom = passObj.GetComponent<ButtonEx>();
				if (passCom != null && passCom.m_bReceivePass)
				{
					listButton.Add(passCom);
					if (passCom.m_bPassEvent)
					{
						continue;
					}
				}
			}
			break;
		}
		return listButton;
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RemoveAllListeners();
	}
}
