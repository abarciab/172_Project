using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSound : MonoBehaviour
{
    [SerializeField] Sound footstep, footstepHard, footstepGoop, stabSwish, drawSpear, startConvo;
    private bool isDirtSurface = true;

    private void Start()
    {
        footstep = Instantiate(footstep);
        footstepHard = Instantiate(footstepHard);
        stabSwish = Instantiate(stabSwish);
        drawSpear = Instantiate(drawSpear);
        startConvo = Instantiate(startConvo);
        footstepGoop = Instantiate(footstepGoop);

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
        if (Player.i.goopTime > 0) footstepGoop.Play(transform);
        else if (isDirtSurface) footstep.Play(transform);
        else footstepHard.Play(transform);
    }

    public void DrawSpear()
    {
        drawSpear.Play(transform);
    }

    public void StartConversation()
    {
        startConvo.Play();
    }

}
