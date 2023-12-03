using DG.Tweening;
using UnityEngine;

public class TestMainSceneHeroCtrl : MonoBehaviour
{
    public Camera camera3rd;
    public Camera GetCamera()
    {
        return camera3rd;
    }

    private bool isInit = false;
    private CharacterController characterController;

    public Transform followTarget;
    public Animator animator;
    //private float mouseMoveX = 0.0f;
    //private float mouseMoveY = 0.0f;
    public float toTargetDist = 15f;
    public float toTargetHeight = 1.8f;
    private float toTargetDire = 30.0f;
    public float rotationSpeed = 0.1f;
    public float camerZoomSpeed = 10f;

    public float heroMoveSpeed = 0.2f;
    public float heroRotateSpeed = 100f;
    private Vector4 canRotationArea;

    public float CurrJoystickAngle;


    private void Awake()
    {
        if (camera3rd == null)
            camera3rd = Camera.main;
        toTargetDist = 15f;
        toTargetHeight = 1.8f;
        toTargetDire = 30.0f;
        rotationSpeed = 0.1f;

        if (followTarget != null)
            characterController = followTarget.GetComponent<CharacterController>();

        canRotationArea = new Vector4(Screen.width / 2 + 100, Screen.width - 100, 100, Screen.height - 100);
        CurrJoystickAngle = -1;
    }

    private void Start()
    {

    }

    private void OnEnable()
    {

    }

    private void Update()
    {

    }

    Quaternion tempQuat;
    Vector3 simulatedGravityVec3;
    float simulatedGravitySpeed;

    Vector2 tempTouchXY = Vector2.zero;

    private bool IsRotationArea(Vector2 screePos)
    {
        return screePos.x >= canRotationArea.x && screePos.x <= canRotationArea.y && screePos.y >= canRotationArea.z && screePos.y <= canRotationArea.w;
    }

    private void LateUpdate()
    {
        if (followTarget == null)
        {
            return;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        bool isRun = false;
        Vector3 rotate = Vector3.zero;

        float zoomAmount = Input.GetAxis("Mouse ScrollWheel");
        if (zoomAmount != 0)
        {
            toTargetDist -= zoomAmount * camerZoomSpeed * Time.deltaTime;
        }
        Vector2 rotationTouchSP = Input.mousePosition;
        if (Input.GetMouseButton(1))
        {

            if (tempTouchXY == Vector2.zero)
            {
                tempTouchXY = rotationTouchSP;
            }

            float mix = rotationTouchSP.x - tempTouchXY.x;
            float miy = rotationTouchSP.y - tempTouchXY.y;
            Vector3 old = transform.rotation.eulerAngles;
            float rotaX = -miy * rotationSpeed + old.x;
            float rotaY = mix * rotationSpeed + old.y;
            if (rotaX >= 70 && rotaX < 180)
                rotaX = 70;
            if (rotaX <= 355 && rotaX >= 180)
                rotaX = 355;
            tempQuat = (mix != 0 || miy != 0) ? Quaternion.Euler(rotaX, rotaY, 0) : transform.rotation;
            transform.rotation = tempQuat;
            tempTouchXY = rotationTouchSP;
        }
        else
        {
            tempTouchXY = Vector2.zero;
        }

        if (Input.GetKey(KeyCode.W))
        {
            PlayAnim(TestAnimType.Run);

            followTarget.rotation = Quaternion.Euler(0, camera3rd.transform.rotation.eulerAngles.y, 0);

            characterController.Move(followTarget.forward * heroMoveSpeed);

            isRun = true;
        }
        if (Input.GetKey(KeyCode.S))
        {
            PlayAnim(TestAnimType.Run);

            followTarget.rotation = Quaternion.Euler(0, camera3rd.transform.rotation.eulerAngles.y + 180, 0);

            characterController.Move(followTarget.forward * heroMoveSpeed);

            isRun = true;
        }
        if (Input.GetKey(KeyCode.A))
        {
            //rotate.y -= heroRotateSpeed * Time.deltaTime;
            PlayAnim(TestAnimType.Run);

            followTarget.rotation = Quaternion.Euler(0, camera3rd.transform.rotation.eulerAngles.y- 90, 0);

            characterController.Move(followTarget.forward * heroMoveSpeed);

            isRun = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            //rotate.y += heroRotateSpeed * Time.deltaTime;
            PlayAnim(TestAnimType.Run);

            followTarget.rotation = Quaternion.Euler(0, camera3rd.transform.rotation.eulerAngles.y + 90, 0);

            characterController.Move(followTarget.forward * heroMoveSpeed);

            isRun = true;
        }

        if (rotate != Vector3.zero)
        {
            followTarget.rotation = Quaternion.Euler(followTarget.rotation.eulerAngles + rotate);
        }

        if(Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A))
        {
            heroMoveSpeed = 0.1f;

            followTarget.rotation = Quaternion.Euler(0, camera3rd.transform.rotation.eulerAngles.y - 45, 0);
        }
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D))
        {
            heroMoveSpeed = 0.1f;

            followTarget.rotation = Quaternion.Euler(0, camera3rd.transform.rotation.eulerAngles.y + 45, 0);
        }
        if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A))
        {
            heroMoveSpeed = 0.1f;

            followTarget.rotation = Quaternion.Euler(0, camera3rd.transform.rotation.eulerAngles.y - 135, 0);
        }
        if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
        {
            heroMoveSpeed = 0.1f;

            followTarget.rotation = Quaternion.Euler(0, camera3rd.transform.rotation.eulerAngles.y + 135, 0);
        }
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
        {
            if (!isRun)
            {
                PlayAnim(TestAnimType.Idle);
            }

            heroMoveSpeed = 0.2f;
        }
#else
        int rotationTouchIndex = -1;
        Vector2 rotationTouchSP = Vector2.zero;
 //       Debug.LogError("~~ Input.touchCount:" + Input.touchCount);
 //       if (Input.touchCount == 1)
	//{
 //       Debug.LogError("~~ mouseMoveX:" + mouseMoveX + " - mouseMoveY:" + mouseMoveY + " - mix:" + Input.GetAxis("Mouse X") + " - miy:" + Input.GetAxis("Mouse Y"));
	//}

        if (Input.touchCount == 1 && IsRotationArea(Input.GetTouch(0).position))
        {
            rotationTouchIndex = 0;
            rotationTouchSP = Input.GetTouch(0).position;
        }
        else if (Input.touchCount == 2)
        {
            if (IsRotationArea(Input.GetTouch(0).position))
            {
                rotationTouchIndex = 0;
                rotationTouchSP = Input.GetTouch(0).position;
            }
            else if (IsRotationArea(Input.GetTouch(1).position))
            {
                rotationTouchIndex = 1;
                rotationTouchSP = Input.GetTouch(1).position;
            }
            else
            {
                tempTouchXY = Vector2.zero;
            }
        }
        else
        {
           tempTouchXY = Vector2.zero;
        }
        //Debug.LogError("~~ rotationTouchIndex:" + rotationTouchIndex + " - rotationTouchSP:" + rotationTouchSP);
        if (rotationTouchIndex != -1 && rotationTouchSP != Vector2.zero)
        {
            if (tempTouchXY == Vector2.zero)
            {
                tempTouchXY = rotationTouchSP;
            }
          
            float mix = rotationTouchSP.x - tempTouchXY.x;
            float miy = rotationTouchSP.y - tempTouchXY.y;
            Vector3 old = transform.rotation.eulerAngles;
            float rotaX = -miy * rotationSpeed + old.x;
            float rotaY = mix * rotationSpeed + old.y;
            if (rotaX >= 70 && rotaX < 180)
                rotaX = 70;
            if (rotaX <= 355 && rotaX >= 180)
                rotaX = 355;
            tempQuat = (mix != 0 || miy != 0) ? Quaternion.Euler(rotaX, rotaY, 0) : transform.rotation;
            transform.rotation = tempQuat;
            tempTouchXY = rotationTouchSP;
        }
#endif
        var negDistance = new Vector3(0.0f, 0.0f, -toTargetDist);
        transform.position = transform.rotation * negDistance;
        // 防抖动
        transform.position += (followTarget.position + Vector3.up * toTargetHeight);

        // 不在地面上模拟重力 
        if (!characterController.isGrounded)
        {
            simulatedGravitySpeed += -9.8f * 5 * Time.deltaTime;
            if (simulatedGravitySpeed < -10.0f)
            {
                simulatedGravitySpeed = -10.0f;
            }
        }
        simulatedGravityVec3.y = simulatedGravitySpeed;
        simulatedGravityVec3 *= Time.deltaTime;
        characterController.Move(simulatedGravityVec3);
    }

    private void OnDisable()
    {

    }

    private void OnDestroy()
    {

    }

    public void SetActive(bool bActive)
    {
        if (bActive)
        {
            if (followTarget == null)
            {
                return;
            }
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void InitCameraParam()
    {
        tempQuat = Quaternion.Euler(0, 0, 0);
        transform.rotation = tempQuat;
        var negDistance = new Vector3(0.0f, 0.0f, -toTargetDist);
        transform.position = transform.rotation * negDistance;
        // 防抖动
        transform.position += (followTarget.position + Vector3.up * toTargetHeight);

        if (!isInit)
        {
            gameObject.SetActive(true);
        }
        isInit = true;
    }

    private enum TestAnimType
    {
        None,
        Idle,
        Run,
    }
    private TestAnimType currAnim = TestAnimType.None;
    private void PlayAnim(TestAnimType animType)
    {
        if (currAnim == animType && animator != null)
        {
            return;
        }
        switch (animType)
        {
            case TestAnimType.Idle:
            default:
                animator.ResetTrigger("run");
                animator.SetTrigger("loopidle");
                break;
            case TestAnimType.Run:
                animator.ResetTrigger("loopidle");
                animator.SetTrigger("run");
                break;
        }
        currAnim = animType;
    }

    public void PlayIdle()
    {
        PlayAnim(TestAnimType.Idle);
    }

    public void Init(Camera camera, Transform fllowTarget)
    {
        camera3rd = camera;
        followTarget = fllowTarget;
        animator = fllowTarget.GetComponentInChildren<Animator>();
        toTargetDist = 15f;
        toTargetHeight = 1.8f;
        rotationSpeed = 0.2f;
        camerZoomSpeed = 10;
        heroMoveSpeed = 0.2f;
        heroRotateSpeed = 100;

        characterController = fllowTarget.gameObject.GetComponent<CharacterController>();
        if (characterController == null)
            characterController = fllowTarget.gameObject.AddComponent<CharacterController>();
        characterController.slopeLimit = 90;
        characterController.stepOffset = 0.3f;
        characterController.skinWidth = 0.08f;
        characterController.minMoveDistance = 0.001f;
        characterController.center = new Vector3(0, 0.66f, 0);
        characterController.radius = 0.35f;
        characterController.height = 1.3f;

        InitCameraParam();
    }

    GameObject cube = null;
    /// <summary>
    /// 遥感移动
    /// </summary>
    /// <param name="v2Pos"></param>
    /// <param name="v2Speed"></param>
    public void JoystickMove(Vector2 v2Pos, Vector2 v2Speed)
    {
        // v2Pos: 上(0,1) 下(0,-1) 左(1,0) 右(-1,0)
        // v2Speed
        // 以相机的正前方为遥感方位基准

        Vector3 testpos = camera3rd.transform.position + camera3rd.transform.forward * 10;
        testpos.y = camera3rd.transform.position.y;


        // 相机同一水平面上的正方向
        Vector2 cameraHorizontalForward = (new Vector2(testpos.x, testpos.z) - new Vector2(camera3rd.transform.position.x, camera3rd.transform.position.z)).normalized;
        //Debug.LogError("~~ JoystickMove cameraHorizontalForward:" + cameraHorizontalForward);

        float angle = Vector2.Angle(cameraHorizontalForward, v2Pos);

        // 顺时针0~360
        float JoysticAngle = Vector2.Angle(Vector2.up, v2Pos);
        if ((v2Pos.x > 0 && v2Pos.y < 0) || (v2Pos.x > 0 && v2Pos.y > 0) || (JoysticAngle == 90 && v2Pos.x > 0 && (Mathf.Abs(v2Pos.y) <= 0.05f)))
        {
            JoysticAngle = 180 - JoysticAngle + 180;
        }


        Vector2 p = GetRotatePosition(cameraHorizontalForward, Vector2.zero, -JoysticAngle);
        CurrJoystickAngle = JoysticAngle;
        testpos.x = camera3rd.transform.position.x + p.x * 10;
        testpos.y = camera3rd.transform.position.y;
        testpos.z = camera3rd.transform.position.z + p.y * 10;
        //if (cube == null)
        //{
        //    cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //}

        // 角色朝向
        testpos = testpos - camera3rd.transform.position;
        testpos = testpos + followTarget.transform.position;

        //cube.transform.position = testpos;
        followTarget.LookAt(testpos);

        PlayAnim(TestAnimType.Run);
        characterController.Move((testpos - followTarget.transform.position).normalized * heroMoveSpeed);
    }

    public void EndJoystickMove()
    {
        PlayAnim(TestAnimType.Idle);
        CurrJoystickAngle = -1;
    }

    public Vector2 GetRotatePosition(Vector2 tp, Vector2 cp, float angele)
    {
        float ex = (tp.x - cp.x) * Mathf.Cos(angele * Mathf.Deg2Rad) -
                     (tp.y - cp.y) * Mathf.Sin(angele * Mathf.Deg2Rad) + cp.x;
        float ey = (tp.y - cp.y) * Mathf.Cos(angele * Mathf.Deg2Rad) +
                     (tp.x - cp.x) * Mathf.Sin(angele * Mathf.Deg2Rad) + cp.y;
        return new Vector2(ex, ey);
    }


    private bool isResetLens = false;
    public void ResetLens()
    {
        if (isResetLens)
        {
            return;
        }

        isResetLens = true;
        transform.DORotate(followTarget.transform.eulerAngles, 1f).SetEase(Ease.OutExpo).OnComplete(() =>
        {
            var negDistance = new Vector3(0.0f, 0.0f, -toTargetDist);
            transform.position = transform.rotation * negDistance;
            // 防抖动
            transform.position += (followTarget.position + Vector3.up * toTargetHeight);

            isResetLens = false;
        });
    }
}
