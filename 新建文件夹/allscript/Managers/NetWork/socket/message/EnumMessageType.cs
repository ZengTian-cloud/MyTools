using System;
public enum EnumMessageType
{
    REQUEST = 1,
    RESPONSE = 2,
    PUSH = 3
}

public enum NetState
{
    DEFAUIT = 0,//初始状态
    CONNECTED= 1,//连接中
    FAIL = 2,//连接错误了
    OPERATION = 3,//运行中
    RECONNECT=4,//重连中
    RECMAX=5,//重连次数超限
    EXIT=6//准备退出了
}
