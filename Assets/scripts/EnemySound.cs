using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySound : MonoBehaviour
{
    [SerializeField] Sound footstepSound, hurtSound, attack1Sound;
    
    private void Start()
    {
        if (footstepSound) footstepSound = Instantiate(footstepSound);
        if (attack1Sound) attack1Sound = Instantiate(attack1Sound);
        
        //return;
        //footstepSound = Instantiate(attack1Sound);
    }

    public void TakeHit()
    {
        //AudioManager.instance.PlaySound(hurtSound, hurtSource);
    }

    public void PlayFootstep()
    {
        //if (footstepSound) footstepSound.Play(transform);
    }

    public void PlayAttack1()
    {
        if (attack1Sound) attack1Sound.Play(transform);
    }
}
