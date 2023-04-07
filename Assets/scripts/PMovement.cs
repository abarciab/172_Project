using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PMovement : MonoBehaviour
{
    [SerializeField] float forwardSpeed, runSpeed, stoppingFriction = 0.025f, rotationSpeed = 0.5f, cameraAlignSmoothness = 0.2f, stepSpeed, stepTime, strafeSpeed = 4, dashSpeed, dashTime, KBsmoothness = 0.5f;
    [HideInInspector] public bool goForward, turnLeft, turnRight, goBack, running, alignToCamera, stepping, dashing, strafe, attacking, pressLeft, pressRight, knockedBack, posing;
    public bool sitting;

    [Header("TEST")]
    public bool alignToEnemy;
    public float rotation, stunned, KB;
    Rigidbody rb;
    Player p;
    Vector3 dashDir;
    Transform poseLookTarget;

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

    public void SlowDownAndPose(Transform lookTarget = null) {
        posing = true;
        if (lookTarget) poseLookTarget = lookTarget;
    }

    IEnumerator StopDash() {
        yield return new WaitForSeconds(dashTime);
        if (alignToEnemy) AlignToEnemy(1);
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

        if (posing) {
            if (poseLookTarget == null) return;

            transform.LookAt(poseLookTarget);
            var orig = transform.localEulerAngles;
            var rot_ = transform.localEulerAngles;
            rot_.x = rot_.z = 0;
            transform.localRotation = Quaternion.Lerp(Quaternion.Euler(orig), Quaternion.Euler(rot_), 0.025f);
            return;
        }

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

    void AlignToEnemy(float smoothnessOverride = -1) {

        if (smoothnessOverride == -1) smoothnessOverride = cameraAlignSmoothness;
        var enemy = CameraState.i.GetLockedEnemy();
        var dist = Vector3.Distance(transform.position, enemy.transform.position);
        if (goForward && dist < 2.5) return;

        
        var _rot = transform.localEulerAngles;
        var originalRot = _rot;
        transform.LookAt(enemy.transform);
        var lookAtRot = transform.localEulerAngles;
        _rot.y = lookAtRot.y;
        transform.localEulerAngles = Vector3.Lerp(originalRot, _rot, smoothnessOverride);
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
        if (attacking) speed = 0;

        var verticalVel = rb.velocity;
        verticalVel.x = verticalVel.z = 0;

        Vector3 horizontalDir = !attacking ? GetStrafeDir() : Vector3.zero;
        var KBdir = Vector3.zero;
        if (knockedBack) {
            KBdir = (transform.position - source.transform.position).normalized;
            KBdir.y = 0;
        }

        if (posing) rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.025f);
        else if (knockedBack) rb.velocity = KBdir * KB + verticalVel;
        else if (dashing) rb.velocity = (dashDir * dashSpeed) + verticalVel;
        else if (stepping) rb.velocity = (transform.forward * stepSpeed) + verticalVel;

        else if (goForward) rb.velocity = (transform.forward + horizontalDir).normalized * speed + verticalVel;
        else if (goBack) rb.velocity = (transform.forward * -1 + horizontalDir).normalized * speed + verticalVel;
        else rb.velocity = Vector3.Lerp(rb.velocity, horizontalDir * speed + verticalVel, stoppingFriction);

        if (!alignToEnemy && !attacking) AlignModel();
        else {
            Transform model = transform.GetChild(0);
            model.transform.localRotation = Quaternion.Lerp(model.localRotation, Quaternion.Euler(Vector3.zero), 0.2f);
        }
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
        if (!alignToCamera && !alignToEnemy) return strafeDir;

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
