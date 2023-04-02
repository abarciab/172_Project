using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraZone : MonoBehaviour {
    [Header("On enter")]
    [SerializeField] CameraState.StateName enterState;

    [Header("On exit")]
    [SerializeField] bool switchOnExit;
    [SerializeField] bool switchToDefault;
    [SerializeField] CameraState.StateName exitState;

    [Header("KeyPress")]
    [SerializeField] KeyCode key;
    [SerializeField] CameraState.StateName keyState;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>() != null) {
            if (enterState != CameraState.StateName.None) CameraState.i.SwitchToState(enterState);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!switchOnExit) return;

        if (other.GetComponent<Player>() != null) {
            if (exitState != CameraState.StateName.None) CameraState.i.SwitchToState(exitState);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<Player>() != null) {
            if (keyState != CameraState.StateName.None && Input.GetKeyDown(key)) CameraState.i.SwitchToState(keyState);
        }
    }

}