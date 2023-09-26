using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CheckBox : MonoBehaviour
{
    [SerializeField] Image img;
    [SerializeField] Color checkedColor, uncheckedColor, hoverColor;
    bool active;

    [SerializeField] UnityEvent activeEvent, inactiveEvent;
    [SerializeField] bool startActive;

    private void Start()
    {
        if (startActive) Click();
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

        if (active && activeEvent != null) activeEvent.Invoke();
        if (!active && inactiveEvent != null) inactiveEvent.Invoke();
    }
}
