using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PMovement : MonoBehaviour
{
    [SerializeField] float forwardSpeed;
    [SerializeField] float runSpeed;
    [SerializeField] float stoppingFriction = 0.025f;
    [SerializeField] float rotationSpeed = 0.5f;
    [SerializeField] float cameraAlignSmoothness = 0.2f;
    [SerializeField] float _moveDirAlignSmoothness = 5;
    [SerializeField] float stepSpeed;
    [SerializeField] float stepTime;
    [SerializeField] float strafeSpeed = 4;
    [SerializeField] float dashSpeed;
    [SerializeField] float dashTime;
    [SerializeField] float KBSmoothness = 0.5f;
    [SerializeField] float goopMult = 0.5f;

    [HideInInspector] public bool goForward;
    [HideInInspector] public bool turnLeft;
    [HideInInspector] public bool turnRight;
    [HideInInspector] public bool goBack;
    [HideInInspector] public bool running;
    [SerializeField, ReadOnly] public bool rolling;
    [HideInInspector] public bool strafe;
    [HideInInspector] public bool attacking;
    [HideInInspector] public bool pressLeft;
    [HideInInspector] public bool pressRight;
    [HideInInspector] public bool knockedBack;

    [Header("TEST")]
    bool stopped;
    public float rotation;
    public float stunned;
    public float KB;
    Rigidbody rb;
    Player p;
    Vector3 rollDir;

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

    public void Roll() {
        if (rolling) return;
        rolling = true;

        rollSound.Play();
        rollDir = GetRollDir();
        StartCoroutine(StopRoll());
    }

    public void StopMovement()
    {
        stopped = true;
        rb.velocity = Vector3.zero;
        rolling = false;
    }

    public void ResumeMovement()
    {
        stopped = false;
    }

    IEnumerator StopRoll() {
        yield return new WaitForSeconds(dashTime);
        rolling = false;
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

        SetPlayerStats();

        if (stopped) return;
        Turn();
        Move();

        if (KB > 0.01f) KB = Mathf.Lerp(KB, 0, KBSmoothness);
        else knockedBack = false;
    }

    void Turn()
    {
        if (rolling || attacking) return;

        AlignToCamera();
    }

    void AlignToCamera() {
        var camForward = CameraState.i.transform.forward;
        camForward.y = 0;
        transform.forward = Vector3.Lerp(transform.forward, camForward, cameraAlignSmoothness * Time.deltaTime);

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

        if (stopped) rb.velocity = Vector3.zero;
        else if (knockedBack) rb.velocity = KBdir * KB + verticalVel;
        else if (rolling) rb.velocity = (rollDir * dashSpeed) + verticalVel;

        else if (goForward) rb.velocity = (transform.forward + horizontalDir).normalized * speed + verticalVel;
        else if (goBack) rb.velocity = (transform.forward * -1 + horizontalDir).normalized * speed + verticalVel;
        else rb.velocity = Vector3.Lerp(rb.velocity, horizontalDir * speed + verticalVel, stoppingFriction);

        if (attacking) {
            Transform model = transform.GetChild(0);
            model.transform.localRotation = Quaternion.Lerp(model.localRotation, Quaternion.Euler(Vector3.zero), 0.2f); //TEMP
        }
        else AlignModel();
    }

    void AlignModel()
    {
        Transform model = transform.GetChild(0);

        if (p.InCombat || GetComponent<PFighting>().chargingSpear()) { 
            model.transform.localEulerAngles = Vector3.zero;  
            return; 
        }

        if (rb.velocity.magnitude <= 0.01f) return;
        
        var originalRot = model.transform.localEulerAngles;
        model.LookAt(transform.position + rb.velocity.normalized);
        var targetRot = model.transform.localEulerAngles;
        targetRot.x = originalRot.x;
        targetRot.z = originalRot.z;
        model.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(originalRot), Quaternion.Euler(targetRot), _moveDirAlignSmoothness * Time.deltaTime);

    }

    Vector3 GetRollDir() {
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
        strafe = !attacking;

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
