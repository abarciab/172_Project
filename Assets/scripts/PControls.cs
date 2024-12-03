using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PMovement))]
public class PControls : MonoBehaviour
{
    [SerializeField, SearchableEnum] KeyCode _forward = KeyCode.W;
    [SerializeField, SearchableEnum] KeyCode _left = KeyCode.A;
    [SerializeField, SearchableEnum] KeyCode _backward = KeyCode.S;
    [SerializeField, SearchableEnum] KeyCode _right = KeyCode.D;
    [SerializeField, SearchableEnum] KeyCode _run = KeyCode.LeftShift;
    [SerializeField, SearchableEnum] KeyCode _roll = KeyCode.Space;
    [SerializeField, SearchableEnum] KeyCode _interactKey = KeyCode.E;
    [SerializeField, SearchableEnum] KeyCode _abilityKey = KeyCode.E;
    [SerializeField, SearchableEnum] KeyCode _pauseKey = KeyCode.Escape;
    
    [SerializeField] bool mouseMove;
    PMovement move;
    PFighting fight;
    Player player;

    bool toggleRun;
    bool runMode; 

    public void SetToggleRunOn() {
        toggleRun = true;
    }

    public void DisableToggleRun() {
        toggleRun = false;
    }

    private void Start()
    {
        move = GetComponent<PMovement>();
        fight = GetComponent<PFighting>();
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(_pauseKey) && player.canRoll) GameManager.i.TogglePause();
        if (GameManager.i.paused) return;

        
        if (!fight.stabbing && !fight.chargingSpear() && Input.GetKeyDown(_roll) && player.canRoll) move.Roll();

        move.goForward = Input.GetKey(_forward);
        move.goBack = Input.GetKey(_backward);
        move.pressLeft = Input.GetKey(_left);
        move.pressRight = Input.GetKey(_right);
        if (!toggleRun) move.running = Input.GetKey(_run) && player.canRun;
        else {
            move.running = runMode && player.canRun;
            if (Input.GetKeyDown(_run)) runMode = !runMode;
        }

        if (Input.GetKeyDown(_interactKey)) player.ActivateInteractable();
        if (!Input.GetKey(_forward) && !Input.GetKey(_left) && !Input.GetKey(_right) && !Input.GetKey(_backward)) move.running = false;

        if (Input.GetMouseButtonDown(0)) fight.StartAimingSpear();
        if (Input.GetMouseButtonUp(0)) fight.ThrowSpear();
        if (Input.GetMouseButtonDown(1)) fight.Stab();
        if (Input.GetKeyDown(_abilityKey)) fight.ActivateShockwave();
    }
}
