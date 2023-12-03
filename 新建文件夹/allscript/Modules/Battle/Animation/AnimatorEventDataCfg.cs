using System;
using System.Collections.Generic;

/// <summary>
/// 动画事件配置数据
/// </summary>
public class AnimatorEventDataCfg
{
	public AnimatorEventData entrance;//出场

	public AnimatorEventData idle;

	public AnimatorEventData idle_2;//

    public AnimatorEventData move;

    public AnimatorEventData move_2;

    public AnimatorEventData stun;//眩晕

    public AnimatorEventData death;//死亡

    public AnimatorEventData attack_1;//攻击

    public AnimatorEventData attack_2;//攻击

    public AnimatorEventData skill1_1;

    public AnimatorEventData skill1_2;

    public AnimatorEventData skill2_1;

    public AnimatorEventData skill2_2;

    public AnimatorEventData skill3_1;

    public AnimatorEventData skill3_2;

	public AnimatorEventData showidle;

	public AnimatorEventData showloopidle;
}


public class AnimatorEventData
{
	public List<ActNameData> actname;

	public List<EffectNameData> effects;

	public List<long> events;//事件延时 每一次执行一次攻击逻辑

	public List<JumpData> jump;

}

public class ActNameData
{
	public long anilen;//动画长度

	public long logiclen;

	public string name;//动画名字

	public long offset;

	public string tname;

	public long uid;
}

public class EffectNameData
{
	public long uid;

	public V3 angle;

	public double duration;

	public string effabname;

	public string effectname;

	public bool ignore;

	public string name;

	public string point;

	public V3 pos;

	public V3 scale;

	public double time;
}

public class JumpData
{
	public double time1;//开始时间

	public double time2;//到达时间

	public double time3;//回跳时间

	public double time4;//归位时间

	public double dist;

	public double dire;
}


