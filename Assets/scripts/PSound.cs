using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSound : MonoBehaviour
{
    public void PlaySwoosh() {
        AudioManager.instance.PlaySound(0, gameObject);
    }

    public void PlayFootStep()
    {
        AudioManager.instance.PlaySound(3, gameObject);
    }
}
