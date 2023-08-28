using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEventCoord : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] Sound hoverSound, clickSound;
    [SerializeField] string hoverBool = "Hover";

    private void Start()
    {
        if (hoverSound != null) hoverSound = Instantiate(hoverSound);
        if (clickSound != null) clickSound = Instantiate(clickSound);
    }

    public void SetTrigger(string trigger)
    {
        anim.SetTrigger(trigger);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void Click()
    {
        if (clickSound != null) clickSound.Play();
    }

    public void Hover()
    {
        if (hoverSound != null) hoverSound.Play();
        anim.SetBool(hoverBool, true);
    }

    public void EndHover()
    {
        anim.SetBool(hoverBool, false);
    }
}
