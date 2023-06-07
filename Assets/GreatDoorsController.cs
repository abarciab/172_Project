using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreatDoorsController : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] Sound doorOpenSound;

    void Start()
    {
        anim.enabled = false;
        doorOpenSound = Instantiate(doorOpenSound);
    }

    public void OpenDoors()
    {
        //anim.Play("Armature|DoorOpening");
        anim.enabled = true;
        doorOpenSound.Play();
    }

}
