using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraZone : MonoBehaviour {
    [Header("On enter")]
    [SerializeField] CameraState.StateName enterState;
    [SerializeField] List<GameObject> enableOnEnter;
    [SerializeField] bool stopAndPose;
    [SerializeField] Transform poseTarget;

    [Header("On exit")]
    [SerializeField] bool switchOnExit;
    [SerializeField] bool switchToDefault, destroyOnExit;
    [SerializeField] CameraState.StateName exitState;

    [Header("KeyPress")]
    [SerializeField] KeyCode key;
    [SerializeField] CameraState.StateName keyState;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>() != null) {
            if (enterState != CameraState.StateName.None) CameraState.i.SwitchToState(enterState);
            foreach (var obj in enableOnEnter) if (obj != null && !obj.activeInHierarchy) obj.SetActive(true);
            if (stopAndPose) Player.i.GetComponent<PMovement>().SlowDownAndPose(poseTarget);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!switchOnExit) return;

        if (other.GetComponent<Player>() != null) {
            if (destroyOnExit) Destroy(gameObject);
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