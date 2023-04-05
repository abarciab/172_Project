using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PMovement))]
public class PControls : MonoBehaviour
{
    [SerializeField] KeyCode forward = KeyCode.W, left = KeyCode.A, backward = KeyCode.S, right = KeyCode.D, run = KeyCode.LeftShift, standUpKey = KeyCode.Space, roll = KeyCode.LeftControl;
    [SerializeField] bool mouseMove;
    [SerializeField] float sitControlTime = 1f;
    float timeSitting;
    PMovement move;
    PFighting fight;

    private void Start()
    {
        move = GetComponent<PMovement>();
        fight = GetComponent<PFighting>();
    }

    private void Update()
    {
        if (move.sitting) timeSitting += Time.deltaTime;
        if (timeSitting >= sitControlTime) GlobalUI.i.DisplayPrompt("press alt to stand up");

        if (move.sitting && Input.GetKeyDown(standUpKey)) { move.sitting = false; GlobalUI.i.HidePrompt(); }

        if (Input.GetMouseButtonDown(0) && !move.sitting) fight.PressAttack();

        if (!move.sitting && Input.GetKeyDown(roll)) move.Dash();

        if (Input.GetMouseButtonDown(2)) Player.i.ToggleLockOn();

        move.goForward = Input.GetKey(forward);
        move.goBack = Input.GetKey(backward);
        move.pressLeft = Input.GetKey(left);
        move.pressRight = Input.GetKey(right);

        if (Input.GetKeyDown(run)) move.running = !move.running;
        if (!Input.GetKey(forward) && !Input.GetKey(left) && !Input.GetKey(right) && !Input.GetKey(backward)) move.running = false;
        if (Input.GetKeyDown(run)) fight.PutAwayStaff();

        if (move.alignToCamera) return;
        move.turnRight = Input.GetKey(right);
        move.turnLeft = Input.GetKey(left);
    }
}
