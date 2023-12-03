using LitJson;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PushHandle
{
    private static volatile PushHandle inst;
    private static object syncLock = new object();
    private PushHandle() { }

    public static PushHandle Inst
    {
        get
        {
            if (inst == null)
            {
                lock (syncLock)
                {
                    inst = new PushHandle();
                }

            }
            return inst;
        }
    }
    private EventExecutor eventExecutor = new EventExecutor();

    public void handlePushMessage(IGameMessage message) {

        eventExecutor.Execute(() =>
        {
            handle(message);
        });
    }

    /// <summary>
    /// PUSH消息处理机制，针对命令号单独的处理方法会优先于此方法执行
    /// </summary>
    /// <param name="message"></param>
    private void handle(IGameMessage message) {

        Debug.Log("处理PUSH消息 MessageId：" + message.GetHeader().MessageId);
        JsonData data = null;
        if (message.getContent() != null&&!message.getContent().Equals("")) {
            data = JsonMapper.ToObject(new JsonReader(message.getContent()));
        }
        switch (message.GetHeader().MessageId)
        {
            case 100:
                //处理回调
                exitNotice(data);
                break;
            case 101:
                // 101: 邮件通知
                // 102~106: 好友
            case 102:
            case 103:       
            case 104: 
            case 105:  
            case 106:
                FriendManager.Instance.DisposeMailFriendPushNotif(message.GetHeader().MessageId, message);
                break;
            case 109://任务完成通知
                TaskMsgManager.Instance.PushHandleTask(message);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 服务器通知下线
    /// </summary>
    /// <param name="data"></param>
    private void exitNotice(JsonData data) {
        int type = data["type"].ToInt32();

        switch (type) { 
            case 0:
                SceneManager.LoadScene(0);
                break;
            case 1:
            case 2:
            case 3:
            default:
                string msg = data["msg"].ToString();
                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() => {
                    GameCenter.mIns.m_UIMgr.PopWindowPrefab(new PopWinStyle("错误", msg, 1, (confirm) =>
                    {
                        //退回到登录 界面
                        GameCenter.mIns.ExitGame();
                    }));
                });
                break;

        }


        
    }
}
