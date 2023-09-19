using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CameraController : MonoBehaviour
{
    [SerializeField] CameraState.State manualState;
    [Space(18)]
    [SerializeField] bool useManaual;
    [SerializeField] bool addManualState;
    [SerializeField] int manualStateNum;

    [Header("Transition parameters")]
    [SerializeField] float blendSmoothness;
    [SerializeField] float fixedToMouseTransitionTime = 1, parentMoveSmoothness = 0.5f;
    [HideInInspector] public float mouseTransitionTimeLeft;
    [SerializeField, Range(0,1 )] float inputX, inputY;
    [SerializeField] bool debug;

    [Header("Dependencies")]
    [SerializeField] Player player;
    [SerializeField] GameObject cam,camTarget;
    [SerializeField] CameraState camState;
    [SerializeField] CameraFocusManager focusMan;

    CameraState.State currentState = new CameraState.State();
    float _blendSmoothness;

    [HideInInspector] public float lastMouseMoveDist;

    private void Start()
    {
        if (!Application.isPlaying) return;
        
        currentState = new CameraState.State(camState.current);
    }

    private void Update()
    {
        if (GameManager.i && GameManager.i.paused) return;

        if (addManualState) {
            camState.AddState(manualState, manualStateNum);
            addManualState = false;
            manualStateNum = -1;
        }
        _blendSmoothness = Application.isPlaying ? currentState.transitionSmoothness : 1;

        if (!Application.isPlaying || !GameManager.i.paused) return;

        //if (Input.GetKeyDown(KeyCode.Escape)) UnlockMouse();
        //if (Input.GetMouseButtonDown(0)) LockMouse();
    }

    private void LateUpdate()
    {
        if (GameManager.i && GameManager.i.paused) return;

        if (!player) player = FindObjectOfType<Player>();
        if (!player || !cam || !camState) return;

        if (Application.isPlaying) mouseTransitionTimeLeft -= Time.deltaTime;
        else mouseTransitionTimeLeft = -1;

        var state = camState.current;
        if (useManaual) state = manualState;
        if (!currentState.equals(state))LerpToState(state);
        if (currentState == null) return;

        SetCamPosition(currentState);
        SetCamLookDir(currentState);
        SetFollow(currentState);
        SetParentLookDir(currentState);
    }

    void LerpToState(CameraState.State o)
    {
        if (!Application.isPlaying) {
            currentState = o;
            return;
        }

        currentState.Lerp(o);
    }

    void SetCamPosition(CameraState.State s)
    {
        var pos = cam.transform.localPosition;
        pos.x = s.camX;
        pos.y = s.camY;
        pos.z = s.camZ;

        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, pos, _blendSmoothness);
        camTarget.transform.localPosition = Vector3.Lerp(camTarget.transform.localPosition, pos + s.camTargetOffset, _blendSmoothness);
    }
    void SetCamLookDir(CameraState.State s)
    {
        var targetRot = cam.transform.localEulerAngles;
        var original = targetRot;
        cam.transform.LookAt(camTarget.transform);
        targetRot.y = cam.transform.localEulerAngles.y;
        cam.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(original), Quaternion.Euler(targetRot), _blendSmoothness);
    }

    void SetFollow(CameraState.State s)
    {
        if (s.followPlayer) transform.position = Vector3.Lerp(transform.position, player.transform.position + s.parentPosOffset, parentMoveSmoothness);
    }

    void SetParentLookDir(CameraState.State s)
    {
        Vector3 targetForward = transform.position;
        if (s.parentLookTarget == CameraState.ParentLookTarget.Obj && s.objFocus == null) s.objFocus = focusMan.GetFocus(s.focusIndex);

        if (s.parentLookTarget == CameraState.ParentLookTarget.PlayerForward || mouseTransitionTimeLeft > 0) targetForward = player.transform.forward;
        if (s.parentLookTarget == CameraState.ParentLookTarget.Obj && s.objFocus != null) targetForward = (s.objFocus.transform.position + s.parentLookOffset - transform.position).normalized; 

        if ((s.parentLookTarget != CameraState.ParentLookTarget.None && s.parentLookTarget != CameraState.ParentLookTarget.Mouse) || mouseTransitionTimeLeft > 0) {
            transform.forward = Vector3.Lerp(transform.forward, targetForward, s.parentRotSmoothness);
            return;
        }

        if (!Application.isPlaying) {

            float _x = 360 * inputX;
            float _y = Mathf.Abs(s.parentRotLimitsY.x - s.parentRotLimitsY.y) * inputY;

            transform.localEulerAngles = new Vector3(_y, _x, 0);
            return;
        }

        //if (s.parentLookTarget != CameraState.ParentLookTarget.Mouse) return;

        float mouseX = Input.GetAxis("Mouse X") * s.mouseXSens;
        float mouseY = Input.GetAxis("Mouse Y") * s.mouseYSens * -1;

        transform.localEulerAngles += new Vector3(mouseY, mouseX, 0);
        float y = transform.localEulerAngles.x;
        if (y > 180 && y < 360 + s.parentRotLimitsY.x) {
            transform.localEulerAngles = new Vector3(360 + s.parentRotLimitsY.x, transform.localEulerAngles.y, 0);
        }
        else if (y < 180 && y > s.parentRotLimitsY.y) {
            transform.localEulerAngles = new Vector3(s.parentRotLimitsY.y, transform.localEulerAngles.y, 0);
        }

        lastMouseMoveDist = Mathf.Abs(mouseX) + Mathf.Abs(mouseY);
    }

    private void OnDrawGizmos()
    {
        if (!debug) return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(camTarget.transform.position, 0.1f);
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(cam.transform.position, 0.1f);
    }
}
