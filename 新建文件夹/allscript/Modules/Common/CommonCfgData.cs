using System;
using System.Collections.Generic;

/// <summary>
/// 常量表配置
/// </summary>
public class CommonCfgData
{
	public string note;//常量名

	public string constant;//常量译名

	public string value;//参数
}
public class CostCfgData
{
    public long costid;
	public string describe;
	public int type;
	public int pid1;
	public int pid2;
	public int pid3;
	public int pid4;
	public int pid5;
	public int num1;
	public int num2;
	public int num3;
	public int num4;
	public int num5;

	public List<CostData> getCosts()
	{
        List<CostData> list = new List<CostData>();
		if (pid1 > 0) {
            CostData data = new CostData();
			data.propid = pid1;
			data.count = num1;
            list.Add(data);
        }
        if (pid2 > 0)
        {
            CostData data = new CostData();
            data.propid = pid2;
            data.count = num2;
            list.Add(data);
        }
        if (pid3 > 0)
        {
            CostData data = new CostData();
            data.propid = pid3;
            data.count = num3;
            list.Add(data);
        }
        if (pid4 > 0)
        {
            CostData data = new CostData();
            data.propid = pid4;
            data.count = num4;
            list.Add(data);
        }
        if (pid5 > 0)
        {
            CostData data = new CostData();
            data.propid = pid5;
            data.count = num5;
            list.Add(data);
        }
        return list;
    }
}




