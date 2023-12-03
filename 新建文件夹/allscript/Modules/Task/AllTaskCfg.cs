using System;
/////////////////////////任务配置表数据结构类 所有任务相关表结构在这里声明

/// <summary>
/// 任务总表
/// </summary>
public class TaskBaseCfg
{
    //任务id
    public long taskid;

    //任务类型
    /*任务服务器逻辑类别
    1=一次性完成
    2=顺序触发，一次性完成
    3=获取任务后触发，一次性完成
    4=按日期刷新
    5=按活动周期刷新
    6=按条件解锁，一次性完成
    7=累计类任务，按档位完成，后续档位隐藏
    8=累计类任务，所有档位全显示*/
    public int tasktype;

    public string describe;

    /*所属功能
    1=主线任务
    2=探索任务
    3=冒险任务
    4=同伴任务
    5=日常
    6=成就
    7=里程碑
    8=活动
    9=周常
    10=每日委托*/
    public int model;

    //任务跳转id
    public long jumpid;
}
//-----------------------------------------------------相关模块功能>>>start--------------------------------------------
/// <summary>
/// 任务分表-成就
/// </summary>
public class TaskAchievementCfg
{
    //任务ID
    public long taskid;
    //分组
    public int group;
    //是否要生成任务条
    public int progressbar;
}

/// <summary>
/// 任务分表-里程碑
/// </summary>
public class TaskStageCfg
{
    //任务ID
    public long taskid;
    //阶段ID
    public int stage;
    //跳转ID
    public long jumpid;
}

/// <summary>
/// 任务分表-冒险任务
/// </summary>
public class TaskAdventureCfg
{
    //任务ID
    public long taskid;
    //所属任务条目的条目名
    public string name1;
    //是否可追踪
    public int trace;
    //追踪后是否可跳转
    public long jumpid;
}

/// <summary>
/// 任务分表-日常
/// </summary>
public class TaskDailyCfg
{
    //任务ID
    public long taskid;
    //任务奖励活跃点
    public int dropactvalue;
    //跳转ID
    public long jumpid;
}

/// <summary>
/// 任务分表-探索任务
/// </summary>
public class TaskExploreCfg
{
    //任务ID
    public long taskid;
    //所属任务条目名称
    public string name1;
    //是否可追踪
    public int trace;
    //跳转ID
    public long jumpid;
}

/// <summary>
/// 任务分表-同伴任务
/// </summary>
public class TaskPartnerCfg
{
    //任务ID
    public long taskid;
    //所属任务条目名称
    public string name1;
    //是否可追踪
    public int trace;
    //跳转ID
    public long jumpid;
}

/// <summary>
/// 任务分表-周常
/// </summary>
public class TaskWeekCfg
{
    //任务ID
    public long taskid;
    //任务活跃点奖励
    public int dropactvalue;
    //跳转ID
    public long jumpid;
}

/// <summary>
/// 任务分表-主线任务
/// </summary>
public class TaskMainCfg
{
    //任务ID
    public long taskid;
    //任务排序
    public int sort;
    //章节名
    public string name;
    //任务条目名
    public string name1;
    //奖励是否自动领取
    public int autoreward;
    //奖励预览
    public long rewardpreview;  
}
//-----------------------------------------------------相关模块功能>>>end--------------------------------------------


//-----------------------------------------------------分组相关>>>start--------------------------------------------
/// <summary>
/// 分组表-成就任务
/// </summary>
public class TaskAchievemrntGroupCfg
{
    //分组
    public int group;
    //分组名称
    public string name;
}

/// <summary>
/// 分组表-航行日志
/// </summary>
public class TaskNormalGroupCfg
{
    //页签顺序
    public int sort;
    //页签配置表
    public int model;
    //页签名称
    public string name;
}

/// <summary>
/// 分组表-任务分类
/// </summary>
public class TaskMainGroupCfg
{
    //页签顺序
    public int sort;
    //页签配置表
    public int model;
    //页签名称
    public string name;
}

//-----------------------------------------------------分组相关>>>end--------------------------------------------


//-----------------------------------------------------任务类别>>>start 该类型表客户端需要的信息基本由后端下发，一般情况下不会解析--------------------------------------------

///

//-----------------------------------------------------任务类别>>>end--------------------------------------------


