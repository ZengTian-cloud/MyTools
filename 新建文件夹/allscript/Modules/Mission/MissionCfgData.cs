public class MissionCfgData
{
    public long mission;//关卡id

    public int areaid;//区块id

    public int chapter;//章节id

    public int isplot;//是否是主线(0=否,1=是)

    public int type;//关卡类型:1=战斗关卡 2=剧情 3=隐藏 4=解谜

    public string parent;//父节点1(多个"id1|id2" '|' 分割)

    public string name1;//章节名

    public string name2;//关卡名

    public string name3;//地图代号

    public string position;//节点位置坐标

    public string cost;//消耗组ID

    public long dropid;//掉落组ID（玩法掉落组)

    public long redropid;//重复挑战掉落组（玩法掉落组）

    // 关卡解锁条件1：
    /*
        101=初始解锁
        201=”与“通过a关卡b星解锁（关卡id;星数）
        202=“或”通过a关卡b星解锁（关卡id;星数）
        301=拥有a道具b数量解锁（道具id;数量|道具id;数量）
        302=消耗a道具b数量解锁（道具id;数量|道具id;数量）
        501=剧情选择解锁（剧情选项id）
        601=章节探索度解锁（探索度百分比）
        701=世界等级达到解锁（世界等级）
        801=达成成就解锁
    */
    public int unlock1;//关卡解锁条件1

    public string unlock1param;//关卡解锁参数 (1010010101;-1)

    public int change;//xx关卡解锁后，使该关卡使用次要解锁条件（仅501使用）

    public string changeparam;//关卡解锁参数

    public int unlock2;//次要解锁条件(change != -1时才有数据)

    public string unlock2param;//关卡解锁参数 (1010010101;-1)

    public int ishide;//关卡未解锁时是否显示为问号节点 0=否 1=是

    public int suggestlv;//推荐等级

    public int core;//是否核心关卡

    public string corepicture;//核心关卡图片
}

