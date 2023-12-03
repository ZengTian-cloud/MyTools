using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MessageCacheInfo
{
    public MessageCacheInfo(int messageId, int seqId,IGameMessage msg, Action<int, int, string> tcb, Action<int> messageSeqAction) { 
        this.MessageId = messageId;
        this.SeqId = seqId;
        this.Tcb= tcb;
        this.MessageSeqAction= messageSeqAction;
        this.Message = msg;
    }
    public int MessageId { get; set; }
    public int SeqId { get; set; }
    public IGameMessage Message { get; set; }
    public Action<int, int, string> Tcb { get; set; }
    public Action<int> MessageSeqAction { get; set; }
}
