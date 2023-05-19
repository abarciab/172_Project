using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpearUITracker : MonoBehaviour
{
    [SerializeField] float timeToWait = 1.5f;
    bool TimeThrown;
    PFighting fight;
    ThrownStaff spear;
    Image img;
    RectTransform rectT;

    private void Start()
    {
        fight = Player.i.GetComponent<PFighting>();
        img = GetComponent<Image>();
        spear = FindObjectOfType<ThrownStaff>();
        rectT = GetComponent<RectTransform>();
    }

    private void Update()
    {
        var vpPos = Camera.main.WorldToViewportPoint(spear.transform.position);
        rectT.anchoredPosition = new Vector2(Screen.width * vpPos.x, Screen.height * vpPos.y);
    }


}
