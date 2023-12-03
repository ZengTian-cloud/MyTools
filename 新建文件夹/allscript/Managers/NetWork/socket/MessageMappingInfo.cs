using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class MessageMappingInfo
{
    public MessageMappingInfo() { }
    public MessageMappingInfo(ServerMessageDispatch serverMessageDispatch,int messageid) {
        UniTask.Void(async () => {
            await UniTask.Delay(8000);
            if (ResponseMessage != null && ResponseMessage.GetHeader() != null && ResponseMessage?.GetHeader()?.MessageId <= -1)
            {
                ResponseMessage.GetHeader().ErrorCode = -999;
                Debug.LogError($"NetManager 请求超时了。。。。。8000 seqid:{ClientSeqID},MessageId:{messageid}");
                GameCenter.mIns.m_UIMgr.EnThreadPreQueue(() =>
                {
                    GameCenter.mIns.m_UIMgr.PopMsg($"请求超时：code:{-999}, messageid:{messageid},timeout:{8000}");
                });
                Action?.Invoke(ResponseMessage);
                serverMessageDispatch.DeMessageMapping(ClientSeqID);
            }
            
        });
    }

    public int ClientSeqID { get; set; }
    public IGameMessage ResponseMessage { get; set; }
    public Action<IGameMessage> Action { get; set; }
    public long mtime { get; set; }

}
