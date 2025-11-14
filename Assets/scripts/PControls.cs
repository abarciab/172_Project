using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PMovement))]
public class PControls : MonoBehaviour
{

    [SerializeField, SearchableEnum] KeyCode _interactKey = KeyCode.E;
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


        if (Input.GetKeyDown(_interactKey)) player.Interact();

        if (Input.GetMouseButtonDown(0)) fight.StartAimingSpear();
        if (Input.GetMouseButtonUp(0)) fight.ThrowSpear();
        if (Input.GetMouseButtonDown(1)) fight.Stab();
    }
}
