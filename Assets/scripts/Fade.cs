using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    [SerializeField] bool fade = true;
    [SerializeField] Image img;

    private void Start()
    {
        img.color = new Color(0, 0, 0);
    }

    private void Update()
    {
        if (!fade) return;

        var col = img.color;
        col.a = Mathf.Lerp(col.a, 0, 0.025f);
        img.color = col;
        if (img.color.a <= 0.05f) gameObject.SetActive(false);
    }
}
