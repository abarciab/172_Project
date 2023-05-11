using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PMovement))]
public class PControls : MonoBehaviour
{
    [SerializeField] KeyCode forward = KeyCode.W, left = KeyCode.A, backward = KeyCode.S, right = KeyCode.D, run = KeyCode.LeftShift,
        standUpKey = KeyCode.Space, roll = KeyCode.LeftControl, interactKey = KeyCode.E, abilityKey = KeyCode.E, pauseKey = KeyCode.Escape;
    [SerializeField] bool mouseMove;
    [SerializeField] float sitControlTime = 1f;
    float timeSitting;
    PMovement move;
    PFighting fight;
    Player player;

    [Header("Facts")]
    [SerializeField] Fact throwFact;
    [SerializeField] Fact stabFact, swFact;

    private void Start()
    {
        move = GetComponent<PMovement>();
        fight = GetComponent<PFighting>();
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey) && !GameManager.i.paused) GameManager.i.TogglePause();

        if (GameManager.i.paused) return;

        if (move.sitting) timeSitting += Time.deltaTime;
        if (move.sitting && timeSitting >= sitControlTime) GlobalUI.i.DisplayPrompt("press alt to stand up");

        if (move.posing) return;

        if (move.sitting && Input.GetKeyDown(standUpKey)) { move.sitting = false; GlobalUI.i.HidePrompt(); }
        if (!move.sitting && Input.GetKeyDown(roll)) move.Roll();

        move.goForward = Input.GetKey(forward);
        move.goBack = Input.GetKey(backward);
        move.pressLeft = Input.GetKey(left);
        move.pressRight = Input.GetKey(right);
        move.running = Input.GetKey(run);

        if (Input.GetKeyDown(interactKey)) player.ActivateInteractable();
        if (!Input.GetKey(forward) && !Input.GetKey(left) && !Input.GetKey(right) && !Input.GetKey(backward)) move.running = false;

        if (Input.GetMouseButtonDown(0) && FactManager.i.IsPresent(throwFact)) fight.StartAimingSpear();
        if (Input.GetMouseButtonUp(0) && FactManager.i.IsPresent(throwFact)) fight.ThrowStaff();
        if (Input.GetMouseButtonDown(1) && FactManager.i.IsPresent(stabFact)) fight.Stab();
        if (Input.GetKeyDown(abilityKey) && FactManager.i.IsPresent(swFact)) fight.ActivateShockwave();

        if (move.alignToCamera) return;
        move.turnRight = Input.GetKey(right);
        move.turnLeft = Input.GetKey(left);
    }
}
