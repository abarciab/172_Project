using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTester : MonoBehaviour
{
    [SerializeField] Sound sound;
    Sound instanced;
    public bool play;
    private void Start()
    {
        instanced = Instantiate(sound);
    }

    void PlaySound()
    {
        instanced.Delete();
        instanced = Instantiate(sound);

        instanced.Play(transform);
    }

    private void Update()
    {
        if (play) {
            play = false;
            PlaySound();
        }
    }
}
