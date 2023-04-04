using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PMovement : MonoBehaviour
{
    [SerializeField] float forwardSpeed, runSpeed, stoppingFriction = 0.025f, rotationSpeed = 0.5f, cameraAlignSmoothness = 0.2f, stepSpeed, stepTime, strafeSpeed = 4, dashSpeed, dashTime, KBsmoothness = 0.5f;
    [HideInInspector] public bool goForward, turnLeft, turnRight, goBack, running, alignToCamera, stepping, dashing, strafe, attacking, pressLeft, pressRight;
    public bool sitting;

    [Header("TEST")]
    public bool alignToEnemy;
    public float rotation, stunned, KB;
    Rigidbody rb;
    Player p;
    Vector3 dashDir;

    bool knockedBack;
    GameObject source;

    public void KnockBack(GameObject _source, float _KB)
    {

        source = _source;
        KB = _KB;
        knockedBack = true;
    }

    public void StepForward()
    {
        stepping = true;
        StartCoroutine(StopStep());
    }

    public void Dash() {
        dashDir = GetDashDir();
        dashing = true;
        StartCoroutine(StopDash());
    }

    IEnumerator StopDash() {
        yield return new WaitForSeconds(dashTime);
        dashing = false;
    }

    IEnumerator StopStep()
    {
        yield return new WaitForSeconds(stepTime);
        stepping = false;
    }

    private void Start()
    {
        rotation = transform.localEulerAngles.y;
        p = GetComponent<Player>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        attacking = GetComponent<PFighting>().attacking;
        alignToEnemy = CameraState.i.GetLockedEnemy() != null;
        alignToCamera = CameraState.i.mouseControl;
        SetPlayerStats();
        if (running) stepping = false;

        if (sitting) return;
        Turn();
        Move();

        if (KB > 0.01f) KB = Mathf.Lerp(KB, 0, KBsmoothness);
        else knockedBack = false;
    }

    void Turn()
    {
        if (stepping || dashing || attacking) return;

        if (alignToCamera) {
            AlignToCamera();
            return;
        }

        if (alignToEnemy) {
            AlignToEnemy();
            return;
        }

        if (turnLeft) rotation -= rotationSpeed;
        if (turnRight) rotation += rotationSpeed;

        var rot = transform.localEulerAngles;
        rot.y = rotation;
        transform.localEulerAngles = rot;
    }

    void AlignToEnemy() {

        if (goForward) return;

        var enemy = CameraState.i.GetLockedEnemy();
        var _rot = transform.localEulerAngles;
        var originalRot = _rot;
        transform.LookAt(enemy.transform);
        var lookAtRot = transform.localEulerAngles;
        _rot.y = lookAtRot.y;
        transform.localEulerAngles = Vector3.Lerp(originalRot, _rot, cameraAlignSmoothness);
    }

    void AlignToCamera() {
        var camForward = CameraState.i.transform.forward;
        camForward.y = 0;
        transform.forward = Vector3.Lerp(transform.forward, camForward, cameraAlignSmoothness);

        turnLeft = turnRight = false;
        if (transform.eulerAngles.y - rotation > 0.01f) turnLeft = true;
        else if (rotation - transform.eulerAngles.y > 0.01f) turnRight = true;
        rotation = transform.eulerAngles.y;
    }

    void Move()
    {
        float speed = running ? runSpeed : forwardSpeed;

        var verticalVel = rb.velocity;
        verticalVel.x = verticalVel.z = 0;

        Vector3 horizontalDir = !attacking ? GetStrafeDir() : Vector3.zero;

        if (knockedBack) rb.velocity = (transform.position - source.transform.position).normalized * KB + verticalVel;
        else if(dashing) rb.velocity = (dashDir * dashSpeed) + verticalVel;
        else if (stepping) rb.velocity = (transform.forward * stepSpeed) + verticalVel;

        else if (goForward) rb.velocity = (transform.forward + horizontalDir).normalized * speed + verticalVel;
        else if (goBack) rb.velocity = (transform.forward * -1 + horizontalDir).normalized * speed + verticalVel;
        else rb.velocity = Vector3.Lerp(rb.velocity, horizontalDir * speed + verticalVel, stoppingFriction);

        if (!alignToEnemy && !attacking) AlignModel();
    }

    void AlignModel()
    {
        Transform model = transform.GetChild(0);

        if (rb.velocity.magnitude <= 0.01f) {
            model.transform.localRotation = Quaternion.Lerp(model.transform.localRotation, Quaternion.identity, 0.2f);
            return;
        }
        
        var originalRot = model.transform.localEulerAngles;
        model.LookAt(transform.position + rb.velocity.normalized);
        var targetRot = model.transform.localEulerAngles;
        targetRot.x = originalRot.x;
        targetRot.z = originalRot.z;
        model.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(originalRot), Quaternion.Euler(targetRot), 0.2f);

        Debug.DrawRay(transform.position, rb.velocity.normalized, Color.blue);
    }

    Vector3 GetDashDir() {
        return goBack ? transform.forward * -1 : (pressRight ? transform.right : (pressLeft ? transform.right * -1 : transform.forward));
    }

    Vector3 GetStrafeDir() {
        var strafeDir = Vector3.zero;
        strafe = alignToEnemy && !attacking;
        if (pressLeft) strafeDir = transform.right * -1;
        else if (pressRight) strafeDir = transform.right;
        else strafe = false;

        return strafeDir;
    }

    void SetPlayerStats()
    {
        p.speed3D = rb.velocity;
        p.forwardSpeed = Vector3.Dot(rb.velocity, transform.GetChild(0).forward);
    }
}
