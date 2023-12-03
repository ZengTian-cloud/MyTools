/*
using System;
using System.Collections.Generic;
using Basics;
#if !UNITY_EDITOR && UNITY_ANDROID
using Unity.Notifications.Android;
#elif !UNITY_EDITOR && (UNITY_IPHONE || UNITY_IOS)
using Unity.Notifications.iOS;
#endif
public class Notificationdata
{
	public string id;
	public string title;
	public string body;
	public long sendtime;
	public DateTime dateTime;
	public bool issend = false;
	public Notificationdata(string id, string title, string body, long sendtime)
	{
		this.id = id;
		this.title = title;
		this.body = body;
		this.sendtime = sendtime;
		this.issend = false;
		dateTime = nettool.convertdatetimetoint(sendtime);
	}
}

public class LocalNotification : SingletonOjbect
{

	public Dictionary<string, Notificationdata> notificationdates = new Dictionary<string, Notificationdata>();

	public void UpdateNotificationdata(string id, string title, string body, long sendtime)
	{

		Notificationdata data = new Notificationdata(id, title, body, sendtime);
		if (!notificationdates.ContainsKey(id))
		{
			notificationdates.Add(id, data);
		}
		else
		{
			if (notificationdates[id].sendtime != sendtime)
			{
				notificationdates[id] = data;
			}
		}
	}

	private void SendAllNotification()
	{

		foreach (var Notify in notificationdates)
		{
			if (Notify.Value.issend == false)
			{
				Notify.Value.issend = true;
				NotificationMessage(Notify.Value.title, Notify.Value.body, Notify.Value.dateTime);
			}
		}
	}

	void InitNotificationChannel()
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		var c = new AndroidNotificationChannel () {
			Id = "channel_id",
				Name = "Default Channel",
				Importance = Importance.High,
				Description = "Generic notifications",

		};
		AndroidNotificationCenter.RegisterNotificationChannel (c);
#endif
	}

	void NotificationMessage(string title, string message, DateTime dateTime)
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		var notification = new AndroidNotification ();
		notification.Title = title;
		notification.Text = message;
		notification.FireTime = dateTime;
		zxlogger.logerror ("-------xl--------" + notification.Title + "____" + notification.Text + "______" + notification.FireTime);
		AndroidNotificationCenter.SendNotification (notification, "channel_id");

#elif !UNITY_EDITOR && (UNITY_IPHONE || UNITY_IOS)
		var iosnotification = new iOSNotification ();
		var sendtime = dateTime;
		iosnotification.Identifier = "channel_id";
		iosnotification.Title = title;
		iosnotification.Body = message;
		iosnotification.Trigger = new iOSNotificationCalendarTrigger ()
		{
			Year = sendtime.Year,
			Month = sendtime.Month,
			Day = sendtime.Day,
			Hour = sendtime.Hour,
			Minute = sendtime.Minute,
			Second = sendtime.Second,
		};
		iosnotification.ShowInForeground = false;
		iosnotification.CategoryIdentifier = "channel_id";
		iOSNotificationCenter.ScheduleNotification (iosnotification);
#endif
	}

	//游戏退出时调用
	void OnApplicationQuit()
	{ //每天中午12点推送
		zxlogger.logerror("退出游戏调用消息推送2");
		InitNotificationChannel();
		zxlogger.logerror("退出游戏调用消息推送");
		SendAllNotification();
	} //游戏进入后台时或者从后台进入前台时调用

	void OnApplicationPause(bool paused)
	{
		if (paused)
		{
			zxlogger.logerror("后台游戏调用消息推送1");
			InitNotificationChannel();
			zxlogger.logerror("后台游戏调用消息推送");
			SendAllNotification();
		}
		else
		{
			CleanNotification();
		}
	}

	//清除推送消息,在Awake中调用
	void CleanNotification()
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		zxlogger.logerror ("android移除消息推送");
		AndroidNotificationCenter.DeleteNotificationChannel ("channel_id");
#elif !UNITY_EDITOR && (UNITY_IPHONE || UNITY_IOS)
		zxlogger.logerror ("ios移除消息推送");
		iOSNotificationCenter.RemoveScheduledNotification ("channel_id");
#endif
	}
}
*/