using System;
using System.Collections.Generic;
using UnityEngine;

public class FBXBindBonesHelper : MonoBehaviour
{
    /*
     "body":"niya_1_body",
    "point_ass":"point_ass",
    "point_backside":"point_backside",
    "point_foot":"point_foot",
    "foot":"point_foot",
    "point_head":"point_head",
    "head":"point_headtop",
    "point_headtop":"point_headtop",
    "point_lefthand":"point_lefthand",
    "point_righthand":"point_righthand",
    */
    private Transform root;
    [SerializeField]
    public Transform Body;
    [SerializeField]
    public Transform Point_ass;
    [SerializeField]
    public Transform Point_backside;
    [SerializeField]
    public Transform Point_foot;
    [SerializeField]
    public Transform Point_head;
    [SerializeField]
    public Transform Point_headtop;
    [SerializeField]
    public Transform Point_lefthand;
    [SerializeField]
    public Transform Point_righthand;
    [SerializeField]
    public Transform Point_weapon;

    private Dictionary<FBXBoneType, Transform> bones = new Dictionary<FBXBoneType, Transform>();
    public Dictionary<FBXBoneType, Transform> Bones { get { return bones; } }
    private void Start()
    {
        //Bind();
        //foreach (var item in bones)
        //{
        //    Debug.LogError(" type:" + item.Key + " - val:" + item.Value);
        //}
    }

    public void Bind()
    {
        root = transform;
        Transform[] childs = GetComponentsInChildren<Transform>();
        foreach (Transform child in childs)
        {
            if (child.name.Contains("point"))
            {
                foreach (FBXBoneType fbxBoneType in Enum.GetValues(typeof(FBXBoneType)))
                {
                    if (fbxBoneType.ToString().Equals(child.name))
                    {
                        if (!bones.ContainsKey(fbxBoneType))
                        {
                            bones.Add(fbxBoneType, child);
                            SetTran(fbxBoneType, child);
                        }
                        else
                        {
                            Debug.LogError("add same child:" + child.name);
                        }
                    }
                }
            }
        }
    }

    private void SetTran(FBXBoneType fbxBoneType, Transform tran)
    {
        switch (fbxBoneType)
        {
            case FBXBoneType.body: Body = tran; break;
            case FBXBoneType.point_ass: Point_ass = tran; break;
            case FBXBoneType.point_backside: Point_backside = tran; break;
            case FBXBoneType.point_foot: Point_foot = tran; break;
            case FBXBoneType.point_head: Point_head = tran; break;
            case FBXBoneType.point_headtop: Point_headtop = tran; break;
            case FBXBoneType.point_lefthand: Point_lefthand = tran; break;
            case FBXBoneType.point_righthand: Point_righthand = tran; break;
            case FBXBoneType.point_weapon: Point_weapon = tran;break;
        }
    }

    public Transform GetBone(FBXBoneType fbxBoneType)
    {
        if (bones == null || bones.Count <=0)
        {
            Bind();
        }
        return bones.ContainsKey(fbxBoneType) ? bones[fbxBoneType] : root;
    }

    public Transform GetBoneByString(string type)
    {
       return GetBone((FBXBoneType)Enum.Parse(typeof(FBXBoneType), type));
    }
}

public enum FBXBoneType
{
    body,
    point_ass,
    point_backside,
    point_foot,
    point_head,
    point_headtop,
    point_lefthand,
    point_righthand,
    point_weapon,
}
