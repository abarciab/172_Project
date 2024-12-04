using MyBox;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PMovement : MonoBehaviour
{
    private Player _p;
    
    [HideInInspector] public bool IsRunning;

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
    [SerializeField] float forwardSpeed;
    [SerializeField] float runSpeed;
    [SerializeField] float goopMult = 0.5f;
    [SerializeField] float stoppingFriction = 0.025f;

    [Header("Rotation")]
    [SerializeField] float rotationSpeed = 0.5f;
    [SerializeField] float cameraAlignSmoothness = 0.2f;
    [SerializeField] float _moveDirAlignSmoothness = 5;

    [Header("Rolling")]
    [SerializeField] float dashSpeed;
    [SerializeField] float dashTime;

    //
    //
    //


    [SerializeField] float KBSmoothness = 0.5f;

    [HideInInspector] public bool rolling;

    [HideInInspector] public bool turnLeft;
    [HideInInspector] public bool turnRight;
    [HideInInspector] public bool strafe;
    [HideInInspector] public bool attacking;
    [HideInInspector] public bool knockedBack;
    [HideInInspector] public float goopTime;
    [HideInInspector] public float rotation;
    [HideInInspector] public float stunned;
    [HideInInspector] public float KB;

    private bool _stopped;
    private Vector3 _rollDir;
    private Vector3 _source;

    public float ForwardSpeed => Vector3.Dot(_p.RB.velocity, transform.GetChild(0).forward);
    public void ResumeMovement() => _stopped = false;

    private void Start()
    {
        rotation = transform.localEulerAngles.y;
        _p = GetComponent<Player>();   
    }

    private void Update()
    {
        if (_stopped) return;

        //if (!fight.stabbing && !fight.chargingSpear() && Input.GetKeyDown(_roll) && player.canRoll) move.Roll();
        if (Input.GetKeyDown(_roll)) Roll();

        IsRunning = Input.GetKey(_run);

        //if (!Input.GetKey(_forward) && !Input.GetKey(_left) && !Input.GetKey(_right) && !Input.GetKey(_backward)) move.IsRunning = false;

        goopTime -= Time.deltaTime;

        if (!rolling || !attacking) AlignToCamera();

        Move();

        if (KB > 0.01f) KB = Mathf.Lerp(KB, 0, KBSmoothness);
        else knockedBack = false;
    }

    public void KnockBack(GameObject _source, float _KB, Vector3 offset)
    {
        if (_source == null || _KB <= 0) return;

        this._source = _source.transform.position + offset;
        KB = _KB;
        knockedBack = true;
    }

    public async void Roll() {
        if (rolling) return;
        rolling = true;

        _p.Sounds.Get(PSoundKey.ROLL).Play();
        _rollDir = GetRollDir();

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

        turnLeft = turnRight = false;
        if (transform.eulerAngles.y - rotation > 0.01f) turnLeft = true;
        else if (rotation - transform.eulerAngles.y > 0.01f) turnRight = true;
        rotation = transform.eulerAngles.y;
    }

    void Move()
    {
        float speed = IsRunning ? runSpeed : forwardSpeed;
        speed = goopTime > 0 ? speed * goopMult : speed;
        if (attacking) speed = 0;

        var verticalVel = _p.RB.velocity;
        verticalVel.x = verticalVel.z = 0;

        Vector3 horizontalDir = !attacking ? GetStrafeDir() : Vector3.zero;
        var KBdir = Vector3.zero;
        if (knockedBack) {
            KBdir = (transform.position - _source).normalized;
            KBdir.y = 0;
        }

        if (_stopped) _p.RB.velocity = Vector3.zero;
        else if (knockedBack) _p.RB.velocity = KBdir * KB + verticalVel;
        else if (rolling) _p.RB.velocity = (_rollDir * dashSpeed) + verticalVel;

        else if (Input.GetKey(_forward)) _p.RB.velocity = (transform.forward + horizontalDir).normalized * speed + verticalVel;
        else if (Input.GetKey(_backward)) _p.RB.velocity = (transform.forward * -1 + horizontalDir).normalized * speed + verticalVel;
        else _p.RB.velocity = Vector3.Lerp(_p.RB.velocity, horizontalDir * speed + verticalVel, stoppingFriction);

        if (attacking) {
            Transform model = transform.GetChild(0);
            model.transform.localRotation = Quaternion.Lerp(model.localRotation, Quaternion.Euler(Vector3.zero), 0.2f); //TEMP
        }
        else AlignModel();
    }

    void AlignModel()
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

    private Vector3 GetRollDir() {
        Vector3 dir = Vector3.zero;
        if (Input.GetKey(_right)) dir += transform.right;
        if (Input.GetKey(_left)) dir += transform.right * -1;
        if (Input.GetKey(_forward)) dir += transform.forward;
        if (Input.GetKey(_backward)) dir += transform.forward * -1;

        return dir.normalized;
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
