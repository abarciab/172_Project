using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSound : MonoBehaviour
{
    [SerializeField] Sound footstep, stabSwish;

    private void Start()
    {
        footstep = Instantiate(footstep);
        stabSwish = Instantiate(stabSwish);
    }

    public void PlaySwoosh() {
        stabSwish.Play(transform);
    }

    public void PlayFootStep()
    {
        footstep.Play(transform);
    }
}
