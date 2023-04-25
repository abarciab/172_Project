using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySound : MonoBehaviour
{
    [SerializeField] int footstepSound, hurtSound, attack1Sound;
    AudioSource footstepSource, hurtSource;

    private void Start()
    {
        footstepSource = gameObject.AddComponent<AudioSource>();
        hurtSource = gameObject.AddComponent<AudioSource>();
    }

    public void TakeHit()
    {
        AudioManager.instance.PlaySound(hurtSound, hurtSource);
    }

    public void PlayFootstep()
    {
        AudioManager.instance.PlaySound(footstepSound, footstepSource);
    }

    public void PlayAttack1()
    {
        AudioManager.instance.PlaySound(attack1Sound, footstepSource);
    }
}
