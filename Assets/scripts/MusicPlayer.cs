using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] AudioSource music1Source, music2Source, ambientSource;

    private void Start()
    {
        AudioManager.instance.PlaySound(6, music1Source);
        AudioManager.instance.PlaySound(7, ambientSource);
    }
}
