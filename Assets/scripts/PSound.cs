using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSound : MonoBehaviour
{
    [SerializeField] Sound footstep, stabSwish, drawSpear;

    private void Start()
    {
        footstep = Instantiate(footstep);
        stabSwish = Instantiate(stabSwish);
        drawSpear = Instantiate(drawSpear);
    }

    public void PlaySwoosh() {
        stabSwish.Play(transform);
    }

    public void PlayFootStep()
    {
        footstep.Play(transform, false);
    }

    public void DrawSpear()
    {
        drawSpear.Play(transform);
    }
}
