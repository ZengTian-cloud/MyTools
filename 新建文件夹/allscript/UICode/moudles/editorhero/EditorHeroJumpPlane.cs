using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EditorHeroJumpPlane : MonoBehaviour
{
    public Transform root;

    public Button btnJumpPlane;
    public GameObject jumpPlaneContent;
    public TMP_InputField txTime1;
    public TMP_InputField txTime2;
    public TMP_InputField txTime3;
    public TMP_InputField txTime4;
    public TMP_InputField txDist;
    public TMP_InputField txDire;
    public Button btnPlayJump;

    public GameObject coordObj;
    public Button btnCoord;
    public EditorHeroJumpData currData;
    Action<EditorHeroJumpData> action;
    EditorHero editorHero;

    public Button btnSave;
    public Button btnClear;
    public Button btnChooseAnim;
    public TMP_Text txCurrSetAnim;
    public string currSetAnim;
    public void Init(EditorHero editorHero)
    {
        this.editorHero = editorHero;
        btnJumpPlane.onClick.RemoveAllListeners();
        btnJumpPlane.onClick.AddListener(() =>
        {
            if (editorHero == null || editorHero.curEHD == null)
            {
                GameCenter.mIns.m_UIMgr.PopMsg("请先选择英雄!");
                return;
            }

            if (jumpPlaneContent.activeSelf)
            {
                // SetData();
                // GameCenter.mIns.m_UIMgr.PopMsg("数据已保存!");
                SetActive(false);
            }
            else
            {
                SetActive(true, currData, (newdata) =>
                {
                    currData = newdata;
                });
                this.action?.Invoke(currData);
            }
        });


        btnPlayJump.onClick.RemoveAllListeners();
        btnPlayJump.onClick.AddListener(() =>
        {
            // SetData();
            // GameCenter.mIns.m_UIMgr.PopMsg("数据已保存!");
            // play!
            StartJump();
            if (!string.IsNullOrEmpty(currSetAnim))
            {
                List<string> playAnimNames = editorHero.curEHD.GetTheAnimPlayedAnimNames(currSetAnim);
                string pa = "";
                if (playAnimNames != null && playAnimNames.Count > 0)
                {
                    pa = playAnimNames[0];
                }
                editorHero.PlayAnim(currSetAnim, pa);
            }
        });

        btnCoord.onClick.RemoveAllListeners();
        btnCoord.onClick.AddListener(() =>
        {
            if (coordObj != null)
            {
                coordObj.SetActive(!coordObj.activeSelf);
            }
        });

        btnSave.onClick.RemoveAllListeners();
        btnSave.onClick.AddListener(() =>
        {
            if (string.IsNullOrEmpty(currSetAnim))
            {
                GameCenter.mIns.m_UIMgr.PopMsg("请先选择一个动画!");
                return;
            }
            SetData();
            GameCenter.mIns.m_UIMgr.PopMsg("数据保存成功!");
        });

        btnClear.onClick.RemoveAllListeners();
        btnClear.onClick.AddListener(() =>
        {
            if (string.IsNullOrEmpty(currSetAnim))
            {
                GameCenter.mIns.m_UIMgr.PopMsg("请先选择一个动画!");
                return;
            }
            SetData(true);
            SetUI(null);
            GameCenter.mIns.m_UIMgr.PopMsg("数据清除成功!");
        });

        btnChooseAnim.onClick.RemoveAllListeners();
        btnChooseAnim.onClick.AddListener(() =>
        {
            if (editorHero != null)
            {
                editorHero.PopList(true, "jumpplane", (animName) => {
                    Debug.Log("`` currSetAnim:" + currSetAnim + " - animName:" + animName);
                    if (string.IsNullOrEmpty(animName))
                    {
                        return;
                    }
                    if (editorHero != null && editorHero.curEHD != null)
                    {
                        EditorHeroActionData editorHeroActionData = editorHero.curEHD.GetAnimDataByName(animName);
                        //Debug.Log("~~ editorHeroActionData:" + editorHeroActionData.ToString());
                        if (editorHeroActionData != null && editorHeroActionData.editorHeroActionAnimListDatas != null && editorHeroActionData.editorHeroActionAnimListDatas.Count > 0)
                        {
                            currSetAnim = animName;
                            txCurrSetAnim.text = "当前选中动画:" + animName;
                            SetUI(editorHeroActionData.jumpData);
                            currData = editorHeroActionData.jumpData;
                        }
                        else
                        {
                            GameCenter.mIns.m_UIMgr.PopMsg("当前选中的动画没有编辑绑定对应动画!");
                        }
                    }
                    else
                    {
                        GameCenter.mIns.m_UIMgr.PopMsg("选中失败，数据错误!");
                    }
                });
            }
        });
        currSetAnim = "";
        txCurrSetAnim.text = "当前选中动画:";
    }

    public void SetActive(bool b, EditorHeroJumpData editorHeroJumpData = null, Action<EditorHeroJumpData> action = null)
    {
        if (!b)
        {
            jumpPlaneContent.gameObject.SetActive(b);
            return;
        }

        this.action = action;
        if (editorHero == null)
        {
            return;
        }
        if (editorHero.curEHD != null)
        {
            heroTran = editorHero.curEHD.animator.transform;
            heroOriPos = heroTran.position;
        }

        currData = editorHeroJumpData;
        SetUI(currData);
        jumpPlaneContent.gameObject.SetActive(b);
    }

    public void SetUI(EditorHeroJumpData data)
    {
        txTime1.text = data == null ? "0" : data.timeEnter1.ToString();
        txTime2.text = data == null ? "0" : data.timeEnter2.ToString();
        txTime3.text = data == null ? "0" : data.timeLevae1.ToString();
        txTime4.text = data == null ? "0" : data.timeLevae2.ToString();
        txDist.text = data == null ? "0" : data.dist.ToString();
        txDire.text = data == null ? "0" : data.dire.ToString();

        if (currData == null)
        {
            currData = new EditorHeroJumpData();
        }
    }

    private void SetData(bool isClear = false)
    {
        if (currData == null)
            currData = new EditorHeroJumpData();

        currData.timeEnter1 = float.Parse(txTime1.text);
        currData.timeEnter2 = float.Parse(txTime2.text);
        currData.timeLevae1 = float.Parse(txTime3.text);
        currData.timeLevae2 = float.Parse(txTime4.text);
        currData.dist = float.Parse(txDist.text);
        currData.dire = float.Parse(txDire.text);

        if (editorHero != null && editorHero.curEHD != null && !string.IsNullOrEmpty(currSetAnim))
        {
            editorHero.curEHD.GetAnimDataByName(currSetAnim).UpdateJumpData(currSetAnim, isClear ? null : currData);
        }
    }
     
    Vector3 heroOriPos;
    Transform heroTran;
    bool bStart = false;
    float timer = 0;
    int state = 0; // 0=enter , 1=leave
    Vector3 jumpStartPosition;
    Vector3 jumpEndPosition;
    float moveSpeed;
    public void Update()
    {
        if (heroTran == null)
            return;

        if (currData == null)
        {
            EndJump();
            return;
        }

        if (bStart)
        {
            timer += Time.deltaTime;
            if (state == 0)
            {
                // enter
                if (timer >= currData.timeEnter1 * 0.001)
                {
                    // 开始移动
                    heroTran.position = Vector3.MoveTowards(heroTran.position, jumpEndPosition, Time.deltaTime * moveSpeed);
                }
                if (timer >= currData.timeEnter2 * 0.001)
                {
                    Debug.Log(string.Format("``end enter re dist :{0}, useTime:{1}", Vector3.Distance(heroTran.position, jumpEndPosition), timer));
                    state = 1;
                    // 速度计算
                    float leaveTime = 0.0001f;
                    if (currData.timeLevae2 > currData.timeLevae1 && currData.timeLevae1 > currData.timeEnter2)
                    {
                        leaveTime = (currData.timeLevae2 - currData.timeLevae1) * 0.001f;
                    }
                    moveSpeed = currData.dist / leaveTime;
                }
            }
            else if (state == 1)
            {
                // leave
                if (timer >= currData.timeLevae1 * 0.001)
                {
                    // 开始移动
                    //heroTran.position = Vector3.MoveTowards(heroTran.position, jumpStartPosition, Time.deltaTime * moveSpeed);
                    heroTran.position = Vector3.MoveTowards(heroTran.position, jumpStartPosition, Time.deltaTime * moveSpeed);
                    if (moveSpeed >= 1000)
                    {
                        heroTran.position = jumpStartPosition;
                    }
                }
                if (timer >= currData.timeLevae2 * 0.001)
                {
                    Debug.Log(string.Format("``end leave re dist :{0}, useTime:{1}, levae speed:{2}", Vector3.Distance(heroTran.position, jumpStartPosition), timer, moveSpeed));
                    EndJump();
                }
            }
        }
    }

    private void StartJump()
    {
        if (string.IsNullOrEmpty(currSetAnim))
        {
            GameCenter.mIns.m_UIMgr.PopMsg("请先选择一个动画!");
            return;
        }

        if (currData.timeEnter2 == 0 || currData.timeEnter2 <= currData.timeEnter1)
        {
            GameCenter.mIns.m_UIMgr.PopMsg("请输入正确的时间(time1,time2)!");
            return;
        }

        if (currData.dist <= 0)
        {
            GameCenter.mIns.m_UIMgr.PopMsg("请输入正确的距离!");
            return;
        }
        heroTran.position = heroOriPos;

        // 起始点
        jumpStartPosition = heroOriPos;

        // 计算结束点
        // dire:x轴正方向为0度，逆时针增加
        float dire = currData.dire;
        jumpEndPosition = new Vector3(heroOriPos.x + currData.dist, heroOriPos.y, heroOriPos.z);
        Debug.Log(" 1 - jumpStartPosition: " + jumpStartPosition + " - jumpEndPosition:" + jumpEndPosition);
        if (dire > 0)
        {
            Vector2 heroOriPos_xz = new Vector2(heroOriPos.x, heroOriPos.z);
            Vector2 jumpEndPosition_xz = new Vector2(jumpEndPosition.x, jumpEndPosition.z);
            Vector2 rotatePoint_xz = GetRotatePosition(jumpEndPosition_xz, heroOriPos_xz, currData.dire);
            jumpEndPosition = new Vector3(rotatePoint_xz.x, jumpEndPosition.y, rotatePoint_xz.y);
        }
        heroTran.position = jumpStartPosition;
        Debug.Log(" 2 - jumpStartPosition: " + jumpStartPosition + " - jumpEndPosition:" + jumpEndPosition);

        // 速度计算
        float enterTime = (currData.timeEnter2 - currData.timeEnter1) * 0.001f;
        moveSpeed = currData.dist / enterTime;
        Debug.Log(string.Format("`` start move: dist:{0}, dire:{1}, time:{2}, speed:{3}, jumpStartPosition:{4}, jumpEndPosition:{5}", currData.dist, currData.dire, enterTime, moveSpeed, jumpStartPosition, jumpEndPosition));

        bStart = true;
        timer = 0;
        state = 0;
    }

    public static Vector2 GetRotatePosition(Vector2 tp, Vector2 cp, float angele)
    {
        float ex = (tp.x - cp.x) * Mathf.Cos(angele * Mathf.Deg2Rad) -
                     (tp.y - cp.y) * Mathf.Sin(angele * Mathf.Deg2Rad) + cp.x;
        float ey = (tp.y - cp.y) * Mathf.Cos(angele * Mathf.Deg2Rad) +
                     (tp.x - cp.x) * Mathf.Sin(angele * Mathf.Deg2Rad) + cp.y;
        return new Vector2(ex, ey);
    }

    private void EndJump()
    {
        if (heroTran == null)
            return;
        heroTran.position = heroOriPos;
        bStart = false;
        timer = 0;
        state = 0;
    }
}

public class EditorHeroJumpData
{
    public float timeEnter1;
    public float timeEnter2;
    public float timeLevae1;
    public float timeLevae2;
    public float dist;
    public float dire;
}

