using System.Collections.Generic;
using LitJson;
using UnityEngine;

public enum ItemType
{
    // 碎片
    Debris = 1,
    // 礼包
    GiftBag = 2,
    // 装备
    Equipment = 3,
    // 材料
    Materials = 4,
}

public class ItemData
{
    // 索引id(唯一id?)
    public int Id { get; private set; }
    // 数量
    public int Number { get; set; }
    // 配置id
    public long Pid { get; private set; }
    public ItemData(int id, long pid, int number)
    {
        Id = id;
        Number = number;
        Pid = pid;
    }

    public void ChangedNumber(int newNumber)
    {
        Number = newNumber;
    }

    public override string ToString()
    {
        return $"item:Id:{Id}-pid:{Pid}-num:{Number}";
    }
}

/// <summary
/// 仓库物品排序类型
/// </summary>
public enum WarehouseSortType
{
    // 按照id排序-正序
    IDForward = 1,
    // 按照id排序-倒序
    IDInverted = 2,
    // 按照质量排序-正序
    QualityForward = 3,
    // 按照质量排序-倒序
    QualityInverted = 4,
    // 按照类型排序-正序
    TypeForward = 5,
    // 按照类型排序-倒序
    TypeInverted = 6,
    // 按照数量排序-正序
    NumberForward = 7,
    // 按照数量排序-倒序
    NumberInverted = 8,
}

public class WarehouseManager : SingletonNotMono<WarehouseManager>
{
    // 当前排序类型(保存至本地数据)
    private WarehouseSortType currWarehouseSortType = WarehouseSortType.IDForward;
    // 选中的物品标签(当次登陆生效)
    public ItemType currSelectItemTabType = ItemType.Debris;
    // 选中的物品id(当次登陆生效)
    public int currSelectedItemID = 0;

    // 当前仓库数据
    private Dictionary<long, ItemData> datas = new Dictionary<long, ItemData>();

    public Dictionary<long, ItemData> GetDatas()
    {
        return datas;
    }

    /// <summary>
    /// 当物品数量变化
    /// </summary>
    /// <param name="id"></param>
    /// <param name="newNumber"></param>
    public void OnItemNumberChanged(long pid, int newNumber)
    {
        if (datas.ContainsKey(pid))
        {
            if (newNumber == 0)
            {
                OnRemoveItem(pid);
            }
            else
            {
                datas[pid].ChangedNumber(newNumber);
                GameEventMgr.Distribute(GEKey.OnItemNumberChanged, pid, newNumber);
            }
        }
    }

    /// <summary>
    /// 获得物品时提示
    /// </summary>
    public void ShowChangingTip(JsonData changing)
    {
        if (changing.ContainsKey("prop"))
        {
            Debug.Log("!!!!!!!!!!!!!");
            JsonData props = changing["prop"];
            GameCenter.mIns.m_UIMgr.Open<showiteminfo>(props);
            foreach (JsonData item in props)
            {
                long pid = long.Parse(item["pid"].ToString());
                int num = int.Parse(item["num"].ToString());
                ItemCfgData cfg = GameCenter.mIns.m_CfgMgr.GetItemCfgData(pid);
                GameCenter.mIns.m_UIMgr.PopMsg($"获得物品[{GameCenter.mIns.m_LanMgr.GetLan(cfg.name)}]*{num}");
            }
        }
    }

    /// <summary>
    /// 仓库新加入一个物品
    /// </summary>
    /// <param name="jsonData"></param>
    /// <param name="isInit"></param>
    public void OnAddItem(JsonData jsonData, bool isInit = false)
    {
        if (jsonData != null)
        {
            OnAddItem(int.Parse(jsonData["id"].ToString()), long.Parse(jsonData["pid"].ToString()), int.Parse(jsonData["num"].ToString()), isInit);
        }
    }

    /// <summary>
    /// 仓库新加入一个物品
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pid"></param>
    /// <param name="number"></param>
    /// <param name="isInit"></param>
    public void OnAddItem(int id, long pid, int number, bool isInit = false)
    {
        if (!datas.ContainsKey(pid))
        {
            ItemData itemData = new ItemData(id, pid, number);
            datas.Add(pid, itemData);
            if (!isInit)
            {
                GameEventMgr.Distribute(GEKey.OnItemAdd, itemData);
            }
        }
        else {
            ItemData itemData = GetItemData(pid);
            itemData.Number = number;
        }

        ItemCfgData itemCfgData = GameCenter.mIns.m_CfgMgr.GetItemCfgData(pid);
    }

    /// <summary>
    /// 从仓库移除一个物品
    /// </summary>
    /// <param name="pid"></param>
    public void OnRemoveItem(long pid)
    {
        if (datas.ContainsKey(pid))
        {
            datas.Remove(pid);
            GameEventMgr.Distribute(GEKey.OnItemRemove, pid);
        }
    }

    /// <summary>
    /// 获取一个item
    /// </summary>
    /// <param name="pid"></param>
    /// <returns></returns>
    public ItemData GetItemData(long pid)
    {
        if (datas.ContainsKey(pid))
        {
            return datas[pid];
        }
        return null;
    }

    /// <summary>
    /// 获取一个item
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ItemData GetItemDataById(int id)
    {
        foreach (var item in datas)
        {
            if (item.Value.Id == id)
            {
                return item.Value;
            }
        }
        return null;
    }


    /// <summary>
    /// 是否存在item
    /// </summary>
    /// <param name="pid"></param>
    /// <returns></returns>
    public bool HasItem(long pid)
    {
        return GetItemData(pid) != null;
    }

    /// <summary>
    /// 获取item数量
    /// </summary>
    /// <param name="pid"></param>
    /// <returns></returns>
    public int GetItemNumber(long pid)
    {
        ItemData itemData = GetItemData(pid);
        return (itemData == null) ? 0 : itemData.Number;
    }

    /// <summary>
    /// 物品数量是否充足
    /// </summary>
    /// <param name="pid"></param>
    /// <param name="compareNumber"></param>
    /// <returns></returns>
    public bool HasEnoughItem(long pid, int compareNumber)
    {
        return GetItemNumber(pid) >= compareNumber;
    }

    /// <summary>
    /// 修改排序类型
    /// </summary>
    /// <param name="wareHoseSortType"></param>
    /// <returns></returns>
    public bool ChangedSortType(WarehouseSortType warehouseSortType)
    {
        if (currWarehouseSortType == warehouseSortType)
        {
            return false;
        }

        // todo:save to local data

        currWarehouseSortType = warehouseSortType;
        return true;
    }

    /// <summary>
    /// 排序
    /// </summary>
    private void Sort(WarehouseSortType warehouseSortType)
    {
        // todo
    }


    /// <summary>
    /// 通过品质获取相关数据
    /// </summary>
    /// <param name="quality">品质</param>
    /// <param name="stype">类型(select:选中框, detailbg:详情背景, itemframe:物品背景框)</param>
    /// <returns></returns>
    public string GetItemParamByQuality(int quality, ItemParamByQualityType ipqtype)
    {
        string retstr = "";
        switch (quality)
        {
            case 1:
            default:
                retstr = ipqtype == ItemParamByQualityType.Select ? "Cy_tag_xuanzhong_blue" :
                    (ipqtype == ItemParamByQualityType.DetailBg ? "ui_pack_pnl_pinzhi_l" :
                    (ipqtype == ItemParamByQualityType.LongDetailBg ? "ui_c_pnl_tishi_lv":
                    "ui_c_pnl_wupin_l"));
                break;
            case 2:
                retstr = ipqtype == ItemParamByQualityType.Select ? "Cy_tag_xuanzhong_purple" :
                    (ipqtype == ItemParamByQualityType.DetailBg ? "ui_pack_pnl_pinzhi_z" :
                    (ipqtype == ItemParamByQualityType.LongDetailBg ? "ui_c_pnl_tishi_lv":
                    "ui_c_pnl_wupin_z"));
                break;
            case 3:
                retstr = ipqtype == ItemParamByQualityType.Select ? "Cy_tag_xuanzhong_blue" :
                    (ipqtype == ItemParamByQualityType.DetailBg ? "ui_pack_pnl_pinzhi_l" :
                    (ipqtype == ItemParamByQualityType.LongDetailBg ? "ui_c_pnl_tishi_lan":
                    "ui_c_pnl_wupin_l"));
                break;
            case 4:
                retstr = ipqtype == ItemParamByQualityType.Select ? "Cy_tag_xuanzhong_purple" :
                    (ipqtype == ItemParamByQualityType.DetailBg ? "ui_pack_pnl_pinzhi_z" :
                    (ipqtype == ItemParamByQualityType.LongDetailBg ? "ui_c_pnl_tishi_zi":
                    "ui_c_pnl_wupin_z"));
                break;
            case 5:
            retstr = ipqtype == ItemParamByQualityType.Select ? "Cy_tag_xuanzhong_yellow" :
                    (ipqtype == ItemParamByQualityType.DetailBg ? "ui_pack_pnl_pinzhi_h" :
                    (ipqtype == ItemParamByQualityType.LongDetailBg ? "ui_c_pnl_tishi_cheng":
                    "ui_c_pnl_wupin_h"));
                break;
            case 6:
                retstr = ipqtype == ItemParamByQualityType.Select ? "Cy_tag_xuanzhong_red" :
                    (ipqtype == ItemParamByQualityType.DetailBg ? "ui_pack_pnl_pinzhi_hs" :
                    (ipqtype == ItemParamByQualityType.LongDetailBg ? "ui_c_pnl_tishi_hong":
                    "ui_c_pnl_wupin_hs"));
                break;
        }
        return retstr;
    }
}

/// <summary>
/// 通过品质获取相关数据
/// </summary>
public enum ItemParamByQualityType
{
    Select,
    DetailBg,
    ItemFrame,
    LongDetailBg
}
