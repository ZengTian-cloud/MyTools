using System;
/// <summary>
/// 掉落组配置结构
/// </summary>
public class DropCfgData
{
	public long dropid;//掉落ID

	public int sort;//序号

	public long onlyid;//唯一id；

	public string describe;//备注

	public int type;//类型

	public int group;//每次掉落运算对同dropid下每一个group进行计算

    public int times;//每个grop的计算次数（即进行多少次掉落运算）

	public long pid;//物品id

	public int min;//最小数量

	public int max;//最大数量

	public int weight;//概率权重

	public int shownum;//展示数量 0-不展示  1-展示

	public int label;//展示标签 0-不展示 1-展示

}

