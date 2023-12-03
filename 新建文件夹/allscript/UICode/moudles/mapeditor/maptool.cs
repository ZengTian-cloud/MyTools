using System;
using System.Collections.Generic;
using LitJson;

public static class maptool
{

    //地图的json数据转换为c#类数据
    public static MapData MapJsonToClassData(string js)
    {
        return JsonMapper.ToObject<MapData>(js);   
    }
   
}

public class MapData
{
    public string cfgname;
    public List<CellData> maplist;
}

public class CellData
{
    public int state;
    public string mat;
    public V2 index;
    public V3 pos;
    public V3 rot;
}

public class V2
{
    public double x;
    public double y;
}

public class V3
{
    public double x;
    public double y;
    public double z;
}



