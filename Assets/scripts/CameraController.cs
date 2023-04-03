using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

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
    [SerializeField] float fixedToMouseTransitionTime = 1;
    [HideInInspector] public float mouseTransitionTimeLeft;

    [Header("Dependencies")]
    [SerializeField] Player p;
    [SerializeField] GameObject cam,camTarget;
    [SerializeField] CameraState camState;

    CameraState.State currentState = new CameraState.State();

    private void Start()
    {
        if (!Application.isPlaying) return;
        
        currentState = new CameraState.State(camState.current);
        LockMouse();
    }

    private void Update()
    {
        if (addManualState) {
            camState.AddState(manualState, manualStateNum);
            addManualState = false;
            manualStateNum = -1;
        }
        if (!Application.isPlaying) return;

        if (Input.GetKeyDown(KeyCode.Escape)) UnlockMouse();
        if (Input.GetMouseButtonDown(0)) LockMouse();
    }

    void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LateUpdate()
    {
        if (!p || !cam || !camState) return;

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

        if (currentState.limitParentRot) LimitParentRot(currentState);
    }

    void LerpToState(CameraState.State o)
    {
        if (!Application.isPlaying) {
            currentState = o;
            return;
        }

        currentState.playerX = Mathf.Lerp(currentState.playerX, o.playerX, blendSmoothness);
        currentState.playerY = Mathf.Lerp(currentState.playerY, o.playerY, blendSmoothness);
        currentState.zoom = Mathf.Lerp(currentState.zoom, o.zoom, blendSmoothness);
        currentState.parentRotSmoothness = Mathf.Lerp(currentState.parentRotSmoothness, o.parentRotSmoothness, blendSmoothness); 

        currentState.limitsX = Vector2.Lerp(currentState.limitsX, o.limitsX, blendSmoothness);
        currentState.limitsY = Vector2.Lerp(currentState.limitsY, o.limitsY, blendSmoothness);
        currentState.zoomLimits = Vector2.Lerp(currentState.zoomLimits, o.zoomLimits, blendSmoothness);
        currentState.parentRotLimits = Vector2.Lerp(currentState.parentRotLimits, o.parentRotLimits, blendSmoothness);

        currentState.camTargetOffset = Vector3.Lerp(currentState.camTargetOffset, o.camTargetOffset, blendSmoothness);
        currentState.camParentPlayerOffset = Vector3.Lerp(currentState.camParentPlayerOffset, o.camParentPlayerOffset, blendSmoothness);
        currentState.objTargetOffset = Vector3.Lerp(currentState.objTargetOffset, o.objTargetOffset, blendSmoothness);

        currentState.lookAtPlayer = o.lookAtPlayer;
        currentState.followPlayer = o.followPlayer;
        currentState.limitParentRot = o.limitParentRot;

        if (currentState.parentLookTarget != CameraState.ParentLookTarget.Mouse && o.parentLookTarget == CameraState.ParentLookTarget.Mouse) mouseTransitionTimeLeft = fixedToMouseTransitionTime;
        currentState.parentLookTarget = o.parentLookTarget;

        currentState.objFocus = o.objFocus;
    }

    void SetCamPosition(CameraState.State s)
    {
        //if (s.parentLookTarget == CameraState.ParentLookTarget.Mouse) return;

        var pos = cam.transform.localPosition;
        pos.x = Mathf.Abs(s.limitsX.x - s.limitsX.y) * (1 - s.playerX) + s.limitsX.x;
        pos.y = Mathf.Abs(s.limitsY.x - s.limitsY.y) * s.playerY + s.limitsY.x;
        pos.z = Mathf.Abs(s.zoomLimits.x - s.zoomLimits.y) * s.zoom + s.zoomLimits.x;

        cam.transform.localPosition = pos;
        camTarget.transform.localPosition = pos + s.camTargetOffset;
    }
    void SetCamLookDir(CameraState.State s)
    {
        if (s.lookAtPlayer) cam.transform.LookAt(p.transform);
        else cam.transform.LookAt(camTarget.transform);
    }

    void SetFollow(CameraState.State s)
    {
        if (s.followPlayer) transform.position = p.transform.position + s.camParentPlayerOffset;
    }

    void SetParentLookDir(CameraState.State s)
    {
        Vector3 targetForward = transform.position;

        if (s.parentLookTarget == CameraState.ParentLookTarget.PlayerForward || mouseTransitionTimeLeft > 0) targetForward = p.transform.forward;
        if (s.parentLookTarget == CameraState.ParentLookTarget.Obj && s.objFocus != null) targetForward = (s.objFocus.transform.position + s.objTargetOffset - transform.position).normalized; 

        if ((s.parentLookTarget != CameraState.ParentLookTarget.None && s.parentLookTarget != CameraState.ParentLookTarget.Mouse) || mouseTransitionTimeLeft > 0) {
            transform.forward = Vector3.Lerp(transform.forward, targetForward, s.parentRotSmoothness);
            return;
        }

        if (!Application.isPlaying) return;

        float mouseX = Input.GetAxis("Mouse X") * s.mouseXSens;
        float mouseY = Input.GetAxis("Mouse Y") * s.mouseYSens * -1;
        transform.Rotate(mouseY, mouseX, 0);

        var angles = transform.eulerAngles;
        angles.z = 0;
        transform.eulerAngles = angles;
    }

    void LimitParentRot(CameraState.State s) {
        var rot = transform.localEulerAngles;
        rot.x = Mathf.Clamp(rot.x, s.parentRotLimits.x, s.parentRotLimits.y);
        transform.localEulerAngles = rot;
    }
}
