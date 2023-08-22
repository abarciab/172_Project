using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckBox : MonoBehaviour
{
    [SerializeField] Image img;
    [SerializeField] Color checkedColor, uncheckedColor, hoverColor;
    bool active;

    private void Start()
    {
        img.color = active ? checkedColor : uncheckedColor;
    }

    public void Hover()
    {
        img.color = hoverColor;
    }

    public void EndHover()
    {
        img.color = active ? checkedColor : uncheckedColor;
    }

    public void Click()
    {
        active = !active;
        EndHover();
    }
}
