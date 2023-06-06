using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSound : MonoBehaviour
{
    [SerializeField] Sound footstep, footstepHard, stabSwish, drawSpear;
    private bool isDirtSurface = true;

    private void Start()
    {
        footstep = Instantiate(footstep);
        footstepHard = Instantiate(footstepHard);
        stabSwish = Instantiate(stabSwish);
        drawSpear = Instantiate(drawSpear);

    }

    public void PlaySwoosh() {
        stabSwish.Play(transform);
    }
    public void SetSoftSurface(bool isSoft)
    {
        isDirtSurface = isSoft;
    }

    public void PlayFootStep()
    {
        if (isDirtSurface)
            footstep.Play(transform, false);
        else
            footstepHard.Play(transform, false);
    }

    public void DrawSpear()
    {
        drawSpear.Play(transform);
    }


}
