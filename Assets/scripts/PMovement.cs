using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PMovement : MonoBehaviour
{
    [SerializeField] float forwardSpeed, runSpeed, stoppingFriction = 0.025f, rotationSpeed = 0.5f, cameraAlignSmoothness = 0.2f, stepSpeed, stepTime, strafeSpeed = 4, dashSpeed, dashTime, KBsmoothness = 0.5f, goopMult = 0.5f;
    [HideInInspector] public bool goForward, turnLeft, turnRight, goBack, running, alignToCamera, stepping, rolling, strafe, attacking, pressLeft, pressRight, knockedBack, posing;
    public bool sitting;

    [Header("TEST")]
    bool alignToEnemy, stopped;
    public float rotation, stunned, KB;
    Rigidbody rb;
    Player p;
    Vector3 rollDir;
    Transform poseLookTarget;

    [Header("Sounds")]
    [SerializeField] Sound rollSound;

    [HideInInspector] public float goopTime; 

    Vector3 source;

    public void KnockBack(GameObject _source, float _KB, Vector3 offset)
    {
        if (_source == null || _KB <= 0) return;

        source = _source.transform.position + offset;
        KB = _KB;
        knockedBack = true;
    }

    public void StepForward()
    {
        return;

        stepping = true;
        StartCoroutine(StopStep());
    }

    public void Roll() {
        //if (GetComponent<PFighting>().basicAttacking || GetComponent<PFighting>().hvyAttacking) return;

        if (!rolling) rollSound.Play();
        rollDir = GetDashDir();
        rolling = true;
        StartCoroutine(StopDash());
    }

    public void StopMovement()
    {
        stopped = true;   
    }

    public void ResumeMovement()
    {
        stopped = false;
    }

    public void SlowDownAndPose(Transform lookTarget = null) {
        posing = true;
        if (lookTarget) poseLookTarget = lookTarget;
    }

    IEnumerator StopDash() {
        yield return new WaitForSeconds(dashTime);
        if (alignToEnemy) AlignToEnemy(1);
        rolling = false;
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
        rollSound = Instantiate(rollSound);
    }

    private void Update()
    {
        goopTime -= Time.deltaTime;

        //attacking = GetComponent<PFighting>().basicAttacking || GetComponent<PFighting>().hvyAttacking;
        alignToEnemy = CameraState.i.GetLockedEnemy() != null;
        alignToCamera = CameraState.i.mouseControl;
        SetPlayerStats();
        if (running) stepping = false;

        if (sitting || stopped) return;
        Turn();
        Move();

        if (KB > 0.01f) KB = Mathf.Lerp(KB, 0, KBsmoothness);
        else knockedBack = false;
    }

    void Turn()
    {
        if (stepping || rolling || attacking) return;

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
        speed = goopTime > 0 ? speed * goopMult : speed;
        if (attacking) speed = 0;

        var verticalVel = rb.velocity;
        verticalVel.x = verticalVel.z = 0;

        Vector3 horizontalDir = !attacking ? GetStrafeDir() : Vector3.zero;
        var KBdir = Vector3.zero;
        if (knockedBack) {
            KBdir = (transform.position - source).normalized;
            KBdir.y = 0;
        }

        if (posing) rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.025f);
        else if (knockedBack) rb.velocity = KBdir * KB + verticalVel;
        else if (rolling) rb.velocity = (rollDir * dashSpeed) + verticalVel;
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

        if (p.InCombat()) { model.transform.localEulerAngles = Vector3.zero;  return; }

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

    }

    Vector3 GetDashDir() {
        Vector3 dir = Vector3.zero;
        if (pressRight) dir += transform.right;
        if (pressLeft) dir += transform.right * -1;
        if (goForward) dir += transform.forward;
        if (goBack) dir += transform.forward * -1;

        return dir.normalized;
    }

    Vector3 GetStrafeDir() {
        var strafeDir = Vector3.zero;
        strafe = false;
        if (!alignToCamera && !alignToEnemy) return strafeDir;
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
