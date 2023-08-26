using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySound : MonoBehaviour
{
    [SerializeField] Sound footstepSound, hurtSound, attack1Sound, attack2Sound, attack3Sound;
    
    private void Start()
    {
        if (footstepSound) footstepSound = Instantiate(footstepSound);
        if (attack1Sound) attack1Sound = Instantiate(attack1Sound);
        if (hurtSound) hurtSound = Instantiate(hurtSound);
        if (attack2Sound) attack2Sound = Instantiate(attack2Sound);
        if (attack3Sound) attack3Sound = Instantiate(attack3Sound);
    }

    public void TakeHit()
    {
        if (hurtSound) hurtSound.Play();
    }

    public void PlayFootstep()
    {
        if (footstepSound) footstepSound.Play(transform);
    }

    public void PlayAttack1()
    {
        if (attack1Sound) attack1Sound.Play(transform);
    }
    public void PlayAttack2()
    {
        if (attack2Sound) attack2Sound.Play(transform);
    }

    public void PlayAttack3()
    {
        if (attack3Sound) attack3Sound.Play(transform);
    }
}
