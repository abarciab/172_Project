using MyBox;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PMovement : MonoBehaviour
{
    private Player _p;

    [Header("Controls")]
    [SerializeField, SearchableEnum] KeyCode _forward = KeyCode.W;
    [SerializeField, SearchableEnum] KeyCode _left = KeyCode.A;
    [SerializeField, SearchableEnum] KeyCode _backward = KeyCode.S;
    [SerializeField, SearchableEnum] KeyCode _right = KeyCode.D;
    [SerializeField, SearchableEnum] KeyCode _run = KeyCode.LeftShift;
    [SerializeField, SearchableEnum] KeyCode _roll = KeyCode.Space;

    //
    //
    //

    [Header("movement")]
    [SerializeField] float _walkSpeed;
    [SerializeField] float _runSpeed;
    [SerializeField] private float _accelerationSnapiness = 5;
    [SerializeField] float goopMult = 0.5f;
    [SerializeField] float stoppingFriction = 0.025f;

    [Header("Rotation")]
    [SerializeField] float rotationSpeed = 0.5f;
    [SerializeField] float cameraAlignSmoothness = 0.2f;
    [SerializeField] float _moveDirAlignSmoothness = 5;

    [Header("Rolling")]
    [SerializeField] private float _rollWaitTime = 0.1f;
    [SerializeField] float dashSpeed;
    [SerializeField] float dashTime;

    //
    //
    //


    [SerializeField] float KBSmoothness = 0.5f;

    [HideInInspector] public bool rolling;
    [HideInInspector] public bool strafe;
    [HideInInspector] public bool attacking;
    [HideInInspector] public bool knockedBack;
    [HideInInspector] public float goopTime;
    [HideInInspector] public float rotation;
    [HideInInspector] public float stunned;
    [HideInInspector] public float KB;

    private bool _stopped;
    private Vector3 _rollDir;
    [SerializeField, ReadOnly] private float _oldSpeed;

    public bool IsRunning => Input.GetKey(_run);
    public float ForwardSpeed => Vector3.Dot(_p.RB.velocity, transform.GetChild(0).forward);
    public void ResumeMovement() => _stopped = false;
    private float _targetSpeed => Input.GetKey(_run) ? _runSpeed : _walkSpeed;

    private void Start()
    {
        rotation = transform.localEulerAngles.y;
        _p = GetComponent<Player>();   
    }

    private void Update()
    {
        if (_stopped) return;

        goopTime -= Time.deltaTime;

        if (Input.GetKeyDown(_roll)) Roll();
        if (!rolling || !attacking) AlignToCamera();
        Move();

        //if (KB > 0.01f) KB = Mathf.Lerp(KB, 0, KBSmoothness);
        //else knockedBack = false;
    }

    public void KnockBack(GameObject _source, float _KB, Vector3 offset)
    {
        if ( _KB <= 0) return;
        KB = _KB;
        knockedBack = true;
    }

    public async void Roll() {
        if (rolling || GetInputDir().magnitude < 0.01f) return;
        rolling = true;



        _p.Anim.Roll();
        _p.Sounds.Get(PSoundKey.ROLL).Play();
        _rollDir = GetMoveDir();

        await Task.Delay(Mathf.RoundToInt(dashTime * 1000));
        rolling = false;
    }

    public void StopMovement()
    {
        _stopped = true;
        _p.RB.velocity = Vector3.zero;
        rolling = false;
    }

    void AlignToCamera() {
        var camForward = GameManager.i.Camera.transform.forward;
        camForward.y = 0;
        transform.forward = Vector3.Lerp(transform.forward, camForward, cameraAlignSmoothness * Time.deltaTime);

        bool turningLeft = false;
        bool turningRight = false;
        if (transform.eulerAngles.y - rotation > 0.01f) turningRight = true;
        else if (rotation - transform.eulerAngles.y > 0.01f) turningLeft = true;

        _p.Anim.SetTurning(turningLeft, turningRight);

        rotation = transform.eulerAngles.y;
    }

    private void Move()
    {
        if (_stopped) {
            _p.RB.velocity = Vector3.zero;
            return;
        }

        var inputDir = GetInputDir();
        var moveDir = GetMoveDir(inputDir);

        var targetSpeed = inputDir.magnitude > 0 ? _targetSpeed : 0;
        float speed = Mathf.Lerp(_oldSpeed, targetSpeed, _accelerationSnapiness * Time.deltaTime);
        _oldSpeed = speed;
        if (rolling) {
            speed = dashSpeed;
            moveDir = _rollDir;
        }

        _p.Anim.SetSpeed(speed > 0.01f ? (speed > _walkSpeed + 0.01f ? PAnimSpeeds.RUNNING : PAnimSpeeds.WALKING) : PAnimSpeeds.STILL);

        var vert = _p.RB.velocity.y;

        _p.RB.velocity = (moveDir * speed) + (Vector3.up * vert);

        AlignModelWithMoveDir();

    }

    private Vector3 GetMoveDir()
    {
        return GetMoveDir(GetInputDir());
    }

    private Vector3 GetMoveDir(Vector2 inputDir)
    {
        var dir = new Vector3();

        dir += transform.right * inputDir.x;
        dir += transform.forward * inputDir.y;

        return dir.normalized;
    }

    private Vector2 GetInputDir()
    {
        var dir = Vector2.zero;
        if (Input.GetKey(_forward)) dir.y = 1;
        if (Input.GetKey(_backward)) dir.y = -1;
        if (Input.GetKey(_right)) dir.x = 1;
        if (Input.GetKey(_left)) dir.x = -1;
        return dir;
    }

    void AlignModelWithMoveDir()
    {
        Transform model = transform.GetChild(0);

        if (_p.InCombat || GetComponent<PFighting>().chargingSpear()) { 
            model.transform.localEulerAngles = Vector3.zero;  
            return; 
        }

        if (_p.RB.velocity.magnitude <= 0.01f) return;
        
        var originalRot = model.transform.localEulerAngles;
        model.LookAt(transform.position + _p.RB.velocity.normalized);
        var targetRot = model.transform.localEulerAngles;
        targetRot.x = originalRot.x;
        targetRot.z = originalRot.z;
        model.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(originalRot), Quaternion.Euler(targetRot), _moveDirAlignSmoothness * Time.deltaTime);
    }

    private Vector3 GetStrafeDir() {
        var strafeDir = Vector3.zero;
        strafe = !attacking;

        if (Input.GetKey(_right)) strafeDir = transform.right;
        if (Input.GetKey(_left)) strafeDir = transform.right * -1;
        else strafe = false;

        return strafeDir;
    }
}
