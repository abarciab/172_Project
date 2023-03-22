using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PMovement : MonoBehaviour
{
    [SerializeField] float forwardSpeed, runSpeed, backSpeed, stoppingFriction = 0.025f, rotationSpeed = 0.5f, cameraAlignSmoothness = 0.2f, stepSpeed, stepTime;
    [HideInInspector] public bool goForward, turnLeft, turnRight, goBack, running, alignToCamera, stepping;
    public bool sitting;
    
    float rotation;
    Rigidbody rb;
    Player p;

    public void StepForward()
    {
        stepping = true;
        StartCoroutine(StopStep());
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
        alignToCamera = CameraState.i.mouseControl;
        SetPlayerStats();

        if (sitting) return;
        Turn();
        Move();
    }

    void Turn()
    {
        if (alignToCamera) {
            var camForward = CameraState.i.transform.forward;
            camForward.y = 0;
            transform.forward = Vector3.Lerp(transform.forward, camForward, cameraAlignSmoothness);

            turnLeft = turnRight = false;
            if (rotation < transform.eulerAngles.y) turnLeft = true;
            else if (rotation > transform.eulerAngles.y) turnRight = true;
            rotation = transform.eulerAngles.y;
            return;
        }

        if (turnLeft) rotation -= rotationSpeed;
        if (turnRight) rotation += rotationSpeed;

        var rot = transform.localEulerAngles;
        rot.y = rotation;
        transform.localEulerAngles = rot;
    }

    void Move()
    {
        float speed = running ? runSpeed : forwardSpeed;

        var verticalVel = rb.velocity;
        verticalVel.x = verticalVel.z = 0;

        if (goForward) rb.velocity = (transform.forward * speed) + verticalVel;
        else if (goBack) rb.velocity = (transform.forward * backSpeed * -1) + verticalVel;
        else if (stepping) rb.velocity = (transform.forward * stepSpeed) + verticalVel;
        else rb.velocity = Vector3.Lerp(rb.velocity, verticalVel, stoppingFriction);
    }

    void SetPlayerStats()
    {
        p.speed3D = rb.velocity;
        p.forwardSpeed = Vector3.Dot(rb.velocity, transform.forward);
    }
}
