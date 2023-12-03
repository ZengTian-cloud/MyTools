using System;
using System.Collections.Concurrent;

public class PushMessageInfo
{
    public int MessageId { get; set; }
    public ConcurrentDictionary<string, Action<IGameMessage>> Callbacks = new ConcurrentDictionary<string, Action<IGameMessage>>();
}
