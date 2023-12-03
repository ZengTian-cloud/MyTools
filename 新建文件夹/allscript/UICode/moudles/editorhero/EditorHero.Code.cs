using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class EditorHero
{
    #region 定义和生命周期
    public override UILayerType uiLayerType => UILayerType.Normal;
    public override string uiAtlasName => "";
 
    protected override void OnInit()
    {
        // 添加相机
        GameCenter.mIns.m_CamMgr.AddCameraToMainCamera(camera);
        //检测热更
        GameCenter.mIns.m_HttpMgr.SendData("POST", 30, "", "", (state, content, val) => {

        });
        //InstHero(101006);
        SetUI();
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        // 注册update
        GameCenter.mIns.m_UIMgr.AddUpdateWin(this);
    }


    public override void UpdateWin()
    {
        RayModel();
        CameraCtrl();
        if (monoScript_editorHeroPopPlane != null)
        {
            monoScript_editorHeroPopPlane.Update();
        }
        if (monoScript_editorHeroEffectPopPlane != null)
        {
            monoScript_editorHeroEffectPopPlane.Update();
        }
        UpdateEffect();
    }

    protected override void OnClose()
    {
        base.OnClose();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
    #endregion

    #region UI
    public EditorHeroEffectPopData copyEffectPopData;

    public void PopList(bool b, string sType, Action<string> action)
    {
        if (monoScript_editorHeroPopList != null)
        {
            monoScript_editorHeroPopList.SetActive(b, sType, this, action);
        }
    }

    partial void OnClickClose()
    {
        this.Close();
    }


    partial void OnClickSaveEvent() {
        if (curEHD != null)
        {
            Debug.Log("~~ OnClickSaveEvent curEHD: " + curEHD.id);
            EditorHeroHelper.HeroActionEventDataToJsonData(curEHD);//, curEHD.animator.runtimeAnimatorController.animationClips);
        }
        else
        {
            GameCenter.mIns.m_UIMgr.PopMsg("请先选择英雄!");
        }
    }

    partial void OnClickSavePoint() {
        if (curEHD != null)
        {
            Debug.Log("~~ OnClickSaveEvent curEHD: " + curEHD.id);
            EditorHeroHelper.HeroActionPointToJsonData(curEHD.animator.transform, curEHD.id);
        }
        else
        {
            GameCenter.mIns.m_UIMgr.PopMsg("请先选择英雄!");
        }
    }

    private bool apHide = true;
    partial void OnClickMovePlane()
    {
        if (apHide)
        {
            animPlane.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100);
        }
        else
        {
            animPlane.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(600, -100);
        }
        apHide = !apHide;
    }

    // init ui
    public void SetUI()
    {
        ap_animItem.gameObject.SetActive(false);
        animPlane.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(600, -100);
        List<int> heroIds = new List<int>();
        heroIds.Add(0);
        string[] paths = Directory.GetDirectories(pathtool.loadrespath + "/allmodel/zroles/heros");
        foreach (var p in paths)
        {
            //Debug.Log("`` --------------------------:" + p);
            //p.LastIndexOf("/role");
            //Debug.Log("`` ss:" + p.LastIndexOf("role"));
            //Debug.Log("`` len:" + (p.Length - p.LastIndexOf("role")));
            int si = p.LastIndexOf("role");
            if (si < p.Length)
            {
                int l = p.Length - si;
                if (l + si <= p.Length)
                {
                    string s = p.Substring(si, l);
                    int heroId = int.Parse(s.Split(new char[] { '_' })[1]);
                    //Debug.Log("`` s:" + s + " - heroId:" + heroId);
                    heroIds.Add(heroId);
                }
            }
        }

        string[] monster_paths = Directory.GetDirectories(pathtool.loadrespath + "/allmodel/zroles/monster");
        foreach (var p in monster_paths)
        {
            //Debug.Log("`` --------------------------:" + p);
            //p.LastIndexOf("/role");
            //Debug.Log("`` ss:" + p.LastIndexOf("role"));
            //Debug.Log("`` len:" + (p.Length - p.LastIndexOf("role")));
            int si = p.LastIndexOf("role");
            if (si < p.Length)
            {
                int l = p.Length - si;
                if (l + si <= p.Length)
                {
                    string s = p.Substring(si, l);
                    int heroId = int.Parse(s.Split(new char[] { '_' })[1]);
                    //Debug.Log("`` s:" + s + " - heroId:" + heroId);
                    heroIds.Add(heroId);
                }
            }
        }

        ap_dropdown.ClearOptions();
        foreach (var id in heroIds)
        {
            TMP_Dropdown.OptionData op = new TMP_Dropdown.OptionData();
            op.text = id.ToString();
            ap_dropdown.options.Add(op);
        }

        ap_dropdown.onValueChanged.AddListener((index)=> {
            //Debug.Log("index:" + index);
            int selectHeroId = heroIds[index];
            if (selectHeroId > 0)
            {
                InstHero(selectHeroId);
                InitEHListItem();
            }
        });

        if (monoScript_editorHeroJumpPlane != null)
        {
            monoScript_editorHeroJumpPlane.Init(this);
        }
    }

    // 英雄选中下拉框


    // 英雄的动画ui列表
    private List<EHListItem> ehListItems = new List<EHListItem>();
    private void InitEHListItem()
    {
        ap_animItem.gameObject.SetActive(false);
        if (curEHD == null)
        {
            ehListItems = new List<EHListItem>();
            return;
        }

        ResetEHList();

        // AnimationClip[] animcs = curEHD.animator.runtimeAnimatorController.animationClips;
        // // 自带动画
        // foreach (var anim in animcs)
        // {
        //     GameObject o = GameObject.Instantiate(ap_animItem.gameObject);
        //     o.name = curEHD.id + "_" + anim.name;
        //     Debug.Log("anim clip:" + anim.name);
        //     EHListItem eHListItem = new EHListItem(o.transform, anim.name, this);

        //     ehListItems.Add(eHListItem);
        //     o.transform.SetParent(ap_animItem.parent);
        //     o.transform.localScale = Vector3.one;
        //     o.SetActive(true);
        // }

        // 现在使用固定状态
        /*
            入场：entrance
            待机：idle
            待机2：idle_2
            移动：move
            移动2：move_2
            眩晕：stun
            死亡：death
            普攻：attack_1
            普攻2：attack_2
            战技1：skill1_1
            战技2：skill1_2
            秘技1：skill2_1
            秘技2：skill2_2
            终结技1：skill3_1
            终结技2：skill3_2
            养成界面展示动作：showidle
            养成界面展示动作：showloopidle
        */

        List<string> extAct = new List<string>() { 
            "entrance", "idle", "idle_2", "move", "move_2", "stun", "death", 
            "attack_1", "attack_2", "skill1_1", "skill1_2", "skill2_1", "skill2_2", "skill3_1", "skill3_2","showidle","showloopidle"};
        foreach (var name in extAct)
        {
            GameObject o = GameObject.Instantiate(ap_animItem.gameObject);
            o.name = curEHD.id + "_" + name;
            EHListItem eHListItem = new EHListItem(o.transform, name, this);

            ehListItems.Add(eHListItem);
            o.transform.SetParent(ap_animItem.parent);
            o.transform.localScale = Vector3.one;
            o.SetActive(true);
        }
    }

    private void ResetEHList()
    {
        if (ehListItems.Count > 0)
        {
            foreach (var item in ehListItems)
            {
                item.OnDestroy();
            }
            ehListItems = new List<EHListItem>();
        }
    }

    public void UpdateEHListItem(string animName)
    {
        foreach (var item in ehListItems)
        {
            if (item.animName == animName)
            {
                item.SetBtnAnimSetTx();
            }
        }
    }




    #endregion

    #region Model

    private Dictionary<int, EditorHeroData> cacheInstHeros = new Dictionary<int, EditorHeroData>();
    public EditorHeroData curEHD = null;
    private async void InstHero(int heroId)
    {
        if (curEHD != null)
        {
            if (curEHD.id != heroId)
            {
                SetFromCache(curEHD);
                curEHD = null;
            }
            else
            {
                return;
            }
        }

        EditorHeroData editorHeroData = GetFromCache(heroId);
        if (editorHeroData == null)
        {
            GameObject heroObj = heroId.ToString().StartsWith("5") ? await ResourcesManager.Instance.LoadPrefabSync("role", "role_" + heroId.ToString()) : await ResourcesManager.Instance.LoadPrefabSync("role", "role_" + heroId.ToString());
            editorHeroData = new EditorHeroData(heroId, heroObj);
        }
        curEHD = editorHeroData;
        InitHeroObjParam(curEHD);
    }


    private EditorHeroData GetFromCache(int heroId)
    {
        EditorHeroData editorHeroData = null;
        bool has = cacheInstHeros.TryGetValue(heroId, out editorHeroData);
        if (has)
        {
            cacheInstHeros.Remove(heroId);
        }
        return editorHeroData;
    }

    private void SetFromCache(EditorHeroData editorHeroData)
    {
        if (cacheInstHeros.ContainsKey(editorHeroData.id))
        {
            return;
        }
        cacheInstHeros.Add(editorHeroData.id, editorHeroData);
        editorHeroData.obj.transform.SetParent(cacheNode);
    }

    private void InitHeroObjParam(EditorHeroData editorHeroData)
    {
        if (editorHeroData == null)
        {
            return;
        }
        Debug.Log("`` editorHeroData.obj:" + editorHeroData.obj);
        Debug.Log("`` heroNode.obj:" + heroNode);
        editorHeroData.obj.transform.SetParent(heroNode);
        editorHeroData.obj.transform.localScale = Vector3.one;
        editorHeroData.obj.transform.localPosition = Vector3.zero;
        editorHeroData.obj.transform.localRotation = Quaternion.Euler(0, 180, 0);

        PlayAnim();
    }

    public void PlayAnim(string animAtLabelName = "", string animName = "")
    {
        if (curEHD == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(animName))
        {
            curEHD.animator.Play("loopidle", 0, 0);
        }
        else
        {
            curEHD.animator.Play(animName, 0, 0);
        }
        Debug.LogWarning("`` PlayAnim animName:" + animName + " - animAtLabelName:" + animAtLabelName);
        PlayEffect(animAtLabelName);
    }

    private float rotationSpeed = 1000;
    private void RayModel()
    {
        if (curEHD == null)
        {
            return;
        }

        if (Input.GetMouseButton(0))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                Debug.DrawLine(ray.origin, hitInfo.point);
                GameObject gameObj = hitInfo.collider.gameObject;
                if (gameObj.name == "heroNode")
                {
                    // Debug.Log("click object name is " + gameObj.name);
                    float axisX = Input.GetAxis("Mouse X");
                    Vector3 currEuler = curEHD.obj.transform.localRotation.eulerAngles;
                    currEuler.y -= axisX * rotationSpeed * Time.deltaTime;
                    curEHD.obj.transform.localRotation = Quaternion.Euler(currEuler);
                    //Debug.Log("axisX:" + axisX + " - axisY:" + axisY);
                    //float 
                }
            }
        }
    }

    #endregion

    #region Camera Ctrl
    private float cameraRotationSpeed = 70;
    private float cameraMoveSpeed = 5;
    private float camerZoomSpeed = 20;
    float mouseMoveX;
    float mouseMoveY;
    float toTargetDist = 7;
    float toTargetHeight = 2;
    Transform followTarget;
    bool cameraInit = false;
    private void CameraCtrl()
    {
        if (followTarget == null)
        {
            followTarget = cameraLook;
        }

        if (!cameraInit)
        {
            DoCamera(true);
            cameraInit = !cameraInit;
        }
        //if (Input.GetMouseButton(1))
        //{
        //    mouseMoveX += (Input.GetAxis("Mouse X") * rotationSpeed);
        //    mouseMoveY -= (Input.GetAxis("Mouse Y") * rotationSpeed);
        //    tempQuat = Quaternion.Euler(mouseMoveY, mouseMoveX, 0);
        //    transform.rotation = tempQuat;
        //}
        //var negDistance = new Vector3(0.0f, 0.0f, -toTargetDist);
        //transform.position = transform.rotation * negDistance;
        //// 防抖动
        //transform.position += (followTarget.position + Vector3.up * toTargetHeight);
        DoCamera();
    }

    private void DoCamera(bool force = false)
    {
        //float zoomAmount = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetMouseButton(1) || force) // || zoomAmount != 0 || force)
        {
            float zoomAmount = 0;
            if (Input.GetKey(KeyCode.W))
            {
                zoomAmount = camerZoomSpeed * Time.deltaTime;
                toTargetDist -= zoomAmount;
            }
            if (Input.GetKey(KeyCode.S))
            {
                zoomAmount = camerZoomSpeed * Time.deltaTime;
                toTargetDist += zoomAmount;
            }
            //if (zoomAmount != 0)
            //{
            //    //camera.fieldOfView -= zoomAmount * camerZoomSpeed * Time.deltaTime;
            //    toTargetDist -= zoomAmount * camerZoomSpeed * Time.deltaTime;
            //}

            float axisX = Input.GetAxis("Mouse X");
            float axisY = Input.GetAxis("Mouse Y");
            // x+ == up, x- == down, y- == left, y+ == right
            //Vector3 cr = camera.transform.localRotation.eulerAngles;
            //cr.y += axisX * cameraRotationSpeed * Time.deltaTime;
            //cr.x -= axisY * cameraRotationSpeed * Time.deltaTime;
            //cr.z = 0;
            //camera.transform.localRotation = Quaternion.Euler(cr);

            mouseMoveX += (axisX * cameraRotationSpeed);
            mouseMoveY -= (axisY * cameraRotationSpeed);
            camera.transform.rotation = Quaternion.Euler(mouseMoveY, mouseMoveX, 0);
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -toTargetDist);
            camera.transform.position = camera.transform.rotation * negDistance;
            // 防抖动
            camera.transform.position += (followTarget.position + Vector3.up * toTargetHeight);

            //float moveDist = cameraMoveSpeed * Time.deltaTime;
            //if (Input.GetKey(KeyCode.W))
            //{
            //    camera.transform.localPosition = camera.transform.forward * moveDist + camera.transform.localPosition;
            //}
            //else if (Input.GetKey(KeyCode.S))
            //{
            //    camera.transform.localPosition = camera.transform.forward * -moveDist + camera.transform.localPosition;
            //}
            //else if (Input.GetKey(KeyCode.A))
            //{
            //    camera.transform.localPosition = Vector3.left * moveDist + camera.transform.localPosition;
            //}
            //else if (Input.GetKey(KeyCode.D))
            //{
            //    camera.transform.localPosition = Vector3.right * moveDist + camera.transform.localPosition;
            //}
        }
    }
    #endregion


    #region PlayEffect

    private class PE
    {
        public int heroId;
        public float timer;
        public EditorHeroEffectPopData data;
    }

    private List<PE> waitPlayEffectDatas = new List<PE>();


    public void PlayEffect(string animName)
    {
        waitPlayEffectDatas = new List<PE>();
        EditorHeroActionData editorHeroActionData = curEHD.GetAnimDataByName(animName);
        if (editorHeroActionData != null && editorHeroActionData.effectData != null && editorHeroActionData.effectData.Count > 0)
        {
            foreach (var item in editorHeroActionData.effectData)
            {
                PE pe = new PE();
                Debug.Log("delay:" + item.delay);
                pe.timer = (float)(item.delay * 0.001);
                pe.data = item;
                pe.heroId = curEHD.id;
                waitPlayEffectDatas.Add(pe);
            }
        }
        else
        {
            Debug.Log("~~ PlayEffect Not has effectData! animName:" + animName + " - editorHeroActionData is nil:" + (editorHeroActionData == null).ToString());
        }
    }

    private void UpdateEffect()
    {
        if (waitPlayEffectDatas.Count > 0)
        {
            for (int i = waitPlayEffectDatas.Count - 1; i >= 0; i--)
            {
                waitPlayEffectDatas[i].timer = waitPlayEffectDatas[i].timer - Time.deltaTime;
                if (waitPlayEffectDatas[i].timer <= 0)
                {
                    LoadEffect(waitPlayEffectDatas[i]);
                    waitPlayEffectDatas.RemoveAt(i);
                }
            }
        }
    }

    private void LoadEffect(PE pe)
    {
#if UNITY_EDITOR
        string path = EditorHeroHelper.GetEffectResLoadPath(pe.heroId);
        GameObject o = GameObject.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path + "/" + pe.data.effectResName + ".prefab"));

        Transform t = curEHD.animator.transform;
        Transform parent = null;
        FBXBindBonesHelper fBXBindBonesHelper = t.GetComponent<FBXBindBonesHelper>();
        if (fBXBindBonesHelper != null)
        {
            parent = fBXBindBonesHelper.GetBoneByString(pe.data.effectPoint);
        }
        else
        {
            parent = t.Find(pe.data.effectPoint);
        }
        if (parent == null)
        {
            parent = t.Find("point_" + pe.data.effectPoint);
        }

        if (parent == null)
        {
            Transform[] childs = t.GetComponentsInChildren<Transform>();
            foreach (var ct in childs)
            {
                if (ct.name == pe.data.effectPoint)
                {
                    parent = ct;
                }
            }
        }

        o.transform.parent = parent;
        o.transform.localPosition = Vector3.zero + pe.data.offest;
        o.transform.localScale = pe.data.scale.x == 0 ? Vector3.one : pe.data.scale;
        o.transform.localRotation = Quaternion.Euler(pe.data.rotation);
        o.SetActive(true);
#endif
    }

    #endregion





    //private long uid = 0;
    //private void InitUid()
    //{
    //    uid = 0;
    //}

    //public long GetSampleUID()
    //{
    //    return uid++;
    //}
}

public class EditorHeroData
{
    public int id;
    public GameObject obj;

    public Animator animator;
    public List<EditorHeroActionData> actDatas;
    public EditotHeroPointData pointData;

    public EditorHeroData(int id, GameObject obj)
    {
        this.id = id;
        this.obj = obj;

        animator = obj.GetComponent<Animator>();
        //List<EditorHeroActionData> _actFileDatas = EditorHeroHelper.ReadEventFileToData(id);
        //Debug.LogError("~~~~~~~~~~ _actFileDatas:" + _actFileDatas.Count);
        //if (_actFileDatas != null && _actFileDatas.Count > 0)
        //{
        //    actDatas = _actFileDatas;
        //}
        //else
        //{
        //    actDatas = new List<EditorHeroActionData>();

        //    foreach (var c in animator.runtimeAnimatorController.animationClips)
        //    {
        //        Debug.Log("~~~~~~~~~~ c:" + c.name);
        //        EditorHeroActionData editorHeroActionData = new EditorHeroActionData();
        //        editorHeroActionData.animName = c.name;
        //        editorHeroActionData.editorHeroActionAnimListDatas = new List<EditorHeroActionAnimListData>();
        //        editorHeroActionData.events = new List<int>();
        //        editorHeroActionData.effectData = new List<EditorHeroEffectPopData>();
        //        actDatas.Add(editorHeroActionData);
        //    }
        //}

        // 现在是自定义动画
        actDatas = new List<EditorHeroActionData>();
        List<string> extAct = new List<string>() {
            "entrance", "idle", "idle_2", "move", "move_2", "stun", "death",
            "attack_1", "attack_2", "skill1_1", "skill1_2", "skill2_1", "skill2_2", "skill3_1", "skill3_2","showidle","showloopidle"};
        foreach (var name in extAct)
        {
            EditorHeroActionData editorHeroActionData = new EditorHeroActionData();
            editorHeroActionData.animName = name;
            editorHeroActionData.editorHeroActionAnimListDatas = new List<EditorHeroActionAnimListData>();
            editorHeroActionData.events = new List<int>();
            editorHeroActionData.effectData = new List<EditorHeroEffectPopData>();
            actDatas.Add(editorHeroActionData);
        }

        // 已经编辑了的
        List<EditorHeroActionData> _actFileDatas = EditorHeroHelper.ReadEventFileToData(id);
        //Debug.LogError("~~~~~~~~~~ _actFileDatas:" + _actFileDatas.Count);
        for (int i = _actFileDatas.Count - 1; i >= 0; i--)
        {
            //Debug.LogError("~~~~~~~~~~ _actFileDatas[i].animName:" + _actFileDatas[i].animName);
            for (int j = actDatas.Count - 1; j >= 0; j--)
            {
                //Debug.LogError("~~~~~~~~~~ actDatas[j].animName:" + actDatas[j].animName);
                if (actDatas[j].animName == _actFileDatas[i].animName)
                {
               
                    actDatas[j] = _actFileDatas[i];
                    break;
                }
            }
        }

        pointData = EditorHeroHelper.ReadEventFileToEditotHeroPointData(id);

        
        /// 初始化
        //AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;
        //foreach (var clip in animationClips)
        //{
        //    EditorHeroActionData editorHeroActionData = new EditorHeroActionData();
        //    editorHeroActionData.animName = clip.name;
        //    actDatas.Add(editorHeroActionData);
        //}
    }

    public EditorHeroActionData GetAnimDataByName(string animName)
    {
        foreach (var item in actDatas)
        {
            if (item.animName == animName)
            {
                return item;
            }
        }
        return null;
    }

    public List<string> GetTheAnimPlayedAnimNames(string animName)
    {
        List<string> playedAnims = new List<string>();
        foreach (var item in actDatas)
        {
            if (item.animName == animName)
            {
                foreach (var d in item.editorHeroActionAnimListDatas)
                {
                    playedAnims.Add(d.animName);
                }
                return playedAnims;
            }
        }
        return playedAnims;
    }

    public void SetActionData(string animName, EditorHeroActionData editorHeroActionData)
    {
        for (int i = 0; i <= actDatas.Count -1; i++)
        {
            if (actDatas[i].animName == animName)
            {
                actDatas[i] = editorHeroActionData;
            }
        }
    }

    public void SetEffectData(string animName, List<EditorHeroEffectPopData> effectData)
    {
        for (int i = 0; i <= actDatas.Count - 1; i++)
        {
            if (actDatas[i].animName == animName)
            {
                actDatas[i].effectData = effectData;
            }
        }
    }

    public void AddEffect(string animName, EditorHeroEffectPopData effectData)
    {
        Debug.Log("~~ AddEffect effectData: " + effectData.uid +" - anim:" + effectData.animName + " - c:" + actDatas.Count);
        for (int i = 0; i <= actDatas.Count - 1; i++)
        {
            Debug.Log("~~ AddEffect effectData: " + effectData.uid + " - actDatas[i].animName:" + actDatas[i].animName + " - animName:" + animName);
            if (actDatas[i].animName == animName)
            {
                actDatas[i].effectData.Add(effectData);
            }
        }
    }

    public void RemoveEffect(string animName, EditorHeroEffectPopData effectData)
    {
        if (effectData.uid <= 0)
        {
            return;
        }
        Debug.Log("~~ AddRemoveEffectEffect effectData: " + effectData.uid + " - anim:" + effectData.animName + " - c:" + actDatas.Count);
        for (int i = 0; i <= actDatas.Count - 1; i++)
        {
            if (actDatas[i].animName == animName)
            {
                for (int j = actDatas[i].effectData.Count - 1; j >= 0; j--)
                {
                    Debug.Log("~~ RemoveEffect effectData: " + effectData.uid);
                    if (actDatas[i].effectData[j].uid == effectData.uid)
                    {
                        actDatas[i].effectData.RemoveAt(j);
                    }
                }
            }
        }
    }

    public void SetPointData(EditotHeroPointData editotHeroPointData)
    {
        pointData = editotHeroPointData;
    }
}

public class EditotHeroPointData
{
    public Dictionary<string, string> paths = new Dictionary<string, string>();
    public Dictionary<string, Vector3> logicpos = new Dictionary<string, Vector3>();
}