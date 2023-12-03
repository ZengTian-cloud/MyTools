using UnityEngine;

public class NpcConfig : Config
{
    /// <summary>
    /// 交互触发类型 1：点击开始交互 2：进入范围自动开始交互
    /// </summary>
    public int interactTriggerType;
    /// <summary>
    /// 初次交互ID
    /// </summary>
    public readonly long firstInteractionId;
    /// <summary>
    /// 默认交互ID
    /// </summary>
    public readonly long defaultInteractionId;
    /// <summary>
    /// 名称
    /// </summary>
    public readonly string name;
    /// <summary>
    /// 称号
    /// </summary>
    public readonly string name1;
    /// <summary>
    /// 预制体路径
    /// </summary>
    public readonly string prefabPath;
    /// <summary>
    /// 位置
    /// </summary>
    public readonly string position;
    /// <summary>
    /// 角度
    /// </summary>
    public readonly string rotation;
    /// <summary>
    /// 可交互距离
    /// </summary>
    public readonly int interoperableDistance;
    /// <summary>
    /// 交互时是否面向玩家
    /// </summary>
    public readonly int isFaceToPlayer;
    /// <summary>
    /// 出现条件
    /// </summary>
    public readonly string showCondition;
    /// <summary>
    /// 出现条件判断类型
    /// </summary>
    public readonly int showCheck;

    private Vector3 _position;
    /// <summary>
    /// 位置
    /// </summary>
    public Vector3 Position
    {
        get
        {
            if (_position.Equals(default))
            {
                string[] pos = position.Split(';');

                _position = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
            }
            return _position;
        }
    }
    private Vector3 _rotation;
    /// <summary>
    /// 角度
    /// </summary>
    public Vector3 Rotation
    {
        get
        {
            if (_rotation.Equals(default))
            {
                string[] rot = rotation.Split(';');

                _rotation = new Vector3(float.Parse(rot[0]), float.Parse(rot[1]), float.Parse(rot[2]));
            }
            return _rotation;
        }
    }
    public bool IsFaceToPlayer
    {
        get => isFaceToPlayer == 1;
    }
}

