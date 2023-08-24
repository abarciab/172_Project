using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEventCoord : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] string hoverBool = "Hover";

    public void SetTrigger(string trigger)
    {
        anim.SetTrigger(trigger);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void Hover()
    {
        anim.SetBool(hoverBool, true);
    }

    public void EndHover()
    {
        anim.SetBool(hoverBool, false);
    }
}
